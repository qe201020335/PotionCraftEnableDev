using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PotionCraft.Assemblies.AchievementsSystem;
using PotionCraft.ManagersSystem.Application;
using PotionCraft.ManagersSystem.Debug;
using PotionCraft.SceneLoader;
using PotionCraft.Settings;

namespace EnableDev;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static ManualLogSource Log;

    private const string HarmonyID = "com.github.qe201020335.PotionCraftEnableDev";
    private readonly Harmony _harmony = new Harmony(HarmonyID);

    private static ConfigEntry<bool> EnableDevModeOnStart;
    private static ConfigEntry<bool> EnableCheatCodes;
    internal static ConfigEntry<bool> EnableAchievementsOnDevMode { get; private set; }

    private void Awake()
    {
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Log = Logger;

        EnableDevModeOnStart = Config.Bind("General", "EnableDevModeOnStart", true,
            "Whether the game will start with developer mode enabled as default");

        EnableCheatCodes = Config.Bind("General", "EnableCheatCodes", true,
            "Enable cheat codes?");
        
        EnableAchievementsOnDevMode = Config.Bind("General", "EnableAchievementsOnDevMode", true,
            "Enable achievements when developer mode is active?");

        _harmony.PatchAll();
    }

    internal static void ModifyBuildConfig()
    {
        Log.LogInfo("Modifying game build settings!");
        if (EnableDevModeOnStart.Value)
        {
            Log.LogDebug("Enable dev mode on start");
            Settings<ApplicationManagerSettings>.Asset.developerModeOnStartInBuild = true;
        }

        if (EnableCheatCodes.Value)
        {
            Log.LogDebug("Enable cheat codes");
            Settings<DebugManagerSettings>.Asset.cheatCodesDebug = true;
        }
    }
}

[HarmonyPatch(typeof(LoadingQueue), "Add")]
public static class LoadingQueuePatch
{
    [HarmonyPrefix]
    public static void Patch(string name, Action action)
    {
        // Plugin.Log.LogDebug($"{name}");
        if (name == "InitializeDeveloperMode")
        {
            Plugin.ModifyBuildConfig();
        }
    }
}

[HarmonyPatch]
public static class AchievementsManagerPatch
{
    [HarmonyTargetMethods]
    public static IEnumerable<MethodBase> Methods()
    {
        return typeof(AchievementsManager)
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .Where(m => m.Name == nameof(AchievementsManager.UnlockAchievement))
            .Append(AccessTools.Method(typeof(AchievementsManager), nameof(AchievementsManager.RetrounlockAchievement)));
    }
    
    [HarmonyPrefix]
    public static void Patch(ref bool isDeveloperMode)
    {
        if (Plugin.EnableAchievementsOnDevMode.Value)
        {
            isDeveloperMode = false;
        }
    }
}