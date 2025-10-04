using MelonLoader;
using UnityEngine;
using System.IO;
using System.Text;
using System.Reflection;
using Il2CppInterop.Runtime;
using System;
using System.Linq;

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

        public static void DumpMyPlayerMethods()
        {
            MelonLogger.Msg("Dumping all MyPlayer class methods...");

            outputBuffer.Clear();
            outputBuffer.AppendLine("=== MyPlayer CLASS METHODS ===");
            outputBuffer.AppendLine($"Timestamp: {DateTime.Now}");
            outputBuffer.AppendLine();
            outputBuffer.AppendLine("Searching for methods related to granting weapons, tomes, and items...");
            outputBuffer.AppendLine();

            try
            {
                // Find MyPlayer type
                Type myPlayerType = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    myPlayerType = assembly.GetType("Il2Cpp.MyPlayer");
                    if (myPlayerType != null)
                        break;
                }

                if (myPlayerType == null)
                {
                    outputBuffer.AppendLine("ERROR: MyPlayer type not found!");
                    WriteToFile("myplayer_methods_ERROR");
                    return;
                }

                outputBuffer.AppendLine($"Found type: {myPlayerType.FullName}");
                outputBuffer.AppendLine($"Assembly: {myPlayerType.Assembly.GetName().Name}");
                outputBuffer.AppendLine();

                // Get all methods (public and non-public, instance and static)
                var allFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
                var methods = myPlayerType.GetMethods(allFlags);

                // Filter for potentially relevant methods
                var relevantKeywords = new[] {
                    "weapon", "tome", "item", "unlock", "grant", "give", "add", "acquire", "obtain",
                    "equip", "pickup", "collect", "inventory", "upgrade", "level"
                };

                var relevantMethods = methods.Where(m =>
                    relevantKeywords.Any(k => m.Name.ToLower().Contains(k))
                ).OrderBy(m => m.Name).ToList();

                outputBuffer.AppendLine("=== POTENTIALLY RELEVANT METHODS ===");
                outputBuffer.AppendLine($"Found {relevantMethods.Count} methods matching keywords: {string.Join(", ", relevantKeywords)}");
                outputBuffer.AppendLine();

                foreach (var method in relevantMethods)
                {
                    DumpMethodSignature(method);
                }

                outputBuffer.AppendLine();
                outputBuffer.AppendLine("=== ALL PUBLIC METHODS ===");
                var publicMethods = methods.Where(m => m.IsPublic).OrderBy(m => m.Name).ToList();
                outputBuffer.AppendLine($"Found {publicMethods.Count} public methods");
                outputBuffer.AppendLine();

                foreach (var method in publicMethods)
                {
                    DumpMethodSignature(method);
                }

                outputBuffer.AppendLine();
                outputBuffer.AppendLine("=== ALL PRIVATE/PROTECTED METHODS ===");
                var privateMethods = methods.Where(m => !m.IsPublic).OrderBy(m => m.Name).ToList();
                outputBuffer.AppendLine($"Found {privateMethods.Count} private/protected methods");
                outputBuffer.AppendLine();

                foreach (var method in privateMethods)
                {
                    DumpMethodSignature(method);
                }

                WriteToFile("myplayer_methods");
            }
            catch (Exception ex)
            {
                outputBuffer.AppendLine($"ERROR: {ex.Message}");
                outputBuffer.AppendLine($"Stack trace: {ex.StackTrace}");
                WriteToFile("myplayer_methods_ERROR");
                MelonLogger.Error($"Failed to dump MyPlayer methods: {ex.Message}");
            }
        }

        private static void DumpMethodSignature(MethodInfo method)
        {
            try
            {
                // Build method signature
                string visibility = method.IsPublic ? "public" : method.IsPrivate ? "private" : "protected";
                string staticMod = method.IsStatic ? "static " : "";
                string returnType = GetFriendlyTypeName(method.ReturnType);
                string methodName = method.Name;

                // Get parameters
                var parameters = method.GetParameters();
                string paramList = string.Join(", ", parameters.Select(p =>
                    $"{GetFriendlyTypeName(p.ParameterType)} {p.Name}"
                ));

                outputBuffer.AppendLine($"{visibility} {staticMod}{returnType} {methodName}({paramList})");
            }
            catch (Exception ex)
            {
                outputBuffer.AppendLine($"[Error dumping method: {ex.Message}]");
            }
        }

        private static string GetFriendlyTypeName(Type type)
        {
            try
            {
                // Handle generic types
                if (type.IsGenericType)
                {
                    var genericTypeDef = type.GetGenericTypeDefinition();
                    var genericArgs = type.GetGenericArguments();
                    string genericName = genericTypeDef.Name.Split('`')[0];
                    string genericParams = string.Join(", ", genericArgs.Select(t => GetFriendlyTypeName(t)));
                    return $"{genericName}<{genericParams}>";
                }

                // Handle arrays
                if (type.IsArray)
                {
                    return $"{GetFriendlyTypeName(type.GetElementType())}[]";
                }

                // Simplify common types
                if (type.FullName != null)
                {
                    if (type.FullName.StartsWith("Il2Cpp"))
                    {
                        // Strip Il2Cpp prefix for readability
                        return type.FullName.Replace("Il2Cpp", "").Replace("Assets.Scripts.", "");
                    }
                    if (type.FullName.StartsWith("System."))
                    {
                        return type.Name;
                    }
                }

                return type.Name;
            }
            catch
            {
                return type.ToString();
            }
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

        // Dump all enum values from game assemblies
        public static void DumpEnums()
        {
            MelonLogger.Msg("Dumping all game enums...");

            outputBuffer.Clear();
            outputBuffer.AppendLine("=== GAME ENUM VALUES ===");
            outputBuffer.AppendLine($"Timestamp: {DateTime.Now}");
            outputBuffer.AppendLine();

            try
            {
                // EItem enum
                DumpEnumValues("Il2CppAssets.Scripts.Inventory__Items__Pickups.Items.EItem");
                outputBuffer.AppendLine();

                // EWeapon enum
                DumpEnumValues("Il2Cpp.EWeapon");
                outputBuffer.AppendLine();

                // ETome enum
                DumpEnumValues("Il2CppAssets.Scripts._Data.Tomes.ETome");
                outputBuffer.AppendLine();

                WriteToFile("enum_dump");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed to dump enums: {ex.Message}");
                MelonLogger.Error($"Stack trace: {ex.StackTrace}");
            }
        }

        private static void DumpEnumValues(string fullTypeName)
        {
            try
            {
                // Find the type in loaded assemblies
                Type enumType = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    enumType = assembly.GetType(fullTypeName);
                    if (enumType != null)
                        break;
                }

                if (enumType == null)
                {
                    outputBuffer.AppendLine($"Enum not found: {fullTypeName}");
                    return;
                }

                outputBuffer.AppendLine($"=== {enumType.Name} ===");
                outputBuffer.AppendLine($"Namespace: {enumType.Namespace}");
                outputBuffer.AppendLine($"Full name: {enumType.FullName}");
                outputBuffer.AppendLine();

                if (!enumType.IsEnum)
                {
                    outputBuffer.AppendLine($"Type is not an enum!");
                    return;
                }

                // Get all enum values
                var values = Enum.GetValues(enumType);
                var names = Enum.GetNames(enumType);

                outputBuffer.AppendLine($"Total values: {values.Length}");
                outputBuffer.AppendLine();

                for (int i = 0; i < names.Length; i++)
                {
                    var value = values.GetValue(i);
                    var intValue = Convert.ToInt32(value);
                    outputBuffer.AppendLine($"{names[i]} = {intValue}");
                }

                outputBuffer.AppendLine();
                outputBuffer.AppendLine("---");
            }
            catch (Exception ex)
            {
                outputBuffer.AppendLine($"Error dumping {fullTypeName}: {ex.Message}");
            }
        }
    }
}
