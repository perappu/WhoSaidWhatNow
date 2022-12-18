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

    // ChatListener class is heavily based around how Snooper handles messages
    // However, we only keep messages if the player is already being tracked -- otherwise they are discarded
    // I'm hesitant to keep the entire chat log in memory, need to find a way to read the backlog instead of just reacting to the message event if possible
    internal class ChatListener : IDisposable
    {
        public List<Player> Players;
        readonly private ChatGui chatGui;
        readonly private Configuration configuration;

        public ChatListener(List<Player> players, ChatGui passedChatGui, Configuration passedConfiguration, ClientState clientState, TargetManager targetManager, SigScanner sigScanner)

        {
            Players = players;
            chatGui = passedChatGui;
            chatGui.ChatMessage += OnChatMessage;
            configuration = passedConfiguration;
        }

        //THIS IS VERY IMPORTANT
        //if you do not do Dispose() like this it will created a brand new lost OnChatMessage thread.
        //I accidentally had like 15+ once. my game crashed
        public void Dispose()
        {
            chatGui.ChatMessage -= OnChatMessage;
        }

        private void OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (configuration.IsOn == true)
            {
                string senderName = sender.ToString();
                //The basic ToString here includes any friends list icons and the server name, so we have to do Contains() for now
                Player? result = Players.Find(x => senderName.Contains(x.Name));
                //PluginLog.Debug("onchat message triggered" + senderName);

                //this would be gross if it was messages but it should be okay given a person will probably only have 4-5 players tracked a time
                if (result != null)
                {
                    result.ChatEntries.Add(new ChatEntry(senderId, senderName, message.ToString(), type, DateTime.Now));
                }
            }
        }
    }
}
