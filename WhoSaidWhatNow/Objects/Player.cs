using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;
using System.Diagnostics.CodeAnalysis;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;

namespace WhoSaidWhatNow.Objects
{
    
    // Player object
    // ID is the ObjectID from Dalamud, which is an uint
    // Name is the player's name, as it is currently parsed will include friend group icon and server
    // Server is the players server name as a string for building messages
    public class Player
    {
        public uint? ID { get; set; }
        public string Name { get; init; }
        public string Server { get; init; }
        public bool RemoveDisabled { get; init; }

        public Player(uint id, string name, string server, bool removeDisabled = false)
        {
            ID = id;
            Name = name;
            Server = server;
            RemoveDisabled = removeDisabled;
        }

        public Player(string name, string server, bool removeDisabled = false)
        {
            ID = null;
            Name = name;
            Server = server;
        }
        
        public Player(GameObject gameObject, bool removeDisabled = false)
        {
            ID = gameObject.ObjectId;
            Name = gameObject.Name.ToString();
            PlayerCharacter? player = CastPlayer(gameObject);
            RemoveDisabled = removeDisabled;

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

            if (playerCharacter != null)
            {
                Server = playerCharacter.HomeWorld.GameData!.Name.ToString();
            }
            else
            {
                Server = "ServerNotFound";
            }
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
                return null;
            }
        }

    }
}
