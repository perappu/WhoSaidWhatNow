using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
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

        /// <summary>
        /// Remove player by player object
        /// </summary>
        /// <param name="player">Player object to remove</param>
        public static void RemovePlayer(Player player)
        {
            if (player is not null)
            {
                Plugin.Players.Remove(player);
            }
        }

        /// <summary>
        /// Remove player by string player name
        /// </summary>
        /// <param name="playerName">Player name to remove</param>
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
            Plugin.Logger.Debug($"Currently Logged In Player was changed or null. New: {Plugin.Config.CurrentPlayer}");
        }

        // TRACKED PLAYER METHODS //
        /// <summary>
        /// Add tracked player to config. Calls CheckTrackedPlayers() and SortPlayers().
        /// </summary>
        /// <param name="player">TrackedPlayer to add</param>
        public static void AddTrackedPlayer(TrackedPlayer player)
        {
            Plugin.Config.AlwaysTrackedPlayers.Add(player);
            CheckTrackedPlayers();
            Plugin.Config.Save();
            SortPlayers();
        }

        /// <summary>
        /// Removes tracked player from config. Keeps them in Config.Player list, but enables remove.
        /// </summary>
        /// <param name="player">TrackedPlayer to remove</param>
        public static void RemoveTrackedPlayer(TrackedPlayer player)
        {
            Plugin.Config.AlwaysTrackedPlayers.Remove(player);
            var findPlayer = Plugin.Players.Find(x => x.GetNameTag().Equals(player.GetNameTag()));
            if (findPlayer is not null)
            {
                findPlayer.RemoveDisabled = false;
                Plugin.Logger.Debug($"Removed whitelisted player: {player.Name} {player.Server}");
            }
            Plugin.Config.Save();
            SortPlayers();
        }

        
        /// <summary>
        /// Change tracked player to given color. Also finds them in Player list and changes their value.
        /// </summary>
        /// <param name="player">TrackedPlayer object</param>
        /// <param name="color">Vector4 color</param>
        public static void ColorTrackedPlayer(TrackedPlayer player, Vector4 color)
        {
            var findPlayer = Plugin.Players.Find(x => x.Name == player.Name);
            if (findPlayer is not null) findPlayer.NameColor = color;
            player.Color = color;
            Plugin.Config.Save();
        }

        /// <summary>
        /// cycle through tracked player list to make sure they're all being tracked. if not, add them
        /// add them with RemoveDisabled = true. removal is handled elsewhere
        /// </summary>
        public static void CheckTrackedPlayers()
        {
            foreach (var player in Plugin.Config.AlwaysTrackedPlayers)
            {
                var findPlayer = Plugin.Players.Find(x => x.Name.Equals(player.Name));
                if (findPlayer is null)
                {
                    //create new tracked player w/o an ID
                    Plugin.Players.Add(new Player(player.Name, player.Server, player.Color, true));
                    PluginLog.LogDebug($"Added new whitelisted player: {player.Name} {player.Server}");
                }
                else
                {
                    findPlayer.NameColor = player.Color;
                    findPlayer.RemoveDisabled = true;
                }
            }
        }

        /// <summary>
        /// Sort the players by user at top -> always tracked -> everyone else.
        /// </summary>
        public static void SortPlayers()
        {
            var topPlayers = Plugin.Players.Where(x => x.RemoveDisabled == true).ToList();
            var otherPlayers = Plugin.Players.Where(x => x.RemoveDisabled != true).ToList();

            //we only need to do remove and return stuff if the player isn't already at the top
            if (!topPlayers[0].Name.Equals(Plugin.Config.CurrentPlayer))
            {
                var i = topPlayers.FindIndex(x => x.Name == Plugin.Config.CurrentPlayer);
                var currentPlayer = topPlayers[i-1];
                topPlayers.RemoveAt(i);
                topPlayers.Insert(0, currentPlayer);
            }

            Plugin.Players = topPlayers.Concat(otherPlayers).ToList();
        }

        /// <summary>
        /// cast a generic gameobject as a PlayerCharacter
        /// </summary>
        /// <param name="obj">Generic GameObject</param>
        /// <returns></returns>
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

        /// <summary>
        /// Create color based off of hash of a name
        /// </summary>
        /// <param name="name">name string</param>
        /// <returns></returns>
        public static Vector4 SetNameColor(string name)
        {
            var nameColor = ConfigurationUtils.GenerateRgba((uint)name.GetHashCode());
            nameColor.X += 0.2f;
            nameColor.Y += 0.2f;
            nameColor.Z += 0.2f;
            return nameColor;
        }
    }
}
