using Dalamud.DrunkenToad;
using System;
using System.Collections.Generic;
using System.Linq;
using WhoSaidWhatNow.Objects;

namespace WhoSaidWhatNow.Services
{
    public class FileService
    {
        
        /// <summary>
        /// Save individual character log to file
        /// </summary>
        /// <param name="path">file path passed from save file dialog</param>
        public static void SaveIndividualLog(string path)
        {
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, false))
                {
                    foreach (var c in from KeyValuePair<DateTime, ChatEntry> c in Plugin.ChatEntries
                                      where Plugin.Config.ChannelToggles[c.Value.Type] == true && c.Value.Sender.Name.Contains(Plugin.SelectedPlayer.Name)
                                      select c)
                    {
                        string tag = Plugin.Config.Formats[c.Value.Type];
                        file.WriteLine(c.Value.CreateMessage(tag));
                    }
                    ChatGuiExtensions.PluginPrint(Plugin.ChatGui, "Successfully saved log: " + path);
                }
            }
            catch
            {
                ChatGuiExtensions.PluginPrint(Plugin.ChatGui, "Failed to save log.");
            }
        }


        /// <summary>
        /// Save group log to file
        /// </summary>
        /// <param name="path">file path passed from save file dialog</param>
        /// <param name="players">passed dictionary of players</param>
        public static void SaveGroupLog(string path, Dictionary<Player, Boolean> players)
        {
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, false))
                {
                    foreach (var c in Plugin.ChatEntries)
                    {
                        // if we are displaying this type of message;
                        if (Plugin.Config.ChannelToggles[c.Value.Type] == true)
                        {
                            // and if the player is among the tracked;
                            var p = Plugin.Players.Find(p => c.Value.Sender.Name.Contains(p.Name));
                            if (players[p!])
                            {
                                string tag = Plugin.Config.Formats[c.Value.Type];
                                file.WriteLine(c.Value.CreateMessage(tag));
                            }
                        }
                    }

                    ChatGuiExtensions.PluginPrint(Plugin.ChatGui, "Successfully saved log: " + path);
                }
            }
            catch
            {
                ChatGuiExtensions.PluginPrint(Plugin.ChatGui, "Failed to save log.");
            }
        }

    }
}
