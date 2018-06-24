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

    internal static class MatrixUtils
    {
        // https://code.google.com/archive/p/slimmath/
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
        // based on MyQuaternion.SlerpUnclamped from https://gist.github.com/aeroson/043001ca12fe29ee911e
        public static Quaternion Slerp(Quaternion a, Quaternion b, float t, bool longestPath)
        {
            return Slerp(ref a, ref b, t, longestPath);
        }

        public static Quaternion Slerp(ref Quaternion a, ref Quaternion b, float t, bool longestPath)
        {
            // if either input is zero, return the other.
            if (a.LengthSquared() == 0.0f)
            {
                if (b.LengthSquared() == 0.0f)
                {
                    return Quaternion.Identity;
                }
                return b;
            }
            else if (b.LengthSquared() == 0.0f)
            {
                return a;
            }


            float cosHalfAngle = a.W * b.W + Vector3.Dot(new Vector3(a.X, a.Y, a.Z), new Vector3(b.X, b.Y, b.Z));

            if (cosHalfAngle >= 1.0f || cosHalfAngle <= -1.0f)
            {
                // angle = 0.0f, so just return one input.
                return a;
            }
            else if (longestPath || (!longestPath && cosHalfAngle < 0.0f))
            {
                b.X = -b.X;
                b.Y = -b.Y;
                b.Z = -b.Z;
                b.W = -b.W;
                cosHalfAngle = -cosHalfAngle;
            }

            float blendA;
            float blendB;
            if (cosHalfAngle < 0.99f)
            {
                // do proper slerp for big angles
                float halfAngle = (float)System.Math.Acos(cosHalfAngle);
                float sinHalfAngle = (float)System.Math.Sin(halfAngle);
                float oneOverSinHalfAngle = 1.0f / sinHalfAngle;
                blendA = (float)System.Math.Sin(halfAngle * (1.0f - t)) * oneOverSinHalfAngle;
                blendB = (float)System.Math.Sin(halfAngle * t) * oneOverSinHalfAngle;
            }
            else
            {
                // do lerp if angle is really small.
                blendA = 1.0f - t;
                blendB = t;
            }

            Quaternion result = new Quaternion(blendA * new Vector3(a.X, a.Y, a.Z) + blendB * new Vector3(b.X, b.Y, b.Z), blendA * a.W + blendB * b.W);
            if (result.LengthSquared() > 0.0f)
                return Quaternion.Normalize(result);
            else
                return Quaternion.Identity;
        }
    }
}
