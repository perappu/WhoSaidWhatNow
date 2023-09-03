using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using WhoSaidWhatNow.Objects;

namespace WhoSaidWhatNow.Utils
{
    public static class ChatUtils
    {
        /// <summary>
        /// Properly formats the passed data as a chat message and adds it to the log.
        /// </summary>
        public static void ShowMessage(KeyValuePair<DateTime, ChatEntry> c)
        {
            try
            {
                //tuple representing text that goes before/after player name
                Tuple<string, string> tag = Plugin.Config.GUIFormats[c.Value.Type];

                //dictionary for each chunk of text
                Dictionary<string, Vector4> chunks = new Dictionary<string, Vector4>();

                //timestamp. no intellisense i am not simplifying this, it looks like ass
                if (Plugin.Config.ShowTimestamp)
                {
                    chunks.Add($"[{c.Value.Time.ToShortTimeString()}] ", Plugin.Config.ChatColors[c.Value.Type]);
                }

                //if there isn't anything before the player name, ignore item1
                if (tag.Item1 != String.Empty)
                    chunks.Add(tag.Item1, Plugin.Config.ChatColors[c.Value.Type]);

                //string splitting garbage handling for standard emotes
                //for if we're the sender
                if (c.Value.Type == Dalamud.Game.Text.XivChatType.StandardEmote && c.Value.Sender.Name.Equals(Plugin.Config.CurrentPlayer))
                {
                    chunks.Add(c.Value.Message.Split(' ')[0], c.Value.Sender.NameColor);
                    //add message
                    chunks.Add(String.Format(tag.Item2, " " + String.Join(' ', c.Value.Message.Split(' ').Skip(1).ToArray())), Plugin.Config.ChatColors[c.Value.Type]);
                }
                //if someone else is the sender
                else if (c.Value.Type == Dalamud.Game.Text.XivChatType.StandardEmote)
                {
                    chunks.Add(c.Value.Sender.GetNameTag(), c.Value.Sender.NameColor);
                    //add message
                    chunks.Add(String.Format(tag.Item2, " " + String.Join(' ', c.Value.Message.Split(' ').Skip(2).ToArray())), Plugin.Config.ChatColors[c.Value.Type]);
                //normal messages
                } else {
                    chunks.Add(c.Value.Sender.GetNameTag(), c.Value.Sender.NameColor);
                    //add message
                    chunks.Add(String.Format(tag.Item2, c.Value.Message), Plugin.Config.ChatColors[c.Value.Type]);
                }

                ChatUtils.WrappedColoredText(chunks);
            }
            catch (Exception e)
            {
                PluginLog.LogError($"{e.Message} Unable to display message.");
            }

        }

        /// <summary>
        /// prints a single "paragraph" of colored text all on one line
        /// i never want to convert C++ code to C# ever again
        /// https://github.com/ocornut/imgui/issues/2313#issuecomment-458084296
        /// </summary>
        /// <param name="chunks">Dictionary<string, Vector4> of color chunks of text to put on same wrapped line</param>
        public static unsafe void WrappedColoredText(Dictionary<string, Vector4> chunks)
        {
            float wrapWidth = ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X;

            foreach (var chunk in chunks)
            {
                var bytes = Encoding.UTF8.GetBytes(chunk.Key);
                fixed (byte* text = bytes)
                {
                    byte* textStart = text;
                    byte* textEnd = text + bytes.Length;

                    ImFont* Font = ImGui.GetFont().NativePtr;

                    do
                    {
                        float widthRemaining = ImGui.GetContentRegionAvail().X;
                        byte* drawEnd = ImGuiNative.ImFont_CalcWordWrapPositionA(Font, 1.0f, textStart, textEnd, widthRemaining);

                        if (textStart == drawEnd)
                        {
                            ImGui.NewLine();
                            drawEnd = ImGuiNative.ImFont_CalcWordWrapPositionA(Font, 1.0f, textStart, textEnd, widthRemaining);
                        }

                        ImGui.PushStyleColor(ImGuiCol.Text, chunk.Value);
                        ImGuiNative.igTextUnformatted(textStart, drawEnd);
                        ImGui.PopStyleColor();

                        //non-native SameLine will add spaces
                        if (textStart == drawEnd || drawEnd == textEnd)
                        {
                            ImGuiNative.igSameLine(0, 0);
                            break;
                        }

                        textStart = drawEnd;

                        while (textStart < textEnd)
                        {
                            char c = (char)*textStart;
                            if (c == ' ')
                            {
                                textStart++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    } while (true);
                }
            }
            //we printed the entire chat chunk, so put a manual newline
            ImGui.NewLine();
        }

        /// <summary>
        /// shorthand function for colored text
        /// </summary>
        /// <param name="text">text to print</param>
        /// <param name="color">Vector4 color</param>
        /// <param name="sameline">whether or not this should include sameline at start, true by default</param>
        public static void ColoredText(string text, Vector4 color, bool sameline = true)
        {
            if (sameline) ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.TextUnformatted(text);
            ImGui.PopStyleColor();
        }
    }
}
