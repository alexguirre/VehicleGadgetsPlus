namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;
    using System.Windows.Forms;

    using Rage;

    using VehicleGadgetsPlus.Memory;
    using VehicleGadgetsPlus.VehicleGadgets.XML;

    internal sealed unsafe class RotatingPart : VehicleGadget
    {
        readonly RotatingPartEntry rotatingPartDataEntry;

        Condition.ConditionDelegate[] activationConditions;
        int? boneIndex;
        
        readonly phArchetypeDamp* archetype;

        public RotatingPart(Vehicle vehicle, VehicleGadgetEntry dataEntry) : base(vehicle, dataEntry)
        {
            rotatingPartDataEntry = (RotatingPartEntry)dataEntry;

            CVehicle* veh = ((CVehicle*)vehicle.MemoryAddress);

            fragInstGta* inst = veh->inst;
            archetype = inst->archetype;

            int boneIndex = Util.GetBoneIndex(vehicle, rotatingPartDataEntry.BoneName);
            if (boneIndex == -1)
                throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone \"{rotatingPartDataEntry.BoneName}\" for the RotatingPart");

            this.boneIndex = boneIndex;

            activationConditions = Condition.GetConditionsFromString(rotatingPartDataEntry.ActivationConditions);
        }

        public override void Update(bool isPlayerIn)
        {
            if (boneIndex != null && (activationConditions.Length <= 0 || Array.TrueForAll(activationConditions, (c) => c(this))))
            {
                NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[boneIndex.Value]);
                Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.RotationAxis(rotatingPartDataEntry.RotationAxis, MathHelper.ConvertDegreesToRadians(rotatingPartDataEntry.RotationSpeed * Game.FrameTime)) * (*matrix);
                *matrix = newMatrix;
            }
        }
    }
}
