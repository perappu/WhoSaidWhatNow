using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using WhoSaidWhatNow.Objects;
using WhoSaidWhatNow.Utils;

namespace WhoSaidWhatNow.Windows;

public class TabGroups
{
    private static int Counter = 1;

    public TabGroups(MainWindow main, Plugin plugin)
    {
        var playerList = PlayerUtils.GetCurrentAndPlayers();

        if (ImGui.BeginTabItem("Groups"))
        {
            main.individualOpen = false;
            main.toggleWindow(true);

            ImGui.BeginTabBar("###groups");
            
            // populate the list of selectable groups.
            foreach (var g in Plugin.Groups)
            {
                var index = g.Key;
                var group = g.Value;
                var name = group.NAME;
                var players = group.PLAYERS;
                if (ImGui.BeginTabItem($"{name}###Tab_{index}"))
                {
                    Plugin.SelectedGroup = index;
                    var filtered = new Player[playerList.Count];

                    if (ImGui.BeginPopupContextItem("###groupName"))
                    {
                        var input = String.Empty;
                        ImGui.InputTextWithHint($"##{index}", "Enter the group name...", ref name, 30);
                        Plugin.Groups[index] = (name, players);

                        if (ImGui.Button("Delete"))
                        {
                            Plugin.Groups.Remove(index);
                        }

                        ImGui.EndPopup();
                    }
                    ImGui.BeginChild(MainWindow.ID_PANEL_LEFT, new Vector2(205 * ImGuiHelpers.GlobalScale, 0), true);
                    ImGui.InputTextWithHint("", "Filter by name...", ref Plugin.FilterPlayers, 40, ImGuiInputTextFlags.EnterReturnsTrue);
                    playerList.Where(p => p.Name.ToLower().Contains(Plugin.FilterPlayers.ToLower())).ToList().CopyTo(filtered);
                    foreach (var p in filtered)
                    {
                        try
                        {
                            players.TryGetValue(p, out var isActive);
                            ImGui.Checkbox(p.Name, ref isActive);
                            players[p] = isActive;
                        }
                        catch { }
                    }
                    ImGui.EndChild();
                    ImGui.SameLine();

                    // construct chatlog.
                    ImGui.BeginChild(MainWindow.ID_PANEL_RIGHT, new Vector2(0, 0), true, ImGuiWindowFlags.MenuBar);

                    // add menu bar with chat log button
                    if (ImGui.BeginMenuBar())
                    {
                        //push font to make our menus with FA icons
                        using (ImRaii.PushFont(UiBuilder.IconFont))
                        {
                            if (ImGui.MenuItem(FontAwesomeIcon.Save.ToIconString()))
                            {
                                FileUtils.DialogSaveGroup(name, players);
                            }
                        }
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("Save log to .txt file");
                        }
                        ImGui.EndMenuBar();
                    }

                    ImGui.EndChild();

                    ImGui.BeginChild(MainWindow.ID_PANEL_RIGHT);
                    ImGui.BeginGroup();
                    // for all chat entries;

                    foreach (var c in Plugin.ChatEntries)
                    {
                        try
                        {
                            // if we are displaying this type of message;
                            if (Plugin.Config.ChannelToggles[c.Value.Type] == true)
                            {
                                // and if the player is among the tracked;
                                var p = playerList.Find(p => c.Value.Sender.Name.Contains(p.Name));
                                if (players[p!])
                                {
                                    ChatUtils.ShowMessage(c);
                                }
                            }
                        }
                        catch
                        {
                            // trying to access a player that doesn't exist, just ignore
                        }
                    }
                    ImGui.EndGroup();
                    ImGui.EndChild();


                    ImGui.EndTabItem();

                }
            }

            if (ImGui.TabItemButton("+", ImGuiTabItemFlags.Trailing | ImGuiTabItemFlags.NoTooltip))
            {
                Counter++;
                Plugin.Groups.Add($"{Counter}", ($"Group {Counter}", playerList.ToDictionary(p => p, p => false)));
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
            ImGui.EndTabItem();

        }

    }

}
