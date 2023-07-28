using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using WhoSaidWhatNow.Objects;

namespace WhoSaidWhatNow.Utils
{
    public class PlayerUtils
    {

        public PlayerUtils() { }

        /// <summary>
        /// Attempts to add given GameObject to tracked players
        /// </summary>
        /// <param name="target">GameObject resembling a Player Character to add</param>
        /// <param name="removeDisabled">Boolean value indicating where or not the player can be removed from the main UI</param>
        /// <returns>Returns bool indicating if add was successful</returns>
        public static bool AddPlayer(GameObject? target, bool removeDisabled = false)
        {

            if (target == null || target.ObjectKind != ObjectKind.Player)
            {
                return false;
            }
            else if (Plugin.Players.Any(x => x.Name.Equals(target.Name.ToString())))
            {
                return false;
            }
            else
            {
                Plugin.Players.Add(new Player(target, removeDisabled));
                return true;
            }
        }

        //Remove player by player object
        public static void RemovePlayer(Player player)
        {
            if (player is not null)
            {
                Plugin.Players.Remove(player);
            }
        }

        //Remove player by string player name
        public static void RemovePlayer(string playerName)
        {
            var player = Plugin.Players.Find(x => x.Name == playerName);
            if (player is not null)
            {
                Plugin.Players.Remove(player);
            }
        }

        /// <summary>
        /// Set the current player to the currently logged in character
        /// </summary>
        public static void SetCurrentPlayer()
        {
            //if switched characters, remove old character and replace with new one
            //adds to top of list with insert
            if (!Plugin.Config.CurrentPlayer.ToString().Equals(Plugin.ClientState.LocalPlayer!.Name.ToString()))
            {
                RemovePlayer(Plugin.Config.CurrentPlayer);
            }
            Plugin.Config.CurrentPlayer = Plugin.ClientState.LocalPlayer!.Name.ToString();
            AddPlayer(Plugin.ClientState.LocalPlayer!, true);
            PluginLog.LogDebug("Currently Logged In Player was changed or null. New: " + Plugin.Config.CurrentPlayer);
        }

        public static void AddTrackedPlayer(Tuple<string, string, Vector4> player)
        {
            Plugin.Config.AlwaysTrackedPlayers.Add(player);
            CheckTrackedPlayers();
            Plugin.Config.Save();
            SortPlayers();
        }

        public static void RemoveTrackedPlayer(Tuple<string, string, Vector4> player)
        {
            Plugin.Config.AlwaysTrackedPlayers.Remove(player);
            var findPlayer = Plugin.Players.Find(x => x.Name.Equals(player.Item1));
            if (findPlayer is not null)
            {
                findPlayer.RemoveDisabled = false;
                PluginLog.LogDebug("Removed whitelisted player: " + player.Item1 + " " + player.Item2);
            }
            Plugin.Config.Save();
            SortPlayers();
        }

        /// <summary>
        /// cycle through tracked player list to make sure they're all being tracked. if not, add them
        /// add them with RemoveDisabled = true. removal is handled elsewhere
        /// </summary>
        public static void CheckTrackedPlayers()
        {
            foreach (var player in Plugin.Config.AlwaysTrackedPlayers)
            {
                var findPlayer = Plugin.Players.Find(x => x.Name.Equals(player.Item1));
                if (findPlayer is null)
                {
                    //create new tracked player w/o an ID
                    Plugin.Players.Add(new Player(player.Item1, player.Item2, player.Item3, true));
                    PluginLog.LogDebug("Added new whitelisted player: " + player.Item1 + " " + player.Item2);
                }
                else
                {
                    findPlayer.RemoveDisabled = true;
                }
            }
        }

        /// <summary>
        /// Sort the players by user at top -> always tracked -> everyone else.
        /// </summary>
        public static void SortPlayers()
        {
            List<Player> topPlayers = Plugin.Players.Where(x => x.RemoveDisabled == true).ToList();
            List<Player> otherPlayers = Plugin.Players.Where(x => x.RemoveDisabled != true).ToList();

            //we only need to do remove and return stuff if the player isn't already at the top
            if (!topPlayers[0].Name.Equals(Plugin.Config.CurrentPlayer))
            {
                int i = topPlayers.FindIndex(x => x.Name == Plugin.Config.CurrentPlayer);
                Player currentPlayer = topPlayers[i];
                topPlayers.RemoveAt(i);
                topPlayers.Insert(0, currentPlayer);
            }

            Plugin.Players = topPlayers.Concat(otherPlayers).ToList();
        }

        //cast a generic gameobject as a PlayerCharacter
        public static PlayerCharacter? CastPlayer(GameObject obj)
        {
            try
            {
                var character = (PlayerCharacter)obj;
                return character;
            }
            catch
            {
                PluginLog.LogDebug("Could not cast object as player");
                return null;
            }
        }

        public static Vector4 SetNameColor(string name)
        {
            Vector4 nameColor = ConfigurationUtils.GenerateRgba((uint)name.GetHashCode());
            nameColor.X += 0.2f;
            nameColor.Y += 0.2f;
            nameColor.Z += 0.2f;
            return nameColor;
        }
    }
}
