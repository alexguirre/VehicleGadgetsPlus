namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System.Collections.Generic;

    using Rage;

    internal static class Condition
    {
        public delegate bool ConditionDelegate(VehicleGadget gadget);


        public static IReadOnlyDictionary<string, ConditionDelegate> ConditionsByName = new Dictionary<string, ConditionDelegate>()
        {
            { "EngineOn", (g) => g.Vehicle.IsEngineOn },
            { "EngineOff", (g) => !g.Vehicle.IsEngineOn },
            { "SirenOn", (g) => g.Vehicle.IsSirenOn },
            { "SirenOff", (g) => !g.Vehicle.IsSirenOn },
        };

        public static ConditionDelegate[] GetConditionsFromString(string conditions)
        {
            string[] splittedConditions = conditions.Replace(" ", "").Split(',');

            List<ConditionDelegate> delegates = new List<ConditionDelegate>();
            for (int i = 0; i < splittedConditions.Length; i++)
            {
                if(ConditionsByName.TryGetValue(splittedConditions[i], out ConditionDelegate del))
                {
                    delegates.Add(del);
                }
                else
                {
                    Game.LogTrivial($"The condition with the name '{splittedConditions[i]}' doesn't exist.");
                }
            }

            return delegates.ToArray();
        }
    }
}
