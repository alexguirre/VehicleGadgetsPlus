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
                if (isPlayerIn)
                {
                    if (rotatingPartDataEntry.IsToggle)
                    {
                        if (CheckConditions())
                        {
                            rotating = !rotating;
                        }
                    }
                    else
                    {
                        rotating = CheckConditions();
                    }
                }

                if (rotating)
                {
                    Vector3 axis = rotatingPartDataEntry.RotationAxis;
                    float degrees = rotatingPartDataEntry.RotationSpeed * Game.FrameTime;
                    bone.RotateAxis(axis, degrees);
                }
            }
        }

        private bool CheckConditions()
        {
            return (conditions.Length <= 0 || Array.TrueForAll(conditions, (c) => c(Vehicle)));
        }
    }
}
