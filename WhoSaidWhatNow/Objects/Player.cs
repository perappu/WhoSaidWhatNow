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
        public uint ID { get; set; }
        public string Name { get; init; }
        public string Server { get; init; }

        public Player(uint id, string name, string server)
        {
            ID = id; 
            Name = name;
            Server = server;
        }

        public Player(GameObject gameObject)
        {
            ID = gameObject.ObjectId;
            Name = gameObject.Name.ToString();
            try
            {
                Server = ((PlayerCharacter)gameObject).HomeWorld.GameData.Name.ToString();
            }
             catch
            {
                Server = "ServerNotFound";
            }
        }
    }
}
