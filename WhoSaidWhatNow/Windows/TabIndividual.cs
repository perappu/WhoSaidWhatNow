using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Interface;
using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using WhoSaidWhatNow.Objects;
using WhoSaidWhatNow.Services;

namespace WhoSaidWhatNow.Windows;

public class TabIndividual
{
    public TabIndividual(MainWindow mainWindow)
    {

        if (ImGui.BeginTabItem("Individual"))
        {

            //janky handling for if we're coming back to this tab from the groups tab
            if (Plugin.SelectedPlayer is null)
            {
                mainWindow.toggleWindow(false);
            }

            // Creating left and right panels
            // you can redeclare BeginChild() with the same ID to add things to them, which we do for chatlog
            ImGui.BeginChild(MainWindow.ID_PANEL_LEFT, new Vector2(230 * ImGuiHelpers.GlobalScale, 0), true, ImGuiWindowFlags.MenuBar);

            if (ImGui.BeginMenuBar())
            {
                ImGui.BeginDisabled(!(Plugin.TargetManager.Target != null && Plugin.TargetManager.Target.ObjectKind == ObjectKind.Player));
                if (ImGui.MenuItem("Add Target"))
                {
                    PlayerService.AddPlayer(Plugin.TargetManager.Target);
                }
                ImGui.EndDisabled();

                if (Plugin.SelectedPlayer is not null)
                {
                    ImGui.BeginDisabled(Plugin.SelectedPlayer.RemoveDisabled);
                    if (ImGui.MenuItem("Remove Target"))
                    {
                        mainWindow.RemovePlayer();
                    }
                    ImGui.EndDisabled();
                }

                    ImGui.EndMenuBar();
            }

            ImGui.EndChild();
            ImGui.SameLine();
            ImGui.BeginChild(MainWindow.ID_PANEL_RIGHT, new Vector2(0, 0), true);
            ImGui.EndChild();

            //Populating selectable list
            foreach (var p in Plugin.Players)
            {
                ImGui.BeginChild(MainWindow.ID_PANEL_LEFT);
                mainWindow.AddPlayerSelectable(p);
                ImGui.EndChild();
            }

            // Build the chat log
            // it's worth noting all of this stuff stays in memory and is only hidden when it's "closed"
            ImGui.BeginChild(MainWindow.ID_PANEL_RIGHT);
            ImGui.BeginGroup();

            if (Plugin.SelectedPlayer is not null)
            {
                foreach (var c in from KeyValuePair<DateTime, ChatEntry> c in Plugin.ChatEntries
                                  where Plugin.Config.ChannelToggles[c.Value.Type] == true && c.Value.Sender.Name.Contains(Plugin.SelectedPlayer.Name)
                                  select c)
                {
                    MainWindow.ShowMessage(c);
                }
            } 
            ImGui.EndGroup();

            if (Plugin.Config.AutoscrollOnOpen && MainWindow.justOpened)
            {
                //i don't understand math, make this actually work better
                ImGui.SetScrollHereY(0.999f);
                MainWindow.justOpened = false;
            }

            ImGui.EndChild();
            ImGui.EndTabItem();
        }

    }


}
