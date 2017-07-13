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
        
        int? ladderBaseIndex;
        int? ladderMainIndex;
        int[] ladderExtensionsIndices;
        int? ladderBucketIndex;
        
        readonly float maxExtensionDistance;
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
                ladderExtensionsIndices = new int[ladderDataEntry.Extensions.Length];

                string[] groupNames = inst->FragType->PhysicsLodGroup->Lod1->GetGroupNames();

                for (int i = 0; i < ladderDataEntry.Extensions.Length; i++)
                {
                    int boneIndex = Util.GetBoneIndex(vehicle, ladderDataEntry.Extensions[i].BoneName);
                    if (boneIndex == -1)
                        throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone \"{ladderDataEntry.Extensions[i].BoneName}\" for the Ladder Extension #{i}");

                    ladderExtensionsIndices[i] = boneIndex;
                    maxExtensionDistance += ladderDataEntry.Extensions[i].ExtensionDistance;
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
            if (ladderExtensionsIndices != null)
            {
                if (Game.IsKeyDownRightNow(Keys.NumPad9))
                {
                    float currentDist = GetCurrentLadderExtensionDistance();
                    if (currentDist < maxExtensionDistance)
                    {
                        int i = 0;
                        float d = 0.0f;
                        for (; i < ladderDataEntry.Extensions.Length; i++)
                        {
                            d += ladderDataEntry.Extensions[i].ExtensionDistance;
                            if (currentDist <= d)
                                break;
                        }

                        NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[ladderExtensionsIndices[i]]);
                        float moveDist = ladderDataEntry.Extensions[i].MoveSpeed * Game.FrameTime;
                        Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.Translation(0.0f, moveDist, 0.0f) * (*matrix);
                        *matrix = newMatrix;
                    }
                }

                if (Game.IsKeyDownRightNow(Keys.NumPad3))
                {
                    float currentDist = GetCurrentLadderExtensionDistance();
                    if (currentDist > 0.04f)
                    {
                        int i = ladderDataEntry.Extensions.Length - 1;
                        float d = maxExtensionDistance;
                        for (; i >= 0; i--)
                        {
                            d -= ladderDataEntry.Extensions[i].ExtensionDistance;
                            if (currentDist > d)
                                break;
                        }

                        NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[ladderExtensionsIndices[i]]);
                        float moveDist = ladderDataEntry.Extensions[i].MoveSpeed * Game.FrameTime;
                        Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.Translation(0.0f, -moveDist, 0.0f) * (*matrix);
                        *matrix = newMatrix;
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

        private float GetCurrentLadderExtensionDistance()
        {
            if (ladderExtensionsIndices == null)
                return 0.0f;

            float dist = 0.0f;

            for (int i = 0; i < ladderDataEntry.Extensions.Length; i++)
            {
                NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[ladderExtensionsIndices[i]]);
                Vector3 translation = Util.DecomposeTranslation(*matrix);
                Vector3 origTranslation = Util.GetBoneOriginalTranslation(Vehicle, unchecked((uint)Util.GetBoneIndex(Vehicle, ladderDataEntry.Extensions[i].BoneName)));

                dist += Vector3.Distance(translation, origTranslation);
                
            }

            return dist;
        }
    }
}
