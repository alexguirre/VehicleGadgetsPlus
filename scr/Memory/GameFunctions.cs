namespace VehicleGadgetsPlus.Memory
{
    using System;
    using System.Runtime.InteropServices;

    using Rage;

    internal static unsafe class GameFunctions
    {
        public delegate int GetBoundIndexForBoneDelegate(fragInstGta* fragInst, int boneIndex);
        public delegate void sub_13D50C4_Delegate(void* phLevel, ushort levelIndex);
        public delegate void sub_13D4618_Delegate(void* phLevel, ushort levelIndex);
        public delegate void sub_13D4ED4_Delegate(void* phLevel, ushort levelIndex);
        //public delegate void phArticulatedCollider_sub_13C3270_Delegate(void* articulatedCollider, phBoundComposite* bound);
        public delegate void fragInst_PoseBoundsFromSkeleton_Delegate(fragInstGta* inst, bool a2, bool a3, bool a4, sbyte a5, long a6);
        public delegate void MatrixMagic_Delegate(out NativeMatrix4x4 mtx,
                                                  in NativeVector4 aM1, in NativeVector4 aM2, in NativeVector4 aM3, in NativeVector4 aM4,
                                                  in NativeVector4 bM1, in NativeVector4 bM2, in NativeVector4 bM3, in NativeVector4 bM4);
        public delegate void fragInst_PoseArticulatedBodyFromBounds_Delegate(fragInstGta* inst, sbyte a5, long a6);

        public static GetBoundIndexForBoneDelegate GetBoundIndexForBone { get; private set; }
        public static sub_13D50C4_Delegate sub_13D50C4 { get; private set; }
        public static sub_13D4618_Delegate sub_13D4618 { get; private set; }
        public static sub_13D4ED4_Delegate sub_13D4ED4 { get; private set; }
        //public static phArticulatedCollider_sub_13C3270_Delegate phArticulatedCollider_sub_13C3270 { get; private set; }
        public static fragInst_PoseBoundsFromSkeleton_Delegate fragInst_PoseBoundsFromSkeleton { get; private set; }
        public static MatrixMagic_Delegate MatrixMagic { get; private set; }
        public static fragInst_PoseArticulatedBodyFromBounds_Delegate fragInst_PoseArticulatedBodyFromBounds { get; private set; }

        public static void** phLevelActiveInstance { get; private set; }

        internal static bool Init()
        {
            IntPtr address = Game.FindPattern("85 D2 78 44 4C 8B 49 68 4D 85 C9 74 29 49 8B 81 ?? ?? ?? ??");
            if (AssertAddress(address, nameof(GetBoundIndexForBone)))
            {
                GetBoundIndexForBone = Marshal.GetDelegateForFunctionPointer<GetBoundIndexForBoneDelegate>(address);
            }

            address = Game.FindPattern("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 50 48 63 F2 0F 29 74 24");
            if (AssertAddress(address, nameof(sub_13D50C4)))
            {
                sub_13D50C4 = Marshal.GetDelegateForFunctionPointer<sub_13D50C4_Delegate>(address);
            }

            IntPtr[] addresses = Game.FindAllOccurrencesOfPattern("81 FA ?? ?? ?? ?? 0F 84 ?? ?? ?? ?? 48 89 5C 24 ?? 48 89 74 24 ?? 57");
            address = addresses.Length >= 1 ? addresses[0] : IntPtr.Zero;
            if (AssertAddress(address, nameof(sub_13D4618)))
            {
                sub_13D4618 = Marshal.GetDelegateForFunctionPointer<sub_13D4618_Delegate>(address);
            }

            address = addresses.Length >= 2 ? addresses[1] : IntPtr.Zero;
            if (AssertAddress(address, nameof(sub_13D4ED4)))
            {
                sub_13D4ED4 = Marshal.GetDelegateForFunctionPointer<sub_13D4ED4_Delegate>(address);
            }

            //address = Game.FindPattern("40 53 48 83 EC 20 48 8B D9 0F B7 89 ?? ?? ?? ?? 48 8B C2");
            //if (AssertAddress(address, nameof(phArticulatedCollider_sub_13C3270)))
            //{
            //    phArticulatedCollider_sub_13C3270 = Marshal.GetDelegateForFunctionPointer<phArticulatedCollider_sub_13C3270_Delegate>(address);
            //}

            address = Game.FindPattern("48 8B C4 48 89 58 18 44 88 48 20 88 50 10 55 56 57");
            if (AssertAddress(address, nameof(fragInst_PoseBoundsFromSkeleton)))
            {
                fragInst_PoseBoundsFromSkeleton = Marshal.GetDelegateForFunctionPointer<fragInst_PoseBoundsFromSkeleton_Delegate>(address);
            }

            address = Game.FindPattern("4C 8B DC 48 81 EC ?? ?? ?? ?? 0F 28 02 48 8B 94 24 ?? ?? ?? ?? 48 8B 84 24 ?? ?? ?? ?? 41 0F 29 73");
            if (AssertAddress(address, nameof(MatrixMagic)))
            {
                MatrixMagic = Marshal.GetDelegateForFunctionPointer<MatrixMagic_Delegate>(address);
            }

            address = Game.FindPattern("48 8B 0D ?? ?? ?? ?? 45 33 C0 8B D3 E8 ?? ?? ?? ?? 48 83 BF");
            if (AssertAddress(address, nameof(phLevelActiveInstance)))
            {
                address = address + *(int*)(address + 3) + 7;
                phLevelActiveInstance = (void**)address;
            }

            address = Game.FindPattern("48 8B C4 4C 89 40 18 88 50 10 55 53 56 57 41 54");
            if (AssertAddress(address, nameof(fragInst_PoseArticulatedBodyFromBounds)))
            {
                fragInst_PoseArticulatedBodyFromBounds = Marshal.GetDelegateForFunctionPointer<fragInst_PoseArticulatedBodyFromBounds_Delegate>(address);
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
