using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using System;
using WhoSaidWhatNow.Services;

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

        public Player(uint id, string name, string server, bool removeDisabled = false)
        {
            ID = id;
            Name = name;
            Server = server;
            RemoveDisabled = removeDisabled;
            TimeAdded = DateTime.UtcNow;
        }

        public Player(string name, string server, bool removeDisabled = false)
        {
            ID = null;
            Name = name;
            Server = server;
            RemoveDisabled = removeDisabled;
            TimeAdded = DateTime.UtcNow;
        }
        
        public Player(GameObject gameObject, bool removeDisabled = false)
        {
            ID = gameObject.ObjectId;
            Name = gameObject.Name.ToString();
            PlayerCharacter? player = PlayerService.CastPlayer(gameObject);
            RemoveDisabled = removeDisabled;
            TimeAdded = DateTime.UtcNow;

            if (player != null)
            {
                Server = player.HomeWorld.GameData!.Name.ToString();
            }
            else
            {
                Server = "ServerNotFound";
            }
        }

        public Player(PlayerCharacter playerCharacter, bool removeDisabled = false)
        {
            ID = playerCharacter.ObjectId;
            Name = playerCharacter.Name.ToString();
            RemoveDisabled = removeDisabled;
            TimeAdded = DateTime.UtcNow;

            if (playerCharacter != null)
            {
                Server = playerCharacter.HomeWorld.GameData!.Name.ToString();
            }
            else
            {
                Server = "ServerNotFound";
            }
        }

    }
}
