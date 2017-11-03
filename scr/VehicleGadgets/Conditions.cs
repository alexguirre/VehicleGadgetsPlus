namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;
    using System.IO;
    using System.Text;
    using System.Linq;
    using System.Collections.Generic;
    using System.Reflection;
    using System.CodeDom.Compiler;

    using Microsoft.CSharp;

    using Rage;

    using VehicleGadgetsPlus.VehicleGadgets.XML.Conditions;

    internal static class Conditions
    {
        public delegate bool? ConditionDelegate(Vehicle vehicle, bool isPlayerInsideVehicle);

        private const string ConditionCodeClassName = "__ConditionCode__";
        private const string ConditionBaseCodeTemplate = @"
using System;
using System.Linq;
using System.Windows.Forms;
using Rage;
using Rage.Native;

static class " + ConditionCodeClassName + @"
{{
    {0}
}}
";
        private const string ConditionMethodCodeTemplate = @"
    public static bool? {0}(Vehicle vehicle, bool isPlayerInsideVehicle)
    {{
        {1}
    }}
";
        public static readonly string CompiledConditionsCacheFilePath = Path.Combine(Plugin.ConditionsFolder, "CompiledConditions.cache");
        public static readonly string DefaultConditionsXmlFilePath = Path.Combine(Plugin.ConditionsFolder, "DefaultConditions.xml");

        private static readonly Dictionary<string, ConditionDelegate> conditionsByName = new Dictionary<string, ConditionDelegate>(); 

        public static void LoadConditions(Dictionary<Model, ConditionEntry[]> extraConditionsByModel = null)
        {
            if (!Directory.Exists(Plugin.ConditionsFolder))
                Directory.CreateDirectory(Plugin.ConditionsFolder);

            ConditionEntry[] defaultEntries = File.Exists(DefaultConditionsXmlFilePath) ? GetDefaultConditionsFromFile() : GetDefaultConditions();

            LoadConditionsEntries(defaultEntries, extraConditionsByModel);
        }

        private static ConditionEntry[] GetDefaultConditionsFromFile()
        {
            ConditionsConfig config = Util.Deserialize<ConditionsConfig>(DefaultConditionsXmlFilePath);
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

            Util.Serialize(DefaultConditionsXmlFilePath, config);

            return config.Conditions;
        }

        private static void LoadConditionsEntries(ConditionEntry[] defaultEntries, Dictionary<Model, ConditionEntry[]> extraConditionsByModel)
        {
            if (!LoadConditionsFromCache(defaultEntries, extraConditionsByModel))
            {
                CompileConditions(defaultEntries, extraConditionsByModel);
            }
        }

        private static void CompileConditions(ConditionEntry[] defaultEntries, Dictionary<Model, ConditionEntry[]> extraConditionsByModel)
        {
            Game.LogTrivial("Compiling conditions");

            StringBuilder methods = new StringBuilder();
            List<string> entriesNames = new List<string>();
            if (defaultEntries != null)
            {
                foreach (ConditionEntry entry in defaultEntries)
                {
                    string name = entry.Name;
                    methods.AppendFormat(ConditionMethodCodeTemplate, entry.Name, entry.Code);
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
                        entriesNames.Add(name);
                    }
                }
            }

            string code = String.Format(ConditionBaseCodeTemplate, methods.ToString());

            Game.LogTrivialDebug(code);

            CompilerParameters parameters = new CompilerParameters();
            parameters.IncludeDebugInformation = false;
            parameters.GenerateInMemory = false;
            parameters.GenerateExecutable = false;
            parameters.CompilerOptions = "/optimize /platform:x64";
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
            else
            {
                Type type = results.CompiledAssembly.GetType(ConditionCodeClassName);
                foreach (string name in entriesNames)
                {
                    conditionsByName.Add(name, (ConditionDelegate)type.GetMethod(name).CreateDelegate(typeof(ConditionDelegate)));
                }

                Game.LogTrivial("Conditions compiled, saving to cache");
                new CompiledConditionsCacheFile(CompiledConditionsCacheFile.GetCurrentXmlFiles(), results.CompiledAssembly).Write(CompiledConditionsCacheFilePath);
            }
        }

        private static bool LoadConditionsFromCache(ConditionEntry[] defaultEntries, Dictionary<Model, ConditionEntry[]> extraConditionsByModel)
        {
            if (File.Exists(CompiledConditionsCacheFilePath))
            {
                Game.LogTrivial("Conditions cache file exists");

                CompiledConditionsCacheFile cacheFile = new CompiledConditionsCacheFile();
                if (cacheFile.Read(CompiledConditionsCacheFilePath))
                {
                    if (cacheFile.CompareXmlFiles(CompiledConditionsCacheFile.GetCurrentXmlFiles()))
                    {
                        List<string> names = new List<string>();
                        if (defaultEntries != null)
                        {
                            foreach (ConditionEntry entry in defaultEntries)
                            {
                                string name = entry.Name;
                                names.Add(name);
                            }
                        }
                        if (extraConditionsByModel != null)
                        {
                            foreach (KeyValuePair<Model, ConditionEntry[]> extraEntriesPair in extraConditionsByModel)
                            {
                                foreach (ConditionEntry entry in extraEntriesPair.Value)
                                {
                                    string name = GetMethodNameFor(extraEntriesPair.Key, entry.Name);
                                    names.Add(name);
                                }
                            }
                        }

                        Type type = cacheFile.LoadAssembly().GetType(ConditionCodeClassName);
                        foreach (string name in names)
                        {
                            conditionsByName.Add(name, (ConditionDelegate)type.GetMethod(name).CreateDelegate(typeof(ConditionDelegate)));
                        }
                        Game.LogTrivial("Loaded conditions from cache");

                        return true;
                    }
                    else
                    {
                        Game.LogTrivial("There has been changes to the XML files since the cache was made, deleting cache");
                        File.Delete(CompiledConditionsCacheFilePath);
                    }
                }
                else
                {
                    Game.LogTrivial("Failed to read cache, deleting cache");
                    File.Delete(CompiledConditionsCacheFilePath);
                }

                cacheFile = null;
            }

            return false;
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

    internal sealed class CompiledConditionsCacheFile
    {
        public const uint FileHeader = 'V' | 'G' << 8 | 'C' << 16 | 'C' << 24;

        public int XmlFilesCount { get; private set; }
        public uint[] XmlFilesNamesHashes { get; private set; }
        public byte[][] XmlFilesContentMD5Hashes { get; private set; }

        public int AssemblyBytesCount { get; private set; }
        public byte[] AssemblyBytes { get; private set; }

        public CompiledConditionsCacheFile()
        {
        }

        public CompiledConditionsCacheFile(string[] xmlFiles, Assembly assembly)
        {
            if(xmlFiles == null)
            {
                throw new ArgumentNullException(nameof(xmlFiles));
            }

            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (assembly.IsDynamic)
            {
                throw new NotSupportedException();
            }

            XmlFilesCount = xmlFiles.Length;
            XmlFilesNamesHashes = new uint[xmlFiles.Length];
            for (int i = 0; i < xmlFiles.Length; i++)
            {
                XmlFilesNamesHashes[i] = Game.GetHashKey(Path.GetFileNameWithoutExtension(xmlFiles[i]));
            }
            XmlFilesContentMD5Hashes = new byte[xmlFiles.Length][];
            for (int i = 0; i < xmlFiles.Length; i++)
            {
                using (FileStream file = new FileStream(xmlFiles[i], FileMode.Open))
                {
                    XmlFilesContentMD5Hashes[i] = Encoding.Default.GetBytes(CryptographyHelper.GenerateMD5Hash(file));
                }
            }

            string location = assembly.Location;
            if (String.IsNullOrWhiteSpace(location))
            {
                throw new InvalidOperationException();
            }

            AssemblyBytes = File.ReadAllBytes(location).ToArray();
            AssemblyBytesCount = AssemblyBytes.Length;
        }

        public Assembly LoadAssembly()
        {
            return Assembly.Load(AssemblyBytes);
        }

        public bool CompareXmlFiles(string[] xmlFiles)
        {
            if (xmlFiles == null)
            {
                throw new ArgumentNullException(nameof(xmlFiles));
            }
            
            if(XmlFilesCount != xmlFiles.Length)
            {
                return false;
            }

            foreach (string xmlFile in xmlFiles)
            {
                uint nameHash = Game.GetHashKey(Path.GetFileNameWithoutExtension(xmlFile));
                if (!XmlFilesNamesHashes.Contains(nameHash))
                    return false;

                byte[] b;
                using (FileStream file = new FileStream(xmlFile, FileMode.Open))
                {
                    b = Encoding.Default.GetBytes(CryptographyHelper.GenerateMD5Hash(file));
                }

                bool isEqualToAny = false;
                foreach (byte[] md5Hash in XmlFilesContentMD5Hashes)
                {
                    if (md5Hash.Length == b.Length)
                    {
                        for (int i = 0; i < md5Hash.Length; i++)
                        {
                            if(md5Hash[i] != b[i])
                            {
                                break;
                            }

                            if(i == md5Hash.Length - 1)
                            {
                                isEqualToAny = true;
                            }
                        }
                    }

                    if (isEqualToAny)
                    {
                        break;
                    }
                }
                
                if(!isEqualToAny)
                {
                    return false;
                }
            }

            return true;
        }

        public void Write(string fileName)
        {
            using (FileStream file = new FileStream(fileName, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(file))
            {
                writer.Write(FileHeader);

                writer.Write(XmlFilesCount);
                for (int i = 0; i < XmlFilesCount; i++)
                {
                    writer.Write(XmlFilesNamesHashes[i]);
                    writer.Write(XmlFilesContentMD5Hashes[i], 0, 32);
                }

                writer.Write(AssemblyBytesCount);
                writer.Write(AssemblyBytes, 0, AssemblyBytesCount);
            }
        }

        public bool Read(string fileName)
        {
            using (FileStream file = new FileStream(fileName, FileMode.Open))
            using (BinaryReader reader = new BinaryReader(file))
            {
                try
                {
                    uint header = reader.ReadUInt32();
                    if (header != FileHeader)
                        return false;

                    XmlFilesCount = reader.ReadInt32();
                    XmlFilesNamesHashes = new uint[XmlFilesCount];
                    XmlFilesContentMD5Hashes = new byte[XmlFilesCount][];
                    for (int i = 0; i < XmlFilesCount; i++)
                    {
                        XmlFilesNamesHashes[i] = reader.ReadUInt32();
                        XmlFilesContentMD5Hashes[i] = reader.ReadBytes(32);
                    }

                    AssemblyBytesCount = reader.ReadInt32();
                    AssemblyBytes = reader.ReadBytes(AssemblyBytesCount);
                }
                catch (EndOfStreamException ex)
                {
                    return false;
                }
            }

            return true;
        }

        public static string[] GetCurrentXmlFiles()
        {
            List<string> fileNames = new List<string>();
            fileNames.AddRange(Directory.EnumerateFiles(Plugin.VehicleConfigsFolder, "*.xml", SearchOption.TopDirectoryOnly));
            fileNames.Add(Conditions.DefaultConditionsXmlFilePath);
            return fileNames.ToArray();
        }
    }
}
