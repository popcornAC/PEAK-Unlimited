// <copyright file="Plugin.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PEAKUnlimited;

using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PEAKUnlimited.Core;
using PEAKUnlimited.Core.Interfaces;
using PEAKUnlimited.Core.Services;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Zorro.Core;

// Here are some basic resources on code style and naming conventions to help
// you in your first CSharp plugin!
// https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
// https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names
// https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces

// This BepInAutoPlugin attribute comes from the Hamunii.BepInEx.AutoPlugin
// NuGet package, and it will generate the BepInPlugin attribute for you!
// For more info, see https://github.com/Hamunii/BepInEx.AutoPlugin
[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private Harmony harmony;
    private static FieldInfo oldPipField;

    public ConfigurationManager.PluginConfig pluginConfig;
    public GameStateManager gameStateManager;
    public INetworkService networkService;
    public IItemService itemService;
    public IUIManager uiManager;

    public static Plugin currentInstance;

    // Add these fields to store config entries
    private BepInEx.Configuration.ConfigEntry<int> configMaxPlayers;
    private BepInEx.Configuration.ConfigEntry<bool> configExtraMarshmallows;
    private BepInEx.Configuration.ConfigEntry<bool> configExtraBackpacks;
    private BepInEx.Configuration.ConfigEntry<bool> configLateMarshmallows;
    private BepInEx.Configuration.ConfigEntry<int> configCheatExtraMarshmallows;
    private BepInEx.Configuration.ConfigEntry<int> configCheatExtraBackpacks;

    private void Awake()
    {
        Logger = base.Logger;
        currentInstance = this;

        // Initialize configuration
        this.InitializeConfiguration();

        // Initialize services
        this.InitializeServices();

        // Apply initial configuration
        this.ApplyConfigurationFromUI();

        // Set up Harmony patches
        this.SetupHarmonyPatches();
    }

    private void InitializeServices()
    {
        this.gameStateManager = new GameStateManager();
        this.networkService = new PhotonNetworkService();
        this.itemService = new UnityItemService();
        this.uiManager = new UnityUIManager();

        // Set the configuration in the UI manager
        if (this.uiManager is UnityUIManager unityUIManager)
        {
            unityUIManager.SetConfiguration(this.pluginConfig, this.ApplyConfigurationFromUI);
            unityUIManager.SetGameStateManager(this.gameStateManager);
        }
    }

    private void InitializeConfiguration()
    {
        this.configMaxPlayers = this.Config.Bind(
            "General",
            "MaxPlayers",
            20,
            "The maximum number of players you want to be able to join your lobby (Including yourself). Warning: untested, higher numbers may be unstable! Range: 1-20");

        this.configExtraMarshmallows = this.Config.Bind(
            "General",
            "ExtraMarshmallows",
            true,
            "Controls whether additional marshmallows are spawned for the extra players");

        this.configExtraBackpacks = this.Config.Bind(
            "General",
            "ExtraBackpacks",
            true,
            "Controls whether additional backpacks have a chance to be spawned for extra players");

        this.configLateMarshmallows = this.Config.Bind(
            "General",
            "LateJoinMarshmallows",
            false,
            "Controls whether additional marshmallows are spawned for players who join late (mid run), and removed for those who leave early (Experimental + Untested)");

        this.configCheatExtraMarshmallows = this.Config.Bind(
            "General",
            "Cheat Marshmallows",
            0,
            "(Cheat, disabled by default) This will set the desired amount of marshmallows to the campfires as a cheat, requires ExtraMarshmallows to be enabled. Capped at 30.");

        this.configCheatExtraBackpacks = this.Config.Bind(
            "General",
            "Cheat Backpacks",
            0,
            "(Cheat, disabled by default) Sets how many backpacks will spawn as a cheat, requires ExtraBackpacks to also be enabled. Capped at 10.");

        this.pluginConfig = ConfigurationManager.CreateFromBepInExConfig(
            this.configMaxPlayers,
            this.configExtraMarshmallows,
            this.configExtraBackpacks,
            this.configLateMarshmallows,
            this.configCheatExtraMarshmallows,
            this.configCheatExtraBackpacks);
    }

    private void InitializeReflection()
    {
        oldPipField = typeof(EndScreen).GetField("oldPip", BindingFlags.NonPublic | BindingFlags.Instance);
        if (oldPipField == null)
        {
            Logger.LogWarning("Could not find oldPip field in EndScreen class - end screen UI may not display correctly for extra players.");
        }
    }

    private void ApplyConfiguration()
    {
        NetworkConnector.MAX_PLAYERS = this.pluginConfig.MaxPlayers;
        Logger.LogInfo($"Plugin {Id} set the Max Players to {NetworkConnector.MAX_PLAYERS}!");
    }

    /// <summary>
    /// Applies configuration changes from the UI.
    /// </summary>
    public void ApplyConfigurationFromUI()
    {
        // Update network connector with new max players
        NetworkConnector.MAX_PLAYERS = this.pluginConfig.MaxPlayers;

        // Update game state manager with new configuration
        if (this.gameStateManager != null)
        {
            this.gameStateManager.UpdateConfiguration(this.pluginConfig);
        }

        // Sync UI config to config file
        this.configMaxPlayers.Value = this.pluginConfig.MaxPlayers;
        this.configExtraMarshmallows.Value = this.pluginConfig.ExtraMarshmallows;
        this.configExtraBackpacks.Value = this.pluginConfig.ExtraBackpacks;
        this.configLateMarshmallows.Value = this.pluginConfig.LateJoinMarshmallows;
        this.configCheatExtraMarshmallows.Value = this.pluginConfig.CheatExtraMarshmallows;
        this.configCheatExtraBackpacks.Value = this.pluginConfig.CheatExtraBackpacks;
        this.Config.Save();
    }

    private void SetupHarmonyPatches()
    {
        this.harmony = new Harmony(Id);

        // Apply patches based on configuration
        if (this.pluginConfig.ExtraMarshmallows)
        {
            this.harmony.PatchAll(typeof(AwakePatch));
            this.harmony.PatchAll(typeof(OnPlayerLeftRoomPatch));
            this.harmony.PatchAll(typeof(OnPlayerEnteredRoomPatch));
        }

        // Always apply end screen patches
        this.harmony.PatchAll(typeof(EndSequenceRoutinePatch));
        this.harmony.PatchAll(typeof(WaitingForPlayersUIPatch));
        this.harmony.PatchAll(typeof(EndScreenStartPatch));
        this.harmony.PatchAll(typeof(EndScreenNextPatch));
    }

    private void Update()
    {
        // Update UI manager
        if (this.uiManager != null)
        {
            this.uiManager.Update();
        }

        // Check for F1 key to toggle configuration UI
        if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F1))
        {
            if (this.uiManager != null)
            {
                this.uiManager.ToggleConfigurationUI();
            }
        }

        // Check for ESC key to close configuration UI
        if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Escape))
        {
            if (this.uiManager != null && this.uiManager.IsUIVisible)
            {
                this.uiManager.HideConfigurationUI();
            }
        }
    }

    public static List<Vector3> GetEvenlySpacedPointsAroundCampfire(int numPoints, float innerRadius, float outerRadius, Vector3 campfirePosition, Segment advanceToSegment)
    {
        return GameLogic.GetEvenlySpacedPointsAroundCampfire(
            numPoints, innerRadius, outerRadius, campfirePosition, SetToGround);
    }

    private static List<GameObject> SpawnMarshmallows(int number, Vector3 campfirePosition, Segment advanceToSegment)
    {
        List<GameObject> marshmallows = new List<GameObject>();
        Item obj = SingletonAsset<ItemDatabase>.Instance.itemLookup[46];
        
        foreach (Vector3 position in GetEvenlySpacedPointsAroundCampfire(number, 2.5f, 3f, campfirePosition,
                     advanceToSegment))
        {
            marshmallows.Add(Add(obj, position).gameObject);
        }

        return marshmallows;
    }

    private static Vector3 SetToGround(Vector3 vector)
    {
        return HelperFunctions.GetGroundPos(vector, HelperFunctions.LayerType.TerrainMap);
    }

    private static Item Add(Item item, Vector3 position)
    {
        if (!PhotonNetwork.IsConnected)
        {
            return null;
        }

        return PhotonNetwork.Instantiate("0_Items/" + item.name, position, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f)).GetComponent<Item>();
    }

    public class AwakePatch
    {
        [HarmonyPatch(typeof(Campfire), "Awake")]
        [HarmonyPostfix]
        private static void Postfix(Campfire __instance)
        {
            if (currentInstance == null || currentInstance.pluginConfig == null)
            {
                return;
            }

            if (currentInstance.pluginConfig.ExtraBackpacks)
            {
                currentInstance.itemService.SpawnExtraBackpacks(__instance.transform.position, __instance.advanceToSegment);
            }

            if (currentInstance.pluginConfig.ExtraMarshmallows)
            {
                currentInstance.itemService.SpawnExtraMarshmallows(__instance.transform.position, __instance.advanceToSegment);
            }
        }
    }

    public class OnPlayerEnteredRoomPatch
    {
        [HarmonyPatch(typeof(PlayerConnectionLog), "OnPlayerEnteredRoom")]
        [HarmonyPostfix]
        private static void Postfix(PlayerConnectionLog __instance)
        {
            if (currentInstance == null || currentInstance.pluginConfig == null || !currentInstance.pluginConfig.LateJoinMarshmallows)
            {
                return;
            }

            if (currentInstance.networkService.IsMasterClient)
            {
                currentInstance.gameStateManager.AddPlayer();
                currentInstance.itemService.SpawnLateJoinMarshmallow();
            }
        }
    }

    public class OnPlayerLeftRoomPatch
    {
        [HarmonyPatch(typeof(PlayerConnectionLog), "OnPlayerLeftRoom")]
        [HarmonyPostfix]
        private static void Postfix(PlayerConnectionLog __instance)
        {
            if (currentInstance == null || currentInstance.pluginConfig == null || !currentInstance.pluginConfig.LateJoinMarshmallows)
            {
                return;
            }

            if (currentInstance.networkService.IsMasterClient)
            {
                currentInstance.gameStateManager.RemovePlayer();
                currentInstance.itemService.RemoveLateJoinMarshmallow();
            }
        }
    }

    public class WaitingForPlayersUIPatch
    {
        [HarmonyPatch(typeof(WaitingForPlayersUI), "Update")]
        [HarmonyPrefix]
        private static void Prefix(WaitingForPlayersUI __instance)
        {
            if (currentInstance == null)
            {
                return;
            }

            if (__instance.scoutImages.Length >= Character.AllCharacters.Count)
            {
                return;
            }

            var newScoutImages = new Image[Character.AllCharacters.Count];
            Image original = __instance.scoutImages[0];
            for (int i = 0; i < Character.AllCharacters.Count; i++)
            {
                if (i < currentInstance.gameStateManager.VanillaMaxPlayersValue)
                {
                    newScoutImages[i] = __instance.scoutImages[i];
                }
                else
                {
                    newScoutImages[i] = Instantiate(original, original.transform.parent);
                }
            }

            __instance.scoutImages = newScoutImages;
        }
    }

    public class EndScreenNextPatch
    {
        [HarmonyPatch(typeof(EndScreen), "Next")]
        [HarmonyPostfix]
        private static void Postfix(EndScreen __instance)
        {
            if (currentInstance == null)
            {
                return;
            }

            currentInstance.gameStateManager.DeleteEndScreen(__instance);
        }
    }

    public class EndScreenStartPatch
    {
        [HarmonyPatch(typeof(EndScreen), "Start")]
        [HarmonyPostfix]
        private static void Postfix(EndScreen __instance)
        {
            if (currentInstance == null)
            {
                return;
            }

            currentInstance.gameStateManager.DeactivateEndScreen(__instance);
        }
    }

    public class EndSequenceRoutinePatch
    {
        [HarmonyPatch(typeof(EndScreen), "EndSequenceRoutine")]
        [HarmonyPostfix]
        private static void Postfix(EndScreen __instance)
        {
            if (currentInstance == null)
            {
                return;
            }

            for (int i = 4; i < Character.AllCharacters.Count; i++)
            {
                Logger.LogInfo("Deactivating an end screen");

                // Don't display the extra names since it blocks the chart
                currentInstance.networkService.Destroy(__instance.scoutWindows[i].gameObject);
                Logger.LogInfo("Deleted an end screen");
            }
        }
    }
}
