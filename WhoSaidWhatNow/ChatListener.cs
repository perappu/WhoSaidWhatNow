using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhoSaidWhatNow.Objects;
using Dalamud.Game;
using Dalamud.Game.Text.SeStringHandling.Payloads;

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
            //The basic ToString here includes any friends list icons and the server name, so we have to do Contains() for now
            Player? result = Players.Find(x => senderName.Contains(x.Name));
            PluginLog.Debug("onchat message triggered " + senderName);

            //this would be gross if it was messages but it should be okay given a person will probably only have 4-5 players tracked a time
            if (result != null) {
                result.ChatEntries.Add(new ChatEntry(senderId, senderName, message.ToString(), type, DateTime.Now));
            }
        }
    }
}
