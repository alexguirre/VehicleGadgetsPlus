namespace VehicleGadgetsPlus.Memory
{
    using System;
    using System.Runtime.InteropServices;

    using Rage;

    internal static unsafe class GameFunctions
    {
        public delegate int fragInst_GetBoundIndexForBone_Delegate(fragInstGta* inst, int boneIndex);
        public delegate void fragInst_PoseBoundsFromSkeleton_Delegate(fragInstGta* inst, bool a2, bool a3, bool a4, sbyte a5, long a6);

        public static fragInst_GetBoundIndexForBone_Delegate fragInst_GetBoundIndexForBone { get; private set; }
        public static fragInst_PoseBoundsFromSkeleton_Delegate fragInst_PoseBoundsFromSkeleton { get; private set; }

        internal static bool Init()
        {
            IntPtr address = Game.FindPattern("85 D2 78 44 4C 8B 49 68 4D 85 C9 74 29 49 8B 81");
            if (AssertAddress(address, nameof(fragInst_GetBoundIndexForBone)))
            {
                fragInst_GetBoundIndexForBone = Marshal.GetDelegateForFunctionPointer<fragInst_GetBoundIndexForBone_Delegate>(address);
            }

            address = Game.FindPattern("48 8B C4 48 89 58 18 44 88 48 20 88 50 10 55 56 57");
            if (AssertAddress(address, nameof(fragInst_PoseBoundsFromSkeleton)))
            {
                fragInst_PoseBoundsFromSkeleton = Marshal.GetDelegateForFunctionPointer<fragInst_PoseBoundsFromSkeleton_Delegate>(address);
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
