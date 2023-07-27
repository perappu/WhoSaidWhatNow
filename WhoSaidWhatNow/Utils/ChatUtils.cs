using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhoSaidWhatNow.Utils
{
    public class ChatUtils
    {

        public static void ColoredText(string text, Vector4 color, bool sameline = true)
        {
            if (sameline) ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.TextWrapped(text);
            ImGui.PopStyleColor();
        }
    }
}
