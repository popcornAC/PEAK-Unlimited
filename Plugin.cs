using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using Photon.Pun;
using Steamworks;
using Photon.Realtime;
using UnityEngine;
using Zorro.Core;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace PeakUnlimited;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private static int NEW_MAX_PLAYERS;
    private static ConfigEntry<int> configMaxPlayers;
    private static ConfigEntry<bool> configExtraMarshmallows;
    private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        configMaxPlayers = Config.Bind
        (
            "General",
            "MaxPlayers",
            20,
            "The maximum number of players you want to be able to join your lobby (Including yourself). Warning: untested, higher numbers may be unstable! Range: 1-20"
        );
        
        configExtraMarshmallows = Config.Bind
        (
            "General",
            "ExtraMarshmallows",
            true,
            "Controls whether additional marshmallows are spawned for the extra players (Experimental)"
        );

        NEW_MAX_PLAYERS = configMaxPlayers.Value;

        if (NEW_MAX_PLAYERS == 0)
        {
            NEW_MAX_PLAYERS = 1;
        }
        else if (NEW_MAX_PLAYERS > 30)
        {
            NEW_MAX_PLAYERS = 30;
        }

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} set the Max Players to " + NEW_MAX_PLAYERS + "!");
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is patching!");
        harmony.PatchAll(typeof(HostRoomOptionsPatch));
        harmony.PatchAll(typeof(PlayClickedPatch));
        if (configExtraMarshmallows.Value) {
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} extra marshmallows are enabled!");
            harmony.PatchAll(typeof(AwakePatch));
        }
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} has patched!");
    }

    private static List<Vector3> GetEvenlySpacedPointsAroundCampfire(int numPoints, float innerRadius, float outerRadius, Vector3 campfirePosition)
    {
        List<Vector3> points = new List<Vector3>();
    
        for (int i = 0; i < numPoints; i++)
        {
            float radius = outerRadius;
            if (i % 2 == 0)
            {
                radius = innerRadius;
            }
            
            float angle = i * Mathf.PI * 2f / numPoints; // Even spacing: 2π / n
            float x = radius * Mathf.Cos(angle);
            float z = radius * Mathf.Sin(angle);
            points.Add(setToGround(new Vector3(x, 0f, z) + campfirePosition));
        }

        return points;
    }

    private static Vector3 setToGround(Vector3 vector)
    {
        return HelperFunctions.GetGroundPos(vector, HelperFunctions.LayerType.Terrain);
    }

    private void spawnMarshmallows(int number)
    {
        Vector3 campfirePosition = GameObject.Find("Campfire").transform.position;
        ItemDatabase itemDatabase = SingletonAsset<ItemDatabase>.Instance;
        Item marshmallowItem = itemDatabase.itemLookup[46];
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} " + marshmallowItem.GetName());
        marshmallowItem.GetName();
        List<Vector3> marshmallowPositions = GetEvenlySpacedPointsAroundCampfire(number, 2.5f, 3, campfirePosition);
        foreach (Vector3 marshmallowPosition in marshmallowPositions)
        {
            Add(marshmallowItem, marshmallowPosition);
        }
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} added with position: " + marshmallowItem.GetName());
    }
    
    private static void spawnMarshmallows(int number, Vector3 campfirePosition)
    {
        ItemDatabase itemDatabase = SingletonAsset<ItemDatabase>.Instance;
        Item marshmallowItem = itemDatabase.itemLookup[46];
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} " + marshmallowItem.GetName());
        marshmallowItem.GetName();
        List<Vector3> marshmallowPositions = GetEvenlySpacedPointsAroundCampfire(number, 2.5f, 3, campfirePosition);
        foreach (Vector3 marshmallowPosition in marshmallowPositions)
        {
            Add(marshmallowItem, marshmallowPosition);
        }
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} added with position: " + marshmallowItem.GetName());
    }

    private static List<Vector3> getCampfirePositions()
    {
        List<Vector3> positions = new List<Vector3>();
        Campfire[] campfires = Resources.FindObjectsOfTypeAll<Campfire>()
            .Where(c => c.gameObject.scene.IsValid())
            .ToArray();
        foreach (Campfire campfire in campfires)
        {
            Logger.LogInfo("Found campfire!");
            positions.Add(campfire.gameObject.transform.position); 
        }
        return positions;
    }
    
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Y))
        {
            spawnMarshmallows(20);
        }
        if (Input.GetKeyUp(KeyCode.U))
        {
            foreach (Vector3 campfirePosition in getCampfirePositions())
            {
                Logger.LogInfo("Spawning campfire mms!");
                spawnMarshmallows(20, campfirePosition);
            }
        }
    }
    
    private static void Add(Item item, Vector3 position)
    {
        if (!PhotonNetwork.IsConnected)
            return;
        Logger.LogInfo((object) string.Format("Spawn item: {0} at {1}", (object) item, (object) position));
        PhotonNetwork.Instantiate("0_Items/" + item.name, position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)).GetComponent<Item>();
    }

    public class HostRoomOptionsPatch 
    {
        [HarmonyPatch(typeof(NetworkConnector), "HostRoomOptions")]
        [HarmonyPostfix]
        static void Postfix(ref RoomOptions __result)
        {
            Logger.LogInfo("Start of host room options patch!");
            __result = new RoomOptions()
            {
                IsVisible = false,
                MaxPlayers = NEW_MAX_PLAYERS
            };
        }
    }
    
    public class PlayClickedPatch 
    {
        [HarmonyPatch(typeof(MainMenuMainPage), "PlayClicked")]
        [HarmonyPrefix]
        static bool Prefix()
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, NEW_MAX_PLAYERS);
            return false;
        }
    }
    
    public class AwakePatch
    {
        [HarmonyPatch(typeof(Campfire), "Awake")]
        [HarmonyPostfix]
        static void Postfix(Campfire __instance)
        {
            Logger.LogInfo("Start of campfire patch!");
            if (PlayerHandler.GetAllPlayers().Count < 5) { return; }
            
            if (!__instance.gameObject.transform.parent.gameObject.name.ToLower().Contains("wings"))
            {
                Vector3 campfirePosition = __instance.gameObject.transform.position;
                //mmmm magic numbers
                spawnMarshmallows(PlayerHandler.GetAllPlayers().Count+1-4+20, campfirePosition);
            }
        }
    }
}
