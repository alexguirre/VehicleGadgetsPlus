namespace VehicleGadgetsPlus.Memory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Size = 0x28)]
    internal unsafe struct crSkeleton
    {
        [FieldOffset(0x0000)] public crSkeletonData* Data;
        [FieldOffset(0x0008)] public NativeMatrix4x4* Transform;
        [FieldOffset(0x0010)] public NativeMatrix4x4* ObjectMatrices;
        [FieldOffset(0x0018)] public NativeMatrix4x4* GlobalMatrices;
        [FieldOffset(0x0020)] public int NumBones;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct crSkeletonData
    {
        [FieldOffset(0x0020)] public crBoneData* Bones;
        [FieldOffset(0x0028)] public NativeMatrix4x4* TransformsInverted;
        [FieldOffset(0x0030)] public NativeMatrix4x4* Transforms;
        [FieldOffset(0x0038)] public ushort* ParentIndices;
        [FieldOffset(0x005E)] public ushort NumBones;

        public string GetBoneNameForIndex(uint index)
        {
            if (index >= NumBones)
                return null;

            return Bones[index].GetName();
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x50)]
    internal unsafe struct crBoneData
    {
        [FieldOffset(0x0000)] public NativeVector4 Rotation;
        [FieldOffset(0x0010)] public NativeVector3 Translation;

        [FieldOffset(0x0032)] public ushort ParentIndex;

        [FieldOffset(0x0038)] public IntPtr NamePtr;

        [FieldOffset(0x0042)] public ushort Index;

        public string GetName() => NamePtr == null ? null : Marshal.PtrToStringAnsi(NamePtr);
    }
}
