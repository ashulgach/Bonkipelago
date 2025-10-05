using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Bonkipelago
{
    public class ConnectionStatusUI
    {
        private static ConnectionStatusUI instance;
        public static ConnectionStatusUI Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ConnectionStatusUI();
                }
                return instance;
            }
        }

        private GameObject statusTextObject;
        private Text statusText;
        private string currentSceneName = "";

        private ConnectionStatusUI()
        {
            // Register scene load handler
            SceneManager.sceneLoaded += (System.Action<Scene, LoadSceneMode>)OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            currentSceneName = scene.name;
            MelonLogger.Msg($"Scene loaded: {scene.name}");

            // Clean up old UI
            if (statusTextObject != null)
            {
                UnityEngine.Object.Destroy(statusTextObject);
                statusTextObject = null;
                statusText = null;
            }

            // Create UI if on main menu and connected
            if (IsMainMenuScene(scene.name))
            {
                CreateStatusUI();
            }
        }

        private bool IsMainMenuScene(string sceneName)
        {
            // Common main menu scene names in Unity games
            return sceneName.ToLower().Contains("menu") ||
                   sceneName.ToLower().Contains("main") ||
                   sceneName == "MainMenu" ||
                   sceneName == "Menu";
        }

        private void CreateStatusUI()
        {
            try
            {
                // Find the Canvas in the scene
                Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    MelonLogger.Warning("Could not find Canvas for connection status UI");
                    return;
                }

                // Create GameObject for status text
                statusTextObject = new GameObject("ArchipelagoConnectionStatus");
                statusTextObject.transform.SetParent(canvas.transform, false);

                // Add Text component
                statusText = statusTextObject.AddComponent<Text>();
                statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                statusText.fontSize = 16;
                statusText.color = Color.green;
                statusText.alignment = TextAnchor.UpperRight;

                // Position in top-right corner
                RectTransform rectTransform = statusTextObject.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(1, 1);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.pivot = new Vector2(1, 1);
                rectTransform.anchoredPosition = new Vector2(-10, -10);
                rectTransform.sizeDelta = new Vector2(400, 30);

                // Update text
                UpdateStatusText();

                MelonLogger.Msg("Connection status UI created successfully");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error creating connection status UI: {ex.Message}");
                MelonLogger.Error($"Stack trace: {ex.StackTrace}");
            }
        }

        public void Update()
        {
            // Update the text if we're on the main menu
            if (statusText != null && IsMainMenuScene(currentSceneName))
            {
                UpdateStatusText();
            }
        }

        private void UpdateStatusText()
        {
            if (statusText == null) return;

            var manager = ArchipelagoManager.Instance;
            if (manager.IsConnected)
            {
                // Extract hostname and port from server URL
                string serverUrl = BonkipelagoConfig.ServerUrl;
                statusText.text = $"Connected to Archipelago {serverUrl}";
                statusText.color = Color.green;
                statusTextObject.SetActive(true);
            }
            else
            {
                // Hide when not connected
                statusTextObject.SetActive(false);
            }
        }

        public void ForceUpdate()
        {
            // Check if we need to create UI (e.g., after connecting on main menu)
            if (statusTextObject == null && IsMainMenuScene(currentSceneName))
            {
                CreateStatusUI();
            }
            else
            {
                UpdateStatusText();
            }
        }
    }
}
