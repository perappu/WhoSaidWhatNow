using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok;
using ImGuiNET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using WhoSaidWhatNow.Objects;
using static Lumina.Data.Parsing.Layer.LayerCommon;

namespace WhoSaidWhatNow.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin plugin;
    public List<Player> Players;
    public SortedList<DateTime, ChatEntry> ChatEntries;
    private TargetManager targetManager;
    private Player? selectedPlayer = null;
    private string errorMessage = string.Empty;
    private bool open = false;
    private bool btnDisable = true;

    //define constraints for when the right panel is open/closed
    //TODO: set minimum/maximum when "closed" but infinitely resizable when expanded
    WindowSizeConstraints openConstraints = new WindowSizeConstraints
    {
        MinimumSize = new Vector2(500, 330),
        MaximumSize = new Vector2(1000, 1000)
    };
    WindowSizeConstraints closedConstraints = new WindowSizeConstraints
    {
        MinimumSize = new Vector2(220, 330),
        MaximumSize = new Vector2(220, 330)
    };

    public MainWindow(Plugin plugin, List<Player> trackedPlayers, SortedList<DateTime, ChatEntry> chatEntries, TargetManager targetManager)
        : base("Who Said What Now", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.MenuBar)
    {
        this.SizeConstraints = closedConstraints;

        this.plugin = plugin;
        this.Players = trackedPlayers;
        this.targetManager = targetManager;
        this.ChatEntries = chatEntries;
    }

    //I honestly have no idea how to dispose of windows correctly
    //TODO: make sure this is ok?
    public void Dispose() { }

    //IsTargetPlayer() checks to see if the current target is a player
    //if so it'll make the new player object, if not return false
    private bool IsTargetPlayer()
    {
        GameObject? target = null;
        target = targetManager.Target;

        if (target == null || target.ObjectKind != ObjectKind.Player)
        {
            return false;
        }
        else if (Players.Any(x => x.ID == target.ObjectId))
        {
            return false;
        }
        else
        {
            AddNewPlayer(target);
            return true;
        }
    }

    //AddNewPlayer() creates a player based on a game object and adds it to the main list
    private uint AddNewPlayer(GameObject gameObject)
    {
        Players.Add(new Player(gameObject));
        return gameObject.ObjectId;
    }

    //ShowMessage() creates an ImGui text wrapped given a player and a keyvalue datetime chatentry
    private void ShowMessage(KeyValuePair<DateTime, ChatEntry> c)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Configuration.ChatColors[c.Value.Type]);
        string tag = Configuration.Formats[c.Value.Type];
        ImGui.TextWrapped(c.Value.CreateMessage(tag));
        ImGui.PopStyleColor();
    }

    //Draw() the main window
    public override void Draw()
    {
        //Creating menu bar
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.MenuItem("Open Settings"))
            {
                this.plugin.DrawConfigUI();
            }
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 0f, 0f, 1f));
            ImGui.Text(this.plugin.configuration.IsOn == true ? "On" : "Off");
            ImGui.PopStyleColor();

            ImGui.EndMenuBar();
        }

        //INDIVIDUAL TAB
        ImGui.BeginTabBar("###WhoSaidWhatNow_Tab_Bar");
        if (ImGui.BeginTabItem("Individual"))
        {

            // Creating left and right panels
            // you can redeclare BeginChild() with the same ID to add things to them, which we do for chatlog
            ImGui.BeginChild("###WhoSaidWhatNow_LeftPanel_Child", new Vector2(205 * ImGuiHelpers.GlobalScale, 0), true, ImGuiWindowFlags.MenuBar);

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.MenuItem("Add Target"))
                {
                    IsTargetPlayer();
                }

                ImGui.BeginDisabled(selectedPlayer is null);
                if (ImGui.MenuItem("Remove Target"))
                {
                    if (selectedPlayer is not null)
                    {
                        Players.Remove(selectedPlayer);
                        open = false;
                        selectedPlayer = null;
                        //we have to manually close the window here
                        this.SizeConstraints = closedConstraints;
                    }
                }
                ImGui.EndDisabled();
                ImGui.EndMenuBar();
            }

            ImGui.EndChild();
            ImGui.SameLine();
            ImGui.BeginChild("###WhoSaidWhatNow_RightPanel_Child", new Vector2(0, 0), true);
            ImGui.EndChild();

            //Populating selectable list
            for (var i = 0; Players.Count > i; i++)
            {
                Player player = Players[i];

                ImGui.BeginChild("###WhoSaidWhatNow_LeftPanel_Child");
                ImGui.BeginGroup();

                if (ImGui.Selectable("###WhoSaidWhatNow_Player_Selectable_" + player.ID, true, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        // ignored
                        // I'm honestly not sure why the original snooper had this but I'm going to guess
                        // it's to make sure it explicitly ignores people buttonmashing
                    }

                    //if we're clicking on the current player and the window is already open, close it
                    if (open == true && selectedPlayer != null && selectedPlayer.ID == player.ID)
                    {
                        open = false;
                        selectedPlayer = null;
                    }
                    // open content in right panel
                    else
                    {
                        open = true;
                        selectedPlayer = player;
                    }

                }

                //TODO: padding is a bit wacky on the selectable and clicks with the one above it, either remove the padding or add margins
                ImGui.SameLine();
                ImGui.Text(player.Name);
                ImGui.EndGroup();
                ImGui.EndChild();

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

            // Build the chat log
            // it's worth noting all of this stuff stays in memory and is only hidden when it's "closed"
            ImGui.BeginChild("###WhoSaidWhatNow_RightPanel_Child");
            ImGui.BeginGroup();
            if (selectedPlayer is not null)
            {
                foreach (KeyValuePair<DateTime, ChatEntry> c in ChatEntries)
                {
                    if (plugin.configuration.ChannelToggles[c.Value.Type] == true && c.Value.Sender.Name.Contains(selectedPlayer.Name))
                    {
                        ShowMessage(c);
                    }
                }
            }
            ImGui.EndGroup();

            if (plugin.configuration.AutoScroll)
            {
                //i don't understand math, make this actually work better
                ImGui.SetScrollHereY(1.0f);
            }
            
            ImGui.EndChild();
            ImGui.EndTabItem();
        }

        //GROUP TAB
        if (ImGui.BeginTabItem("Groups"))
        {

            // Creating left and right panels
            // you can redeclare BeginChild() with the same ID to add things to them, which we do for chatlog
            ImGui.BeginChild("###WhoSaidWhatNow_LeftPanel_Child", new Vector2(205 * ImGuiHelpers.GlobalScale, 0), true);
            ImGui.EndChild();
            ImGui.SameLine();
            ImGui.BeginChild("###WhoSaidWhatNow_RightPanel_Child", new Vector2(0, 0), true);
            ImGui.EndChild();

            ImGui.BeginChild("###WhoSaidWhatNow_LeftPanel_Child");
            ImGui.BeginGroup();
            //make the "all" selectable
            if (ImGui.Selectable("###WhoSaidWhatNow_Player_Selectable_GroupAll", true, ImGuiSelectableFlags.AllowDoubleClick))
            {
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    // ignored
                    // I'm honestly not sure why the original snooper had this but I'm going to guess
                    // it's to make sure it explicitly ignores people buttonmashing
                }

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
            //TODO: padding is a bit wacky on the selectable and clicks with the one above it, either remove the padding or add margins
            ImGui.SameLine();
            ImGui.Text("All Tracked Players");
            ImGui.EndGroup();
            ImGui.EndChild();

            // Build the chat log
            // it's worth noting all of this stuff stays in memory and is only hidden when it's "closed"
            ImGui.BeginChild("###WhoSaidWhatNow_RightPanel_Child");
            ImGui.BeginGroup();
            foreach (KeyValuePair<DateTime, ChatEntry> c in ChatEntries)
            {
                if (plugin.configuration.ChannelToggles[c.Value.Type] == true)
                {
                    //this is sort of gross but it's only necessary for a "get all" type thing, otherwise we will know the exact players
                    selectedPlayer = Players.Find(x => x.Name == c.Value.Sender.Name);
                    if (selectedPlayer != null) { 
                        ShowMessage(c);
                    }
                }
            }
            ImGui.EndGroup();
            ImGui.EndChild();
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }

}
