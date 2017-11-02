namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;

    using Rage;
    
    using VehicleGadgetsPlus.VehicleGadgets.XML;

    internal sealed class RotatingPart : VehicleGadget
    {
        private readonly RotatingPartEntry rotatingPartDataEntry;
        private readonly Conditions.ConditionDelegate[] conditions;
        private readonly VehicleBone bone;
        private bool rotating;

        public RotatingPart(Vehicle vehicle, VehicleGadgetEntry dataEntry) : base(vehicle, dataEntry)
        {
            rotatingPartDataEntry = (RotatingPartEntry)dataEntry;

            if (!VehicleBone.TryGetForVehicle(vehicle, rotatingPartDataEntry.BoneName, out bone))
            {
                throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone \"{rotatingPartDataEntry.BoneName}\" for the {RotatingPartEntry.XmlName}");
            }

            conditions = Conditions.GetConditionsFromString(vehicle.Model, rotatingPartDataEntry.Conditions);
        }

        public override void Update(bool isPlayerIn)
        {
            if (bone != null)
            {
                if (rotatingPartDataEntry.IsToggle)
                {
                    bool? value = CheckConditions(isPlayerIn);
                    if (value.HasValue && value.Value)
                    {
                        rotating = !rotating;
                    }
                }
                else
                {
                    bool? value = CheckConditions(isPlayerIn);
                    if (value.HasValue)
                    {
                        rotating = value.Value;
                    }
                }
            }

            if (rotating)
            {
                Vector3 axis = rotatingPartDataEntry.RotationAxis;
                float degrees = rotatingPartDataEntry.RotationSpeed * Game.FrameTime;
                bone.RotateAxis(axis, degrees);
            }
        }

        private bool? CheckConditions(bool isPlayerIn)
        {
            if (conditions.Length <= 0)
            {
                return null;
            }

            for (int i = 0; i < conditions.Length; i++)
            {
                bool? v = conditions[i].Invoke(Vehicle, isPlayerIn);
                if (!v.HasValue)
                    return null;

                if (!v.Value)
                    return false;
            }

            return true;
        }
    }
}
