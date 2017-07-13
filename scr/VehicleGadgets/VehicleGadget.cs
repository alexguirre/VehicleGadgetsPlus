namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;
    using System.Collections.Generic;

    using Rage;

    using VehicleGadgetsPlus.VehicleGadgets.XML;

    internal abstract class VehicleGadget
    {
        public Vehicle Vehicle { get; }
        public VehicleGadgetEntry DataEntry { get; }

        protected VehicleGadget(Vehicle vehicle, VehicleGadgetEntry dataEntry)
        {
            Vehicle = vehicle;
            DataEntry = dataEntry;
        }

        public abstract void Update(bool isPlayerIn);


        public static VehicleGadget[] GetGadgetsForVehicle(Vehicle vehicle)
        {
            if(Plugin.VehicleConfigsByModel.TryGetValue(vehicle.Model, out VehicleConfig config))
            {
                VehicleGadget[] exts = new VehicleGadget[config.Gadgets.Length];
                for (int i = 0; i < config.Gadgets.Length; i++)
                {
                    VehicleGadgetEntry entry = config.Gadgets[i];
                    exts[i] = (VehicleGadget)Activator.CreateInstance(entry.GadgetType, vehicle, entry);
                }
                return exts;
            }

            return null;
        }
    }
}
