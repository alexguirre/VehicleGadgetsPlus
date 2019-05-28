namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;

    using Rage;

    using VehicleGadgetsPlus.Conditions;
    using VehicleGadgetsPlus.VehicleGadgets.XML;

    internal sealed class RotatingPart : VehicleGadget
    {
        private readonly RotatingPartEntry rotatingPartDataEntry;
        private readonly ConditionDelegate[] conditions;
        private readonly VehicleBone bone;
        private bool rotating;
        private readonly bool hasRange;
        private readonly Quaternion rangeMin, rangeMax;
        private bool rangeIncreasing;
        private float rangePercentage;

        public override bool RequiresPoseBounds => true;

        public RotatingPart(Vehicle vehicle, VehicleGadgetEntry dataEntry) : base(vehicle, dataEntry)
        {
            rotatingPartDataEntry = (RotatingPartEntry)dataEntry;

            if (!VehicleBone.TryGetForVehicle(vehicle, rotatingPartDataEntry.BoneName, out bone))
            {
                throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone \"{rotatingPartDataEntry.BoneName}\" for the {RotatingPartEntry.XmlName}");
            }

            conditions = Conditions.GetConditionsFromString(vehicle.Model, rotatingPartDataEntry.Conditions);

            if (rotatingPartDataEntry.HasRange)
            {
                hasRange = true;

                Quaternion min = Quaternion.RotationAxis(rotatingPartDataEntry.RotationAxis, MathHelper.ConvertDegreesToRadians(rotatingPartDataEntry.Range.Min));
                Quaternion max = Quaternion.RotationAxis(rotatingPartDataEntry.RotationAxis, MathHelper.ConvertDegreesToRadians(rotatingPartDataEntry.Range.Max));

                rangeMin = bone.OriginalRotation * min;
                rangeMax = bone.OriginalRotation * max;
            }
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
                if (hasRange)
                {
                    {
                        rangePercentage += rotatingPartDataEntry.RotationSpeed * Game.FrameTime * (rangeIncreasing ? 1.0f : -1.0f);
                        Quaternion newRotation = QuaternionUtils.Slerp(rangeMin, rangeMax, rangePercentage, rotatingPartDataEntry.Range.LongestPath);

                        bone.SetRotation(newRotation);

                        if ((rangeIncreasing && rangePercentage >= 1.0f) ||
                            (!rangeIncreasing && rangePercentage <= 0.0f))
                        {
                            rangeIncreasing = !rangeIncreasing;
                        }
                    }

#if DEBUG
                    {
                        Quaternion rotation = MatrixUtils.DecomposeRotation(bone.Matrix);

                        Quaternion vehRot = Vehicle.Orientation;
                        Vector3 pos = Vehicle.GetBonePosition(bone.Index);
                        Debug.DrawLine(pos, pos + ((vehRot * rotation).ToVector() * 2.0f), System.Drawing.Color.Red);

                        Debug.DrawLine(pos, pos + ((vehRot * bone.OriginalRotation).ToVector() * 2.0f), System.Drawing.Color.Blue);
                        
                        Quaternion minRot = vehRot * rangeMin;
                        Quaternion maxRot = vehRot * rangeMax;

                        Debug.DrawLine(pos, pos + (minRot.ToVector() * 2.0f), System.Drawing.Color.Green);
                        Debug.DrawLine(pos, pos + (maxRot.ToVector() * 2.0f), System.Drawing.Color.Purple);
                    }
#endif
                }
                else
                {
                    Vector3 axis = rotatingPartDataEntry.RotationAxis;
                    float degrees = rotatingPartDataEntry.RotationSpeed * Game.FrameTime;
                    bone.RotateAxis(axis, degrees);
                }
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
