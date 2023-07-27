using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Common.Math;
using System;
using WhoSaidWhatNow.Services;
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

        private void SetNameColor(string name)
        {
            int nameHash = name.GetHashCode();
            //float val1 = (float)(((nameHash >> (nameHash.ToString()[1] * 8)) & 0xFF) / 255.0) + 0.4f;
            //float val2 = (float)(((nameHash >> (nameHash.ToString()[2] * 8)) & 0xFF) / 255.0) + 0.4f;
            //float val3 = (float)(((nameHash >> (nameHash.ToString()[3] * 8)) & 0xFF) / 255.0) + 0.4f;

            Random rand = new Random(nameHash);
            float val1 = (float)rand.NextDouble() + (float)rand.NextDouble();
            float val2 = (float)rand.NextDouble() + (float)rand.NextDouble();
            float val3 = (float)rand.NextDouble() + (float)rand.NextDouble();

            Vector4 newColor = new Vector4(val1, val2, val3, 1f);
            PluginLog.LogDebug(newColor.ToString());
            NameColor = newColor;
        }

        public Player(uint id, string name, string server, bool removeDisabled = false)
        {
            ID = id;
            Name = name;
            Server = server;
            RemoveDisabled = removeDisabled;
            TimeAdded = DateTime.UtcNow;
            SetNameColor(Name);
        }

        public Player(string name, string server, bool removeDisabled = false)
        {
            ID = null;
            Name = name;
            Server = server;
            RemoveDisabled = removeDisabled;
            TimeAdded = DateTime.UtcNow;
            SetNameColor(Name);
        }

        public Player(GameObject gameObject, bool removeDisabled = false)
        {
            ID = gameObject.ObjectId;
            Name = gameObject.Name.ToString();
            PlayerCharacter? player = PlayerUtils.CastPlayer(gameObject);
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
            SetNameColor(Name);
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
            SetNameColor(Name);
        }

        public string GetNameTag()
        {
            return Name + "" + Server;
        }
    }
}
