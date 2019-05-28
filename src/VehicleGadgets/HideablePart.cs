namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;

    using Rage;

    using VehicleGadgetsPlus.Memory;
    using VehicleGadgetsPlus.Conditions;
    using VehicleGadgetsPlus.VehicleGadgets.XML;

    internal sealed unsafe class HideablePart : VehicleGadget
    {
        private readonly HideablePartEntry hideablePartDataEntry;
        private readonly ConditionDelegate[] conditions;
        private readonly VehicleBone bone;
        private readonly bool hasBound;
        private readonly int boundIndex;
        private readonly CVehicle* nativeVeh;
        private bool visible = true;

        public HideablePart(Vehicle vehicle, VehicleGadgetEntry dataEntry) : base(vehicle, dataEntry)
        {
            hideablePartDataEntry = (HideablePartEntry)dataEntry;

            if (!VehicleBone.TryGetForVehicle(vehicle, hideablePartDataEntry.BoneName, out bone))
            {
                throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone \"{hideablePartDataEntry.BoneName}\" for the {HideablePartEntry.XmlName}");
            }

            conditions = Conditions.GetConditionsFromString(vehicle.Model, hideablePartDataEntry.Conditions);

            nativeVeh = (CVehicle*)vehicle.MemoryAddress;
            boundIndex = GameFunctions.fragInst_GetBoundIndexForBone(nativeVeh->Inst, bone.Index);
            hasBound = boundIndex != -1;
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
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void Show()
        {
            if (hasBound)
            {
                ShowBound();
            }
            else
            {
                bone.ResetTranslation();
            }
        }

        private void ShowBound()
        {
            fragInstGta* inst = nativeVeh->Inst;
            if (inst == null)
            {
                return;
            }

            fragCacheEntry* entry = inst->CacheEntry;
            if (entry == null)
            {
                return;
            }

            int** flags = entry->BrokenAndHiddenComponentsFlags;
            if (flags == null)
            {
                return;
            }

            (*flags)[boundIndex >> 5] &= ~(1 << (boundIndex & 0x1F));
        }

        private void Hide()
        {
            if (hasBound)
            {
                HideBound();
            }
            else
            {
                bone.SetTranslation(new Vector3(0.0f, 0.0f, -99999.9f));
            }
        }

        private void HideBound()
        {
            fragInstGta* inst = nativeVeh->Inst;
            if (inst == null)
            {
                return;
            }

            fragCacheEntry* entry = inst->CacheEntry;
            if (entry == null)
            {
                return;
            }

            int** flags = entry->BrokenAndHiddenComponentsFlags;
            if (flags == null)
            {
                return;
            }

            (*flags)[boundIndex >> 5] |= 1 << (boundIndex & 0x1F);
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
