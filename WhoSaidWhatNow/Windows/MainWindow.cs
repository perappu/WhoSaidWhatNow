using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
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

    WindowSizeConstraints openConstraints = new WindowSizeConstraints
    {
        MinimumSize = new Vector2(500, 330),
        MaximumSize = new Vector2(700, 800)
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

            ImGui.Text(this.plugin.configuration.IsOn == true ? "On" : "Off");

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
            ImGui.Text(player.Name + " " + player.ID);
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
        if(selectedPlayer != null) {
            for (var i = 0; selectedPlayer.ChatEntries.Count > i; i++)
            {
                ImGui.TextWrapped("[" + selectedPlayer.ChatEntries[i].Time.ToShortTimeString() + "] " + selectedPlayer.ChatEntries[i].Sender + ": " + selectedPlayer.ChatEntries[i].Message);
            }
        }
        ImGui.EndGroup();
        ImGui.EndChild();

    }

}
