namespace VehicleGadgetsPlus.Memory
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Size = 0x130)]
    internal unsafe struct phBoundGeometry
    {
        [FieldOffset(0x0010)] public eBoundType type;
        [FieldOffset(0x0014)] public float boundingSphereRadius;
        [FieldOffset(0x0020)] public NativeVector3 boundingBoxMax;
        [FieldOffset(0x0030)] public NativeVector3 boundingBoxMin;
        [FieldOffset(0x0040)] public NativeVector3 boundingBoxCenter;
        [FieldOffset(0x0050)] public NativeVector3 center;

        [FieldOffset(0x0088)] public phBoundGeometryPolygon* polygons;
        [FieldOffset(0x0090)] public NativeVector3 quantum;
        [FieldOffset(0x00A0)] public NativeVector3 centerGeometry;
        [FieldOffset(0x00B0)] public phBoundGeometryVertex* vertices;
        [FieldOffset(0x00D0)] public uint verticesCount;
        [FieldOffset(0x00D4)] public uint polygonsCount;

    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct phBoundGeometryVertex
    {
        public short X, Y, Z;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct phBoundGeometryPolygon
    {
        public byte b00, b01, b02, b03, b04, b05, b06, b07, b08, b09, b10, b11, b12, b13, b14, b15;

        public int GetPolygonType() => b00 & 0x7;

        public byte[] GetBytes()
        {
            byte[] bytes =
            {
                b00, b01, b02, b03, b04, b05, b06, b07, b08, b09, b10, b11, b12, b13, b14, b15
            };
            return bytes;
        }
    }
}
