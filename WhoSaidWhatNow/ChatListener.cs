using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhoSaidWhatNow.Objects;
using Dalamud.Game;

namespace WhoSaidWhatNow
{
    internal class ChatListener : IDisposable
    {
        public List<Player> Players;
        readonly private ChatGui chatGui;

        public ChatListener(List<Player> players, ChatGui passedChatGui, Configuration configuration, ClientState clientState, TargetManager targetManager, SigScanner sigScanner) 

        {
            Players = players;
            chatGui = passedChatGui;
            chatGui.ChatMessage += OnChatMessage;
        }

        public void Dispose()
        {
            chatGui.ChatMessage -= OnChatMessage;
        }

        private void OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            string senderName = sender.ToString();
            Player? result = Players.Find(x => x.Name == senderName);
            //this would be gross if it was messages but it should be okay given a person will probably only have 4-5 players tracked a time
            if (result != null) {
                result.ChatEntries.Add(new ChatEntry(senderId, sender.ToString(), message.ToString(), type, DateTime.Now));
            }
        }
    }
}
