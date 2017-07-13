namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;
    using System.Windows.Forms;
    using System.Collections.Generic;

    using Rage;

    internal static class Condition
    {
        public delegate bool ConditionDelegate(VehicleGadget gadget);

        public static IReadOnlyDictionary<string, ConditionDelegate> SimpleConditionsByName = new Dictionary<string, ConditionDelegate>()
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
                if(CheckComplexConditions(splittedConditions[i], out ConditionDelegate del1))
                {
                    delegates.Add(del1);
                }
                else if(SimpleConditionsByName.TryGetValue(splittedConditions[i], out ConditionDelegate del2))
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

        private static bool CheckComplexConditions(string str, out ConditionDelegate conditionDelegate)
        {
            const string KeyJustPressedName = "KeyJustPressed";
            const string KeyPressedName = "KeyPressed";
            const string ControllerButtonJustPressedName = "ControllerButtonJustPressed";
            const string ControllerButtonPressedName = "ControllerButtonPressed";

            if (str.StartsWith(KeyJustPressedName))
            {
                string s = str.Remove(0, KeyJustPressedName.Length);
                if(Enum.TryParse<Keys>(s, out Keys key))
                {
                    conditionDelegate = (v) => Game.IsKeyDown(key);
                    return true;
                }
            }
            else if(str.StartsWith(KeyPressedName))
            {
                string s = str.Remove(0, KeyPressedName.Length);
                if (Enum.TryParse<Keys>(s, out Keys key))
                {
                    conditionDelegate = (v) => Game.IsKeyDownRightNow(key);
                    return true;
                }
            }
            else if (str.StartsWith(ControllerButtonJustPressedName))
            {
                string s = str.Remove(0, ControllerButtonJustPressedName.Length);
                if (Enum.TryParse<ControllerButtons>(s, out ControllerButtons b))
                {
                    conditionDelegate = (v) => Game.IsControllerButtonDown(b);
                    return true;
                }
            }
            else if (str.StartsWith(ControllerButtonPressedName))
            {
                string s = str.Remove(0, ControllerButtonPressedName.Length);
                if (Enum.TryParse<ControllerButtons>(s, out ControllerButtons b))
                {
                    conditionDelegate = (v) => Game.IsControllerButtonDownRightNow(b);
                    return true;
                }
            }

            conditionDelegate = null;
            return false;
        }
    }
}
