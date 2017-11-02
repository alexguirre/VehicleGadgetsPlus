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
                if (isPlayerIn)
                {
                    if (hideablePartDataEntry.IsToggle)
                    {
                        if (CheckConditions())
                        {
                            visible = !visible;

                            UpdateBone();
                        }
                    }
                    else
                    {
                        bool prevVisible = visible;
                        visible = !CheckConditions();

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


        private bool CheckConditions()
        {
            return (conditions.Length <= 0 || Array.TrueForAll(conditions, (c) => c(Vehicle)));
        }
    }
}
