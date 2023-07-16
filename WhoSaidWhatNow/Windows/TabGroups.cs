using System;
using System.Collections.Generic;
using System.Numerics;

using Dalamud.Interface;

using ImGuiNET;

using WhoSaidWhatNow;
using WhoSaidWhatNow.Objects;
using WhoSaidWhatNow.Windows;

public class TabGroups
{

    public TabGroups()
    {

        if (ImGui.BeginTabItem("Groups"))
        {
            ImGui.BeginTabBar("###groups");
            // populate the list of selectable groups.
            for (var i = 0; i < Plugin.Groups.Count; i++)
            {
                var g = Plugin.Groups[i];
                if (ImGui.BeginTabItem("Group " + i))
                {
                    ImGui.BeginChild(MainWindow.ID_PANEL_LEFT, new Vector2(205 * ImGuiHelpers.GlobalScale, 0), true); ;
                    foreach (var p in Plugin.Players)
                    {
                        var isActive = false;
                        g.TryGetValue(p, out isActive);
                        if (ImGui.Checkbox(p.Name, ref isActive))
                        {
                            isActive = true;
                            g[p] = true;
                            // TODO filter or don't
                        }
                    }
                    ImGui.EndChild();

                    // construct chatlog.
                    ImGui.BeginChild(MainWindow.ID_PANEL_RIGHT, new Vector2(0, 0), true);
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

            if (ImGui.TabItemButton("+", ImGuiTabItemFlags.Trailing | ImGuiTabItemFlags.NoTooltip))
            {
                Plugin.Groups.Add(new Dictionary<Player, Boolean>());
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
            ImGui.EndTabItem();

        }

    }

}