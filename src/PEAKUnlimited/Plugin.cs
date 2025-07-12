namespace PEAKUnlimited;

using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
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
    private static int newMaxPlayers;
    private static int cheatExtraMarshmallows;
    private static int cheatExtraBackpacks;
    private static bool extraMarshmallows;
    private static ConfigEntry<int> configMaxPlayers;
    private static ConfigEntry<int> configCheatExtraMarshmallows;
    private static ConfigEntry<int> configCheatExtraBackpacks;
    private static ConfigEntry<bool> configExtraBackpacks;
    private static ConfigEntry<bool> configExtraMarshmallows;
    private static ConfigEntry<bool> configLateMarshmallows;
    private static int numberOfPlayers = 1;
    private readonly Harmony harmony = new Harmony(Id);
    private static List<Campfire> campfireList = new List<Campfire>();
    private static bool isAfterAwake = false;
    private static int vanillaMaxPlayers = 4;
    private static Dictionary<Campfire, List<GameObject>> marshmallows = new Dictionary<Campfire, List<GameObject>>();
    private static FieldInfo oldPipField;

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {Id} is loaded!");

        // Initialize reflection field for accessing private oldPip field
        oldPipField = typeof(EndScreen).GetField("oldPip", BindingFlags.NonPublic | BindingFlags.Instance);
        if (oldPipField == null)
        {
            Logger.LogWarning("Could not find oldPip field in EndScreen class - end screen UI may not display correctly for extra players.");
        }

        configMaxPlayers = this.Config.Bind(
            "General",
            "MaxPlayers",
            20,
            "The maximum number of players you want to be able to join your lobby (Including yourself). Warning: untested, higher numbers may be unstable! Range: 1-20");
        newMaxPlayers = configMaxPlayers.Value;

        configExtraMarshmallows = this.Config.Bind(
            "General",
            "ExtraMarshmallows",
            true,
            "Controls whether additional marshmallows are spawned for the extra players");
        extraMarshmallows = configExtraMarshmallows.Value;

        configExtraBackpacks = this.Config.Bind(
            "General",
            "ExtraBackpacks",
            true,
            "Controls whether additional backpacks have a chance to be spawned for extra players");

        configLateMarshmallows = this.Config.Bind(
            "General",
            "LateJoinMarshmallows",
            false,
            "Controls whether additional marshmallows are spawned for players who join late (mid run), and removed for those who leave early (Experimental + Untested)");

        configCheatExtraMarshmallows = this.Config.Bind(
            "General",
            "Cheat Marshmallows",
            0,
            "(Cheat, disabled by default) This will set the desired amount of marshmallows to the campfires as a cheat, requires ExtraMarshmallows to be enabled. Capped at 30.");
        cheatExtraMarshmallows = configCheatExtraMarshmallows.Value;
        if (cheatExtraMarshmallows > 30)
        {
            cheatExtraMarshmallows = 30;
        }

        configCheatExtraBackpacks = this.Config.Bind(
            "General",
            "Cheat Backpacks",
            0,
            "(Cheat, disabled by default) Sets how many backpacks will spawn as a cheat, requires ExtraBackpacks to also be enabled. Capped at 10.");
        cheatExtraBackpacks = configCheatExtraBackpacks.Value;

        if (cheatExtraBackpacks > 10)
        {
            cheatExtraBackpacks = 10;
        }

        if (newMaxPlayers == 0)
        {
            newMaxPlayers = 1;
        }
        else if (newMaxPlayers > 30)
        {
            newMaxPlayers = 30;
        }

        NetworkConnector.MAX_PLAYERS = newMaxPlayers;
        Logger.LogInfo($"Plugin {Id} set the Max Players to " + NetworkConnector.MAX_PLAYERS + "!");
        Logger.LogInfo($"Plugin {Id} is patching!");
        if (extraMarshmallows)
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

            points.Add(SetToGround(new Vector3(x, 0f, z) + campfirePosition));
        }

        return points;
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

    // Test helper methods
    public static bool ValidatePlayerCount(int playerCount, int vanillaMaxPlayers)
    {
        return playerCount > 0 && playerCount <= 30;
    }

    public static bool ValidateMaxPlayers(int maxPlayers)
    {
        return maxPlayers > 0 && maxPlayers <= 30;
    }

    public static bool ValidateCheatMarshmallows(int count)
    {
        return count >= 0 && count <= 30;
    }

    public static bool ValidateCheatBackpacks(int count)
    {
        return count >= 0 && count <= 10;
    }

    public static T[] ExpandArrayForExtraPlayers<T>(T[] originalArray, int newCount)
    {
        var newArray = new T[newCount];
        for (int i = 0; i < originalArray.Length && i < newCount; i++)
        {
            newArray[i] = originalArray[i];
        }

        return newArray;
    }

    public static int CalculateExtraMarshmallows(int currentPlayers, int vanillaMaxPlayers, int cheatMarshmallows)
    {
        if (cheatMarshmallows > 0)
        {
            return cheatMarshmallows - vanillaMaxPlayers;
        }

        return Math.Max(0, currentPlayers - vanillaMaxPlayers);
    }

    public static int CalculateExtraBackpacks(int currentPlayers, int vanillaMaxPlayers)
    {
        int extraPlayers = currentPlayers - vanillaMaxPlayers;
        if (extraPlayers <= 0)
        {
            return 0;
        }

        double backpackChance = extraPlayers * 0.25;
        return (int)backpackChance;
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
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            // Backpack addition
            if (configExtraBackpacks.Value)
            {
                Logger.LogInfo("Backpackification enabled and starting!");
                Item obj = SingletonAsset<ItemDatabase>.Instance.itemLookup[6];
                int numberOfExtraPlayers = numberOfPlayers - vanillaMaxPlayers;
                int number = 0;
                if (numberOfExtraPlayers > 0)
                {
                    double backpackNumber = numberOfExtraPlayers * 0.25;

                    if (backpackNumber % 4 == 0)
                    {
                        number = (int)backpackNumber;
                    }
                    else
                    {
                        number = (int)backpackNumber;
                        if (UnityEngine.Random.Range(0f, 1f) <= backpackNumber - number)
                        {
                            number++;
                        }
                    }
                }

                if (cheatExtraBackpacks > 0 && cheatExtraBackpacks <= 10)
                {
                    number = cheatExtraBackpacks - 1; // Minus one as there is already a backpack present
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

            campfireList.Add(instance);
            Logger.LogInfo("Marshmellowifying campfire...!");
            int amountOfMarshmallowsToSpawn = numberOfPlayers - vanillaMaxPlayers;
            if (cheatExtraMarshmallows != 0)
            {
                Logger.LogInfo("Adding cheatmellows!");
                amountOfMarshmallowsToSpawn = cheatExtraMarshmallows - vanillaMaxPlayers;
                if (numberOfPlayers < vanillaMaxPlayers)
                {
                    amountOfMarshmallowsToSpawn = cheatExtraMarshmallows - numberOfPlayers;
                }
            }

            Plugin.Logger.LogInfo("Start of campfire patch!");
            if (PhotonNetwork.IsMasterClient && (numberOfPlayers > vanillaMaxPlayers || cheatExtraMarshmallows != 0))
            {
                Logger.LogInfo("More than 4 players, preparing to marshmallowify! Number: " + numberOfPlayers);
                Vector3 position = instance.gameObject.transform.position;
                marshmallows.Add(instance, SpawnMarshmallows(amountOfMarshmallowsToSpawn, position, instance.advanceToSegment));
            }
            else
            {
                Logger.LogInfo("Not enough players for extra marshmallows, use the extra marshmallows cheat configuration option to override this!");
            }

            isAfterAwake = true;
        }
    }

    public class OnPlayerEnteredRoomPatch
    {
        [HarmonyPatch(typeof(PlayerConnectionLog), "OnPlayerEnteredRoom")]
        [HarmonyPostfix]
        private static void Postfix(PlayerConnectionLog instance)
        {
            numberOfPlayers++;
            Logger.LogInfo("Someone has joined the room! Number: " + numberOfPlayers + "/" + NetworkConnector.MAX_PLAYERS);

            // Add a marshmallow at each campfire for the new player
            if (!configLateMarshmallows.Value)
            {
                return;
            }

            if (isAfterAwake && PhotonNetwork.IsMasterClient && numberOfPlayers > vanillaMaxPlayers && cheatExtraMarshmallows == 0)
            {
                foreach (Campfire campfire in campfireList)
                {
                    Vector3 position = campfire.gameObject.transform.position;
                    Logger.LogInfo("Spawning a marshmallow!");
                    marshmallows[campfire].Add(SpawnMarshmallows(1, position, campfire.advanceToSegment)[0]);
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
            numberOfPlayers--;
            if (numberOfPlayers < 0)
            {
                numberOfPlayers = 0;
            }

            Logger.LogInfo("Someone has left the room! Number: " + numberOfPlayers + "/" + NetworkConnector.MAX_PLAYERS);
            if (!configLateMarshmallows.Value)
            {
                return;
            }

            if (isAfterAwake && PhotonNetwork.IsMasterClient && numberOfPlayers >= vanillaMaxPlayers && cheatExtraMarshmallows == 0)
            {
                Logger.LogInfo("Removing a marshmallow!");
                foreach (Campfire campfire in campfireList)
                {
                    Logger.LogInfo("Removing a marshmallow! " + marshmallows[campfire].Count);
                    Logger.LogInfo("Removing a marshmallow! " + marshmallows[campfire][0].gameObject.name);
                    Destroy(marshmallows[campfire][0]);
                    marshmallows[campfire].RemoveAt(0);
                    Logger.LogInfo("Removing a marshmallow! " + marshmallows[campfire].Count);
                    Logger.LogInfo("Removing a marshmallow! " + marshmallows[campfire][0].gameObject.name);
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
            if (instance.scoutImages.Length >= Character.AllCharacters.Count)
            {
                return;
            }

            var newScoutImages = new Image[Character.AllCharacters.Count];
            Image original = instance.scoutImages[0];
            for (int i = 0; i < Character.AllCharacters.Count; i++)
            {
                if (i < vanillaMaxPlayers)
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
            if (Character.AllCharacters.Count <= vanillaMaxPlayers)
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
            for (int i = 4; i < Character.AllCharacters.Count; i++)
            {
                Logger.LogInfo("Deactivating an end screen");

                // Don't display the extra names since it blocks the chart
                Destroy(instance.scoutWindows[i].gameObject);
                Logger.LogInfo("Deleted an end screen");
            }
        }
    }
}
