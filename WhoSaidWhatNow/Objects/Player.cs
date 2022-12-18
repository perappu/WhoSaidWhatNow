using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;
using System.Diagnostics.CodeAnalysis;

namespace WhoSaidWhatNow.Objects
{
    public class Player
    {
        public uint ID { get; set; }
        public string Name { get; init; }
        public List<ChatEntry> ChatEntries { get; init; }

        public Player(uint id, string name)
        {
            ID = id; 
            Name = name;
            ChatEntries = new List<ChatEntry>();
        }
    }
}
