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

    public TabGroups(MainWindow main)
    {

        if (ImGui.BeginTabItem("Groups"))
        {
            main.toggleWindow(true);

            ImGui.BeginTabBar("###groups");
            // populate the list of selectable groups.
            for (var i = 0; i < Plugin.Groups.Count; i++)
            {
                var g = Plugin.Groups[i];
                var name = "Group " + (i + 1);
                if (ImGui.BeginTabItem(name))
                {
                    if (ImGui.BeginPopupContextItem())
                    {
                        ImGui.InputText("##edit", ref name, (uint)name.Length * sizeof(Char));
                        if (i > 0)
                        {
                            ImGui.Button("Delete");
                        }
                        ImGui.EndPopup();
                    }
                    ImGui.BeginChild(MainWindow.ID_PANEL_LEFT, new Vector2(205 * ImGuiHelpers.GlobalScale, 0), true);
                    foreach (var p in Plugin.Players)
                    {
                        bool isActive;
                        g.TryGetValue(p, out isActive);
                        ImGui.Checkbox(p.Name, ref isActive);
                        g[p] = isActive;
                    }
                    ImGui.EndChild();
                    ImGui.SameLine();

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
                            var p = Plugin.Players.Find(p => c.Value.Sender.Name.Contains(p.Name));
                            if (g[p!])
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
