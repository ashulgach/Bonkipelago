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
        private static ChestOpening lastProcessedInstance = null;
        private static ItemData lastProcessedItemData = null;
        public static bool IsPlaceholderItem = false;

        static void Prefix(ref ItemData itemData, ChestOpening __instance)
        {
            try
            {
                // Prevent duplicate processing - same instance and same itemData means duplicate call
                if (__instance == lastProcessedInstance && itemData == lastProcessedItemData)
                {
                    MelonLogger.Msg($"Skipping duplicate chest opening call (same instance + itemData)");
                    return;
                }
                lastProcessedInstance = __instance;
                lastProcessedItemData = itemData;

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
                    // Item is for another player - show placeholder but don't grant
                    MelonLogger.Msg($"Item is for another player - showing Key placeholder (will not grant)");
                    itemData.eItem = EItem.Key; // Use Key as placeholder for visual
                    IsPlaceholderItem = true; // Mark as placeholder so we can block granting

                    // TODO: Modify description to show "Found [ItemName] for [PlayerName]'s [Game]"
                    // This might require modifying the chest UI text directly
                }
                else
                {
                    // Item is for us - try to map Archipelago item to game item
                    // TODO: Need mapping from Archipelago item IDs to EItem/EWeapon/ETome
                    MelonLogger.Msg("Item is for us - TODO: map Archipelago item to game item");
                    IsPlaceholderItem = false; // Not a placeholder

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

    // Patch to prevent granting placeholder items
    [HarmonyPatch(typeof(Il2CppAssets.Scripts.Inventory__Items__Pickups.Items.ItemInventory), "AddItem", new System.Type[] { typeof(EItem) })]
    public class BlockPlaceholderItemPatch
    {
        static bool Prefix(EItem eItem)
        {
            try
            {
                // Block if this is a placeholder item
                if (ChestOpenPatch.IsPlaceholderItem && eItem == EItem.Key)
                {
                    MelonLogger.Msg("Blocking placeholder Key item from being granted");
                    ChestOpenPatch.IsPlaceholderItem = false; // Reset flag
                    return false; // Block the method
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in BlockPlaceholderItemPatch: {ex.Message}");
            }

            return true; // Allow normal items
        }
    }
}
