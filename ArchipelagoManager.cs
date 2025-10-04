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
using Il2CppAssets.Scripts.Actors.Player;
using Il2CppAssets.Scripts.Inventory__Items__Pickups;
using Il2CppAssets.Scripts.Inventory__Items__Pickups.Weapons;
using Il2CppAssets.Scripts.Inventory__Items__Pickups.Stats;
using UnityEngine;

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

        // Pending grants queue + scheduling
        private enum PendingKind { Weapon, Tome, Item }
        private class PendingGrant
        {
            public PendingKind Kind;
            public EWeapon Weapon;
            public ETome Tome;
            public EItem Item;
        }

        private readonly System.Collections.Generic.List<PendingGrant> pendingGrants = new System.Collections.Generic.List<PendingGrant>();
        private bool subscribedPlayerInit = false;
        private float nextPendingProcessTime = 0f;

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
                // Mark as unlocked (affects level-up filtering)
                UnlockWeapon(weapon);
                // Try to add immediately if we're in a run and have a slot
                if (TryGrantWeaponInRun(weapon))
                {
                    MelonLogger.Msg($"Granted weapon in run: {weapon}");
                }
                else
                {
                    EnqueueWeapon(weapon);
                }
                granted = true;
            }
            // Check if it's a tome
            else if (tomeNameMap.TryGetValue(itemName, out var tome))
            {
                // Mark as unlocked (affects level-up filtering)
                UnlockTome(tome);
                // Try to add immediately if we're in a run and have a slot
                if (TryGrantTomeInRun(tome))
                {
                    MelonLogger.Msg($"Granted tome in run: {tome}");
                }
                else
                {
                    EnqueueTome(tome);
                }
                granted = true;
            }
            // Check if it's an item
            else if (itemNameMap.TryGetValue(itemName, out var gameItem))
            {
                UnlockItem(gameItem);
                // Always try to grant EItem immediately
                if (TryGrantItemInRun(gameItem))
                {
                    MelonLogger.Msg($"Granted item in run: {gameItem}");
                }
                else
                {
                    EnqueueItem(gameItem);
                }
                granted = true;
            }

            if (!granted)
            {
                MelonLogger.Warning($"Unknown item received from Archipelago: {itemName}");
            }

            helper.DequeueItem();
        }

        // Get the current player's inventory if spawned
        private PlayerInventory GetPlayerInventory()
        {
            try
            {
                var player = MyPlayer.Instance;
                return player != null ? player.inventory : null;
            }
            catch
            {
                return null;
            }
        }

        // Attempt to add a weapon to the live run (if possible)
        private bool TryGrantWeaponInRun(EWeapon eWeapon)
        {
            var inv = GetPlayerInventory();
            if (inv == null)
            {
                MelonLogger.Msg("Player not available yet; weapon will be selectable on level-up.");
                return false;
            }

            try
            {
                // Already have this weapon?
                var wInv = inv.weaponInventory;
                if (wInv == null)
                    return false;

                int currentLevel = wInv.GetWeaponLevel(eWeapon);
                if (currentLevel > 0)
                {
                    MelonLogger.Msg($"Weapon already present: {eWeapon} (level {currentLevel}).");
                    return false;
                }

                // Check slot availability
                if (!InventoryUtility.CanUnlockWeapons())
                {
                    MelonLogger.Msg("No available weapon slots; unlocked for future selection.");
                    return false;
                }

                var data = DataManager.Instance.GetWeapon(eWeapon);
                if (data == null)
                    return false;

                var emptyOffer = new Il2CppSystem.Collections.Generic.List<Il2CppAssets.Scripts.Inventory__Items__Pickups.Stats.StatModifier>();
                wInv.AddWeapon(data, emptyOffer);
                return true;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error granting weapon {eWeapon}: {ex.Message}");
                return false;
            }
        }

        // Attempt to add a tome to the live run (if possible)
        private bool TryGrantTomeInRun(ETome eTome)
        {
            var inv = GetPlayerInventory();
            if (inv == null)
            {
                MelonLogger.Msg("Player not available yet; tome will be selectable on level-up.");
                return false;
            }

            try
            {
                var tInv = inv.tomeInventory;
                if (tInv == null)
                    return false;

                if (tInv.HasTome(eTome))
                {
                    MelonLogger.Msg($"Tome already present: {eTome}.");
                    return false;
                }

                if (!InventoryUtility.CanUnlockTomes())
                {
                    MelonLogger.Msg("No available tome slots; unlocked for future selection.");
                    return false;
                }

                var data = DataManager.Instance.GetTome(eTome);
                if (data == null)
                    return false;

                var emptyOffer = new Il2CppSystem.Collections.Generic.List<Il2CppAssets.Scripts.Inventory__Items__Pickups.Stats.StatModifier>();
                tInv.AddTome(data, emptyOffer, ERarity.Common);
                return true;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error granting tome {eTome}: {ex.Message}");
                return false;
            }
        }

        // Attempt to add an EItem to the live run
        private bool TryGrantItemInRun(EItem eItem)
        {
            var inv = GetPlayerInventory();
            if (inv == null)
            {
                MelonLogger.Msg("Player not available yet; item will be applied when possible.");
                return false;
            }

            try
            {
                inv.itemInventory?.AddItem(eItem);
                // Notify run unlockables logic (some systems update on this hook)
                RunUnlockables.OnItemAdded(eItem);
                return true;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error granting item {eItem}: {ex.Message}");
                return false;
            }
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
            // Subscribe once to player inventory initialization event
            EnsureSubscribedToPlayerEvents();

            // Throttled pending processing
            if (pendingGrants.Count > 0 && Time.time >= nextPendingProcessTime)
            {
                ProcessPendingGrants(5);
                nextPendingProcessTime = Time.time + 0.5f;
            }
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

        // Get the current player's inventory if spawned
        private PlayerInventory GetPlayerInventory()
        {
            try
            {
                var player = MyPlayer.Instance;
                return player != null ? player.inventory : null;
            }
            catch
            {
                return null;
            }
        }

        private void EnsureSubscribedToPlayerEvents()
        {
            if (subscribedPlayerInit) return;
            try
            {
                MyPlayer.A_PlayerInventoryInitialized += OnPlayerInventoryInitialized;
                subscribedPlayerInit = true;
            }
            catch { }
        }

        private void OnPlayerInventoryInitialized()
        {
            ProcessPendingGrants(10);
        }

        private void EnqueueWeapon(EWeapon weapon)
        {
            if (!pendingGrants.Exists(p => p.Kind == PendingKind.Weapon && p.Weapon == weapon))
            {
                pendingGrants.Add(new PendingGrant { Kind = PendingKind.Weapon, Weapon = weapon });
                MelonLogger.Msg($"Queued weapon for later grant: {weapon}");
            }
        }

        private void EnqueueTome(ETome tome)
        {
            if (!pendingGrants.Exists(p => p.Kind == PendingKind.Tome && p.Tome == tome))
            {
                pendingGrants.Add(new PendingGrant { Kind = PendingKind.Tome, Tome = tome });
                MelonLogger.Msg($"Queued tome for later grant: {tome}");
            }
        }

        private void EnqueueItem(EItem item)
        {
            // allow stacks, but avoid immediate duplicate spam
            if (pendingGrants.Count == 0 || pendingGrants[pendingGrants.Count - 1].Item != item)
            {
                pendingGrants.Add(new PendingGrant { Kind = PendingKind.Item, Item = item });
                MelonLogger.Msg($"Queued item for later grant: {item}");
            }
        }

        private void ProcessPendingGrants(int maxCount)
        {
            var inv = GetPlayerInventory();
            if (inv == null) return;

            int processed = 0;
            for (int i = pendingGrants.Count - 1; i >= 0 && processed < maxCount; i--)
            {
                var p = pendingGrants[i];
                bool remove = false;
                switch (p.Kind)
                {
                    case PendingKind.Weapon:
                        if (inv.weaponInventory != null && inv.weaponInventory.GetWeaponLevel(p.Weapon) > 0)
                        {
                            remove = true; // already present
                        }
                        else if (TryGrantWeaponInRun(p.Weapon))
                        {
                            MelonLogger.Msg($"Granted queued weapon: {p.Weapon}");
                            remove = true;
                        }
                        break;
                    case PendingKind.Tome:
                        if (inv.tomeInventory != null && inv.tomeInventory.HasTome(p.Tome))
                        {
                            remove = true; // already present
                        }
                        else if (TryGrantTomeInRun(p.Tome))
                        {
                            MelonLogger.Msg($"Granted queued tome: {p.Tome}");
                            remove = true;
                        }
                        break;
                    case PendingKind.Item:
                        if (inv.itemInventory != null && inv.itemInventory.GetAmount(p.Item) > 0)
                        {
                            remove = true; // already present
                        }
                        else if (TryGrantItemInRun(p.Item))
                        {
                            MelonLogger.Msg($"Granted queued item: {p.Item}");
                            remove = true;
                        }
                        break;
                }

                if (remove)
                {
                    pendingGrants.RemoveAt(i);
                    processed++;
                }
            }
        }
    }
}
