namespace VehicleGadgetsPlus
{
    using System.IO;
    using System.Windows.Forms;
    using System.Collections.Generic;

    using Rage;

    using VehicleGadgetsPlus.Memory;
    using VehicleGadgetsPlus.VehicleGadgets;
    using VehicleGadgetsPlus.VehicleGadgets.XML;

    internal static unsafe class Plugin
    {
        public const string VehicleConfigsFolder = "Vehicle Gadgets+/";

        public static Dictionary<Model, VehicleConfig> VehicleConfigsByModel = new Dictionary<Model, VehicleConfig>();

        private static HashSet<Vehicle> VehiclesChecked = new HashSet<Vehicle>();
        private static List<VehicleGadget> Gadgets = new List<VehicleGadget>();

        private static void Main()
        {
            while (Game.IsLoading)
                GameFiber.Sleep(1000);

            LoadVehicleConfigs();
            
            while (true)
            {
                GameFiber.Yield();
                
                if (Game.IsPaused)
                    continue;

                Vehicle playerVeh = Game.LocalPlayer.Character.CurrentVehicle;
                
                if (playerVeh && !VehiclesChecked.Contains(playerVeh))
                {
                    CreateGadgetsForVehicle(playerVeh);
                }

                for (int i = Gadgets.Count - 1; i >= 0; i--)
                {
                    VehicleGadget g = Gadgets[i];
                    if (g.Vehicle)
                    {
                        g.Update(g.Vehicle == playerVeh);
                    }
                    else
                    {
                        if (VehiclesChecked.Contains(g.Vehicle))
                        {
                            VehiclesChecked.Remove(g.Vehicle);
                        }
                        Gadgets.RemoveAt(i);
                    }
                }

#if DEBUG
                DebugStuff();
#endif
            }
        }

        private static void OnUnload(bool isTerminating)
        {
        }


        private static void CreateGadgetsForVehicle(Vehicle vehicle)
        {
            VehicleGadget[] gadgets = VehicleGadget.GetGadgetsForVehicle(vehicle);
            if (gadgets != null)
            {
                Gadgets.AddRange(gadgets);
            }
            VehiclesChecked.Add(vehicle);
        }

        private static void LoadVehicleConfigs()
        {
            if (!Directory.Exists(VehicleConfigsFolder))
                Directory.CreateDirectory(VehicleConfigsFolder);

            foreach (string fileName in Directory.EnumerateFiles(VehicleConfigsFolder, "*.xml", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    Game.LogTrivial($"Loading config for {Path.GetFileNameWithoutExtension(fileName)}...");
                    VehicleConfig cfg = Util.Deserialize<VehicleConfig>(fileName);
                    Model m = new Model(Path.GetFileNameWithoutExtension(fileName));
                    VehicleConfigsByModel.Add(m, cfg);
                    Game.LogTrivial($"Loaded config for {Path.GetFileNameWithoutExtension(fileName)}");
                }
                catch (System.InvalidOperationException ex)
                {
                    Game.LogTrivial($"Can't load {Path.GetFileName(fileName)}: {ex}");
                }
                catch (System.Xml.XmlException ex)
                {
                    Game.LogTrivial($"Can't load {Path.GetFileName(fileName)}: {ex}");
                }
            }
        }

#if DEBUG
        private static void DebugStuff()
        {
            if (Game.IsKeyDown(Keys.Y))
            {
                VehicleConfig cfg = new VehicleConfig
                {
                    Gadgets = new VehicleGadgetEntry[]
                    {
                            new LadderEntry
                            {
                                Base = new LadderEntry.LadderBase
                                {
                                    BoneName = "ladder_base",
                                    RotationSpeed = 11.0f,
                                    RotationAxis = new XYZ { X = 0f, Y = 0f, Z = 1f },
                                },

                                Main = new LadderEntry.LadderMain
                                {
                                    BoneName = "ladder_main_0",
                                    RotationSpeed = 11.0f,
                                    RotationAxis = new XYZ { X = 1f, Y = 0f, Z = 0f },
                                    MinAngle = 0.0f,
                                    MaxAngle = 65.0f,
                                },

                                Extensions = new[]
                                {
                                    new LadderEntry.LadderExtension
                                    {
                                        BoneName = "ladder_main_1",
                                        MoveSpeed = 3.0f,
                                        ExtensionDistance = 6.0f,
                                    }
                                },

                                Bucket = new LadderEntry.LadderBucket
                                {
                                    BoneName = "ladder_bucket",
                                    RotationSpeed = 10.0f,
                                    RotationAxis = new XYZ { X = 1f, Y = 0f, Z = 0f },
                                    MinAngle = -85.0f,
                                    MaxAngle = 85.0f,
                                }
                            },

                            new OutriggersEntry
                            {
                                LeftOutriggers = new[]
                                {
                                    new OutriggersEntry.Outrigger
                                    {
                                        HorizontalBoneName = "outr_out_l1",
                                        VerticalBoneName = "outr_down_l1",

                                        HorizontalDistance = 0.8f,
                                        HorizontalMoveSpeed = 0.625f,
                                        VerticalDistance = 0.48f,
                                        VerticalMoveSpeed = 0.625f,
                                    },
                                    new OutriggersEntry.Outrigger
                                    {
                                        HorizontalBoneName = "outr_out_l2",
                                        VerticalBoneName = "outr_down_l2",

                                        HorizontalDistance = 0.8f,
                                        HorizontalMoveSpeed = 0.625f,
                                        VerticalDistance = 0.48f,
                                        VerticalMoveSpeed = 0.625f,
                                    },
                                },

                                RightOutriggers = new[]
                                {
                                    new OutriggersEntry.Outrigger
                                    {
                                        HorizontalBoneName = "outr_out_r1",
                                        VerticalBoneName = "outr_down_r1",

                                        HorizontalDistance = 0.8f,
                                        HorizontalMoveSpeed = 0.625f,
                                        VerticalDistance = 0.48f,
                                        VerticalMoveSpeed = 0.625f,
                                    },
                                    new OutriggersEntry.Outrigger
                                    {
                                        HorizontalBoneName = "outr_out_r2",
                                        VerticalBoneName = "outr_down_r2",

                                        HorizontalDistance = 0.8f,
                                        HorizontalMoveSpeed = 0.625f,
                                        VerticalDistance = 0.48f,
                                        VerticalMoveSpeed = 0.625f,
                                    },
                                },
                            },

                            new RadarEntry
                            {
                                Conditions = "EngineOn,SirenOn",
                                BoneName = "radar_part",
                                RotationAxis = new XYZ { X = 0f, Y = 0f, Z = 1f },
                                RotationSpeed = 5.0f,
                            },
                    }
                };

                Util.Serialize(VehicleConfigsFolder + "TEST.xml", cfg);
            }
        }
#endif
    }
}

/*                    VehicleConfig cfg = new VehicleConfig
                    {
                        Extensions = new VehicleExtensionEntry[]
                        {
                            new LadderEntry
                            {
                                Base = new LadderEntry.LadderBase
                                {
                                    BoneName = "ladder_base",
                                    RotationSpeed = 11.0f,
                                    RotationAxis = new XYZ { X = 0f, Y = 0f, Z = 1f },
                                },

                                Main = new LadderEntry.LadderMain
                                {
                                    BoneName = "ladder_main_0",
                                    RotationSpeed = 11.0f,
                                    RotationAxis = new XYZ { X = 1f, Y = 0f, Z = 0f },
                                    MinAngle = 0.0f,
                                    MaxAngle = 65.0f,
                                },

                                Extensions = new[]
                                {
                                    new LadderEntry.LadderExtension
                                    {
                                        BoneName = "ladder_main_1",
                                        MoveSpeed = 3.0f,
                                        ExtensionDistance = 20.0f,
                                    }
                                },

                                Bucket = new LadderEntry.LadderBucket
                                {
                                    BoneName = "ladder_bucket",
                                    RotationSpeed = 10.0f,
                                    RotationAxis = new XYZ { X = 1f, Y = 0f, Z = 0f },
                                    MinAngle = -85.0f,
                                    MaxAngle = 85.0f,
                                }
                            },

                            new OutriggersEntry
                            {
                                LeftOutriggers = new[]
                                {
                                    new OutriggersEntry.Outrigger
                                    {
                                        HorizontalBoneName = "outr_out_l1",
                                        VerticalBoneName = "outr_down_l1",

                                        HorizontalDistance = 0.8f,
                                        HorizontalMoveSpeed = 0.625f,
                                        VerticalDistance = 0.48f,
                                        VerticalMoveSpeed = 0.625f,
                                    },
                                    new OutriggersEntry.Outrigger
                                    {
                                        HorizontalBoneName = "outr_out_l2",
                                        VerticalBoneName = "outr_down_l2",

                                        HorizontalDistance = 0.8f,
                                        HorizontalMoveSpeed = 0.625f,
                                        VerticalDistance = 0.48f,
                                        VerticalMoveSpeed = 0.625f,
                                    },
                                },

                                RightOutriggers = new[]
                                {
                                    new OutriggersEntry.Outrigger
                                    {
                                        HorizontalBoneName = "outr_out_r1",
                                        VerticalBoneName = "outr_down_r1",

                                        HorizontalDistance = 0.8f,
                                        HorizontalMoveSpeed = 0.625f,
                                        VerticalDistance = 0.48f,
                                        VerticalMoveSpeed = 0.625f,
                                    },
                                    new OutriggersEntry.Outrigger
                                    {
                                        HorizontalBoneName = "outr_out_r2",
                                        VerticalBoneName = "outr_down_r2",

                                        HorizontalDistance = 0.8f,
                                        HorizontalMoveSpeed = 0.625f,
                                        VerticalDistance = 0.48f,
                                        VerticalMoveSpeed = 0.625f,
                                    },
                                },
                            }
                        }
                    };
*/
