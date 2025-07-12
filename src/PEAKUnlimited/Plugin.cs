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
    private readonly Harmony harmony = new Harmony(Id);
    private static FieldInfo oldPipField;

    private ConfigurationManager.PluginConfig pluginConfig;
    private GameStateManager gameStateManager;
    private INetworkService networkService;
    private IItemService itemService;

    private static Plugin currentInstance;

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {Id} is loaded!");

        currentInstance = this;

        InitializeServices();
        InitializeConfiguration();
        InitializeReflection();
        ApplyConfiguration();
        SetupHarmonyPatches();
    }

    private void InitializeServices()
    {
        gameStateManager = new GameStateManager();
        networkService = new PhotonNetworkService();
        itemService = new UnityItemService();
    }

    private void InitializeConfiguration()
    {
        var configMaxPlayers = this.Config.Bind(
            "General",
            "MaxPlayers",
            20,
            "The maximum number of players you want to be able to join your lobby (Including yourself). Warning: untested, higher numbers may be unstable! Range: 1-20");

        var configExtraMarshmallows = this.Config.Bind(
            "General",
            "ExtraMarshmallows",
            true,
            "Controls whether additional marshmallows are spawned for the extra players");

        var configExtraBackpacks = this.Config.Bind(
            "General",
            "ExtraBackpacks",
            true,
            "Controls whether additional backpacks have a chance to be spawned for extra players");

        var configLateMarshmallows = this.Config.Bind(
            "General",
            "LateJoinMarshmallows",
            false,
            "Controls whether additional marshmallows are spawned for players who join late (mid run), and removed for those who leave early (Experimental + Untested)");

        var configCheatExtraMarshmallows = this.Config.Bind(
            "General",
            "Cheat Marshmallows",
            0,
            "(Cheat, disabled by default) This will set the desired amount of marshmallows to the campfires as a cheat, requires ExtraMarshmallows to be enabled. Capped at 30.");

        var configCheatExtraBackpacks = this.Config.Bind(
            "General",
            "Cheat Backpacks",
            0,
            "(Cheat, disabled by default) Sets how many backpacks will spawn as a cheat, requires ExtraBackpacks to also be enabled. Capped at 10.");

        pluginConfig = ConfigurationManager.CreateFromBepInExConfig(
            configMaxPlayers,
            configExtraMarshmallows,
            configExtraBackpacks,
            configLateMarshmallows,
            configCheatExtraMarshmallows,
            configCheatExtraBackpacks);
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
        NetworkConnector.MAX_PLAYERS = pluginConfig.MaxPlayers;
        Logger.LogInfo($"Plugin {Id} set the Max Players to {NetworkConnector.MAX_PLAYERS}!");
    }

    private void SetupHarmonyPatches()
    {
        Logger.LogInfo($"Plugin {Id} is patching!");
        
        if (pluginConfig.ExtraMarshmallows)
        {
            Logger.LogInfo($"Plugin {Id} extra marshmallows are enabled!");
            this.harmony.PatchAll(typeof(AwakePatch));
            Logger.LogInfo($"Plugin {Id} late marshmallows are enabled!");
            Logger.LogInfo($"Plugin {Id} left patch enabled!");
            this.harmony.PatchAll(typeof(OnPlayerLeftRoomPatch));
            Logger.LogInfo($"Plugin {Id} joined patch enabled!");
            this.harmony.PatchAll(typeof(OnPlayerEnteredRoomPatch));
        }

        Logger.LogInfo($"Plugin {Id} patching end screen!");
        this.harmony.PatchAll(typeof(EndSequenceRoutinePatch));
        this.harmony.PatchAll(typeof(WaitingForPlayersUIPatch));
        this.harmony.PatchAll(typeof(EndScreenStartPatch));
        this.harmony.PatchAll(typeof(EndScreenNextPatch));
        Logger.LogInfo($"Plugin {Id} has patched!");
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
        Logger.LogInfo((object)("Plugin PeakUnlimited " + obj.GetName()));
        obj.GetName();
        foreach (Vector3 position in GetEvenlySpacedPointsAroundCampfire(number, 2.5f, 3f, campfirePosition,
                     advanceToSegment))
        {
            marshmallows.Add(Add(obj, position).gameObject);
        }

        Logger.LogInfo((object)("Plugin PeakUnlimited added with position: " + obj.GetName()));
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

        Logger.LogInfo((object)string.Format("Spawn item: {0} at {1}", (object)item, (object)position));
        return PhotonNetwork.Instantiate("0_Items/" + item.name, position, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f)).GetComponent<Item>();
    }

    public class AwakePatch
    {
        [HarmonyPatch(typeof(Campfire), "Awake")]
        [HarmonyPostfix]
        private static void Postfix(Campfire instance)
        {
            if (currentInstance == null) return;

            if (!currentInstance.networkService.IsMasterClient)
            {
                return;
            }

            // Backpack addition
            if (currentInstance.pluginConfig.ExtraBackpacks)
            {
                Logger.LogInfo("Backpackification enabled and starting!");
                Item obj = currentInstance.itemService.GetItem(6);
                int number = GameLogic.CalculateExtraBackpacks(currentInstance.gameStateManager.NumberOfPlayers, currentInstance.gameStateManager.VanillaMaxPlayersValue);

                if (currentInstance.pluginConfig.CheatExtraBackpacks > 0 && currentInstance.pluginConfig.CheatExtraBackpacks <= 10)
                {
                    number = currentInstance.pluginConfig.CheatExtraBackpacks - 1; // Minus one as there is already a backpack present
                }

                if (number > 0)
                {
                    foreach (Vector3 position in GetEvenlySpacedPointsAroundCampfire(number, 3.3f, 3.7f,
                                 instance.gameObject.transform.position,
                                 instance.advanceToSegment))
                    {
                        Vector3 finalPosition = position;
                        if (instance.gameObject.transform.parent.gameObject.name.ToLower().Contains("wings"))
                        {
                            finalPosition =
                                position + new Vector3(0, 10f, 0f); // stops backpacks on the beach spawning underground...
                        }

                        Add(obj, finalPosition).transform.parent = instance.gameObject.transform;
                    }
                }
                else
                {
                    Logger.LogInfo("Not enough players to add additional backpacks, use the Cheat Backpack configuration setting if you want to override this!");
                }
            }

            // End of backpack addition

            // Marshmallow addition
            if (instance.gameObject.transform.parent.gameObject.name.ToLower().Contains("wings"))
            {
                return;
            }

            currentInstance.gameStateManager.AddCampfire(instance);
            Logger.LogInfo("Marshmellowifying campfire...!");
            int amountOfMarshmallowsToSpawn = currentInstance.gameStateManager.GetExtraPlayersCount();
            if (currentInstance.pluginConfig.CheatExtraMarshmallows != 0)
            {
                Logger.LogInfo("Adding cheatmellows!");
                amountOfMarshmallowsToSpawn = currentInstance.pluginConfig.CheatExtraMarshmallows - currentInstance.gameStateManager.VanillaMaxPlayersValue;
                if (currentInstance.gameStateManager.NumberOfPlayers < currentInstance.gameStateManager.VanillaMaxPlayersValue)
                {
                    amountOfMarshmallowsToSpawn = currentInstance.pluginConfig.CheatExtraMarshmallows - currentInstance.gameStateManager.NumberOfPlayers;
                }
            }

            Logger.LogInfo("Start of campfire patch!");
            if (currentInstance.networkService.IsMasterClient && (currentInstance.gameStateManager.HasExtraPlayers() || currentInstance.pluginConfig.CheatExtraMarshmallows != 0))
            {
                Logger.LogInfo("More than 4 players, preparing to marshmallowify! Number: " + currentInstance.gameStateManager.NumberOfPlayers);
                Vector3 position = instance.gameObject.transform.position;
                var marshmallowObjects = SpawnMarshmallows(amountOfMarshmallowsToSpawn, position, instance.advanceToSegment);
                currentInstance.gameStateManager.AddMarshmallowsToCampfire(instance, marshmallowObjects);
            }
            else
            {
                Logger.LogInfo("Not enough players for extra marshmallows, use the extra marshmallows cheat configuration option to override this!");
            }

            currentInstance.gameStateManager.SetAfterAwake();
        }
    }

    public class OnPlayerEnteredRoomPatch
    {
        [HarmonyPatch(typeof(PlayerConnectionLog), "OnPlayerEnteredRoom")]
        [HarmonyPostfix]
        private static void Postfix(PlayerConnectionLog instance)
        {
            if (currentInstance == null) return;

            currentInstance.gameStateManager.PlayerJoined();
            Logger.LogInfo("Someone has joined the room! Number: " + currentInstance.gameStateManager.NumberOfPlayers + "/" + NetworkConnector.MAX_PLAYERS);

            // Add a marshmallow at each campfire for the new player
            if (!currentInstance.pluginConfig.LateJoinMarshmallows)
            {
                return;
            }

            if (currentInstance.gameStateManager.IsAfterAwake && 
                currentInstance.networkService.IsMasterClient && 
                currentInstance.gameStateManager.HasExtraPlayers() && 
                currentInstance.pluginConfig.CheatExtraMarshmallows == 0)
            {
                foreach (Campfire campfire in currentInstance.gameStateManager.CampfireList)
                {
                    Vector3 position = campfire.gameObject.transform.position;
                    Logger.LogInfo("Spawning a marshmallow!");
                    var marshmallowList = currentInstance.gameStateManager.Marshmallows[campfire];
                    marshmallowList.Add(SpawnMarshmallows(1, position, campfire.advanceToSegment)[0]);
                }
            }
        }
    }

    public class OnPlayerLeftRoomPatch
    {
        [HarmonyPatch(typeof(PlayerConnectionLog), "OnPlayerLeftRoom")]
        [HarmonyPostfix]
        private static void Postfix(PlayerConnectionLog instance)
        {
            if (currentInstance == null) return;

            currentInstance.gameStateManager.PlayerLeft();
            Logger.LogInfo("Someone has left the room! Number: " + currentInstance.gameStateManager.NumberOfPlayers + "/" + NetworkConnector.MAX_PLAYERS);
            
            if (!currentInstance.pluginConfig.LateJoinMarshmallows)
            {
                return;
            }

            if (currentInstance.gameStateManager.IsAfterAwake && 
                currentInstance.networkService.IsMasterClient && 
                currentInstance.gameStateManager.NumberOfPlayers >= currentInstance.gameStateManager.VanillaMaxPlayersValue && 
                currentInstance.pluginConfig.CheatExtraMarshmallows == 0)
            {
                Logger.LogInfo("Removing a marshmallow!");
                foreach (Campfire campfire in currentInstance.gameStateManager.CampfireList)
                {
                    var marshmallowList = currentInstance.gameStateManager.Marshmallows[campfire];
                    Logger.LogInfo("Removing a marshmallow! " + marshmallowList.Count);
                    Logger.LogInfo("Removing a marshmallow! " + marshmallowList[0].gameObject.name);
                    currentInstance.networkService.Destroy(marshmallowList[0]);
                    marshmallowList.RemoveAt(0);
                    Logger.LogInfo("Removing a marshmallow! " + marshmallowList.Count);
                    if (marshmallowList.Count > 0)
                    {
                        Logger.LogInfo("Removing a marshmallow! " + marshmallowList[0].gameObject.name);
                    }
                }
            }
        }
    }

    public class WaitingForPlayersUIPatch
    {
        [HarmonyPatch(typeof(WaitingForPlayersUI), "Update")]
        [HarmonyPrefix]
        private static void Prefix(WaitingForPlayersUI instance)
        {
            if (currentInstance == null) return;

            if (instance.scoutImages.Length >= Character.AllCharacters.Count)
            {
                return;
            }

            var newScoutImages = new Image[Character.AllCharacters.Count];
            Image original = instance.scoutImages[0];
            for (int i = 0; i < Character.AllCharacters.Count; i++)
            {
                if (i < currentInstance.gameStateManager.VanillaMaxPlayersValue)
                {
                    newScoutImages[i] = instance.scoutImages[i];
                }
                else
                {
                    newScoutImages[i] = Instantiate(original, original.transform.parent);
                }
            }

            instance.scoutImages = newScoutImages;
        }
    }

    public class EndScreenNextPatch
    {
        [HarmonyPatch(typeof(EndScreen), "Next")]
        [HarmonyPostfix]
        private static void PostFix()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Singleton<PeakHandler>.Instance.EndScreenComplete();
            }
        }
    }

    public class EndScreenStartPatch
    {
        [HarmonyPatch(typeof(EndScreen), "Start")]
        [HarmonyPrefix]
        private static void Prefix(EndScreen instance)
        {
            if (currentInstance == null) return;

            if (Character.AllCharacters.Count <= currentInstance.gameStateManager.VanillaMaxPlayersValue)
            {
                return;
            }

            var newScoutWindows = new EndScreenScoutWindow[Character.AllCharacters.Count];
            var newScouts = new Image[Character.AllCharacters.Count];
            var newScoutsAtPeak = new Image[Character.AllCharacters.Count];
            var newOldPip = new Image[Character.AllCharacters.Count];
            var newScoutLines = new Transform[Character.AllCharacters.Count];

            // Get the oldPip array once to avoid repeated reflection calls
            var oldPipArray = oldPipField?.GetValue(instance) as Image[];

            for (int i = 0; i < Character.AllCharacters.Count; i++)
            {
                // Don't do anything to the original ones
                bool withinExisting = i < instance.scouts.Length;
                if (!withinExisting)
                {
                    if ((UnityEngine.Object)instance.scoutWindows[0] == null)
                    {
                        newScoutWindows[i] = null;
                    }
                    else
                    {
                        newScoutWindows[i] = Instantiate(
                            instance.scoutWindows[0],
                            instance.scoutWindows[0].transform.parent);
                    }

                    if ((UnityEngine.Object)instance.scouts[0] == null)
                    {
                        newScouts[i] = null;
                    }
                    else
                    {
                        newScouts[i] = Instantiate(
                            instance.scouts[0],
                            instance.scouts[0].transform.parent);
                    }

                    if ((UnityEngine.Object)instance.scoutsAtPeak[0] == null)
                    {
                        newScoutsAtPeak[i] = null;
                    }
                    else
                    {
                        newScoutsAtPeak[i] = Instantiate(
                            instance.scoutsAtPeak[0],
                            instance.scoutsAtPeak[0].transform.parent);
                    }

                    if (oldPipArray == null || oldPipArray.Length == 0 || (UnityEngine.Object)oldPipArray[0] == null)
                    {
                        newOldPip[i] = null;
                    }
                    else
                    {
                        newOldPip[i] = Instantiate(
                            oldPipArray[0],
                            oldPipArray[0].transform.parent);
                    }

                    if ((UnityEngine.Object)instance.scoutLines[0] == null)
                    {
                        newScoutLines[i] = null;
                    }
                    else
                    {
                        newScoutLines[i] = Instantiate(
                            instance.scoutLines[0],
                            instance.scoutLines[0].transform.parent);
                    }
                }
                else
                {
                    newScoutWindows[i] = instance.scoutWindows[i];
                    newScouts[i] = instance.scouts[i];
                    newScoutsAtPeak[i] = instance.scoutsAtPeak[i];
                    newOldPip[i] = oldPipArray != null && i < oldPipArray.Length ? oldPipArray[i] : null;
                    newScoutLines[i] = instance.scoutLines[i];
                }
            }

            // Reassign arrays with new ones
            instance.scoutWindows = newScoutWindows;
            instance.scouts = newScouts;
            instance.scoutsAtPeak = newScoutsAtPeak;
            oldPipField?.SetValue(instance, newOldPip);
            instance.scoutLines = newScoutLines;
        }
    }

    public class EndSequenceRoutinePatch
    {
        [HarmonyPatch(typeof(EndScreen), "EndSequenceRoutine")]
        [HarmonyPostfix]
        private static void Postfix(EndScreen instance)
        {
            if (currentInstance == null) return;

            for (int i = 4; i < Character.AllCharacters.Count; i++)
            {
                Logger.LogInfo("Deactivating an end screen");

                // Don't display the extra names since it blocks the chart
                currentInstance.networkService.Destroy(instance.scoutWindows[i].gameObject);
                Logger.LogInfo("Deleted an end screen");
            }
        }
    }
}
