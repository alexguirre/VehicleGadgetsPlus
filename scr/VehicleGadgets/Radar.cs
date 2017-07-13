namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;
    using System.Windows.Forms;

    using Rage;

    using VehicleGadgetsPlus.Memory;
    using VehicleGadgetsPlus.VehicleGadgets.XML;

    internal sealed unsafe class Radar : VehicleGadget
    {
        readonly RadarEntry radarDataEntry;

        int? boneIndex;
        
        readonly phArchetypeDamp* archetype;

        public Radar(Vehicle vehicle, VehicleGadgetEntry dataEntry) : base(vehicle, dataEntry)
        {
            radarDataEntry = (RadarEntry)dataEntry;

            CVehicle* veh = ((CVehicle*)vehicle.MemoryAddress);

            fragInstGta* inst = veh->inst;
            archetype = inst->archetype;

            int boneIndex = Util.GetBoneIndex(vehicle, radarDataEntry.BoneName);
            if (boneIndex == -1)
                throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone \"{radarDataEntry.BoneName}\" for the Radar");

            this.boneIndex = boneIndex;
        }

        public override void Update(bool isPlayerIn)
        {
            if (boneIndex != null)
            {
                NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[boneIndex.Value]);
                Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.RotationAxis(radarDataEntry.RotationAxis, MathHelper.ConvertDegreesToRadians(radarDataEntry.RotationSpeed * Game.FrameTime)) * (*matrix);
                *matrix = newMatrix;
            }
        }
    }
}
