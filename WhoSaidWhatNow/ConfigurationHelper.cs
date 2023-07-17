using Dalamud.Game.ClientState;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhoSaidWhatNow.Objects;

namespace WhoSaidWhatNow
{
    public class ConfigurationHelper
    {
        public void refresh()
        {
            PluginLog.LogDebug("refresh called");
            Plugin.Players.Clear();
            Plugin.Config.CurrentPlayer = string.Empty;
            SetCurrentPlayer();
            CheckTrackedPlayers();
        }

        public void reset()
        {
            Plugin.Config.AlwaysTrackedPlayers = new List<Tuple<string, string>>();
            Plugin.Players.Clear();
            Plugin.Config.CurrentPlayer = string.Empty;
            SetCurrentPlayer();
            CheckTrackedPlayers();
        }

        //Set the current player to the currently logged in character
        public void SetCurrentPlayer()
        {
            if (Plugin.Config.CurrentPlayer == null)
            {
                Plugin.Config.CurrentPlayer = Plugin.ClientState.LocalPlayer!.Name.ToString();
                Plugin.Players.Add(new Player(Plugin.ClientState.LocalPlayer!, true));
                PluginLog.LogDebug("Currently Logged In Player was null. Set: " + Plugin.Config.CurrentPlayer);
            }
            //if switched characters, remove old character and replace with new one
            //adds to top of list with insert
            else if (!Plugin.Config.CurrentPlayer.ToString().Equals(Plugin.ClientState.LocalPlayer!.Name.ToString()))
            {
                PluginLog.LogDebug("Currently Logged In Player was changed. Old: " + Plugin.Config.CurrentPlayer);
                Plugin.Players.Remove(Plugin.Players.Find(x => Plugin.Config.CurrentPlayer.Contains(x.Name)));
                Plugin.Config.CurrentPlayer = Plugin.ClientState.LocalPlayer!.Name.ToString();
                Plugin.Players.Insert(0, new Player(Plugin.ClientState.LocalPlayer!, true));
                PluginLog.LogDebug("Currently Logged In Player was changed. New: " + Plugin.Config.CurrentPlayer);
            }
        }

        // cycle through tracked player list to make sure they're all being tracked. if not, add them
        // add them with RemoveDisabled = true. should only be able to be removed via the config window
        public void CheckTrackedPlayers()
        {
            foreach (var player in Plugin.Config.AlwaysTrackedPlayers)
            {
                if (!Plugin.Players.Any(x => x.Name.Equals(player.Item1)))
                {
                    //create new tracked player w/o an ID
                    Plugin.Players.Add(new Player(player.Item1, player.Item2, true));
                    PluginLog.LogDebug("Added new whitelisted player: " + player.Item1 + " " + player.Item2);
                }
            }
        }
    }
}
