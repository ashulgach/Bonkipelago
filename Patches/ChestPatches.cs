using HarmonyLib;
using MelonLoader;
using UnityEngine;
using Il2CppAssets.Scripts.Inventory__Items__Pickups.Chests;
using Il2CppAssets.Scripts.Inventory__Items__Pickups.Items;
using Il2Cpp;

namespace Bonkipelago.Patches
{
    // Patch chest opening to replace item with Archipelago item (counter-based system)
    [HarmonyPatch(typeof(ChestOpening), "OpenChest")]
    public class ChestOpenPatch
    {
        static void Prefix(ref ItemData itemData, ChestOpening __instance)
        {
            try
            {
                // Get the location ID for this chest (based on counter, not position)
                long locationId = ArchipelagoManager.Instance.GetNextChestLocationId();

                MelonLogger.Msg($"===== CHEST OPENING (#{BonkipelagoConfig.ChestsOpened}) =====");
                MelonLogger.Msg($"Original item: {itemData?.eItem}");
                MelonLogger.Msg($"Location ID: {locationId}");

                if (!ArchipelagoManager.Instance.IsConnected)
                {
                    MelonLogger.Warning("Not connected - using vanilla item");
                    // Still increment counter even if not connected
                    ArchipelagoManager.Instance.IncrementChestCounter();
                    return;
                }

                // Check location with Archipelago
                ArchipelagoManager.Instance.CheckLocation(locationId);

                // Increment counter (this also scouts more if needed)
                ArchipelagoManager.Instance.IncrementChestCounter();

                // Get scouted info
                var scoutInfo = ArchipelagoManager.Instance.GetLocationInfo(locationId);

                if (scoutInfo == null)
                {
                    MelonLogger.Warning("Location not scouted yet - using vanilla item");
                    return;
                }

                MelonLogger.Msg($"Archipelago item: {scoutInfo.ItemName} for {scoutInfo.PlayerName}");

                if (!scoutInfo.IsForLocalPlayer)
                {
                    // Item is for another player - show placeholder
                    MelonLogger.Msg($"Item is for another player - showing Key placeholder");
                    itemData.eItem = EItem.Key; // Use Key as placeholder

                    // TODO: Modify description to show "Found [ItemName] for [PlayerName]'s [Game]"
                    // This might require modifying the chest UI text directly
                }
                else
                {
                    // Item is for us - try to map Archipelago item to game item
                    // TODO: Need mapping from Archipelago item IDs to EItem/EWeapon/ETome
                    MelonLogger.Msg("Item is for us - TODO: map Archipelago item to game item");

                    // For now, keep vanilla item
                    // itemData.eItem = mappedItem;
                }

                MelonLogger.Msg("========================");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in ChestOpenPatch: {ex.Message}");
                MelonLogger.Error($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
