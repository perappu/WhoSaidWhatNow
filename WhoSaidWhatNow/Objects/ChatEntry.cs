using Dalamud.Game.Text;
using System;
using System.Linq;

namespace WhoSaidWhatNow.Objects
{
    //ChatEntry object
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
            Type = type;
            Time = time;
            Message = message;
        }

        public string CreateMessage(string tag)
        {
            string time = this.Time.ToShortTimeString();
            string sender = this.Sender.GetNameTag();
            string msg = this.Message.Trim();

            //handling for standard emotes
            if (Type == XivChatType.StandardEmote && Sender.Name.Equals(Plugin.Config.CurrentPlayer))
            {
                sender = String.Empty;
                msg = this.Message.Trim();
            } else if (Type == XivChatType.StandardEmote)
            {
                msg = String.Join(' ',this.Message.Split(' ').Skip(2).ToArray());
            }

            return (Plugin.Config.ShowTimestamp ? $"[{time}]" : String.Empty) + String.Format(tag, sender, msg).Trim();
        }

    }
}
