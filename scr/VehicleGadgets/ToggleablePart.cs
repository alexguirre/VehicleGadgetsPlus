namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;
    using System.Linq;
    using System.Windows.Forms;

    using Rage;

    using VehicleGadgetsPlus.Memory;
    using VehicleGadgetsPlus.VehicleGadgets.XML;

    internal sealed unsafe class ToggleablePart : VehicleGadget
    {
        readonly ToggleablePartEntry toggleablePartDataEntry;

        Condition.ConditionDelegate[] toggleConditions;
        int? boneIndex;

        bool visible = true;

        readonly phArchetypeDamp* archetype;

        public ToggleablePart(Vehicle vehicle, VehicleGadgetEntry dataEntry) : base(vehicle, dataEntry)
        {
            toggleablePartDataEntry = (ToggleablePartEntry)dataEntry;

            CVehicle* veh = ((CVehicle*)vehicle.MemoryAddress);

            fragInstGta* inst = veh->inst;
            archetype = inst->archetype;

            int boneIndex = Util.GetBoneIndex(vehicle, toggleablePartDataEntry.BoneName);
            if (boneIndex == -1)
                throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone \"{toggleablePartDataEntry.BoneName}\" for the ToggleablePart");

            this.boneIndex = boneIndex;

            toggleConditions = Condition.GetConditionsFromString(toggleablePartDataEntry.ToggleConditions);
        }

        public override void Update(bool isPlayerIn)
        {
            if (boneIndex != null && (toggleConditions.Length <= 0 || Array.TrueForAll(toggleConditions, (c) => c(this))))
            {
                visible = !visible;

                if (visible)
                {
                    // show
                    Vector3 origOffset = Util.GetBoneOriginalTranslation(Vehicle, (uint)boneIndex.Value);
                    NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[boneIndex.Value]);
                    matrix->M41 = origOffset.X;
                    matrix->M42 = origOffset.Y;
                    matrix->M43 = origOffset.Z;
                }
                else
                {
                    // hide
                    NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[boneIndex.Value]);
                    matrix->M41 = 0f;
                    matrix->M42 = 0f;
                    matrix->M43 = -99999.9f;
                }
            }
        }
    }
}
