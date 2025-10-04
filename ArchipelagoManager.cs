using MelonLoader;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using Archipelago.MultiClient.Net.Helpers;
using System.Collections.Generic;
using System.Linq;
using Il2Cpp;
using Il2CppAssets.Scripts._Data.Tomes;
using Il2CppAssets.Scripts.Inventory__Items__Pickups.Items;

namespace Bonkipelago
{
    public class ArchipelagoManager
    {
        private static ArchipelagoManager instance;
        public static ArchipelagoManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ArchipelagoManager();
                }
                return instance;
            }
        }

        private ArchipelagoSession session;
        private bool isConnected = false;

        public bool IsConnected => isConnected;

        // Track unlocked weapons, tomes, and items
        private HashSet<EWeapon> unlockedWeapons = new HashSet<EWeapon>();
        private HashSet<ETome> unlockedTomes = new HashSet<ETome>();
        private HashSet<EItem> unlockedItems = new HashSet<EItem>();

        // Cache location->item mappings from scouting
        private Dictionary<long, LocationScoutInfo> scoutedLocations = new Dictionary<long, LocationScoutInfo>();

        // Chest location base ID (chests are commodities: BASE_ID + counter)
        private const long CHEST_BASE_ID = 1000;

        private ArchipelagoManager()
        {
            // Private constructor for singleton

            // Start with only FireStaff unlocked
            unlockedWeapons.Add(EWeapon.FireStaff);

            MelonLogger.Msg("ArchipelagoManager initialized with FireStaff unlocked");
        }

        // Info about what's at a location
        public class LocationScoutInfo
        {
            public long ItemId;
            public string ItemName;
            public int PlayerSlot;
            public string PlayerName;
            public bool IsForLocalPlayer;
        }

        public void Connect(string server, string slotName, string password = null)
        {
            if (isConnected)
            {
                MelonLogger.Warning("Already connected to Archipelago. Disconnect first.");
                return;
            }

            try
            {
                MelonLogger.Msg($"Connecting to Archipelago server at {server}...");

                // Create session
                session = ArchipelagoSessionFactory.CreateSession(server);

                // Register event handlers BEFORE connecting
                RegisterEventHandlers();

                // Attempt to connect and login
                LoginResult result = session.TryConnectAndLogin(
                    "Megabonk",
                    slotName,
                    ItemsHandlingFlags.AllItems,
                    password: password
                );

                if (result.Successful)
                {
                    LoginSuccessful success = (LoginSuccessful)result;
                    isConnected = true;
                    MelonLogger.Msg($"Successfully connected to Archipelago as {slotName}!");
                    MelonLogger.Msg($"Slot: {success.Slot} | Team: {success.Team}");

                    // Scout ahead chest locations
                    MelonLogger.Msg("Scouting chest locations...");
                    ScoutAheadChests(50);
                }
                else
                {
                    LoginFailure failure = (LoginFailure)result;
                    MelonLogger.Error($"Failed to connect to Archipelago: {string.Join(", ", failure.Errors)}");

                    if (failure.ErrorCodes != null && failure.ErrorCodes.Length > 0)
                    {
                        MelonLogger.Error($"Error codes: {string.Join(", ", failure.ErrorCodes)}");
                    }

                    isConnected = false;
                    session = null;
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Exception while connecting to Archipelago: {ex.Message}");
                MelonLogger.Error($"Stack trace: {ex.StackTrace}");
                isConnected = false;
                session = null;
            }
        }

        public void Disconnect()
        {
            if (session != null)
            {
                MelonLogger.Msg("Disconnecting from Archipelago...");
                session.Socket.DisconnectAsync();
                session = null;
                isConnected = false;
                MelonLogger.Msg("Disconnected from Archipelago.");
            }
        }

        private void RegisterEventHandlers()
        {
            // Socket events
            session.Socket.ErrorReceived += OnError;
            session.Socket.SocketClosed += OnSocketClosed;

            // Item received event - will be called for every item on connect/reconnect
            session.Items.ItemReceived += OnItemReceived;
        }

        private void OnError(System.Exception exception, string message)
        {
            MelonLogger.Error($"Archipelago Error: {message}");
            if (exception != null)
            {
                MelonLogger.Error($"Exception: {exception.Message}");
            }
        }

        private void OnSocketClosed(string reason)
        {
            MelonLogger.Warning($"Archipelago connection closed: {reason}");
            isConnected = false;
        }

        private void OnItemReceived(ReceivedItemsHelper helper)
        {
            var item = helper.PeekItem();
            string itemName = session.Items.GetItemName(item.ItemId);
            string playerName = session.Players.GetPlayerName(item.Player);

            MelonLogger.Msg($"Received item: {itemName} from {playerName}");

            // TODO: Actually grant the item in the game
            // For now, just log it

            helper.DequeueItem();
        }

        public void CheckLocation(long locationId)
        {
            if (!isConnected || session == null)
            {
                MelonLogger.Warning($"Cannot check location {locationId}: not connected to Archipelago");
                return;
            }

            MelonLogger.Msg($"Checking location: {locationId}");
            session.Locations.CompleteLocationChecks(locationId);
        }

        public void CompleteGoal()
        {
            if (!isConnected || session == null)
            {
                MelonLogger.Warning("Cannot complete goal: not connected to Archipelago");
                return;
            }

            MelonLogger.Msg("Goal completed! Notifying Archipelago server...");
            session.SetGoalAchieved();
        }

        public void Update()
        {
            // Any per-frame updates needed
            // Archipelago.MultiClient.Net handles most things async
        }

        // Weapon/Tome unlocking
        public bool IsWeaponUnlocked(EWeapon weapon)
        {
            return unlockedWeapons.Contains(weapon);
        }

        public bool IsTomeUnlocked(ETome tome)
        {
            return unlockedTomes.Contains(tome);
        }

        public void UnlockWeapon(EWeapon weapon)
        {
            if (unlockedWeapons.Add(weapon))
            {
                MelonLogger.Msg($"Unlocked weapon: {weapon}");
            }
        }

        public void UnlockTome(ETome tome)
        {
            if (unlockedTomes.Add(tome))
            {
                MelonLogger.Msg($"Unlocked tome: {tome}");
            }
        }

        public List<EWeapon> GetUnlockedWeapons()
        {
            return new List<EWeapon>(unlockedWeapons);
        }

        public List<ETome> GetUnlockedTomes()
        {
            return new List<ETome>(unlockedTomes);
        }

        public bool IsItemUnlocked(EItem item)
        {
            return unlockedItems.Contains(item);
        }

        public void UnlockItem(EItem item)
        {
            if (unlockedItems.Add(item))
            {
                MelonLogger.Msg($"Unlocked item: {item}");
            }
        }

        // Scout a location to find out what item is there
        public async void ScoutLocation(long locationId)
        {
            if (!isConnected || session == null)
            {
                MelonLogger.Warning($"Cannot scout location {locationId}: not connected");
                return;
            }

            try
            {
                MelonLogger.Msg($"Scouting location {locationId}...");

                var scoutResult = await session.Locations.ScoutLocationsAsync(false, locationId);

                if (scoutResult != null && scoutResult.ContainsKey(locationId))
                {
                    var itemInfo = scoutResult[locationId];
                    string itemName = session.Items.GetItemName(itemInfo.ItemId);
                    string playerName = session.Players.GetPlayerName(itemInfo.Player);
                    bool isForUs = itemInfo.Player == session.ConnectionInfo.Slot;

                    var scoutInfo = new LocationScoutInfo
                    {
                        ItemId = itemInfo.ItemId,
                        ItemName = itemName,
                        PlayerSlot = itemInfo.Player,
                        PlayerName = playerName,
                        IsForLocalPlayer = isForUs
                    };

                    scoutedLocations[locationId] = scoutInfo;

                    MelonLogger.Msg($"Location {locationId} contains: {itemName} for {playerName} (us: {isForUs})");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error scouting location {locationId}: {ex.Message}");
            }
        }

        // Get info about what's at a location (must be scouted first)
        public LocationScoutInfo GetLocationInfo(long locationId)
        {
            if (scoutedLocations.TryGetValue(locationId, out var info))
            {
                return info;
            }
            return null;
        }

        // Check if a location has been scouted
        public bool IsLocationScouted(long locationId)
        {
            return scoutedLocations.ContainsKey(locationId);
        }

        // Scout ahead X chest locations (called on connect or when getting close to end of scouted range)
        public async void ScoutAheadChests(int count = 50)
        {
            if (!isConnected || session == null)
            {
                MelonLogger.Warning("Cannot scout chests: not connected to Archipelago");
                return;
            }

            try
            {
                int currentCounter = BonkipelagoConfig.ChestsOpened;

                // Generate location IDs for the next X chests
                var locationIds = new List<long>();
                for (int i = 0; i < count; i++)
                {
                    long locationId = CHEST_BASE_ID + currentCounter + i;
                    if (!IsLocationScouted(locationId))
                    {
                        locationIds.Add(locationId);
                    }
                }

                if (locationIds.Count == 0)
                {
                    MelonLogger.Msg("Next batch already scouted");
                    return;
                }

                MelonLogger.Msg($"Scouting next {locationIds.Count} chest locations (starting from chest #{currentCounter})...");

                // Scout all locations in one batch
                var scoutResult = await session.Locations.ScoutLocationsAsync(false, locationIds.ToArray());

                if (scoutResult != null)
                {
                    foreach (var kvp in scoutResult)
                    {
                        long locationId = kvp.Key;
                        var itemInfo = kvp.Value;

                        string itemName = session.Items.GetItemName(itemInfo.ItemId);
                        string playerName = session.Players.GetPlayerName(itemInfo.Player);
                        bool isForUs = itemInfo.Player == session.ConnectionInfo.Slot;

                        var scoutInfo = new LocationScoutInfo
                        {
                            ItemId = itemInfo.ItemId,
                            ItemName = itemName,
                            PlayerSlot = itemInfo.Player,
                            PlayerName = playerName,
                            IsForLocalPlayer = isForUs
                        };

                        scoutedLocations[locationId] = scoutInfo;
                    }

                    MelonLogger.Msg($"Scouted {scoutResult.Count} chest locations successfully!");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error scouting chests: {ex.Message}");
                MelonLogger.Error($"Stack trace: {ex.StackTrace}");
            }
        }

        // Get location ID for the next chest check
        public long GetNextChestLocationId()
        {
            return CHEST_BASE_ID + BonkipelagoConfig.ChestsOpened;
        }

        // Increment chest counter and check if we need to scout more
        public void IncrementChestCounter()
        {
            BonkipelagoConfig.ChestsOpened++;

            // Scout more if we're getting close to the end of our scouted range
            int currentCounter = BonkipelagoConfig.ChestsOpened;
            int scoutedAhead = scoutedLocations.Keys.Count(id => id >= CHEST_BASE_ID + currentCounter);

            if (scoutedAhead < 10 && isConnected)
            {
                MelonLogger.Msg("Running low on scouted chests, scouting more...");
                ScoutAheadChests(50);
            }
        }
    }
}
