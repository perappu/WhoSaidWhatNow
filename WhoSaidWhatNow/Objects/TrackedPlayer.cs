using System.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhoSaidWhatNow.Objects
{
    public class TrackedPlayer
    {
        public string Name { get; set; }
        public string Server { get; set; }
        public Vector4 Color { get; set; }

        public TrackedPlayer(string name, string server, Vector4 color)
        {
            Name = name;
            Server = server;
            Color = color;
        }
        public string GetNameTag()
        {
            return Name + "" + Server;
        }
    }
}
