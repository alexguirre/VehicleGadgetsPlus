namespace VehicleGadgetsPlus
{
    using System;
    using System.IO;
    using System.Xml.Serialization;

    using Rage;
    using Rage.Exceptions;

    using VehicleGadgetsPlus.Memory;

    internal static unsafe class Util
    {
        // unlike the native/RPH's method, this one works with bones with custom names,
        // for example for a bone named "ladder_base", the native will return -1 but
        // this method will return the proper index.
        public static int GetBoneIndex(Vehicle vehicle, string boneName)
        {
            if (!vehicle)
                throw new InvalidHandleableException(vehicle);

            CVehicle* veh = (CVehicle*)vehicle.MemoryAddress;
            crSkeletonData* skelData = veh->inst->archetype->skeleton->skeletonData;
            uint boneCount = skelData->bonesCount;

            for (uint i = 0; i < boneCount; i++)
            {
                if (skelData->GetBoneNameForIndex(i) == boneName)
                    return unchecked((int)i);
            }

            return -1;
        }

        public static Vector3 GetBoneOriginalTranslation(Vehicle vehicle, int index)
        {
            CVehicle* veh = (CVehicle*)vehicle.MemoryAddress;
            NativeVector3 v = veh->inst->archetype->skeleton->skeletonData->bones[index].translation;
            return v;
        }

        public static Quaternion GetBoneOriginalRotation(Vehicle vehicle, int index)
        {
            CVehicle* veh = (CVehicle*)vehicle.MemoryAddress;
            NativeVector4 v = veh->inst->archetype->skeleton->skeletonData->bones[index].rotation;
            return v;
        }

        public static void Serialize<T>(string fileName, T data)
        {
            using (StreamWriter w = new StreamWriter(fileName, false))
            {
                XmlSerializer ser = new XmlSerializer(typeof(T));
                ser.Serialize(w, data);
            }
        }

        public static T Deserialize<T>(string fileName)
        {
            using (StreamReader r = new StreamReader(fileName))
            {
                XmlSerializer ser = new XmlSerializer(typeof(T));
                return (T)ser.Deserialize(r);
            }
        }
    }

    public sealed class XYZ
    {
        [XmlAttribute] public float X { get; set; }
        [XmlAttribute] public float Y { get; set; }
        [XmlAttribute] public float Z { get; set; }

        public static implicit operator Vector3(XYZ value) => new Vector3(value.X, value.Y, value.Z);
        public static implicit operator XYZ(Vector3 value) => new XYZ { X = value.X, Y = value.Y, Z = value.Z };
    }

    internal static class MatrixUtils
    {
        // https://github.com/alexguirre/slimmath/blob/master/SlimMath/Matrix.cs#L559
        public static bool Decompose(Matrix matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation)
        {
            const float ZeroTolerance = 1e-6f;

            //Source: Unknown
            //References: http://www.gamedev.net/community/forums/topic.asp?topic_id=441695

            //Get the translation.
            translation.X = matrix.M41;
            translation.Y = matrix.M42;
            translation.Z = matrix.M43;

            //Scaling is the length of the rows.
            scale.X = (float)Math.Sqrt((matrix.M11 * matrix.M11) + (matrix.M12 * matrix.M12) + (matrix.M13 * matrix.M13));
            scale.Y = (float)Math.Sqrt((matrix.M21 * matrix.M21) + (matrix.M22 * matrix.M22) + (matrix.M23 * matrix.M23));
            scale.Z = (float)Math.Sqrt((matrix.M31 * matrix.M31) + (matrix.M32 * matrix.M32) + (matrix.M33 * matrix.M33));

            //If any of the scaling factors are zero, than the rotation matrix can not exist.
            if (Math.Abs(scale.X) < ZeroTolerance ||
                Math.Abs(scale.Y) < ZeroTolerance ||
                Math.Abs(scale.Z) < ZeroTolerance)
            {
                rotation = Quaternion.Identity;
                return false;
            }

            //The rotation is the left over matrix after dividing out the scaling.
            Matrix rotationmatrix = new Matrix();
            rotationmatrix.M11 = matrix.M11 / scale.X;
            rotationmatrix.M12 = matrix.M12 / scale.X;
            rotationmatrix.M13 = matrix.M13 / scale.X;

            rotationmatrix.M21 = matrix.M21 / scale.Y;
            rotationmatrix.M22 = matrix.M22 / scale.Y;
            rotationmatrix.M23 = matrix.M23 / scale.Y;

            rotationmatrix.M31 = matrix.M31 / scale.Z;
            rotationmatrix.M32 = matrix.M32 / scale.Z;
            rotationmatrix.M33 = matrix.M33 / scale.Z;

            rotationmatrix.M44 = 1f;

            Quaternion.RotationMatrix(ref rotationmatrix, out rotation);
            return true;
        }

        public static Vector3 DecomposeScale(Matrix matrix)
        {
            Decompose(matrix, out Vector3 scale, out _, out _);
            return scale;
        }

        public static Quaternion DecomposeRotation(Matrix matrix)
        {
            Decompose(matrix, out _, out Quaternion rotation, out _);
            return rotation;
        }

        public static Vector3 DecomposeTranslation(Matrix matrix)
        {
            Decompose(matrix, out _, out _, out Vector3 translation);
            return translation;
        }
    }

    internal static class QuaternionUtils
    {
        public static float Angle(Quaternion a, Quaternion b)
        {
            float f = Quaternion.Dot(a, b);
            return (float)(Math.Acos(Math.Min(Math.Abs(f), 1f)) * 2f * 57.29578f);
        }
    }
}
