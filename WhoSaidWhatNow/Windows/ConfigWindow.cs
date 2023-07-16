using System;
using System.Numerics;

using Dalamud.Interface;
using Dalamud.Interface.Windowing;

using ImGuiNET;

namespace WhoSaidWhatNow.Windows;

public class ConfigWindow : Window, IDisposable
{

    public ConfigWindow() : base(
        "Who Said What Now - Settings",
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Size = new Vector2(400, 500);
        this.SizeCondition = ImGuiCond.Appearing;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // can't ref a property, so use a local copy for config variables

        // design philosophy for us right now is we save automatically
        // if we have more options we may change later, but honestly I think some larger plugins also do this so we're fine

        ImGui.BeginChild(MainWindow.ID_PANEL_LEFT, new Vector2(180 * ImGuiHelpers.GlobalScale, 0), true);
        {
            bool enabled = Plugin.Config.Enabled;
            if (ImGui.Checkbox("Plugin On/Off", ref enabled))
            {
                Plugin.Config.Enabled = enabled;
                Plugin.Config.Save();
            }
            bool autoscroll = Plugin.Config.Autoscroll;
            if (ImGui.Checkbox("Autoscrolling On/Off", ref autoscroll))
            {
                Plugin.Config.Autoscroll = autoscroll;
                Plugin.Config.Save();
            }
        }

        foreach(var player in this.configuration.AlwaysTrackedPlayers)
        {
            ImGui.Text(player.Name);
            ImGui.Text(player.Server);
        }

        string? newName = null;
        string? newServer = null;

        ImGui.EndChild();

        ImGui.SameLine();

        ImGui.BeginChild(MainWindow.ID_PANEL_RIGHT, new Vector2(0, 0), true);
        {
            foreach (var chan in Plugin.Config.ChannelToggles)
            {
                bool val = chan.Value;
                if (ImGui.Checkbox(chan.Key.ToString(), ref val))
                {
                    Plugin.Config.ChannelToggles[chan.Key] = val;
                    Plugin.Config.Save();
                }
            }
        }
        ImGui.EndChild();

    }
}
