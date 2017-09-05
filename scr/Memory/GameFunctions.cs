namespace VehicleGadgetsPlus.Memory
{
    using System;
    using System.Runtime.InteropServices;

    using Rage;

    internal static unsafe class GameFunctions
    {
        public delegate int GetBoundIndexForBoneDelegate(fragInstGta* fragInst, int boneIndex);


        public static GetBoundIndexForBoneDelegate GetBoundIndexForBone { get; private set; }


        internal static bool Init()
        {
            IntPtr address = Game.FindPattern("85 D2 78 44 4C 8B 49 68 4D 85 C9 74 29 49 8B 81 ?? ?? ?? ??");
            if (AssertAddress(address, nameof(GetBoundIndexForBone)))
            {
                GetBoundIndexForBone = Marshal.GetDelegateForFunctionPointer<GetBoundIndexForBoneDelegate>(address);
            }

            return !anyAssertFailed;
        }

        private static bool anyAssertFailed = false;
        private static bool AssertAddress(IntPtr address, string name)
        {
            if (address == IntPtr.Zero)
            {
                Game.LogTrivial($"Incompatible game version, couldn't find {name} function address.");
                anyAssertFailed = true;
                return false;
            }

            return true;
        }
    }
}
