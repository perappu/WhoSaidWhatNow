using Dalamud.Game.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhoSaidWhatNow.Objects
{
    //this is pretty much lifted straight from Snooper
    public class ChatEntry
    {
        public uint SenderID { get; set; }
        public string Sender { get; init; }
        public string Message { get; init; }
        public XivChatType Type { get; init; }
        public DateTime Time { get; init; }

        public ChatEntry(uint senderId, string sender, string message, XivChatType type, DateTime time)
        {
            SenderID = senderId;
            Sender = sender;
            Message = message;
            Type = type;
            Time = time;
        }
    }
}
