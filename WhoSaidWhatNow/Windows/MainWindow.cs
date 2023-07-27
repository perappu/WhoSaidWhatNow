using Dalamud.DrunkenToad;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using WhoSaidWhatNow.Objects;
using WhoSaidWhatNow.Utils;

namespace WhoSaidWhatNow.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    public static bool ChatOpen = false;
    internal const String ID_PANEL_LEFT = "###WhoSaidWhatNow_LeftPanel_Child";
    internal const String ID_PANEL_RIGHT = "###WhoSaidWhatNow_RightPanel_Child";

    //janky solution for autoscroll for now...
    public static bool justOpened = false;

    public readonly WindowSizeConstraints closedConstraints = new WindowSizeConstraints
    {
        MinimumSize = new Vector2(250, 330),
        MaximumSize = new Vector2(250, int.MaxValue)
    };
    public readonly WindowSizeConstraints openConstraints = new WindowSizeConstraints
    {
        MinimumSize = new Vector2(700, 330),
        MaximumSize = new Vector2(int.MaxValue, int.MaxValue)
    };

    public MainWindow(Plugin plugin) : base("Who Said What Now", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.MenuBar)
    {
        this.SizeConstraints = closedConstraints;
        this.plugin = plugin;
    }

    // I honestly have no idea how to dispose of windows correctly
    // TODO: make sure this is ok?
    public void Dispose() { }

    public void RemovePlayer()
    {
        if (Plugin.SelectedPlayer is not null)
        {
            PlayerUtils.RemovePlayer(Plugin.SelectedPlayer);
            ChatOpen = false;
            Plugin.SelectedPlayer = null;
            //we have to manually close the window here
            this.SizeConstraints = closedConstraints;
        }
    }

    public void AddAllInRange()
    {
        GameObject[]? playerArray = Plugin.ObjectTable.ToArray();
        List<PlayerCharacter?> nearbyPlayers = playerArray!.Where(x => x.IsValidPlayerCharacter() && x.ObjectId != Plugin.ClientState.LocalPlayer!.ObjectId).Select(x => x as PlayerCharacter).ToList();

        foreach (PlayerCharacter? nearbyPlayer in nearbyPlayers)
        {
            if (!Plugin.Players.Any(x => x.Name.Equals(nearbyPlayer.Name.ToString())))
            {
                PluginLog.LogDebug("nearby player found " + nearbyPlayer.Name.ToString());
                PlayerUtils.AddPlayer(nearbyPlayer);
            }
        }
    }

    /// <summary>
    /// Properly formats the passed data as a chat message and adds it to the log.
    /// </summary>
    public static void ShowMessage(KeyValuePair<DateTime, ChatEntry> c)
    {
        Tuple<string, string> tag = Plugin.Config.GUIFormats[c.Value.Type];

        //ChatUtils.ColoredText($"[{c.Value.Time.ToShortTimeString()}]", Plugin.Config.ChatColors[c.Value.Type], false);

        //if (tag.Item1 != String.Empty)
        //{
        //    ChatUtils.ColoredText(tag.Item1, Plugin.Config.ChatColors[c.Value.Type]);
        //}

        //if (c.Value.Type != Dalamud.Game.Text.XivChatType.StandardEmote)
        //{
        //    ChatUtils.ColoredText(c.Value.Sender.Name, c.Value.Sender.NameColor);
        //}

        //ChatUtils.ColoredText(String.Format(tag.Item2, c.Value.Message), Plugin.Config.ChatColors[c.Value.Type]);

        Dictionary<string, Vector4> lines = new Dictionary<string, Vector4>();

        lines.Add($"[{c.Value.Time.ToShortTimeString()}] ", Plugin.Config.ChatColors[c.Value.Type]);

        if (tag.Item1 != String.Empty) 
            lines.Add(tag.Item1, Plugin.Config.ChatColors[c.Value.Type]);

        if (c.Value.Type != Dalamud.Game.Text.XivChatType.StandardEmote)
            lines.Add(c.Value.Sender.GetNameTag(), c.Value.Sender.NameColor);

        lines.Add(String.Format(tag.Item2, c.Value.Message), Plugin.Config.ChatColors[c.Value.Type]);

        ChatUtils.WrappedColoredText(lines);

    }

    /// <summary>
    /// Toggles window being opened/closed based on current state of open variable
    /// </summary>
    /// <param name="player"></param>
    public void ToggleWindowOpen(Player? player)
    {

        //If player is null, then we just open/close the window. Otherwise we set the selected player to the passed player
        if (player != null)
        {
            //if we're clicking on the current player and the window is already open, close it
            if (ChatOpen == true && Plugin.SelectedPlayer != null && Plugin.SelectedPlayer.Name.Equals(player.Name))
            {
                Plugin.SelectedPlayer = null;
                ChatOpen = false;
            }
            // open content in right panel
            else
            {
                Plugin.SelectedPlayer = player;
                ChatOpen = true;
            }
        }

        //Stuff the selectable should do on click
        if (ChatOpen)
        {

            this.SizeConstraints = openConstraints;
            justOpened = true;

        }
        else
        {
            this.SizeConstraints = closedConstraints;
        }
    }

    // <summary>
    // Force toggles the window open/closed.
    // </summary
    public void toggleWindow(bool open)
    {
        MainWindow.ChatOpen = open;
        SizeConstraints = open ? openConstraints : closedConstraints;
    }

    /// <summary>
    /// Adds the player as a selectable element to the parent.
    /// </summary>
    /// <param name="player">Player to add.</param>
    public void AddPlayerSelectable(Player player)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, player.NameColor);
        ImGui.BeginGroup();

        if (ImGui.Selectable("###WhoSaidWhatNow_Player_Selectable_" + player.Name, player.Name.Equals(Plugin.SelectedPlayer?.Name), ImGuiSelectableFlags.None))
        {
            ToggleWindowOpen(player);
        }

        //TODO: padding is a bit wacky on the selectable and clicks with the one above it, either remove the padding or add margins
        ImGui.SameLine();
        if (player.Name == Plugin.Config.CurrentPlayer)
        {
            ImGui.Text(" " + player.Name + " (YOU)");
        }
        else if (player.RemoveDisabled == true)
        {
            ImGui.Text(" " + player.Name);
        }
        else
        {
            ImGui.Text(player.Name);
        }
        ImGui.EndGroup();
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
                plugin.ToggleConfigUI();
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
        var individual = new TabIndividual(this, plugin);

        //GROUP TAB
        var groups = new TabGroups(this, plugin);

        ImGui.EndTabBar();

    }

}
