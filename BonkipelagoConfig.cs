using MelonLoader;

namespace Bonkipelago
{
    public static class BonkipelagoConfig
    {
        private static MelonPreferences_Category category;
        private static MelonPreferences_Entry<string> serverUrl;
        private static MelonPreferences_Entry<string> slotName;
        private static MelonPreferences_Entry<string> password;
        private static MelonPreferences_Entry<bool> autoConnect;
        private static MelonPreferences_Entry<int> chestsOpened;
        private static MelonPreferences_Entry<string> unlockedWeapons;
        private static MelonPreferences_Entry<string> unlockedTomes;
        private static MelonPreferences_Entry<string> unlockedItems;

        public static string ServerUrl => serverUrl.Value;
        public static string SlotName => slotName.Value;
        public static string Password => password.Value;
        public static bool AutoConnect => autoConnect.Value;
        public static int ChestsOpened
        {
            get => chestsOpened.Value;
            set
            {
                chestsOpened.Value = value;
                Save();
            }
        }

        public static string UnlockedWeapons
        {
            get => unlockedWeapons.Value;
            set
            {
                unlockedWeapons.Value = value;
                Save();
            }
        }

        public static string UnlockedTomes
        {
            get => unlockedTomes.Value;
            set
            {
                unlockedTomes.Value = value;
                Save();
            }
        }

        public static string UnlockedItems
        {
            get => unlockedItems.Value;
            set
            {
                unlockedItems.Value = value;
                Save();
            }
        }

        public static void Initialize()
        {
            // Create preferences category
            category = MelonPreferences.CreateCategory("Bonkipelago");
            category.SetFilePath("UserData/Bonkipelago.cfg");

            // Create preference entries
            serverUrl = category.CreateEntry(
                "ServerUrl",
                "localhost:38281",
                description: "Archipelago server URL (host:port)"
            );

            slotName = category.CreateEntry(
                "SlotName",
                "Player1",
                description: "Your slot/player name in the Archipelago session"
            );

            password = category.CreateEntry(
                "Password",
                "",
                description: "Password for the Archipelago session (leave empty if none)"
            );

            autoConnect = category.CreateEntry(
                "AutoConnect",
                false,
                description: "Automatically connect to Archipelago when the game starts"
            );

            chestsOpened = category.CreateEntry(
                "ChestsOpened",
                0,
                description: "Number of chests opened (counter for Archipelago location checks)"
            );

            unlockedWeapons = category.CreateEntry(
                "UnlockedWeapons",
                "FireStaff",
                description: "Comma-separated list of unlocked weapons"
            );

            unlockedTomes = category.CreateEntry(
                "UnlockedTomes",
                "",
                description: "Comma-separated list of unlocked tomes"
            );

            unlockedItems = category.CreateEntry(
                "UnlockedItems",
                "",
                description: "Comma-separated list of unlocked items"
            );

            // Save the config file
            category.SaveToFile(false);

            MelonLogger.Msg("Bonkipelago configuration loaded.");
            MelonLogger.Msg($"Server: {ServerUrl}");
            MelonLogger.Msg($"Slot: {SlotName}");
            MelonLogger.Msg($"Auto-connect: {AutoConnect}");
            MelonLogger.Msg($"Chests opened: {ChestsOpened}");
        }

        public static void Save()
        {
            category.SaveToFile(false);
        }
    }
}
