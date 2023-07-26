using Dalamud.Logging;
using System;
using System.Collections.Generic;

namespace WhoSaidWhatNow.Services
{
    public class ConfigurationService
    {

        public static void refresh()
        {
            PluginLog.LogDebug("refresh called");
            Plugin.Players.Clear();
            Plugin.Config.CurrentPlayer = string.Empty;
            PlayerService.SetCurrentPlayer();
            PlayerService.CheckTrackedPlayers();
        }

        public static void reset()
        {
            Plugin.Config.AlwaysTrackedPlayers = new List<Tuple<string, string>>();
            Plugin.Players.Clear();
            Plugin.ChatEntries.Clear();
            Plugin.Config.CurrentPlayer = string.Empty;
            PlayerService.SetCurrentPlayer();
            PlayerService.CheckTrackedPlayers();
        }


    }
}
