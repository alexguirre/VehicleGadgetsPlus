namespace VehicleGadgetsPlus.Memory
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeMatrix4x4
    {
        public float M11;
        public float M12;
        public float M13;
        public float M14;

        public float M21;
        public float M22;
        public float M23;
        public float M24;

        public float M31;
        public float M32;
        public float M33;
        public float M34;

        public float M41;
        public float M42;
        public float M43;
        public float M44;

        public NativeVector4 M1 => new NativeVector4(M11, M12, M13, M14);
        public NativeVector4 M2 => new NativeVector4(M21, M22, M23, M24);
        public NativeVector4 M3 => new NativeVector4(M31, M32, M33, M34);
        public NativeVector4 M4 => new NativeVector4(M41, M42, M43, M44);

        public static implicit operator NativeMatrix4x4(Rage.Matrix matrix)
        {
            return new NativeMatrix4x4()
            {
                M11 = matrix.M11,
                M12 = matrix.M12,
                M13 = matrix.M13,
                M14 = matrix.M14,

                M21 = matrix.M21,
                M22 = matrix.M22,
                M23 = matrix.M23,
                M24 = matrix.M24,

                M31 = matrix.M31,
                M32 = matrix.M32,
                M33 = matrix.M33,
                M34 = matrix.M34,

                M41 = matrix.M41,
                M42 = matrix.M42,
                M43 = matrix.M43,
                M44 = matrix.M44,
            };
        }

        public static implicit operator Rage.Matrix(NativeMatrix4x4 matrix)
        {
            return new Rage.Matrix()
            {
                M11 = matrix.M11,
                M12 = matrix.M12,
                M13 = matrix.M13,
                M14 = matrix.M14,

                M21 = matrix.M21,
                M22 = matrix.M22,
                M23 = matrix.M23,
                M24 = matrix.M24,

                M31 = matrix.M31,
                M32 = matrix.M32,
                M33 = matrix.M33,
                M34 = matrix.M34,

                M41 = matrix.M41,
                M42 = matrix.M42,
                M43 = matrix.M43,
                M44 = matrix.M44,
            };
        }
    }
}
