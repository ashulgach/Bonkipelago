using System.Collections.Generic;
using Il2Cpp;
using Il2CppAssets.Scripts._Data.Tomes;
using Il2CppAssets.Scripts.Inventory__Items__Pickups.Items;

namespace Bonkipelago
{
    /// <summary>
    /// Maps Archipelago item IDs to game enums (EWeapon, ETome, EItem)
    /// </summary>
    public static class ItemMapper
    {
        private const long BASE_ID = 42000000;

        // Weapon mappings (BASE_ID + 0 to BASE_ID + 29)
        private static readonly Dictionary<long, EWeapon> weaponMap = new Dictionary<long, EWeapon>
        {
            { BASE_ID + 0, EWeapon.FireStaff },
            { BASE_ID + 1, EWeapon.Bone },
            { BASE_ID + 2, EWeapon.Sword },
            { BASE_ID + 3, EWeapon.Revolver },
            { BASE_ID + 4, EWeapon.Aura },
            { BASE_ID + 5, EWeapon.Axe },
            { BASE_ID + 6, EWeapon.Bow },
            { BASE_ID + 7, EWeapon.Aegis },
            { BASE_ID + 8, EWeapon.Test },
            { BASE_ID + 9, EWeapon.LightningStaff },
            { BASE_ID + 10, EWeapon.Flamewalker },
            { BASE_ID + 11, EWeapon.Rockets },
            { BASE_ID + 12, EWeapon.Bananarang },
            { BASE_ID + 13, EWeapon.Tornado },
            { BASE_ID + 14, EWeapon.Dexecutioner },
            { BASE_ID + 15, EWeapon.Sniper },
            { BASE_ID + 16, EWeapon.Frostwalker },
            { BASE_ID + 17, EWeapon.SpaceNoodle },
            { BASE_ID + 18, EWeapon.DragonsBreath },
            { BASE_ID + 19, EWeapon.Chunkers },
            { BASE_ID + 20, EWeapon.Mine },
            { BASE_ID + 21, EWeapon.PoisonFlask },
            { BASE_ID + 22, EWeapon.BlackHole },
            { BASE_ID + 23, EWeapon.Katana },
            { BASE_ID + 24, EWeapon.BloodMagic },
            { BASE_ID + 25, EWeapon.BluetoothDagger },
            { BASE_ID + 26, EWeapon.Dice },
            { BASE_ID + 27, EWeapon.HeroSword },
            { BASE_ID + 28, EWeapon.CorruptSword },
            { BASE_ID + 29, EWeapon.Shotgun },
        };

        // Tome mappings (BASE_ID + 100 to BASE_ID + 124)
        private static readonly Dictionary<long, ETome> tomeMap = new Dictionary<long, ETome>
        {
            { BASE_ID + 100, ETome.Damage },
            { BASE_ID + 101, ETome.Agility },
            { BASE_ID + 102, ETome.Cooldown },
            { BASE_ID + 103, ETome.Quantity },
            { BASE_ID + 104, ETome.Knockback },
            { BASE_ID + 105, ETome.Armor },
            { BASE_ID + 106, ETome.Health },
            { BASE_ID + 107, ETome.Regeneration },
            { BASE_ID + 108, ETome.Size },
            { BASE_ID + 109, ETome.ProjectileSpeed },
            { BASE_ID + 110, ETome.Duration },
            { BASE_ID + 111, ETome.Evasion },
            { BASE_ID + 112, ETome.Attraction },
            { BASE_ID + 113, ETome.Luck },
            { BASE_ID + 114, ETome.Xp },
            { BASE_ID + 115, ETome.Golden },
            { BASE_ID + 116, ETome.Precision },
            { BASE_ID + 117, ETome.Shield },
            { BASE_ID + 118, ETome.Blood },
            { BASE_ID + 119, ETome.Thorns },
            { BASE_ID + 120, ETome.Bounce },
            { BASE_ID + 121, ETome.Cursed },
            { BASE_ID + 122, ETome.Silver },
            { BASE_ID + 123, ETome.Balance },
            { BASE_ID + 124, ETome.Chaos },
        };

        // Item mappings (BASE_ID + 200 to BASE_ID + 280)
        private static readonly Dictionary<long, EItem> itemMap = new Dictionary<long, EItem>
        {
            { BASE_ID + 200, EItem.Key },
            { BASE_ID + 201, EItem.Beer },
            { BASE_ID + 202, EItem.SpikyShield },
            { BASE_ID + 203, EItem.Bonker },
            { BASE_ID + 204, EItem.SlipperyRing },
            { BASE_ID + 205, EItem.CowardsCloak },
            { BASE_ID + 206, EItem.GymSauce },
            { BASE_ID + 207, EItem.Battery },
            { BASE_ID + 208, EItem.PhantomShroud },
            { BASE_ID + 209, EItem.ForbiddenJuice },
            { BASE_ID + 210, EItem.DemonBlade },
            { BASE_ID + 211, EItem.GrandmasSecretTonic },
            { BASE_ID + 212, EItem.GiantFork },
            { BASE_ID + 213, EItem.MoldyCheese },
            { BASE_ID + 214, EItem.GoldenSneakers },
            { BASE_ID + 215, EItem.SpicyMeatball },
            { BASE_ID + 216, EItem.Chonkplate },
            { BASE_ID + 217, EItem.LightningOrb },
            { BASE_ID + 218, EItem.IceCube },
            { BASE_ID + 219, EItem.DemonicBlood },
            { BASE_ID + 220, EItem.DemonicSoul },
            { BASE_ID + 221, EItem.BeefyRing },
            { BASE_ID + 222, EItem.Dragonfire },
            { BASE_ID + 223, EItem.GoldenGlove },
            { BASE_ID + 224, EItem.GoldenShield },
            { BASE_ID + 225, EItem.ZaWarudo },
            { BASE_ID + 226, EItem.OverpoweredLamp },
            { BASE_ID + 227, EItem.Feathers },
            { BASE_ID + 228, EItem.Ghost },
            { BASE_ID + 229, EItem.SluttyCannon },
            { BASE_ID + 230, EItem.TurboSocks },
            { BASE_ID + 231, EItem.ShatteredWisdom },
            { BASE_ID + 232, EItem.EchoShard },
            { BASE_ID + 233, EItem.SuckyMagnet },
            { BASE_ID + 234, EItem.Backpack },
            { BASE_ID + 235, EItem.Clover },
            { BASE_ID + 236, EItem.Campfire },
            { BASE_ID + 237, EItem.Rollerblades },
            { BASE_ID + 238, EItem.Skuleg },
            { BASE_ID + 239, EItem.EagleClaw },
            { BASE_ID + 240, EItem.Scarf },
            { BASE_ID + 241, EItem.Anvil },
            { BASE_ID + 242, EItem.Oats },
            { BASE_ID + 243, EItem.CursedDoll },
            { BASE_ID + 244, EItem.EnergyCore },
            { BASE_ID + 245, EItem.ElectricPlug },
            { BASE_ID + 246, EItem.BobDead },
            { BASE_ID + 247, EItem.SoulHarvester },
            { BASE_ID + 248, EItem.Mirror },
            { BASE_ID + 249, EItem.JoesDagger },
            { BASE_ID + 250, EItem.WeebHeadset },
            { BASE_ID + 251, EItem.SpeedBoi },
            { BASE_ID + 252, EItem.Gasmask },
            { BASE_ID + 253, EItem.ToxicBarrel },
            { BASE_ID + 254, EItem.HolyBook },
            { BASE_ID + 255, EItem.BrassKnuckles },
            { BASE_ID + 256, EItem.IdleJuice },
            { BASE_ID + 257, EItem.Kevin },
            { BASE_ID + 258, EItem.Borgar },
            { BASE_ID + 259, EItem.Medkit },
            { BASE_ID + 260, EItem.GamerGoggles },
            { BASE_ID + 261, EItem.UnstableTransfusion },
            { BASE_ID + 262, EItem.BloodyCleaver },
            { BASE_ID + 263, EItem.CreditCardRed },
            { BASE_ID + 264, EItem.CreditCardGreen },
            { BASE_ID + 265, EItem.BossBuster },
            { BASE_ID + 266, EItem.LeechingCrystal },
            { BASE_ID + 267, EItem.TacticalGlasses },
            { BASE_ID + 268, EItem.Cactus },
            { BASE_ID + 269, EItem.CageKey },
            { BASE_ID + 270, EItem.IceCrystal },
            { BASE_ID + 271, EItem.TimeBracelet },
            { BASE_ID + 272, EItem.GloveLightning },
            { BASE_ID + 273, EItem.GlovePoison },
            { BASE_ID + 274, EItem.GloveBlood },
            { BASE_ID + 275, EItem.GloveCurse },
            { BASE_ID + 276, EItem.GlovePower },
            { BASE_ID + 277, EItem.Wrench },
            { BASE_ID + 278, EItem.Beacon },
            { BASE_ID + 279, EItem.GoldenRing },
            { BASE_ID + 280, EItem.QuinsMask },
        };

        public enum ItemType
        {
            Unknown,
            Weapon,
            Tome,
            Item
        }

        public class MappedItem
        {
            public ItemType Type { get; set; }
            public EWeapon? Weapon { get; set; }
            public ETome? Tome { get; set; }
            public EItem? Item { get; set; }
        }

        /// <summary>
        /// Map an Archipelago item ID to game enum
        /// </summary>
        public static MappedItem MapItem(long apItemId)
        {
            // Try weapon
            if (weaponMap.TryGetValue(apItemId, out var weapon))
            {
                return new MappedItem
                {
                    Type = ItemType.Weapon,
                    Weapon = weapon
                };
            }

            // Try tome
            if (tomeMap.TryGetValue(apItemId, out var tome))
            {
                return new MappedItem
                {
                    Type = ItemType.Tome,
                    Tome = tome
                };
            }

            // Try item
            if (itemMap.TryGetValue(apItemId, out var item))
            {
                return new MappedItem
                {
                    Type = ItemType.Item,
                    Item = item
                };
            }

            return new MappedItem { Type = ItemType.Unknown };
        }
    }
}
