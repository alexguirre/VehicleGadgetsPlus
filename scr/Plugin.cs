namespace VehicleGadgetsPlus
{
    using System.IO;
    using System.Collections.Generic;

    using Rage;

    using VehicleGadgetsPlus.Memory;
    using VehicleGadgetsPlus.VehicleGadgets;
    using VehicleGadgetsPlus.VehicleGadgets.XML;
    
    internal static unsafe class Plugin
    {
        public const string VehicleConfigsFolder = "Vehicle Gadgets+/";
        public const string SoundsFolder = VehicleConfigsFolder + "Sounds/";

        private static HashSet<PoolHandle> vehiclesChecked = new HashSet<PoolHandle>();
        private static List<VehicleGadget> gadgets = new List<VehicleGadget>();

        public static Dictionary<Model, VehicleConfig> VehicleConfigsByModel = new Dictionary<Model, VehicleConfig>();

        private static void Main()
        {
            while (Game.IsLoading)
                GameFiber.Sleep(1000);

            bool gameFnInit = GameFunctions.Init();

            if (gameFnInit)
            {
                Game.LogTrivialDebug($"Successful {nameof(GameFunctions)} init");
            }
            else
            {
                Game.LogTrivial($"[ERROR] Failed to initialize {nameof(GameFunctions)}, unloading...");
                Game.UnloadActivePlugin();
            }

            LoadVehicleConfigs();

            while (true)
            {
                GameFiber.Yield();

                if (Game.IsPaused)
                    continue;
                
                Vehicle playerVeh = Game.LocalPlayer.Character.CurrentVehicle;

                if (playerVeh && !vehiclesChecked.Contains(playerVeh.Handle))
                {
                    CreateGadgetsForVehicle(playerVeh);
                }

                for (int i = gadgets.Count - 1; i >= 0; i--)
                {
                    VehicleGadget g = gadgets[i];
                    if (g.Vehicle)
                    {
                        g.Update(g.Vehicle == playerVeh);
                    }
                    else
                    {
                        if (vehiclesChecked.Contains(g.Vehicle.Handle))
                        {
                            vehiclesChecked.Remove(g.Vehicle.Handle);
                        }
                        g.Dispose();
                        gadgets.RemoveAt(i);
                    }
                }
            }
        }

        private static void OnUnload(bool isTerminating)
        {
            for (int i = 0; i < gadgets.Count; i++)
            {
                VehicleGadget g = gadgets[i];
                g.Dispose();
            }
            gadgets = null;

            if (SoundPlayer.IsInitialized)
            {
                SoundPlayer.Instance.Dispose();
            }
        }


        private static void CreateGadgetsForVehicle(Vehicle vehicle)
        {
            VehicleGadget[] g = VehicleGadget.GetGadgetsForVehicle(vehicle);
            if (g != null)
            {
                gadgets.AddRange(g);
            }
            vehiclesChecked.Add(vehicle.Handle);
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
    }
}
