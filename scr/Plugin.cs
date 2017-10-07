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

        private static HashSet<PoolHandle> VehiclesChecked = new HashSet<PoolHandle>();
        private static List<VehicleGadget> Gadgets = new List<VehicleGadget>();

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

                if (playerVeh && !VehiclesChecked.Contains(playerVeh.Handle))
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
                        if (VehiclesChecked.Contains(g.Vehicle.Handle))
                        {
                            VehiclesChecked.Remove(g.Vehicle.Handle);
                        }
                        Gadgets.RemoveAt(i);
                    }
                }
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
            VehiclesChecked.Add(vehicle.Handle);
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
