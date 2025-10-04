using HarmonyLib;
using MelonLoader;

namespace Bonkipelago.Patches
{
    // TODO: Find methods that grant items to the player
    // Look for: Inventory system, item granting methods
    // Classes to search: MyPlayer, Inventory, PlayerInventory, etc.

    /*
    // Patch chest item granting
    [HarmonyPatch(typeof(InteractableChest), "GiveItem")] // Example name
    public class ChestItemPatch
    {
        static bool Prefix(ref object item) // Item type might be different
        {
            try
            {
                MelonLogger.Msg("Chest trying to give item...");

                // TODO: Check if item is unlocked in Archipelago
                // If not, replace with a different item or show message
                // item = GetRandomUnlockedItem();
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in ChestItemPatch: {ex.Message}");
            }

            return true;
        }
    }

    // Helper class for granting items from Archipelago
    public static class ItemGranter
    {
        public static void GrantItem(string itemName)
        {
            try
            {
                MelonLogger.Msg($"Granting Archipelago item: {itemName}");

                // TODO: Find the method to add weapon/tome/item to player
                // Might be something like:
                // MyPlayer.instance.AddWeapon(weaponId);
                // MyPlayer.instance.AddTome(tomeId);
                // MyPlayer.instance.AddItem(itemId);
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error granting item {itemName}: {ex.Message}");
            }
        }
    }
    */

    // DNSPY INSTRUCTIONS:
    // 1. Search for "MyPlayer" or "Inventory" or similar
    // 2. Look for methods that add items:
    //    - AddWeapon, GiveWeapon, EquipWeapon
    //    - AddTome, GiveTome
    //    - AddItem, GiveItem
    // 3. Note the parameters (usually an ID or item object)
    // 4. Look for item/weapon/tome definitions or enums
}
