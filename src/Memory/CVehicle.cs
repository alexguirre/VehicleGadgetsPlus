namespace VehicleGadgetsPlus.Memory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct CVehicle
    {
        [FieldOffset(0x0030)] public fragInstGta* Inst;
    }
}
