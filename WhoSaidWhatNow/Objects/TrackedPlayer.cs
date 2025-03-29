using System.Numerics;

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
