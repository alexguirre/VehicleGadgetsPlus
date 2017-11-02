namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;

    using Rage;
    
    using VehicleGadgetsPlus.VehicleGadgets.XML;

    internal sealed class HideablePart : VehicleGadget
    {
        private readonly HideablePartEntry hideablePartDataEntry;
        private readonly Conditions.ConditionDelegate[] conditions;
        private readonly VehicleBone bone;
        private bool visible = true;

        public HideablePart(Vehicle vehicle, VehicleGadgetEntry dataEntry) : base(vehicle, dataEntry)
        {
            hideablePartDataEntry = (HideablePartEntry)dataEntry;

            if (!VehicleBone.TryGetForVehicle(vehicle, hideablePartDataEntry.BoneName, out bone))
            {
                throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone \"{hideablePartDataEntry.BoneName}\" for the {HideablePartEntry.XmlName}");
            }

            conditions = Conditions.GetConditionsFromString(vehicle.Model, hideablePartDataEntry.Conditions);
        }

        public override void Update(bool isPlayerIn)
        {
            if (bone != null)
            {
                if (hideablePartDataEntry.IsToggle)
                {
                    bool? value = CheckConditions(isPlayerIn);
                    if (value.HasValue && value.Value)
                    {
                        visible = !visible;

                        UpdateBone();
                    }
                }
                else
                {
                    bool? value = CheckConditions(isPlayerIn);
                    if (value.HasValue)
                    {
                        bool prevVisible = visible;
                        visible = !value.Value;

                        if (visible != prevVisible)
                        {
                            UpdateBone();
                        }
                    }
                }
            }
        }

        private void UpdateBone()
        {
            if (visible)
            {
                // show
                bone.ResetTranslation();
            }
            else
            {
                // hide
                bone.SetTranslation(new Vector3(0.0f, 0.0f, -99999.9f));
            }
        }


        private bool? CheckConditions(bool isPlayerIn)
        {
            if(conditions.Length <= 0)
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
