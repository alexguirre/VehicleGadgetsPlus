namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;
    using System.Windows.Forms;
    using System.Linq;

    using Rage;

    using VehicleGadgetsPlus.Memory;
    using VehicleGadgetsPlus.VehicleGadgets.XML;

    internal sealed unsafe class Outriggers : VehicleGadget
    {
        enum OutriggersState
        {
            Undeployed,
            Undeploying,
            Deploying,
            Deployed,
        }

        enum UpDownState
        {
            None,
            Up,
            Down,
        }

        readonly OutriggersEntry outriggersDataEntry;

        Outrigger[] outriggers;

        public Outriggers(Vehicle vehicle, VehicleGadgetEntry dataEntry) : base(vehicle, dataEntry)
        {
            outriggersDataEntry = (OutriggersEntry)dataEntry;

            outriggers = new Outrigger[outriggersDataEntry.LeftOutriggers.Length + outriggersDataEntry.RightOutriggers.Length];

            for (int i = 0; i < outriggersDataEntry.LeftOutriggers.Length; i++)
            {
                outriggers[i] = new Outrigger(vehicle, true, outriggersDataEntry.LeftOutriggers[i]);
            }

            for (int i = 0; i < outriggersDataEntry.RightOutriggers.Length; i++)
            {
                outriggers[outriggersDataEntry.RightOutriggers.Length + i] = new Outrigger(vehicle, false, outriggersDataEntry.RightOutriggers[i]);
            }
        }

        public override void Update(bool isPlayerIn)
        {
            float delta = Game.FrameTime;
            for (int i = 0; i < outriggers.Length; i++)
            {
                outriggers[i].Update(delta);
            }

            if (isPlayerIn && Game.IsKeyDown(Keys.O))
            {
                if (outriggers.All(o => o.State == OutriggersState.Undeployed))
                {
                    foreach (Outrigger o in outriggers)
                    {
                        o.State = OutriggersState.Deploying;
                    }
                }
                else if (outriggers.All(o => o.State == OutriggersState.Deployed))
                {
                    foreach (Outrigger o in outriggers)
                    {
                        o.State = OutriggersState.Undeploying;
                        o.VerticalState = UpDownState.Up;
                    }
                }
            }
        }


        private class Outrigger
        {
            // TODO: read state from distances between current and original positions, otherwise when repaired and outriggers deployed, will mess things up
            public OutriggersState State { get; set; }
            public UpDownState VerticalState { get; set; }

            readonly Vehicle veh;
            readonly OutriggersEntry.Outrigger data;
            readonly bool isLeft;

            float currentOutDistance;
            float currentDownDistance;

            int outriggerHorizontalIndex;
            int outriggerVerticalIndex;

            float horizontalMoveSpeed;

            readonly phArchetypeDamp* archetype;

            public Outrigger(Vehicle veh, bool isLeft, OutriggersEntry.Outrigger data)
            {
                this.veh = veh;
                this.data = data;
                this.isLeft = isLeft;

                fragInstGta* inst = ((CVehicle*)veh.MemoryAddress)->inst;
                archetype = inst->archetype;

                int boneIndex = Util.GetBoneIndex(veh, data.HorizontalBoneName);
                if (boneIndex == -1)
                    throw new InvalidOperationException($"The model \"{veh.Model.Name}\" doesn't have the bone \"{data.HorizontalBoneName}\" for the Outrigger Horizontal");

                outriggerHorizontalIndex = boneIndex;


                boneIndex = Util.GetBoneIndex(veh, data.VerticalBoneName);
                if (boneIndex == -1)
                    throw new InvalidOperationException($"The model \"{veh.Model.Name}\" doesn't have the bone \"{data.VerticalBoneName}\" for the Outrigger Vertical");

                outriggerVerticalIndex = boneIndex;

                horizontalMoveSpeed = isLeft ? -data.HorizontalMoveSpeed : data.HorizontalMoveSpeed;
            }

            public void Update(float delta)
            {
                switch (State)
                {
                    case OutriggersState.Undeploying:
                        {
                            if (VerticalState == UpDownState.None) // wait for the legs to go up
                            {
                                float moveDist = horizontalMoveSpeed * delta;
                                currentOutDistance -= Math.Abs(moveDist);

                                NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[outriggerHorizontalIndex]);
                                Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.Translation(-moveDist, 0.0f, 0.0f) * (*matrix);
                                *matrix = newMatrix;

                                if (currentOutDistance <= 0.0f)
                                {
                                    State = OutriggersState.Undeployed;
                                }
                            }
                        }
                        break;
                    case OutriggersState.Deploying:
                        {
                            float moveDist = horizontalMoveSpeed * delta;
                            currentOutDistance += Math.Abs(moveDist);

                            NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[outriggerHorizontalIndex]);
                            Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.Translation(moveDist, 0.0f, 0.0f) * (*matrix);
                            *matrix = newMatrix;

                            if (currentOutDistance >= data.HorizontalDistance)
                            {
                                State = OutriggersState.Deployed;
                                VerticalState = UpDownState.Down;
                            }
                        }
                        break;
                }


                switch (VerticalState)
                {
                    case UpDownState.Up:
                        {
                            float moveDist = data.VerticalMoveSpeed * delta;
                            currentDownDistance -= moveDist;

                            NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[outriggerVerticalIndex]);
                            Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.Translation(0.0f, 0.0f, moveDist) * (*matrix);
                            *matrix = newMatrix;

                            if (currentDownDistance <= 0.0f)
                            {
                                VerticalState = UpDownState.None;
                            }
                        }
                        break;
                    case UpDownState.Down:
                        {
                            float moveDist = data.VerticalMoveSpeed * delta;
                            currentDownDistance += moveDist;

                            NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[outriggerVerticalIndex]);
                            Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.Translation(0.0f, 0.0f, -moveDist) * (*matrix);
                            *matrix = newMatrix;

                            if (currentDownDistance >= data.VerticalDistance)
                            {
                                VerticalState = UpDownState.None;
                            }
                        }
                        break;
                }
            }
        }
    }
}
