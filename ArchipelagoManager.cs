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

        // Item name mappings from Archipelago to game enums
        private static readonly Dictionary<string, EWeapon> weaponNameMap = new Dictionary<string, EWeapon>
        {
            { "Fire Staff", EWeapon.FireStaff },
            { "Bone", EWeapon.Bone },
            { "Sword", EWeapon.Sword },
            { "Revolver", EWeapon.Revolver },
            { "Aura", EWeapon.Aura },
            { "Axe", EWeapon.Axe },
            { "Bow", EWeapon.Bow },
            { "Aegis", EWeapon.Aegis },
            { "Test", EWeapon.Test },
            { "Lightning Staff", EWeapon.LightningStaff },
            { "Flamewalker", EWeapon.Flamewalker },
            { "Rockets", EWeapon.Rockets },
            { "Bananarang", EWeapon.Bananarang },
            { "Tornado", EWeapon.Tornado },
            { "Dexecutioner", EWeapon.Dexecutioner },
            { "Sniper", EWeapon.Sniper },
            { "Frostwalker", EWeapon.Frostwalker },
            { "Space Noodle", EWeapon.SpaceNoodle },
            { "Dragons Breath", EWeapon.DragonsBreath },
            { "Chunkers", EWeapon.Chunkers },
            { "Mine", EWeapon.Mine },
            { "Poison Flask", EWeapon.PoisonFlask },
            { "Black Hole", EWeapon.BlackHole },
            { "Katana", EWeapon.Katana },
            { "Blood Magic", EWeapon.BloodMagic },
            { "Bluetooth Dagger", EWeapon.BluetoothDagger },
            { "Dice", EWeapon.Dice },
            { "Hero Sword", EWeapon.HeroSword },
            { "Corrupt Sword", EWeapon.CorruptSword },
            { "Shotgun", EWeapon.Shotgun }
        };

        private static readonly Dictionary<string, ETome> tomeNameMap = new Dictionary<string, ETome>
        {
            { "Damage Tome", ETome.Damage },
            { "Agility Tome", ETome.Agility },
            { "Cooldown Tome", ETome.Cooldown },
            { "Quantity Tome", ETome.Quantity },
            { "Knockback Tome", ETome.Knockback },
            { "Armor Tome", ETome.Armor },
            { "Health Tome", ETome.Health },
            { "Regeneration Tome", ETome.Regeneration },
            { "Size Tome", ETome.Size },
            { "Projectile Speed Tome", ETome.ProjectileSpeed },
            { "Duration Tome", ETome.Duration },
            { "Evasion Tome", ETome.Evasion },
            { "Attraction Tome", ETome.Attraction },
            { "Luck Tome", ETome.Luck },
            { "XP Tome", ETome.Xp },
            { "Golden Tome", ETome.Golden },
            { "Precision Tome", ETome.Precision },
            { "Shield Tome", ETome.Shield },
            { "Blood Tome", ETome.Blood },
            { "Thorns Tome", ETome.Thorns },
            { "Bounce Tome", ETome.Bounce },
            { "Cursed Tome", ETome.Cursed },
            { "Silver Tome", ETome.Silver },
            { "Balance Tome", ETome.Balance },
            { "Chaos Tome", ETome.Chaos },
            { "Gambler Tome", ETome.Gambler },
            { "Hoarder Tome", ETome.Hoarder }
        };

        private static readonly Dictionary<string, EItem> itemNameMap = new Dictionary<string, EItem>
        {
            { "Key", EItem.Key },
            { "Beer", EItem.Beer },
            { "Spiky Shield", EItem.SpikyShield },
            { "Bonker", EItem.Bonker },
            { "Slippery Ring", EItem.SlipperyRing },
            { "Cowards Cloak", EItem.CowardsCloak },
            { "Gym Sauce", EItem.GymSauce },
            { "Battery", EItem.Battery },
            { "Phantom Shroud", EItem.PhantomShroud },
            { "Forbidden Juice", EItem.ForbiddenJuice },
            { "Demon Blade", EItem.DemonBlade },
            { "Grandmas Secret Tonic", EItem.GrandmasSecretTonic },
            { "Giant Fork", EItem.GiantFork },
            { "Moldy Cheese", EItem.MoldyCheese },
            { "Golden Sneakers", EItem.GoldenSneakers },
            { "Spicy Meatball", EItem.SpicyMeatball },
            { "Chonkplate", EItem.Chonkplate },
            { "Lightning Orb", EItem.LightningOrb },
            { "Ice Cube", EItem.IceCube },
            { "Demonic Blood", EItem.DemonicBlood },
            { "Demonic Soul", EItem.DemonicSoul },
            { "Beefy Ring", EItem.BeefyRing },
            { "Dragonfire", EItem.Dragonfire },
            { "Golden Glove", EItem.GoldenGlove },
            { "Golden Shield", EItem.GoldenShield },
            { "Za Warudo", EItem.ZaWarudo },
            { "Overpowered Lamp", EItem.OverpoweredLamp },
            { "Feathers", EItem.Feathers },
            { "Ghost", EItem.Ghost },
            { "Slutty Cannon", EItem.SluttyCannon },
            { "Turbo Socks", EItem.TurboSocks },
            { "Shattered Wisdom", EItem.ShatteredWisdom },
            { "Echo Shard", EItem.EchoShard },
            { "Sucky Magnet", EItem.SuckyMagnet },
            { "Backpack", EItem.Backpack },
            { "Clover", EItem.Clover },
            { "Campfire", EItem.Campfire },
            { "Rollerblades", EItem.Rollerblades },
            { "Skuleg", EItem.Skuleg },
            { "Eagle Claw", EItem.EagleClaw },
            { "Scarf", EItem.Scarf },
            { "Anvil", EItem.Anvil },
            { "Oats", EItem.Oats },
            { "Cursed Doll", EItem.CursedDoll },
            { "Energy Core", EItem.EnergyCore },
            { "Electric Plug", EItem.ElectricPlug },
            { "Bob Dead", EItem.BobDead },
            { "Soul Harvester", EItem.SoulHarvester },
            { "Mirror", EItem.Mirror },
            { "Joes Dagger", EItem.JoesDagger },
            { "Weeb Headset", EItem.WeebHeadset },
            { "Speed Boi", EItem.SpeedBoi },
            { "Gasmask", EItem.Gasmask },
            { "Toxic Barrel", EItem.ToxicBarrel },
            { "Holy Book", EItem.HolyBook },
            { "Brass Knuckles", EItem.BrassKnuckles },
            { "Idle Juice", EItem.IdleJuice },
            { "Kevin", EItem.Kevin },
            { "Borgar", EItem.Borgar },
            { "Medkit", EItem.Medkit },
            { "Gamer Goggles", EItem.GamerGoggles },
            { "Unstable Transfusion", EItem.UnstableTransfusion },
            { "Bloody Cleaver", EItem.BloodyCleaver },
            { "Credit Card Red", EItem.CreditCardRed },
            { "Credit Card Green", EItem.CreditCardGreen },
            { "Boss Buster", EItem.BossBuster },
            { "Leeching Crystal", EItem.LeechingCrystal },
            { "Tactical Glasses", EItem.TacticalGlasses },
            { "Cactus", EItem.Cactus },
            { "Cage Key", EItem.CageKey },
            { "Ice Crystal", EItem.IceCrystal },
            { "Time Bracelet", EItem.TimeBracelet },
            { "Glove Lightning", EItem.GloveLightning },
            { "Glove Poison", EItem.GlovePoison },
            { "Glove Blood", EItem.GloveBlood },
            { "Glove Curse", EItem.GloveCurse },
            { "Glove Power", EItem.GlovePower },
            { "Wrench", EItem.Wrench },
            { "Beacon", EItem.Beacon },
            { "Golden Ring", EItem.GoldenRing },
            { "Quins Mask", EItem.QuinsMask }
        };

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

            // Grant the item based on type
            bool granted = false;

            // Check if it's a weapon
            if (weaponNameMap.TryGetValue(itemName, out var weapon))
            {
                UnlockWeapon(weapon);
                granted = true;
            }
            // Check if it's a tome
            else if (tomeNameMap.TryGetValue(itemName, out var tome))
            {
                UnlockTome(tome);
                granted = true;
            }
            // Check if it's an item
            else if (itemNameMap.TryGetValue(itemName, out var gameItem))
            {
                UnlockItem(gameItem);
                granted = true;
            }

            if (!granted)
            {
                MelonLogger.Warning($"Unknown item received from Archipelago: {itemName}");
            }

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
