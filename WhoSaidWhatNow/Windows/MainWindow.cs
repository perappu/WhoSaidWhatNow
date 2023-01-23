using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using WhoSaidWhatNow.Objects;
using static Lumina.Data.Parsing.Layer.LayerCommon;

namespace WhoSaidWhatNow.Windows;

public class MainWindow : Window, IDisposable
{
    internal static bool open = false;
    internal const String ID_PANEL_LEFT = "###WhoSaidWhatNow_LeftPanel_Child";
    internal const String ID_PANEL_RIGHT = "###WhoSaidWhatNow_RightPanel_Child";

    private readonly WindowSizeConstraints closedConstraints = new WindowSizeConstraints
    {
        MinimumSize = new Vector2(220, 330),
        MaximumSize = new Vector2(220, 330)
    };
    private readonly WindowSizeConstraints openConstraints = new WindowSizeConstraints
    {
        MinimumSize = new Vector2(600, 330),
        MaximumSize = new Vector2(int.MaxValue, int.MaxValue)
    };

    public MainWindow() : base("Who Said What Now", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.MenuBar)
    {
        this.SizeConstraints = closedConstraints;
    }

    // I honestly have no idea how to dispose of windows correctly
    // TODO: make sure this is ok?
    public void Dispose() { }

    /// <summary>
    /// If current target is player, save to internal list.
    /// </summary>
    /// <returns>True if successful.</returns>
    private bool AddPlayer()
    {
        if (Plugin.TargetManager.Target != null)
        {
            GameObject target = Plugin.TargetManager.Target;

            if (target == null || target.ObjectKind != ObjectKind.Player)
            {
                return false;
            }
            else if (Plugin.Players.Any(x => x.ID == target.ObjectId))
            {
                return false;
            }
            else
            {
                Plugin.Players.Add(new Player(target));
                return true;
            }
        }
        else
        {
            return false;
        }
    }


    private void RemovePlayer()
    {
        if (Plugin.SelectedPlayer is not null)
        {
            Plugin.Players.Remove(Plugin.SelectedPlayer);
            open = false;
            Plugin.SelectedPlayer = null;
            //we have to manually close the window here
            this.SizeConstraints = closedConstraints;
        }
    }

    /// <summary>
    /// Properly formats the passed data as a chat message and adds it to the log.
    /// </summary>
    internal static void ShowMessage(KeyValuePair<DateTime, ChatEntry> c)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Configuration.ChatColors[c.Value.Type]);
        string tag = Configuration.Formats[c.Value.Type];
        ImGui.TextWrapped(c.Value.CreateMessage(tag));
        ImGui.PopStyleColor();
    }

    /// <summary>
    /// Toggles window being opened/closed based on current state of open variable
    /// </summary>
    /// <param name="player"></param>
    internal void ToggleWindowOpen(Player? player)
    {

        //If player is null, then we just open/close the window. Otherwise we set the selected player to the passed player
        if (player != null)
        {
            //if we're clicking on the current player and the window is already open, close it
            if (open == true && Plugin.SelectedPlayer != null && Plugin.SelectedPlayer.ID == player.ID)
            {
                open = false;
                Plugin.SelectedPlayer = null;
            }
            // open content in right panel
            else
            {

                open = true;
                Plugin.SelectedPlayer = player;
            }
        }
        else
        {
            //if we're clicking on the current player and the window is already open, close it
            if (open == true)
            {
                open = false;
            }
            // open content in right panel
            else
            {
                open = true;
            }
        }

        //Stuff the selectable should do on click
        if (open)
        {

            this.SizeConstraints = openConstraints;
        }
        else
        {
            this.SizeConstraints = closedConstraints;
        }
    }

    /// <summary>
    /// Adds the player as a selectable element to the parent.
    /// </summary>
    /// <param name="player">Player to add.</param>
    private void AddPlayerSelectable(Player player)
    {
        ImGui.BeginGroup();

        if (ImGui.Selectable("###WhoSaidWhatNow_Player_Selectable_" + player.ID, true, ImGuiSelectableFlags.AllowDoubleClick))
        {
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                // ignored
                // I'm honestly not sure why the original snooper had this but I'm going to guess
                // it's to make sure it explicitly ignores people buttonmashing
            }

            ToggleWindowOpen(player);
        }

        //TODO: padding is a bit wacky on the selectable and clicks with the one above it, either remove the padding or add margins
        ImGui.SameLine();
        ImGui.Text(player.Name);
        ImGui.EndGroup();
    }

    /// <summary>
    /// Add a context menu for tracked players to the parent element.<br/>
    /// Only handles creating or adding to groups; defer removing to the groups window.<br/>
    /// TODO needs functional testing.
    /// </summary>
    private void ContextMenuPlayer(Player player)
    {
        if (ImGui.BeginPopupContextItem())
        {
            if (ImGui.Selectable("Add to group..."))
            {
                if (ImGui.BeginMenu("MenuGroups"))
                {
                    // User can either create a new group...
                    if (ImGui.Selectable("Create Group"))
                    {
                        Plugin.Groups.Add("New Group", new List<Player> { player });
                        ImGui.CloseCurrentPopup();

                    }
                    ImGui.Separator();
                    // ... or add to an existing group.
                    foreach (var (k, v) in Plugin.Groups)
                    {
                        if (ImGui.Selectable($"{k}"))
                        {
                            v.Add(player);
                            ImGui.CloseCurrentPopup();
                        }
                    }
                }
                ImGui.EndPopup();
            }
        }
    }

    //Draw() the main window
    public override void Draw()
    {
        //Creating menu bar
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.MenuItem("Open Settings"))
            {
                Plugin.DrawConfigUI();
            }
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 0f, 0f, 1f));
            ImGui.Text(Configuration.IsOn == true ? "On" : "Off");
            ImGui.PopStyleColor();

            ImGui.EndMenuBar();
        }

        //INDIVIDUAL TAB
        ImGui.BeginTabBar("###WhoSaidWhatNow_Tab_Bar");
        if (ImGui.BeginTabItem("Individual"))
        {

            // Creating left and right panels
            // you can redeclare BeginChild() with the same ID to add things to them, which we do for chatlog
            ImGui.BeginChild(ID_PANEL_LEFT, new Vector2(205 * ImGuiHelpers.GlobalScale, 0), true, ImGuiWindowFlags.MenuBar);

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.MenuItem("Add Target"))
                {
                    AddPlayer();
                }

                ImGui.BeginDisabled(Plugin.SelectedPlayer is null);
                if (ImGui.MenuItem("Remove Target"))
                {
                    RemovePlayer();
                }
                ImGui.EndDisabled();
                ImGui.EndMenuBar();
            }

            ImGui.EndChild();
            ImGui.SameLine();
            ImGui.BeginChild(ID_PANEL_RIGHT, new Vector2(0, 0), true);
            ImGui.EndChild();

            //Populating selectable list
            foreach (var p in Plugin.Players)
            {
                ImGui.BeginChild(ID_PANEL_LEFT);
                AddPlayerSelectable(p);
                ContextMenuPlayer(p);
                ImGui.EndChild();
            }

            // Build the chat log
            // it's worth noting all of this stuff stays in memory and is only hidden when it's "closed"
            ImGui.BeginChild(ID_PANEL_RIGHT);
            ImGui.BeginGroup();
            if (Plugin.SelectedPlayer is not null)
            {
                foreach (var c in from KeyValuePair<DateTime, ChatEntry> c in Plugin.ChatEntries
                                  where Configuration.ChannelToggles[c.Value.Type] == true && c.Value.Sender.Name.Contains(Plugin.SelectedPlayer.Name)
                                  select c)
                {
                    ShowMessage(c);
                }
            }
            ImGui.EndGroup();

            if (Configuration.AutoScroll)
            {
                //i don't understand math, make this actually work better
                ImGui.SetScrollHereY(1.0f);
            }

            ImGui.EndChild();
            ImGui.EndTabItem();
        }

        //GROUP TAB

    }

}
