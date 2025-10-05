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

                // Check location with Archipelago (mark as chest to avoid double-granting)
                ArchipelagoManager.Instance.CheckLocation(locationId, isChestLocation: true);

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

                    // Modify description to show AP item info
                    itemData.description = $"Found {scoutInfo.ItemName} for {scoutInfo.PlayerName}";
                    itemData.shortDescription = $"{scoutInfo.ItemName} for {scoutInfo.PlayerName}";
                }
                else
                {
                    // Item is for us - map Archipelago item to game item
                    var mappedItem = ItemMapper.MapItem(scoutInfo.ItemId);

                    switch (mappedItem.Type)
                    {
                        case ItemMapper.ItemType.Item:
                            // It's an EItem, we can replace it directly
                            MelonLogger.Msg($"Mapped to EItem: {mappedItem.Item}");
                            itemData.eItem = mappedItem.Item.Value;
                            IsPlaceholderItem = false;
                            break;

                        case ItemMapper.ItemType.Weapon:
                            // Weapons are unlocked (not given), show placeholder with custom text
                            MelonLogger.Msg($"Mapped to weapon unlock: {mappedItem.Weapon} - showing Key placeholder (will unlock via OnItemReceived)");
                            itemData.eItem = EItem.Key;
                            itemData.description = $"Unlocked {mappedItem.Weapon}!";
                            itemData.shortDescription = $"Unlocked {mappedItem.Weapon}!";
                            IsPlaceholderItem = true; // Block granting since it's just an unlock
                            break;

                        case ItemMapper.ItemType.Tome:
                            // Tomes are unlocked (not given), show placeholder with custom text
                            MelonLogger.Msg($"Mapped to tome unlock: {mappedItem.Tome} - showing Key placeholder (will unlock via OnItemReceived)");
                            itemData.eItem = EItem.Key;
                            itemData.description = $"Unlocked {mappedItem.Tome}!";
                            itemData.shortDescription = $"Unlocked {mappedItem.Tome}!";
                            IsPlaceholderItem = true; // Block granting since it's just an unlock
                            break;

                        case ItemMapper.ItemType.Unknown:
                            MelonLogger.Warning($"Unknown item ID {scoutInfo.ItemId} - keeping vanilla item");
                            IsPlaceholderItem = false;
                            break;
                    }
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
