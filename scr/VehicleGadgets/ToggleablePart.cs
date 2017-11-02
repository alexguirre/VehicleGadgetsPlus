namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;

    using Rage;
    
    using VehicleGadgetsPlus.VehicleGadgets.XML;

    internal sealed class HideablePart : VehicleGadget
    {
        private readonly HideablePartEntry hideablePartDataEntry;
        private readonly Condition.ConditionDelegate[] toggleConditions;
        private readonly VehicleBone bone;
        private bool visible = true;

        public HideablePart(Vehicle vehicle, VehicleGadgetEntry dataEntry) : base(vehicle, dataEntry)
        {
            hideablePartDataEntry = (HideablePartEntry)dataEntry;

            if (!VehicleBone.TryGetForVehicle(vehicle, hideablePartDataEntry.BoneName, out bone))
            {
                throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone \"{hideablePartDataEntry.BoneName}\" for the {HideablePartEntry.XmlName}");
            }

            toggleConditions = Condition.GetConditionsFromString(hideablePartDataEntry.ToggleConditions);
        }

        public override void Update(bool isPlayerIn)
        {
            if (bone != null && (toggleConditions.Length <= 0 || Array.TrueForAll(toggleConditions, (c) => c(this))))
            {
                visible = !visible;

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
        }
    }
}
