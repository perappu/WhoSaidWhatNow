using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Common.Math;
using System;
using WhoSaidWhatNow.Utils;

namespace WhoSaidWhatNow.Objects
{
    /// <summary>
    /// Player object
    /// ID is the ObjectID from Dalamud, which is an uint
    /// Name is the player's name, as it is currently parsed will include friend group icon and server
    /// Server is the players server name as a string for building messages
    /// </summary>
    public class Player
    {
        public uint? ID { get; set; }
        public string Name { get; init; }
        public string Server { get; init; }
        public bool RemoveDisabled { get; set; }
        public DateTime TimeAdded { get; set; }
        public Vector4 NameColor { get; set; }

        // constructors
        public Player(uint id, string name, string server, bool removeDisabled = false)
        {
            ID = id;
            Name = name;
            Server = server;
            RemoveDisabled = removeDisabled;
            TimeAdded = DateTime.UtcNow;
            NameColor = PlayerUtils.SetNameColor(Name);
        }

        public Player(string name, string server, Vector4 nameColor, bool removeDisabled = false)
        {
            ID = null;
            Name = name;
            Server = server;
            RemoveDisabled = removeDisabled;
            TimeAdded = DateTime.UtcNow;
            NameColor = nameColor;
        }

        public Player(IGameObject gameObject, bool removeDisabled = false)
        {
            ID = gameObject.EntityId;
            Name = gameObject.Name.ToString();
            var player = PlayerUtils.CastPlayer(gameObject);
            RemoveDisabled = removeDisabled;
            TimeAdded = DateTime.UtcNow;

            Server = player != null ? player.HomeWorld.Value.Name.ToString() : "ServerNotFound";

            NameColor = PlayerUtils.SetNameColor(Name);
        }

        public Player(IPlayerCharacter playerCharacter, bool removeDisabled = false)
        {
            ID = playerCharacter.EntityId;
            Name = playerCharacter.Name.ToString();
            RemoveDisabled = removeDisabled;
            TimeAdded = DateTime.UtcNow;

            Server = playerCharacter.HomeWorld.Value.Name.ToString();
            NameColor = PlayerUtils.SetNameColor(Name);
        }

        public string GetNameTag()
        {
            return Name + (Plugin.Config.ShowServer ? "" + Server : string.Empty);
        }
    }
}
