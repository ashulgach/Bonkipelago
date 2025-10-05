using MelonLoader;
using UnityEngine;
using HarmonyLib;

[assembly: MelonInfo(typeof(Bonkipelago.Core), "Bonkipelago", "1.0.0", "alexs", null)]
[assembly: MelonGame("Ved", "Megabonk")]

namespace Bonkipelago
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Bonkipelago initialized!");
            LoggerInstance.Msg("===========================================");
            LoggerInstance.Msg("Debug Commands:");
            LoggerInstance.Msg("  F2  - Dump all MyPlayer class methods");
            LoggerInstance.Msg("  F3  - Dump all enum values (EItem, EWeapon, ETome)");
            LoggerInstance.Msg("  F4  - Inspect Managers");
            LoggerInstance.Msg("  F5  - Inspect Chests");
            LoggerInstance.Msg("  F6  - Inspect Enemies");
            LoggerInstance.Msg("  F7  - Inspect Player");
            LoggerInstance.Msg("  F8  - Dump all GameObjects in current scene");
            LoggerInstance.Msg("  F9  - Dump root GameObjects (tree view)");
            LoggerInstance.Msg("  F10 - Connect to Archipelago");
            LoggerInstance.Msg("  F11 - Disconnect from Archipelago");
            LoggerInstance.Msg("===========================================");

            // Initialize configuration
            BonkipelagoConfig.Initialize();

            // Initialize Harmony patches (MelonBase provides HarmonyInstance)
            try
            {
                HarmonyInstance.PatchAll();
                LoggerInstance.Msg("Harmony patches applied successfully!");
            }
            catch (System.Exception ex)
            {
                LoggerInstance.Error($"Failed to apply Harmony patches: {ex.Message}");
                LoggerInstance.Error($"Stack trace: {ex.StackTrace}");
            }

            // Auto-connect if enabled
            if (BonkipelagoConfig.AutoConnect)
            {
                LoggerInstance.Msg("Auto-connect enabled. Attempting to connect...");
                ConnectToArchipelago();
            }
        }

        public override void OnUpdate()
        {
            // Debug helper updates
            DebugHelper.Update();

            // Connection status UI updates
            ConnectionStatusUI.Instance.Update();

            // Handle manual connect/disconnect
            if (Input.GetKeyDown(KeyCode.F10))
            {
                ConnectToArchipelago();
            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                ArchipelagoManager.Instance.Disconnect();
                ConnectionStatusUI.Instance.ForceUpdate();
            }
        }

        private void ConnectToArchipelago()
        {
            string server = BonkipelagoConfig.ServerUrl;
            string slot = BonkipelagoConfig.SlotName;
            string password = string.IsNullOrEmpty(BonkipelagoConfig.Password)
                ? null
                : BonkipelagoConfig.Password;

            ArchipelagoManager.Instance.Connect(server, slot, password);
            ConnectionStatusUI.Instance.ForceUpdate();
        }
    }
}