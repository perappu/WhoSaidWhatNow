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
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using WhoSaidWhatNow.Objects;
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

    //thank you snooper
    private static readonly IDictionary<XivChatType, string> Formats = new Dictionary<XivChatType, string>()
        {
            { XivChatType.Say, "{0}: {1}" },
            { XivChatType.TellIncoming, "{0} >> {1}" },
            { XivChatType.StandardEmote, "{1}" },
            { XivChatType.CustomEmote, "{0} {1}" },
            { XivChatType.Shout, "{0} shouts: {1}" },
            { XivChatType.Yell, "{0} yells: {1}" },
            { XivChatType.Party, "({0}) {1}" },
            { XivChatType.CrossParty, "({0}) {1}" },
            { XivChatType.Alliance, "(({0})) {1}" },
            { XivChatType.FreeCompany, "[FC]<{0}> {1}" },
        };

    //define constraints for when the right panel is open/closed
    //TODO: set minimum/maximum when "closed" but infinitely resizable when expanded
    WindowSizeConstraints openConstraints = new WindowSizeConstraints
    {
        MinimumSize = new Vector2(500, 330),
        MaximumSize = new Vector2(900, 900)
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

        //add linkshell indicators
        //this is just ripped straight from snooper... we may have our own way to do it eventually
        for (int i = 1; i <= 8; i++)
        {
            var lsChannel = (XivChatType)((ushort)XivChatType.Ls1 + i - 1);
            Formats.Add(lsChannel, string.Format("[LS{0}]{1}", i, "<{0}> {1}"));

            var cwlsChannel = i == 1 ? XivChatType.CrossLinkShell1 : (XivChatType)((ushort)XivChatType.CrossLinkShell2 + i - 2);
            Formats.Add(cwlsChannel, string.Format("[CWLS{0}]{1}", i, "<{0}> {1}"));
        }
    }

    //I honestly have no idea how to dispose of windows correctly
    //TODO: make sure this is ok?
    public void Dispose() { }

    //AddNewPlayer() creates a player based on a game object and adds it to the main list
    private uint AddNewPlayer(GameObject gameObject)
    {
        uint id = gameObject.DataId;
        string server = ((PlayerCharacter)(gameObject)).HomeWorld.GameData.Name.ToString();
        Players.Add(new Player(gameObject.ObjectId, new string(gameObject.Name.ToString()), server));
        return id;
    }

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

        //non-menu bar buttons
        //TODO: make remove button have checking
        //put it on the right panel as well maybe, since it does nothing when a person isn't selected
        if (ImGui.Button("Add Target"))
        {
            IsTargetPlayer();
        }
        ImGui.SameLine();
        //This is a really janky workaround
        ImGui.InvisibleButton("##dummy", new Vector2(128, 20));
        ImGui.SameLine();
        if (ImGui.Button("Remove Target"))
        {
            //we have to manually close the window here
            if (selectedPlayer is not null)
            {
                Players.Remove(selectedPlayer);
                open = false;
                this.SizeConstraints = closedConstraints;
            }
        }

        //Creating left and right panels
        ////you can redeclare BeginChild() with the same ID to add things to them, which we do for chatlog
        ImGui.BeginChild("###WhoSaidWhatNow_LeftPanel_Child", new Vector2(205 * ImGuiHelpers.GlobalScale, 0), true);
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
                if (open == true && selectedPlayer.ID == player.ID)
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
                if (plugin.configuration.ChannelToggles[c.Value.Type] == true && c.Value.Sender.Contains(selectedPlayer.Name)) { 
                //PluginLog.Debug(Configuration.ChatColors[c.Type].ToString());
                ImGui.PushStyleColor(ImGuiCol.Text, Configuration.ChatColors[c.Value.Type]);

                string time = c.Value.Time.ToShortTimeString();
                string sender = selectedPlayer.Name + "" + selectedPlayer.Server;
                string tag = Formats[c.Value.Type];
                string msg = c.Value.Message;

                ImGui.TextWrapped($"[{time}]" + String.Format(tag, sender, msg));
                ImGui.PopStyleColor();}
            }
        }

        if(plugin.configuration.AutoScroll)
        {
            //i don't understand math, make this actually work better
            ImGui.SetScrollHereY(1.0f);
        }
        ImGui.EndGroup();
        ImGui.EndChild();

    }

}
