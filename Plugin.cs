using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using Steamworks;
using Photon.Realtime;

namespace PeakUnlimited;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private static int NEW_MAX_PLAYERS;
    private static ConfigEntry<int> configMaxPlayers;
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
            10, 
            "The maximum number of players you want to be able to join your lobby. Warning: untested, higher numbers may break!"
        );
        
        NEW_MAX_PLAYERS = configMaxPlayers.Value;
        
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is patching!");
        harmony.PatchAll(typeof(HostRoomOptionsPatch));
        harmony.PatchAll(typeof(PlayClickedPatch));
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} has patched!");
    }
    public class HostRoomOptionsPatch 
    {
        [HarmonyPatch(typeof(NetworkConnector), "HostRoomOptions")]
        [HarmonyPostfix]
        static void Postfix(ref RoomOptions __result)
        {
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
}
