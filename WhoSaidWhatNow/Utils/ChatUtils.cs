using Dalamud.Logging;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace WhoSaidWhatNow.Utils
{
    public static class ChatUtils
    {

        public static unsafe void WrappedColoredText(Dictionary<string, Vector4> lines)
        {
            float wrapWidth = ImGui.GetWindowContentRegionMax().X- ImGui.GetWindowContentRegionMin().X;

            //PluginLog.LogDebug(wrapWidth.ToString());
            foreach (var line in lines)
            {
                var bytes = Encoding.UTF8.GetBytes(line.Key);
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

                        ImGui.PushStyleColor(ImGuiCol.Text, line.Value);
                        ImGuiNative.igTextUnformatted(textStart, drawEnd);
                        ImGui.PopStyleColor();

                        if (textStart == drawEnd || drawEnd == textEnd)
                        {
                            ImGuiNative.igSameLine(0,0);
                            break;
                        }

                        textStart = drawEnd;

                        while (textStart < textEnd)
                        {
                            char c = (char)*textStart;
                            if (c == ' ') {
                                textStart++; }
                            else {
                                break; }
                        }
                    } while (true);
                }
            }
            ImGui.NewLine();
        }

        public static void ColoredText(string text, Vector4 color, bool sameline = true)
        {
            if (sameline) ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.TextUnformatted(text);
            ImGui.PopStyleColor();
        }
    }
}
