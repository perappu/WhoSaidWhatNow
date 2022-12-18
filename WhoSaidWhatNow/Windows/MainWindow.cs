using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
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
using System.Linq;
using System.Numerics;
using WhoSaidWhatNow.Objects;
namespace WhoSaidWhatNow.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin plugin;
    public List<Player> Players;
    private TargetManager targetManager;
    private Player? selectedPlayer = null;
    private string errorMessage = string.Empty;
    private bool open = false;

    //thank you snooper
    private static readonly IDictionary<XivChatType, string> formats = new Dictionary<XivChatType, string>()
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

    public MainWindow(Plugin plugin, List<Player> trackedPlayers, TargetManager targetManager) : base(
        "Who Said What Now", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.MenuBar)
    {
        this.SizeConstraints = closedConstraints;

        this.plugin = plugin;
        this.Players = trackedPlayers;
        this.targetManager = targetManager;

        //add linkshell indicators
        for (int i = 1; i <= 8; i++)
        {
            var lsChannel = (XivChatType)((ushort)XivChatType.Ls1 + i - 1);
            formats.Add(lsChannel, string.Format("[LS{0}]{1}", i, "<{0}> {1}"));

            var cwlsChannel = i == 1 ? XivChatType.CrossLinkShell1 : (XivChatType)((ushort)XivChatType.CrossLinkShell2 + i - 2);
            formats.Add(cwlsChannel, string.Format("[CWLS{0}]{1}", i, "<{0}> {1}"));
        }
    }

    public void Dispose()
    {

    }

    private uint AddNewPlayer(GameObject gameObject)
    {
        uint id = gameObject.DataId;
        Players.Add(new Player(gameObject.ObjectId, new string(gameObject.Name.ToString())));
        return id;
    }

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

    public override void Draw()
    {
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

        if (ImGui.Button("Add Target"))
        {
            IsTargetPlayer();
        }

        // left and right panels
        ImGui.BeginChild("###WhoSaidWhatNow_LeftPanel_Child", new Vector2(205 * ImGuiHelpers.GlobalScale, 0), true);
        ImGui.EndChild();
        ImGui.SameLine();
        ImGui.BeginChild("###WhoSaidWhatNow_RightPanel_Child", new Vector2(0, 0), true);
        ImGui.EndChild();

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
                }

                if (open == true && selectedPlayer.ID == player.ID)
                {
                    open = false;
                }
                // open content in right panel
                else
                {
                    open = true;
                    selectedPlayer = player;
                }

            }
            ImGui.SameLine();
            ImGui.Text(player.Name);
            ImGui.EndGroup();
            ImGui.EndChild();

            if (open)
            {
                this.SizeConstraints = openConstraints;
            }
            else
            {
                this.SizeConstraints = closedConstraints;
            }

        }

        // menu for selected player
        ImGui.BeginChild("###WhoSaidWhatNow_RightPanel_Child");
        ImGui.BeginGroup();
        if (selectedPlayer is not null)
        {
            foreach (ChatEntry c in selectedPlayer.ChatEntries)
            {
                //I have no idea why color isn't working because it seems like it should
                //PluginLog.Debug(Configuration.ChatColors[c.Type].ToString());
                ImGui.PushStyleColor(ImGuiCol.Text, Configuration.ChatColors[c.Type]);
                var time = c.Time.ToShortTimeString();
                var sender = c.Sender;
                var msg = c.Message;
                ImGui.TextWrapped($"[{time}] {sender}: {msg}");
                ImGui.PopStyleColor();
            }
        }
        if(plugin.configuration.AutoScroll)
        {
            //this only scrolls to the top because I don't understand math
            ImGui.SetScrollHereY(1.0f);
        }
        ImGui.EndGroup();
        ImGui.EndChild();

    }

}
