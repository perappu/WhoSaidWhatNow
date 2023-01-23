using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Channels;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using WhoSaidWhatNow;

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
        bool IsOn = Configuration.IsOn;
        bool AutoScroll = Configuration.AutoScroll;

        //design philosophy for us right now is we save automatically
        //if we have more options we may change later, but honestly I think some larger plugins also do this so we're fine

        ImGui.BeginChild("###WhoSaidWhatNow_LeftPanel_Child", new Vector2(180 * ImGuiHelpers.GlobalScale, 0), true);
        if (ImGui.Checkbox("Plugin On/Off", ref IsOn))
        {
            Configuration.IsOn = IsOn;
            Configuration.Save();
        }

        if (ImGui.Checkbox("Autoscrolling On/Off", ref AutoScroll))
        {
            Configuration.AutoScroll = AutoScroll;
            Configuration.Save();
        }
        ImGui.EndChild();

        ImGui.SameLine();

        ImGui.BeginChild("###WhoSaidWhatNow_RightPanel_Child", new Vector2(0, 0), true);
        //I don't like using the generic object but I also scream internally
        foreach (var chan in Configuration.ChannelToggles)
        {
            bool val = chan.Value;
            if (ImGui.Checkbox(chan.Key.ToString(), ref val))
            {
                Configuration.ChannelToggles[chan.Key] = val;
                Configuration.Save();
            }
        }
        ImGui.EndChild();

    }
}
