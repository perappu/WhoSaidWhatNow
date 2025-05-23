using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility;
using WhoSaidWhatNow.Objects;
using WhoSaidWhatNow.Utils;
using Dalamud.Interface.Utility.Raii;

namespace WhoSaidWhatNow.Windows;

public class TabIndividual
{
    public TabIndividual(MainWindow mainWindow, Plugin plugin)
    {

        if (ImGui.BeginTabItem("Individual"))
        {
            mainWindow.individualOpen = true;
            var players = new Player[Plugin.Players.Count];

            //janky handling for if we're coming back to this tab from the groups tab
            if (Plugin.SelectedPlayer is null)
            {
                mainWindow.toggleWindow(false);
            }

            // Creating left and right panels
            // you can redeclare BeginChild() with the same ID to add things to them, which we do for chatlog
            ImGui.BeginChild(MainWindow.ID_PANEL_LEFT, new Vector2(230 * ImGuiHelpers.GlobalScale, 0), true, ImGuiWindowFlags.MenuBar);

            ImGui.InputTextWithHint("", "Filter by name...", ref Plugin.FilterPlayers, 40, ImGuiInputTextFlags.EnterReturnsTrue);
            Plugin.Players.Where(p => p.Name.ToLower().Contains(Plugin.FilterPlayers.ToLower())).ToList().CopyTo(players);

            if (ImGui.BeginMenuBar())
            {
                //i can not believe i have to wrap this in a group to show the hover when the button is disabled

                //button to add targetmanager targeted player
                ImGui.BeginGroup();
                ImGui.BeginDisabled(!(Plugin.TargetManager.Target != null && Plugin.TargetManager.Target.ObjectKind == ObjectKind.Player));
                //push font to make our menus with FA icons
                using (ImRaii.PushFont(UiBuilder.IconFont))
                {
                    if (ImGui.MenuItem(FontAwesomeIcon.UserPlus.ToIconString()))
                    {
                        PlayerUtils.AddPlayer(Plugin.TargetManager.Target);
                    }
                }
                ImGui.EndDisabled();
                ImGui.EndGroup();
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Add currently targeted player");
                }

                // button to remove selected player
                ImGui.BeginGroup();
                ImGui.BeginDisabled(Plugin.SelectedPlayer == null || Plugin.SelectedPlayer.RemoveDisabled);
                using (ImRaii.PushFont(UiBuilder.IconFont))
                {
                    if (ImGui.MenuItem(FontAwesomeIcon.UserMinus.ToIconString()))
                    {
                        mainWindow.RemovePlayerGUI();
                    }
                }
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
                using (ImRaii.PushFont(UiBuilder.IconFont))
                {
                    if (ImGui.MenuItem(FontAwesomeIcon.UserSlash.ToIconString()))
                    {
                        ConfigurationUtils.refresh();
                    }
                }
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
                using (ImRaii.PushFont(UiBuilder.IconFont))
                {
                    ImGui.BeginDisabled(Plugin.SelectedPlayer == null);
                    if (ImGui.MenuItem(FontAwesomeIcon.Save.ToIconString()))
                    {
                        if (Plugin.SelectedPlayer != null)
                        {
                            FileUtils.OpenFileDialog(Plugin.SelectedPlayer.Name);
                        }
                    }
                    ImGui.EndDisabled();
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Save log to .txt file");
                }
                ImGui.BeginGroup();
                ImGui.BeginDisabled(Plugin.SelectedPlayer == null || Plugin.SelectedPlayer.RemoveDisabled);
                using (ImRaii.PushFont(UiBuilder.IconFont))
                {
                    if (ImGui.MenuItem(FontAwesomeIcon.UserCheck.ToIconString()))
                    {
                        if (Plugin.SelectedPlayer != null)
                        {
                            PlayerUtils.AddTrackedPlayer(new TrackedPlayer(Plugin.SelectedPlayer.Name, Plugin.SelectedPlayer.Server, Plugin.SelectedPlayer.NameColor));
                        }
                    }
                }
                ImGui.EndDisabled();
                ImGui.EndGroup();
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Add player to favorites");
                }
                ImGui.EndMenuBar();
            }
            ImGui.EndChild();

            ImGui.BeginChild(MainWindow.ID_PANEL_LEFT);
            //Reopen left window, populate selectable list
            try
            {
                mainWindow.AddPlayerSelectable(Plugin.CurrentPlayer!);
            }
            catch (Exception e)
            {
                Plugin.Logger.Debug("Could not draw player ${p} to selectables.");
                Plugin.Logger.Debug(e.ToString());
            }
            foreach (var p in players)
            {
                
                // catch NPEs?
                try
                {
                    mainWindow.AddPlayerSelectable(p);
                }
                catch (Exception e)
                {
                    Plugin.Logger.Debug("Could not draw player ${p} to selectables.");
                    Plugin.Logger.Debug(e.ToString());
                }
                
            }
            ImGui.EndChild();

            // Reopen right window, build the chat log
            // it's worth noting all of this stuff stays in memory and is only hidden when it's "closed"
            ImGui.BeginChild(MainWindow.ID_PANEL_RIGHT);

            ImGui.BeginGroup();
            if (Plugin.SelectedPlayer is not null)
            {
                foreach (var c in from KeyValuePair<DateTime, ChatEntry> c in Plugin.ChatEntries
                                  where Plugin.Config.ChannelToggles[c.Value.Type] && c.Value.Sender.Name.Contains(Plugin.SelectedPlayer.Name)
                                  select c)
                {
                    ChatUtils.ShowMessage(c);
                }
            }
            ImGui.EndGroup();

            if (Plugin.Config.AutoscrollOnOpen && MainWindow.justOpened)
            {
                //i don't understand math, make this actually work better
                ImGui.SetScrollHereY(1);
                MainWindow.justOpened = false;
            }

            ImGui.EndChild();
            ImGui.EndTabItem();
        }

    }


}
