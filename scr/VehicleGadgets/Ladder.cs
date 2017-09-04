namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;
    using System.Windows.Forms;

    using Rage;

    using VehicleGadgetsPlus.Memory;
    using VehicleGadgetsPlus.VehicleGadgets.XML;

    internal sealed unsafe class Ladder : VehicleGadget
    {
        readonly LadderEntry ladderDataEntry;

        readonly int? ladderBaseIndex;
        readonly int? ladderMainIndex;
        readonly Extension[] ladderExtensions;
        readonly int? ladderBucketIndex;
        
        readonly phArchetypeDamp* archetype;

        public Ladder(Vehicle vehicle, VehicleGadgetEntry dataEntry) : base(vehicle, dataEntry)
        {
            ladderDataEntry = (LadderEntry)dataEntry;

            CVehicle* veh = ((CVehicle*)vehicle.MemoryAddress);

            fragInstGta* inst = veh->inst;
            archetype = inst->archetype;

            if (ladderDataEntry.HasBase)
            {
                int boneIndex = Util.GetBoneIndex(vehicle, ladderDataEntry.Base.BoneName);
                if (boneIndex == -1)
                    throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone \"{ladderDataEntry.Base.BoneName}\" for the Ladder Base");

                ladderBaseIndex = boneIndex;
            }

            if (ladderDataEntry.HasMain)
            {
                int boneIndex = Util.GetBoneIndex(vehicle, ladderDataEntry.Main.BoneName);
                if (boneIndex == -1)
                    throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone \"{ladderDataEntry.Main.BoneName}\" for the Ladder Main");

                ladderMainIndex = boneIndex;
            }

            if (ladderDataEntry.HasExtensions)
            {
                ladderExtensions = new Extension[ladderDataEntry.Extensions.Length];


                for (int i = 0; i < ladderDataEntry.Extensions.Length; i++)
                {
                    int boneIndex = Util.GetBoneIndex(vehicle, ladderDataEntry.Extensions[i].BoneName);
                    if (boneIndex == -1)
                        throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone \"{ladderDataEntry.Extensions[i].BoneName}\" for the Ladder Extension #{i}");

                    ladderExtensions[i] = new Extension(this, ladderDataEntry.Extensions[i], boneIndex);
                }
            }

            if (ladderDataEntry.HasBucket)
            {
                int boneIndex = Util.GetBoneIndex(vehicle, ladderDataEntry.Bucket.BoneName);
                if (boneIndex == -1)
                    throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone \"{ladderDataEntry.Bucket.BoneName}\" for the Ladder Bucket");

                ladderBucketIndex = boneIndex;
            }
        }
        
        public override void Update(bool isPlayerIn)
        {
            if (!isPlayerIn)
                return;
            
            // left/right
            if (ladderBaseIndex != null)
            {
                if (Game.IsKeyDownRightNow(Keys.NumPad6))
                {
                    NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[ladderBaseIndex.Value]);
                    Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.RotationAxis(ladderDataEntry.Base.RotationAxis, MathHelper.ConvertDegreesToRadians(-ladderDataEntry.Base.RotationSpeed * Game.FrameTime)) * (*matrix);
                    *matrix = newMatrix;
                }

                if (Game.IsKeyDownRightNow(Keys.NumPad4))
                {
                    NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[ladderBaseIndex.Value]);
                    Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.RotationAxis(ladderDataEntry.Base.RotationAxis, MathHelper.ConvertDegreesToRadians(ladderDataEntry.Base.RotationSpeed * Game.FrameTime)) * (*matrix);
                    *matrix = newMatrix;
                }
            }


            // up/down
            if (ladderMainIndex != null)
            {
                if (Game.IsKeyDownRightNow(Keys.NumPad8))
                {
                    NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[ladderMainIndex.Value]);
                    Matrix m = *matrix;
                    m.Decompose(out Vector3 unused1, out Quaternion rotation, out Vector3 unused2);

                    // hardcoding getting the Pitch component causes issues when the RotationAxis defined in the config isn't the same as the Axis for the Pitch component
                    // therefore the angle should be retrieved from the quaternion based on the RotationAxis
                    // TODO: fix Ladder angles checks
                    if (rotation.ToRotation().Pitch < ladderDataEntry.Main.MaxAngle)
                    {
                        Matrix rotMatrix = Matrix.RotationAxis(ladderDataEntry.Main.RotationAxis, MathHelper.ConvertDegreesToRadians(ladderDataEntry.Main.RotationSpeed * Game.FrameTime));
                        Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * rotMatrix * (*matrix);
                        *matrix = newMatrix;
                    }
                }


                if (Game.IsKeyDownRightNow(Keys.NumPad2))
                {
                    NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[ladderMainIndex.Value]);
                    Matrix m = *matrix;
                    m.Decompose(out Vector3 unused1, out Quaternion rotation, out Vector3 unused2);

                    if (rotation.ToRotation().Pitch > ladderDataEntry.Main.MinAngle)
                    {
                        Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.RotationAxis(ladderDataEntry.Main.RotationAxis, MathHelper.ConvertDegreesToRadians(-ladderDataEntry.Main.RotationSpeed * Game.FrameTime)) * (*matrix);
                        *matrix = newMatrix;
                    }
                }
            }

            // extend
            if (ladderExtensions != null)
            {
                if (Game.IsKeyDownRightNow(Keys.NumPad9))
                {
                    for (int i = 0; i < ladderExtensions.Length; i++)
                    {
                        ladderExtensions[i].Extend();
                    }
                }

                if (Game.IsKeyDownRightNow(Keys.NumPad3))
                {
                    for (int i = 0; i < ladderExtensions.Length; i++)
                    {
                        ladderExtensions[i].Retract();
                    }
                }
            }


            // bucket up/down
            if (ladderBucketIndex != null)
            {
                if (Game.IsKeyDownRightNow(Keys.NumPad7))
                {
                    NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[ladderBucketIndex.Value]);

                    Matrix m = *matrix;
                    m.Decompose(out Vector3 unused1, out Quaternion rotation, out Vector3 unused2);

                    if (rotation.ToRotation().Pitch < ladderDataEntry.Bucket.MaxAngle)
                    {
                        Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.RotationAxis(ladderDataEntry.Bucket.RotationAxis, MathHelper.ConvertDegreesToRadians(ladderDataEntry.Bucket.RotationSpeed * Game.FrameTime)) * (*matrix);
                        *matrix = newMatrix;
                    }
                }


                if (Game.IsKeyDownRightNow(Keys.NumPad1))
                {
                    NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[ladderBucketIndex.Value]);

                    Matrix m = *matrix;
                    m.Decompose(out Vector3 unused1, out Quaternion rotation, out Vector3 unused2);

                    if (rotation.ToRotation().Pitch > ladderDataEntry.Bucket.MinAngle)
                    {
                        Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.RotationAxis(ladderDataEntry.Bucket.RotationAxis, MathHelper.ConvertDegreesToRadians(-ladderDataEntry.Bucket.RotationSpeed * Game.FrameTime)) * (*matrix);
                        *matrix = newMatrix;
                    }
                }
            }
        }


        private class Extension
        {
            private readonly LadderEntry.LadderExtension extensionData;
            private readonly Ladder owner;
            private readonly int boneIndex;

            public float MaxDistanceSqr { get; }

            public float CurrentDistanceSqr
            {
                get
                {
                    NativeMatrix4x4* matrix = &(owner.archetype->skeleton->desiredBonesMatricesArray[boneIndex]);
                    Vector3 translation = Util.DecomposeTranslation(*matrix);
                    Vector3 origTranslation = Util.GetBoneOriginalTranslation(owner.Vehicle, unchecked((uint)boneIndex));

                    return Vector3.DistanceSquared(translation, origTranslation);
                }
            }

            public Extension(Ladder owner, LadderEntry.LadderExtension data, int boneIndex)
            {
                this.owner = owner;
                extensionData = data;
                this.boneIndex = boneIndex;

                MaxDistanceSqr = extensionData.ExtensionDistance * extensionData.ExtensionDistance;
            }


            public void Extend()
            {
                if (CurrentDistanceSqr >= MaxDistanceSqr)
                    return;

                NativeMatrix4x4* matrix = &(owner.archetype->skeleton->desiredBonesMatricesArray[boneIndex]);
                float moveDist = extensionData.MoveSpeed * Game.FrameTime;
                Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.Translation(0.0f, moveDist, 0.0f) * (*matrix);
                *matrix = newMatrix;
            }

            public void Retract()
            {
                // this could cause issues with high move speeds, with move speed of 1 works fine
                // i.e. CurrentDistance is 0.15,
                //      the translation is -0.3,
                //      in the next tick it will translate properly but the CurrentDistance, since it's an absolute value, will still be 0.15
                //      next tick it will be 0.45, and will keep increasing and won't stop
                //
                // TODO: may need to be fixed Ladder.Extension.Retract
                if (CurrentDistanceSqr < 0.01f * 0.01f)
                    return;

                NativeMatrix4x4* matrix = &(owner.archetype->skeleton->desiredBonesMatricesArray[boneIndex]);
                float moveDist = extensionData.MoveSpeed * Game.FrameTime;
                Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.Translation(0.0f, -moveDist, 0.0f) * (*matrix);
                *matrix = newMatrix;
            }
        }
    }
}
