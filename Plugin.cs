using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PotionCraft.ManagersSystem.Application;
using PotionCraft.ManagersSystem.Debug;
using PotionCraft.SceneLoader;
using PotionCraft.Settings;

namespace EnableDev;

[BepInPlugin(HarmonyID, "EnableDev", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    public static ManualLogSource Log;

    private const string HarmonyID = "com.github.qe201020335.PotionCraftEnableDev";
    private readonly Harmony _harmony = new Harmony(HarmonyID);

    private static ConfigEntry<bool> EnableDevModeOnStart;
    private static ConfigEntry<bool> EnableCheatCodes;

    private void Awake()
    {
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Log = Logger;

        EnableDevModeOnStart = Config.Bind("General", "EnableDevModeOnStart", true,
            "Whether the game will start with developer mode enabled as default");

        EnableCheatCodes = Config.Bind("General", "EnableCheatCodes", true,
            "Enable cheat codes?");

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