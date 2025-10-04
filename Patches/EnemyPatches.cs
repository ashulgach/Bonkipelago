using HarmonyLib;
using MelonLoader;
using Il2CppAssets.Scripts.Actors.Enemies;

namespace Bonkipelago.Patches
{
    [HarmonyPatch(typeof(Enemy), "EnemyDied", new System.Type[] { })]
    public class EnemyDeathPatch
    {
        static void Postfix(Enemy __instance)
        {
            try
            {
                // Check if this is a stage boss
                if (__instance.IsStageBoss())
                {
                    MelonLogger.Msg("===== STAGE BOSS DEFEATED =====");
                    MelonLogger.Msg("Completing Archipelago goal...");

                    if (ArchipelagoManager.Instance.IsConnected)
                    {
                        ArchipelagoManager.Instance.CompleteGoal();
                        MelonLogger.Msg("Goal sent to Archipelago!");
                    }
                    else
                    {
                        MelonLogger.Warning("Not connected to Archipelago - goal completion skipped");
                    }

                    MelonLogger.Msg("===============================");
                }

                // Optional: Also log final boss defeats
                if (__instance.IsFinalBoss())
                {
                    MelonLogger.Msg("===== FINAL BOSS DEFEATED =====");
                    // Could make this configurable later
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in EnemyDeathPatch: {ex.Message}");
                MelonLogger.Error($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
