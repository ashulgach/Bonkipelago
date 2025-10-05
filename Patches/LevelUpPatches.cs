using HarmonyLib;
using MelonLoader;
using Il2Cpp;
using Il2CppAssets.Scripts.UI.InGame.Rewards;
using Il2CppAssets.Scripts._Data.Tomes;
using Il2CppAssets.Scripts._Data;
using UnityEngine.UI;

namespace Bonkipelago.Patches
{
    [HarmonyPatch(typeof(UpgradePicker), "ShuffleUpgrades")]
    public class ShuffleUpgradesPatch
    {
        static void Postfix(UpgradePicker __instance, EEncounter encounterType)
        {
            try
            {
                // Only filter levelup upgrades for now (not chests)
                if (encounterType != EEncounter.Levelup)
                {
                    return;
                }

                MelonLogger.Msg("===== LEVELUP: Filtering upgrades =====");

                // Access the buttons array (the 3 choices)
                var buttons = __instance.buttons;

                if (buttons == null)
                {
                    MelonLogger.Warning("Buttons array is null in UpgradePicker");
                    return;
                }

                int buttonCount = buttons.Count;
                MelonLogger.Msg($"Found {buttonCount} upgrade choices");

                if (buttonCount == 0)
                {
                    MelonLogger.Warning("No buttons found in UpgradePicker");
                    return;
                }

                // Check each button
                for (int i = 0; i < buttonCount; i++)
                {
                    var button = buttons[i];
                    if (button == null || button.upgradable == null)
                    {
                        continue;
                    }

                    // Check if it's a weapon or tome
                    bool isLocked = false;
                    string itemName = "Unknown";

                    // Try casting to WeaponData
                    try
                    {
                        var weaponData = button.upgradable.TryCast<WeaponData>();
                        if (weaponData != null)
                        {
                            itemName = $"Weapon: {weaponData.eWeapon}";
                            isLocked = !ArchipelagoManager.Instance.IsWeaponUnlocked(weaponData.eWeapon);
                        }
                    }
                    catch { }

                    // Try casting to TomeData
                    if (itemName == "Unknown")
                    {
                        try
                        {
                            var tomeData = button.upgradable.TryCast<TomeData>();
                            if (tomeData != null)
                            {
                                itemName = $"Tome: {tomeData.eTome}";
                                isLocked = !ArchipelagoManager.Instance.IsTomeUnlocked(tomeData.eTome);
                            }
                        }
                        catch { }
                    }

                    MelonLogger.Msg($"Choice {i + 1}: {itemName} - {(isLocked ? "LOCKED" : "UNLOCKED")}");

                    // Update button state based on lock status
                    try
                    {
                        if (isLocked)
                        {
                            // Set price impossibly high so it can't be afforded
                            button.price = 999999;

                            // Mark as not affordable (disables the button visually)
                            button.canAfford = false;

                            // Show the "can't afford" overlay for visual feedback
                            if (button.overlayCantAfford != null)
                            {
                                button.overlayCantAfford.SetActive(true);
                            }

                            // Change description to indicate it's locked
                            if (button.t_description != null)
                            {
                                button.t_description.text = "Not unlocked yet!";
                            }

                            MelonLogger.Msg($"  -> Button disabled (locked item)");
                        }
                        else
                        {
                            // Ensure unlocked items are properly enabled
                            button.canAfford = true;

                            // Hide the "can't afford" overlay
                            if (button.overlayCantAfford != null)
                            {
                                button.overlayCantAfford.SetActive(false);
                            }

                            MelonLogger.Msg($"  -> Button enabled (unlocked item)");
                        }
                    }
                    catch (System.Exception btnEx)
                    {
                        MelonLogger.Error($"  -> Error updating button state: {btnEx.Message}");
                    }
                }

                MelonLogger.Msg("=======================================");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in ShuffleUpgradesPatch: {ex.Message}");
                MelonLogger.Error($"Stack trace: {ex.StackTrace}");
            }
        }
    }

    // Block selecting locked items
    [HarmonyPatch(typeof(UpgradePicker), "SelectUpgrade")]
    public class SelectUpgradePatch
    {
        static bool Prefix(IUpgradable upgradable)
        {
            try
            {
                if (upgradable == null)
                {
                    return true; // Allow if no upgradable
                }

                // Check if it's a weapon
                try
                {
                    var weaponData = upgradable.TryCast<WeaponData>();
                    if (weaponData != null)
                    {
                        if (!ArchipelagoManager.Instance.IsWeaponUnlocked(weaponData.eWeapon))
                        {
                            MelonLogger.Warning($"Cannot select {weaponData.eWeapon} - not unlocked yet!");
                            return false; // Block the selection
                        }
                    }
                }
                catch { }

                // Check if it's a tome
                try
                {
                    var tomeData = upgradable.TryCast<TomeData>();
                    if (tomeData != null)
                    {
                        if (!ArchipelagoManager.Instance.IsTomeUnlocked(tomeData.eTome))
                        {
                            MelonLogger.Warning($"Cannot select {tomeData.eTome} - not unlocked yet!");
                            return false; // Block the selection
                        }
                    }
                }
                catch { }

                return true; // Allow selection if unlocked or not a weapon/tome
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in SelectUpgradePatch: {ex.Message}");
                return true; // Allow selection on error to avoid breaking the game
            }
        }
    }
}
