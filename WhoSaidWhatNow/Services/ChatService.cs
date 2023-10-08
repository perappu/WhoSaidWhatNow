using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using System;
using WhoSaidWhatNow.Objects;

namespace WhoSaidWhatNow.Services
{

    /// <summary>
    /// ChatService handles OnChatMessage and related events
    /// </summary>
    internal class ChatService : IDisposable
    {
        internal IChatGui? gui;

        public ChatService(IChatGui gui)

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
            if (Plugin.Config.Enabled == true)
            {
                var senderName = sender.ToString();
                //The basic ToString here includes any friends list icons and the server name, so we have to do Contains() for now
                //this would be gross if it was messages but it should be okay given a person will probably only have 4-5 players tracked a time
                var result = Plugin.Players.Find(x => senderName.Contains(x.Name));
                //PluginLog.Debug("onchat message triggered" + senderName);

                if (result != null)
                {
                    Plugin.ChatEntries.Add(DateTime.Now, new ChatEntry(senderId, result, message.ToString(), type, DateTime.Now));
                }
            }
        }
    }
}
