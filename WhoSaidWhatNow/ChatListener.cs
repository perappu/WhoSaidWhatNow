using System;

using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;

using WhoSaidWhatNow.Objects;

namespace WhoSaidWhatNow
{

    /// <summary>
    /// ChatListener class is heavily based around how Snooper handles messages
    /// <br/>
    /// However, we only keep messages if the player is already being tracked -- otherwise they are discarded
    /// <br/>
    /// I'm hesitant to keep the entire chat log in memory, need to find a way to read the backlog instead of just reacting to the message event if possible
    /// </summary>
    internal class ChatListener : IDisposable
    {
        internal ChatGui? gui;

        public ChatListener(ChatGui gui)

        {
            this.gui = gui;
            this.gui.ChatMessage += OnChatMessage;
        }

        //THIS IS VERY IMPORTANT
        //if you do not do Dispose() like this it will created a brand new lost OnChatMessage thread.
        //I accidentally had like 15+ once. my game crashed
        public void Dispose()
        {
            gui!.ChatMessage -= OnChatMessage;
        }

        private void OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (Configuration.IsOn == true)
            {
                string senderName = sender.ToString();
                //The basic ToString here includes any friends list icons and the server name, so we have to do Contains() for now
                //this would be gross if it was messages but it should be okay given a person will probably only have 4-5 players tracked a time
                Player? result = Plugin.Players.Find(x => senderName.Contains(x.Name));
                //PluginLog.Debug("onchat message triggered" + senderName);

                if (result != null)
                {
                    Plugin.ChatEntries.Add(DateTime.Now, new ChatEntry(senderId, result, message.ToString(), type, DateTime.Now));
                }
            }
        }
    }
}
