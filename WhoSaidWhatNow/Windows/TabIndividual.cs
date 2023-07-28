using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using WhoSaidWhatNow.Objects;
using WhoSaidWhatNow.Utils;

namespace WhoSaidWhatNow.Windows;

public class TabIndividual
{
    public TabIndividual(MainWindow mainWindow, Plugin plugin)
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
                //i can not believe i have to wrap this in a group to show the hover when the button is disabled

                //button to add targetmanager targeted player
                ImGui.BeginGroup();
                ImGui.BeginDisabled(!(Plugin.TargetManager.Target != null && Plugin.TargetManager.Target.ObjectKind == ObjectKind.Player));
                //push font to make our menus with FA icons
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.MenuItem(FontAwesomeIcon.UserPlus.ToIconString()))
                {
                    PlayerUtils.AddPlayer(Plugin.TargetManager.Target);
                }
                ImGui.PopFont();
                ImGui.EndDisabled();
                ImGui.EndGroup();
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Add currently targeted player");
                }

                // button to remove selected player
                ImGui.BeginGroup();
                ImGui.BeginDisabled(Plugin.SelectedPlayer == null || Plugin.SelectedPlayer.RemoveDisabled);
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.MenuItem(FontAwesomeIcon.UserMinus.ToIconString()))
                {
                    mainWindow.RemovePlayer();
                }
                ImGui.PopFont();
                ImGui.EndDisabled();
                ImGui.EndGroup();
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Remove currently opened player");
                }

                ImGui.SameLine(ImGui.GetWindowWidth() - 40);

                // button to remove all manually tracked players, does a refresh() behind the scenes so it's actually rebuilding the list entirely
                ImGui.BeginGroup();
                ImGui.BeginDisabled(!ImGui.GetIO().KeyShift);
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.MenuItem(FontAwesomeIcon.UserSlash.ToIconString()))
                {
                    ConfigurationUtils.refresh();
                }
                ImGui.PopFont();
                ImGui.EndDisabled();
                ImGui.EndGroup();
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Hold shift to clear all manually added players");
                }


                ImGui.EndMenuBar();
            }

            ImGui.EndChild();
            ImGui.SameLine();

            //initialize right window with menu bar
            ImGui.BeginChild(MainWindow.ID_PANEL_RIGHT, new Vector2(0, 0), true, ImGuiWindowFlags.MenuBar);

            if (ImGui.BeginMenuBar())
            {
                //push font to make our menus with FA icons
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.MenuItem(FontAwesomeIcon.Save.ToIconString()))
                {
                    FileUtils.OpenFileDialog(plugin, Plugin.SelectedPlayer.Name);

                }
                ImGui.PopFont();

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Save log to .txt file");
                }

                //push font to make our menus with FA icons

                ImGui.BeginGroup();
                ImGui.BeginDisabled(Plugin.SelectedPlayer == null || Plugin.SelectedPlayer.RemoveDisabled);
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.MenuItem(FontAwesomeIcon.UserCheck.ToIconString()))
                {
                    PlayerUtils.AddTrackedPlayer(new Tuple<string,string>(Plugin.SelectedPlayer.Name, Plugin.SelectedPlayer.Server));
                }
                ImGui.PopFont();
                ImGui.EndDisabled();
                ImGui.EndGroup();
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Add player to favorites");
                }
                ImGui.EndMenuBar();
            }
            ImGui.EndChild();


            //Reopen left window, populate selectable list
            foreach (var p in Plugin.Players)
            {
                ImGui.BeginChild(MainWindow.ID_PANEL_LEFT);
                mainWindow.AddPlayerSelectable(p);
                ImGui.EndChild();
            }

            // Reopen right window, build the chat log
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
