namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;
    using System.IO;
    using System.Text;
    using System.Linq;
    using System.Windows.Forms;
    using System.Collections.Generic;
    using System.CodeDom;
    using System.CodeDom.Compiler;

    using Microsoft.CSharp;

    using Rage;

    using VehicleGadgetsPlus.VehicleGadgets.XML.Conditions;

    internal static class Conditions
    {
        public delegate bool ConditionDelegate(Vehicle vehicle);

        private const string ConditionBaseCodeTemplate = @"
using System;
using System.Linq;
using System.Windows.Forms;
using Rage;
using Rage.Native;

class __ConditionCode__
{{
    {0}
}}
";
        private const string ConditionMethodCodeTemplate = @"
    public static bool {0}(Vehicle vehicle)
    {{
        {1}
    }}
";

        public static readonly string DefaultConditionsXmlFile = Path.Combine(Plugin.ConditionsFolder, "DefaultConditions.xml");

        private static readonly Dictionary<string, ConditionDelegate> conditionsByName = new Dictionary<string, ConditionDelegate>(); 

        public static void CompileConditions(Dictionary<Model, ConditionEntry[]> extraConditionsByModel = null)
        {
            if (!Directory.Exists(Plugin.ConditionsFolder))
                Directory.CreateDirectory(Plugin.ConditionsFolder);

            ConditionEntry[] defaultEntries = File.Exists(DefaultConditionsXmlFile) ? GetDefaultConditionsFromFile() : GetDefaultConditions();

            CompileConditionsEntries(defaultEntries, extraConditionsByModel);
        }

        private static ConditionEntry[] GetDefaultConditionsFromFile()
        {
            ConditionsConfig config = Util.Deserialize<ConditionsConfig>(DefaultConditionsXmlFile);
            return config.Conditions;
        }

        private static ConditionEntry[] GetDefaultConditions()
        {
            ConditionsConfig config = new ConditionsConfig
            {
                Conditions = new[]
                {
                    new ConditionEntry
                    {
                        Name = "EngineOn",
                        Code = "return vehicle.IsEngineOn;",
                    },
                    new ConditionEntry
                    {
                        Name = "EngineOff",
                        Code = "return !vehicle.IsEngineOn;",
                    },
                    new ConditionEntry
                    {
                        Name = "SirenOn",
                        Code = "return vehicle.IsSirenOn;",
                    },
                    new ConditionEntry
                    {
                        Name = "SirenOff",
                        Code = "return !vehicle.IsSirenOn;",
                    }
                }
            };

            Util.Serialize(DefaultConditionsXmlFile, config);

            return config.Conditions;
        }

        private static void CompileConditionsEntries(ConditionEntry[] defaultEntries, Dictionary<Model, ConditionEntry[]> extraConditionsByModel)
        {
            Game.LogTrivial("Compiling conditions...");

            StringBuilder methods = new StringBuilder();
            int entriesCount = 0;
            List<string> entriesNames = new List<string>();
            if (defaultEntries != null)
            {
                foreach (ConditionEntry entry in defaultEntries)
                {
                    string name = entry.Name;
                    methods.AppendFormat(ConditionMethodCodeTemplate, entry.Name, entry.Code);
                    entriesCount++;
                    entriesNames.Add(name);
                }
            }
            if (extraConditionsByModel != null)
            {
                foreach (KeyValuePair<Model, ConditionEntry[]> extraEntriesPair in extraConditionsByModel)
                {
                    foreach (ConditionEntry entry in extraEntriesPair.Value)
                    {
                        string name = GetMethodNameFor(extraEntriesPair.Key, entry.Name);
                        methods.AppendFormat(ConditionMethodCodeTemplate, name, entry.Code);
                        entriesCount++;
                        entriesNames.Add(name);
                    }
                }
            }

            string code = String.Format(ConditionBaseCodeTemplate, methods.ToString());

            Game.LogTrivialDebug(code);

            CompilerParameters parameters = new CompilerParameters();
            parameters.IncludeDebugInformation = false;
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;
            parameters.CompilerOptions = "/optimize";
            parameters.ReferencedAssemblies.AddRange(new[] { "mscorlib.dll", "System.dll", "System.Core.dll", "System.Dynamic.dll", "System.Windows.Forms.dll", "System.Drawing.dll", "Microsoft.CSharp.dll", typeof(Vehicle).Assembly.Location });
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

            if (results.Errors.HasErrors)
            {
                Game.LogTrivial("   Compilation failed");
                Game.LogTrivial("   Errors:");
                foreach (CompilerError e in results.Errors)
                    Game.LogTrivial("       " + e.ToString());
            }

            Type type = results.CompiledAssembly.GetType("__ConditionCode__");
            foreach (string name in entriesNames)
            {
                conditionsByName.Add(name, (ConditionDelegate)type.GetMethod(name).CreateDelegate(typeof(ConditionDelegate)));
            }
        }

        public static ConditionDelegate[] GetConditionsFromString(Model model, string conditions)
        {
            string[] splittedConditions = conditions.Replace(" ", "").Split(',');

            List<ConditionDelegate> delegates = new List<ConditionDelegate>();
            for (int i = 0; i < splittedConditions.Length; i++)
            {
                if(conditionsByName.TryGetValue(GetMethodNameFor(model, splittedConditions[i]), out ConditionDelegate del))
                {
                    delegates.Add(del);
                }
                else if (conditionsByName.TryGetValue(splittedConditions[i], out ConditionDelegate del2))
                {
                    delegates.Add(del2);
                }
                else
                {
                    Game.LogTrivial($"The condition with the name '{splittedConditions[i]}' doesn't exist.");
                }
            }

            return delegates.ToArray();
        }

        private static string GetMethodNameFor(Model model, string conditionName) => $"M{model.Hash}_{conditionName}";
    }
}
