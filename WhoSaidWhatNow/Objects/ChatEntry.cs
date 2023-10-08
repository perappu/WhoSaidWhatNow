using Dalamud.Game.Text;
using System;
using System.Linq;

namespace WhoSaidWhatNow.Objects
{
    //ChatEntry object
    public class ChatEntry(uint senderId, Player sender, string message, XivChatType type, DateTime time)
    {
        public uint SenderID { get; set; } = senderId;
        public Player Sender { get; init; } = sender;
        public string Message { get; init; } = message;
        public XivChatType Type { get; init; } = type;
        public DateTime Time { get; init; } = time;

        public string CreateMessage(string tag)
        {
            var time = Time.ToShortTimeString();
            var sender = Sender.GetNameTag();
            var msg = Message.Trim();

            //handling for standard emotes
            if (Type == XivChatType.StandardEmote && Sender.Name.Equals(Plugin.Config.CurrentPlayer))
            {
                sender = String.Empty;
                msg = Message.Trim();
            } else if (Type == XivChatType.StandardEmote)
            {
                msg = String.Join(' ',Message.Split(' ').Skip(2).ToArray());
            }

            return (Plugin.Config.ShowTimestamp ? $"[{time}]" : String.Empty) + String.Format(tag, sender, msg).Trim();
        }

    }
}
