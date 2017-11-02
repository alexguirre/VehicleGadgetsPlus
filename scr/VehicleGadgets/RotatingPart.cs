namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;

    using Rage;
    
    using VehicleGadgetsPlus.VehicleGadgets.XML;

    internal sealed class RotatingPart : VehicleGadget
    {
        private readonly RotatingPartEntry rotatingPartDataEntry;
        private readonly Condition.ConditionDelegate[] activationConditions;
        private readonly VehicleBone bone;

        public RotatingPart(Vehicle vehicle, VehicleGadgetEntry dataEntry) : base(vehicle, dataEntry)
        {
            rotatingPartDataEntry = (RotatingPartEntry)dataEntry;

            if (!VehicleBone.TryGetForVehicle(vehicle, rotatingPartDataEntry.BoneName, out bone))
            {
                throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone \"{rotatingPartDataEntry.BoneName}\" for the {RotatingPartEntry.XmlName}");
            }

            activationConditions = Condition.GetConditionsFromString(rotatingPartDataEntry.ActivationConditions);
        }

        public override void Update(bool isPlayerIn)
        {
            if (bone != null && (activationConditions.Length <= 0 || Array.TrueForAll(activationConditions, (c) => c(this))))
            {
                Vector3 axis = rotatingPartDataEntry.RotationAxis;
                float degrees = rotatingPartDataEntry.RotationSpeed * Game.FrameTime;
                bone.RotateAxis(axis, degrees);
            }
        }
    }
}
