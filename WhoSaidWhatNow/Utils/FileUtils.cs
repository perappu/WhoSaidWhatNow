using Dalamud.DrunkenToad.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WhoSaidWhatNow.Objects;

namespace WhoSaidWhatNow.Utils
{
    public class FileUtils
    {
        /// <summary>
        /// OpenFileDialog for individual
        /// </summary>
        /// <param name="plugin">the plugin</param>
        /// <param name="playerName">player name</param>
        public static void OpenFileDialog(string playerName)
        {
            Plugin.FileDialogManager.SaveFileDialog("Save log...", "Text File{.txt}",
            Regex.Replace(playerName, "[^a-zA-Z0-9]", string.Empty) + "-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt",
            ".txt", (isOk, selectedFile) =>
            {
                if (isOk)
                {
                    SaveIndividualLog(selectedFile);
                }
            });
        }

        /// <summary>
        /// OpenFileDialog for groups
        /// </summary>
        /// <param name="group">KeyValuePair string for group name, Dictionary<Player, Boolean> for contents of group</param>
        public static void DialogSaveGroup(string name, Dictionary<Player, bool> group)
        {
            Plugin.FileDialogManager.SaveFileDialog("Save log...", "Text File{.txt}",
                Regex.Replace(name, "[^a-zA-Z0-9]", string.Empty) + "-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt",
                ".txt", (isOk, selectedFile) =>
                {
                    if (isOk)
                    {
                        SaveGroupLog(selectedFile, group);
                    }
                });
        }

        /// <summary>
        /// Save individual character log to file
        /// </summary>
        /// <param name="path">file path passed from save file dialog</param>
        public static void SaveIndividualLog(string path)
        {
            try
            {
                using (var file = new System.IO.StreamWriter(path, false))
                {
                    foreach (var c in from KeyValuePair<DateTime, ChatEntry> c in Plugin.ChatEntries
                                      where Plugin.Config.ChannelToggles[c.Value.Type] == true && c.Value.Sender.Name.Contains(Plugin.SelectedPlayer.Name)
                                      select c)
                    {
                        var tag = ConfigurationUtils.ChatTypeToFormat(c.Value.Type);
                        file.WriteLine(c.Value.CreateMessage(tag));
                    }
                    Plugin.ChatGui.Print($"Successfully saved log: {path}", "WhoWhat");
                }
            }
            catch
            {
                Plugin.ChatGui.Print("Failed to save log.", "WhoWhat");
            }
        }


        /// <summary>
        /// Save group log to file
        /// </summary>
        /// <param name="path">file path passed from save file dialog</param>
        /// <param name="players">passed dictionary of players</param>
        public static void SaveGroupLog(string path, Dictionary<Player, bool> players)
        {
            try
            {
                using var file = new System.IO.StreamWriter(path, false);
                foreach (var c in Plugin.ChatEntries)
                {
                    // if we are displaying this type of message;
                    if (Plugin.Config.ChannelToggles[c.Value.Type] == true)
                    {
                        // and if the player is among the tracked;
                        var p = PlayerUtils.GetCurrentAndPlayers().Find(p => c.Value.Sender.Name.Contains(p.Name));
                        if (players[p!])
                        {
                            var tag = ConfigurationUtils.ChatTypeToFormat(c.Value.Type);
                            file.WriteLine(c.Value.CreateMessage(tag));
                        }
                    }
                }

                Plugin.ChatGui.Print($"Successfully saved log: {path}", "WhoWhat");
            }
            catch
            {
                Plugin.ChatGui.Print("Failed to save log.", "WhoWhat");
            }
        }

    }
}
