using MelonLoader;
using UnityEngine;
using System.IO;
using System.Text;
using System.Reflection;
using Il2CppInterop.Runtime;

namespace Bonkipelago
{
    public static class ComponentInspector
    {
        private static StringBuilder outputBuffer = new StringBuilder();

        public static void InspectObjectsWithComponent(string componentName, string outputFileName)
        {
            outputBuffer.Clear();
            outputBuffer.AppendLine($"=== INSPECTING OBJECTS WITH COMPONENT: {componentName} ===");
            outputBuffer.AppendLine($"Timestamp: {System.DateTime.Now}");
            outputBuffer.AppendLine();

            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            int foundCount = 0;

            foreach (GameObject obj in allObjects)
            {
                // Get all components and find the one matching the name
                Component[] components = obj.GetComponents<Component>();
                foreach (Component comp in components)
                {
                    if (comp != null)
                    {
                        try
                        {
                            string typeName = comp.GetIl2CppType().Name;
                            if (typeName == componentName)
                            {
                                foundCount++;
                                InspectGameObject(obj, comp);
                                outputBuffer.AppendLine();
                                break; // Found it, move to next GameObject
                            }
                        }
                        catch
                        {
                            // Skip if we can't read the type
                        }
                    }
                }
            }

            outputBuffer.AppendLine($"=== FOUND {foundCount} OBJECTS ===");
            WriteToFile(outputFileName);
        }

        private static void InspectGameObject(GameObject obj, Component targetComponent)
        {
            outputBuffer.AppendLine($"GameObject: {obj.name}");
            outputBuffer.AppendLine($"Active: {obj.activeInHierarchy}");
            outputBuffer.AppendLine($"Position: {obj.transform.position}");
            outputBuffer.AppendLine();

            InspectComponent(targetComponent);
        }

        private static void InspectComponent(Component component)
        {
            try
            {
                string typeName = component.GetIl2CppType().Name;
                outputBuffer.AppendLine($"Component: {typeName}");

                // Get all fields using reflection - including public, private, and inherited
                var componentType = component.GetType();
                var allFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;

                var fields = componentType.GetFields(allFlags);
                if (fields.Length > 0)
                {
                    outputBuffer.AppendLine("Fields:");
                    foreach (var field in fields)
                    {
                        // Skip Unity internals
                        if (field.Name.StartsWith("m_") || field.Name.StartsWith("NativeFieldInfo") || field.Name.Contains("Ptr"))
                            continue;

                        try
                        {
                            object value = field.GetValue(component);
                            string valueStr = value != null ? value.ToString() : "null";

                            // Truncate very long values
                            if (valueStr.Length > 200)
                            {
                                valueStr = valueStr.Substring(0, 200) + "...";
                            }

                            outputBuffer.AppendLine($"  {field.Name} ({field.FieldType.Name}): {valueStr}");
                        }
                        catch (System.Exception ex)
                        {
                            outputBuffer.AppendLine($"  {field.Name} ({field.FieldType.Name}): [Error: {ex.Message}]");
                        }
                    }
                }

                // Also try properties
                var properties = componentType.GetProperties(allFlags);
                if (properties.Length > 0)
                {
                    outputBuffer.AppendLine("Properties:");
                    foreach (var prop in properties)
                    {
                        // Skip Unity internals and indexers
                        if (prop.Name.StartsWith("m_") || prop.Name.Contains("Ptr") || prop.GetIndexParameters().Length > 0)
                            continue;

                        try
                        {
                            if (prop.CanRead)
                            {
                                object value = prop.GetValue(component);
                                string valueStr = value != null ? value.ToString() : "null";

                                if (valueStr.Length > 200)
                                {
                                    valueStr = valueStr.Substring(0, 200) + "...";
                                }

                                outputBuffer.AppendLine($"  {prop.Name} ({prop.PropertyType.Name}): {valueStr}");
                            }
                        }
                        catch (System.Exception ex)
                        {
                            // Skip unreadable properties silently
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                outputBuffer.AppendLine($"Error inspecting component: {ex.Message}");
            }

            outputBuffer.AppendLine("---");
        }

        private static void WriteToFile(string filename)
        {
            try
            {
                string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fullFilename = $"{filename}_{timestamp}.txt";
                string filepath = Path.Combine("UserData", fullFilename);

                File.WriteAllText(filepath, outputBuffer.ToString());

                MelonLogger.Msg($"Component inspection written to: {filepath}");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Failed to write inspection file: {ex.Message}");
            }
        }

        // Convenience methods for specific components
        public static void InspectChests()
        {
            InspectObjectsWithComponent("InteractableChest", "chest_inspection");
        }

        public static void InspectEnemies()
        {
            InspectObjectsWithComponent("Enemy", "enemy_inspection");
        }

        public static void InspectPlayer()
        {
            InspectObjectsWithComponent("MyPlayer", "player_inspection");
        }

        public static void InspectManagers()
        {
            MelonLogger.Msg("Inspecting game managers...");

            GameObject managersObj = GameObject.Find("Managers");
            if (managersObj != null)
            {
                outputBuffer.Clear();
                outputBuffer.AppendLine("=== GAME MANAGERS ===");
                outputBuffer.AppendLine($"Timestamp: {System.DateTime.Now}");
                outputBuffer.AppendLine();

                Component[] components = managersObj.GetComponents<Component>();
                foreach (Component comp in components)
                {
                    if (comp != null && comp.GetIl2CppType().Name.Contains("Manager"))
                    {
                        InspectComponent(comp);
                        outputBuffer.AppendLine();
                    }
                }

                WriteToFile("managers_inspection");
            }
            else
            {
                MelonLogger.Warning("Managers GameObject not found!");
            }
        }
    }
}
