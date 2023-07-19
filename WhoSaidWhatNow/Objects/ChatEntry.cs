using Dalamud.Game.Text;
using System;

namespace WhoSaidWhatNow.Objects
{
    //ChatEntry object
    //this is pretty much lifted straight from Snooper
    public class ChatEntry
    {
        public uint SenderID { get; set; }
        public Player Sender { get; init; }
        public string Message { get; init; }
        public XivChatType Type { get; init; }
        public DateTime Time { get; init; }

        public ChatEntry(uint senderId, Player sender, string message, XivChatType type, DateTime time)
        {
            SenderID = senderId;
            Sender = sender;
            Message = message;
            Type = type;
            Time = time;
        }

        public string CreateMessage(string tag)
        {
            string time = this.Time.ToShortTimeString();
            string sender = this.Sender.Name + "" + this.Sender.Server;
            string msg = this.Message;

            return $"[{time}]" + String.Format(tag, sender, msg);
        }

    }
}
