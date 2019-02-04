namespace VehicleGadgetsPlus.Memory
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Size = 0xE0)]
    internal unsafe struct fragInstGta
    {
        [FieldOffset(0x0018)] public ushort LevelIndex;

        [FieldOffset(0x0068)] public fragCacheEntry* CacheEntry;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x210)]
    internal unsafe struct fragCacheEntry
    {
        [FieldOffset(0x0118)] public int* BoneIndexToTypeComponent;
        [FieldOffset(0x0120)] public int* TypeComponentToBoneIndex;
        
        [FieldOffset(0x148 + 0x30)] public crSkeleton* Skeleton;
    }
}
