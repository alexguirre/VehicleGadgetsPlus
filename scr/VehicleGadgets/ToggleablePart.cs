namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;

    using Rage;
    
    using VehicleGadgetsPlus.VehicleGadgets.XML;

    internal sealed class ToggleablePart : VehicleGadget
    {
        private readonly ToggleablePartEntry toggleablePartDataEntry;
        private readonly Condition.ConditionDelegate[] toggleConditions;
        private readonly VehicleBone bone;
        private bool visible = true;

        public ToggleablePart(Vehicle vehicle, VehicleGadgetEntry dataEntry) : base(vehicle, dataEntry)
        {
            toggleablePartDataEntry = (ToggleablePartEntry)dataEntry;

            if (!VehicleBone.TryGetForVehicle(vehicle, toggleablePartDataEntry.BoneName, out bone))
            {
                throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone \"{toggleablePartDataEntry.BoneName}\" for the ToggleablePart");
            }

            toggleConditions = Condition.GetConditionsFromString(toggleablePartDataEntry.ToggleConditions);
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
