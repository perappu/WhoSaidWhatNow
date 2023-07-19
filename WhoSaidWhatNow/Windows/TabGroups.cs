using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using WhoSaidWhatNow;
using WhoSaidWhatNow.Objects;
using WhoSaidWhatNow.Windows;

public class TabGroups
{

    public TabGroups()
    {

        if (ImGui.BeginTabItem("Groups"))
        {

            // replace the existing panels by using the same IDs.
            ImGui.BeginChild(MainWindow.ID_PANEL_LEFT, new Vector2(205 * ImGuiHelpers.GlobalScale, 0), true);
            ImGui.EndChild();
            ImGui.SameLine();
            ImGui.BeginChild(MainWindow.ID_PANEL_RIGHT, new Vector2(0, 0), true);
            ImGui.EndChild();

            // populate the list of selectable groups.
            foreach (var g in Plugin.Groups)
            {
                ImGui.BeginChild(MainWindow.ID_PANEL_LEFT);
                addPlayerGroup(g);
                ImGui.EndChild();
            }

            // construct chatlog.
            ImGui.BeginChild(MainWindow.ID_PANEL_RIGHT);
            ImGui.BeginGroup();
            // for all chat entries;
            foreach (var c in Plugin.ChatEntries)
            {
                // if we are displaying this type of message;
                if (Plugin.Config.ChannelToggles[c.Value.Type] == true)
                {
                    // and if the player is among the tracked;
                    if (Plugin.Players.Find(p => p.Name == c.Value.Sender.Name) != null)
                    {
                        MainWindow.ShowMessage(c);
                    }
                }
            }
            ImGui.EndGroup();
            ImGui.EndChild();

            ImGui.EndTabItem();
        }

    }

    /// <summary>
    /// Adds a selectable player group to the parent element.
    /// </summary>
    private void addPlayerGroup(KeyValuePair<String, List<Player>> pair)
    {
        ImGui.BeginGroup();
        if (ImGui.Selectable(pair.Key, true, ImGuiSelectableFlags.None))
        {
            // TODO verify what happens here?
            MainWindow.open = true;
        }
        ImGui.EndGroup();
    }

}
