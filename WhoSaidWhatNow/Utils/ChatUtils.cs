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
            float wrapWidth = ImGui.GetContentRegionAvail().X;

            foreach (var line in lines)
            {
                var bytes = Encoding.UTF8.GetBytes(line.Key);
                byte* textStart = (byte*)bytes[0];
                byte* textEnd = (byte*)bytes[bytes.Length-1];

                ImFont* Font = ImGui.GetFont().NativePtr;

                do
                {
                    float widthRemaining = ImGui.GetContentRegionAvail().X;
                    byte* drawEnd = ImGuiNative.ImFont_CalcWordWrapPositionA(Font, 1.0f, textStart, textEnd, wrapWidth - widthRemaining);

                    if (textStart == drawEnd || drawEnd == textEnd)
                    {
                        ImGui.NewLine();
                        drawEnd = ImGuiNative.ImFont_CalcWordWrapPositionA(Font, 1.0f, textStart, textEnd, wrapWidth - widthRemaining);
                    }

                    ImGui.PushStyleColor(ImGuiCol.Text, line.Value);
                    ImGuiNative.igTextUnformatted(textStart, textStart == drawEnd ? null : drawEnd);
                    ImGui.PopStyleColor();

                    if (textStart == drawEnd || drawEnd == textEnd)
                    {
                        ImGuiNative.igSameLine(0.0f, 0.0f);
                        break;
                    }

                    while (textStart < textEnd)
                    {
                        byte c = (byte)textStart;
                        if (c == ' ') { textStart++; }
                        else if (c == '\n') { textStart++; break; }
                        else { break; }
                    }
                } while (true);

            }


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
