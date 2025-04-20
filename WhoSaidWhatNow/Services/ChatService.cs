using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Linq;
using WhoSaidWhatNow.Objects;
using WhoSaidWhatNow.Utils;
using WhoSaidWhatNow.Windows;
using static Dalamud.Interface.Utility.Raii.ImRaii;

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

        private void OnChatMessage(XivChatType type, int senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (Plugin.Config.Enabled)
            {
                var senderName = sender.ToString();
                //The basic ToString here includes any friends list icons and the server name, so we have to do Contains() for now
                //this would be gross if it was messages but it should be okay given a person will probably only have 4-5 players tracked a time <- written by a past idiot
                var result = PlayerUtils.GetCurrentAndPlayers().Find(x => senderName.Contains(x.Name));
                //PluginLog.Debug("onchat message triggered" + senderName);

                if (result != null)
                {
                    Plugin.ChatEntries.Add(DateTime.Now, new ChatEntry(senderId, result, message.ToString(), type, DateTime.Now));

                    //do stuff that needs to happen when a message is added from the selected player OR selected group
                    if (Plugin.MainWindow.IsOpen)
                    {
                        if ((Plugin.MainWindow.individualOpen == true && Plugin.SelectedPlayer != null && Plugin.SelectedPlayer.ID == result.ID) || (Plugin.MainWindow.individualOpen == false && Plugin.SelectedGroup != null && Plugin.Groups[Plugin.SelectedGroup].PLAYERS[result] == true))
                        {
                            //if we enabled beeps
                            if (Plugin.Config.PlaySound)
                            {
                                UIGlobals.PlaySoundEffect((uint)Plugin.Config.SelectedSound);
                            }
                            if(Plugin.Config.AutoscrollOnNewMessage)
                            {
                                MainWindow.justOpened = true;
                            }
                        }
                    }

                }

                
            }
        }
    }
}
