using Dalamud.Game.Text;
using System;
using System.Linq;

namespace WhoSaidWhatNow.Objects
{
    //ChatEntry object
    public class ChatEntry(int senderId, Player sender, string message, XivChatType type, DateTime time)
    {
        public int SenderID { get; set; } = senderId;
        public Player Sender { get; init; } = sender;
        public string Message { get; init; } = message;
        public XivChatType Type { get; init; } = type;
        public DateTime Time { get; init; } = time;

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
            }
            else if (Type == XivChatType.StandardEmote)
            {
                msg = String.Join(' ', this.Message.Split(' ').Skip(2).ToArray());
            }

            return (Plugin.Config.ShowTimestamp ? $"[{time}]" : string.Empty) + string.Format(tag, sender, msg).Trim();
        }

    }
}
