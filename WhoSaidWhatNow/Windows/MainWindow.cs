using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.DrunkenToad;

using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using WhoSaidWhatNow.Objects;
using LiteDB;
using Dalamud.Logging;

namespace WhoSaidWhatNow.Windows;

public class MainWindow : Window, IDisposable
{
    internal static bool open = false;
    internal const String ID_PANEL_LEFT = "###WhoSaidWhatNow_LeftPanel_Child";
    internal const String ID_PANEL_RIGHT = "###WhoSaidWhatNow_RightPanel_Child";

    private readonly WindowSizeConstraints closedConstraints = new WindowSizeConstraints
    {
        MinimumSize = new Vector2(250, 330),
        MaximumSize = new Vector2(250, 330)
    };
    private readonly WindowSizeConstraints openConstraints = new WindowSizeConstraints
    {
        MinimumSize = new Vector2(700, 330),
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
    internal bool AddPlayer()
    {
        if (Plugin.TargetManager.Target != null)
        {
            GameObject target = Plugin.TargetManager.Target;

            if (target == null || target.ObjectKind != ObjectKind.Player)
            {
                return false;
            }
            else if (Plugin.Players.Any(x => x.Name.Equals(target.Name.ToString())))
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

    internal void RemovePlayer()
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

    internal void AddAllInRange()
    {
        GameObject[]? playerArray = Plugin.ObjectTable.ToArray();
        List<PlayerCharacter?> nearbyPlayers = playerArray!.Where(actor => actor.IsValidPlayerCharacter() && actor.ObjectId != Plugin.ClientState.LocalPlayer!.ObjectId).Select(actor => actor as PlayerCharacter).ToList();

        foreach (PlayerCharacter nearbyPlayer in nearbyPlayers)
        {
            if (!Plugin.Players.Any(x => x.Name.Equals(nearbyPlayer.Name.ToString())))
            {
                PluginLog.LogDebug("nearby player found " + nearbyPlayer.Name.ToString());
                Plugin.Players.Add(new Player(nearbyPlayer));
            }
        }
    }

    /// <summary>
    /// Properly formats the passed data as a chat message and adds it to the log.
    /// </summary>
    internal static void ShowMessage(KeyValuePair<DateTime, ChatEntry> c)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Plugin.Config.ChatColors[c.Value.Type]);
        string tag = Plugin.Config.Formats[c.Value.Type];
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
            if (open == true && Plugin.SelectedPlayer != null && Plugin.SelectedPlayer.Name.Equals(player.Name))
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
    internal void AddPlayerSelectable(Player player)
    {
        ImGui.BeginGroup();

        if (ImGui.Selectable("###WhoSaidWhatNow_Player_Selectable_" + player.Name, true, ImGuiSelectableFlags.None))
        {
            ToggleWindowOpen(player);
        }

        //TODO: padding is a bit wacky on the selectable and clicks with the one above it, either remove the padding or add margins
        ImGui.SameLine();
        if(player.Name == Plugin.Config.CurrentPlayer)
        {
            ImGui.Text(" " + player.Name + " (YOU)");
        } else if (player.RemoveDisabled == true)
        {
            ImGui.Text(" " + player.Name);
        } else
        {
            ImGui.Text(player.Name);
        }
        ImGui.EndGroup();
    }

    /// <summary>
    /// Add a context menu for tracked players to the parent element.<br/>
    /// Only handles creating or adding to groups; defer removing to the groups window.<br/>
    /// TODO needs functional testing.
    /// </summary>
    internal void ContextMenuPlayer(Player player)
    {
        if (ImGui.BeginPopupContextItem())
        {
            if (ImGui.BeginMenu("Add to group..."))
            {
                // User can either create a new group...
                if (ImGui.Selectable("Create Group"))
                {
                    // TODO make name somewhat generated, otherwise key error
                    Plugin.Groups.Add("New Group", new List<Player> { player });
                    ImGui.CloseCurrentPopup();

                }
                ImGui.Separator();
                // ... or add to an existing group.
                foreach (var (k, v) in Plugin.Groups)
                {
                    if (ImGui.Selectable(k))
                    {
                        v.Add(player);
                        ImGui.CloseCurrentPopup();
                    }
                }
                ImGui.EndMenu();
            }
            ImGui.EndPopup();
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
                Plugin.ToggleConfigUI();
            }

            if (ImGui.MenuItem("Add All in Range"))
            {
                 AddAllInRange();
            }

            ImGui.PushStyleColor(ImGuiCol.Text, Plugin.Config.Enabled == true ? Dalamud.Interface.Colors.ImGuiColors.HealerGreen : Dalamud.Interface.Colors.ImGuiColors.DalamudRed);
            ImGui.Text(Plugin.Config.Enabled == true ? "On" : "Off");
            ImGui.PopStyleColor();

            ImGui.EndMenuBar();
        }

        //INDIVIDUAL TAB
        ImGui.BeginTabBar("###WhoSaidWhatNow_Tab_Bar");
        var individual = new TabIndividual(this);

        //GROUP TAB
        var groups = new TabGroups();

        ImGui.EndTabBar();

    }

}
