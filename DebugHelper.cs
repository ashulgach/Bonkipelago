using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text;
using Il2CppInterop.Runtime;

namespace Bonkipelago
{
    public static class DebugHelper
    {
        private static bool loggedThisFrame = false;
        private static StringBuilder outputBuffer = new StringBuilder();

        public static void Update()
        {
            // Press F8 to dump all GameObjects in the current scene to file
            if (Input.GetKeyDown(KeyCode.F8))
            {
                if (!loggedThisFrame)
                {
                    DumpSceneObjectsToFile();
                    loggedThisFrame = true;
                }
            }

            // Press F9 to log all root GameObjects to file (tree view)
            if (Input.GetKeyDown(KeyCode.F9))
            {
                if (!loggedThisFrame)
                {
                    DumpRootObjectsToFile();
                    loggedThisFrame = true;
                }
            }

            // Component inspectors
            if (Input.GetKeyDown(KeyCode.F5))
            {
                ComponentInspector.InspectChests();
            }

            if (Input.GetKeyDown(KeyCode.F6))
            {
                ComponentInspector.InspectEnemies();
            }

            if (Input.GetKeyDown(KeyCode.F7))
            {
                ComponentInspector.InspectPlayer();
            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                ComponentInspector.InspectManagers();
            }

            if (!loggedThisFrame)
            {
                loggedThisFrame = false;
            }
        }

        private static void DumpSceneObjectsToFile()
        {
            outputBuffer.Clear();

            Scene activeScene = SceneManager.GetActiveScene();
            outputBuffer.AppendLine("=== DUMPING ALL SCENE OBJECTS ===");
            outputBuffer.AppendLine($"Active Scene: {activeScene.name}");
            outputBuffer.AppendLine($"Timestamp: {System.DateTime.Now}");

            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            outputBuffer.AppendLine($"Total GameObjects: {allObjects.Length}");
            outputBuffer.AppendLine();

            foreach (GameObject obj in allObjects)
            {
                LogGameObjectToBuffer(obj, 0);
            }

            outputBuffer.AppendLine("=== END SCENE DUMP ===");

            WriteToFile("scene_dump");
        }

        private static void DumpRootObjectsToFile()
        {
            outputBuffer.Clear();

            Scene activeScene = SceneManager.GetActiveScene();
            outputBuffer.AppendLine("=== DUMPING ROOT OBJECTS (TREE VIEW) ===");
            outputBuffer.AppendLine($"Active Scene: {activeScene.name}");
            outputBuffer.AppendLine($"Timestamp: {System.DateTime.Now}");

            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            outputBuffer.AppendLine($"Root GameObjects: {rootObjects.Length}");
            outputBuffer.AppendLine();

            foreach (GameObject obj in rootObjects)
            {
                LogGameObjectTreeToBuffer(obj, 0);
            }

            outputBuffer.AppendLine("=== END ROOT DUMP ===");

            WriteToFile("root_dump");
        }

        private static void LogGameObjectToBuffer(GameObject obj, int indent)
        {
            string indentStr = new string(' ', indent * 2);
            string activeStr = obj.activeInHierarchy ? "[ACTIVE]" : "[INACTIVE]";

            outputBuffer.AppendLine($"{indentStr}{activeStr} {obj.name}");

            Component[] components = obj.GetComponents<Component>();
            foreach (Component component in components)
            {
                if (component != null)
                {
                    try
                    {
                        // Use Il2Cpp type name method
                        string typeName = component.GetIl2CppType().Name;
                        outputBuffer.AppendLine($"{indentStr}  - {typeName}");
                    }
                    catch
                    {
                        // Fallback to regular type name
                        try
                        {
                            outputBuffer.AppendLine($"{indentStr}  - {component.GetType().FullName}");
                        }
                        catch
                        {
                            outputBuffer.AppendLine($"{indentStr}  - [Unknown Component]");
                        }
                    }
                }
            }
        }

        private static void LogGameObjectTreeToBuffer(GameObject obj, int indent)
        {
            string indentStr = new string(' ', indent * 2);
            string activeStr = obj.activeInHierarchy ? "[ACTIVE]" : "[INACTIVE]";

            outputBuffer.AppendLine($"{indentStr}{activeStr} {obj.name}");

            Component[] components = obj.GetComponents<Component>();
            foreach (Component component in components)
            {
                if (component != null)
                {
                    try
                    {
                        // Use Il2Cpp type name method
                        string typeName = component.GetIl2CppType().Name;
                        outputBuffer.AppendLine($"{indentStr}  - {typeName}");
                    }
                    catch
                    {
                        // Fallback to regular type name
                        try
                        {
                            outputBuffer.AppendLine($"{indentStr}  - {component.GetType().FullName}");
                        }
                        catch
                        {
                            outputBuffer.AppendLine($"{indentStr}  - [Unknown Component]");
                        }
                    }
                }
            }

            // Recursively log children
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                Transform child = obj.transform.GetChild(i);
                LogGameObjectTreeToBuffer(child.gameObject, indent + 1);
            }
        }

        private static void WriteToFile(string prefix)
        {
            try
            {
                string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string filename = $"{prefix}_{timestamp}.txt";
                string filepath = Path.Combine("UserData", filename);

                File.WriteAllText(filepath, outputBuffer.ToString());

                MelonLogger.Msg($"Scene dump written to: {filepath}");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Failed to write dump file: {ex.Message}");
            }
        }
    }
}
