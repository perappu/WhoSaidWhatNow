using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace WhoSaidWhatNow.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration configuration;

    public ConfigWindow(Plugin plugin) : base(
        "Who Said What Now - Settings",
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Size = new Vector2(232, 200);
        this.SizeCondition = ImGuiCond.Always;

        this.configuration = plugin.configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // can't ref a property, so use a local copy
        bool IsOn = this.configuration.IsOn;
        bool AutoScroll = this.configuration.AutoScroll;

        if (ImGui.Checkbox("Plugin On/Off", ref IsOn))
        {
            this.configuration.IsOn = IsOn;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            this.configuration.Save();
        }

        if (ImGui.Checkbox("Autoscrolling On/Off", ref AutoScroll))
        {
            this.configuration.AutoScroll = AutoScroll;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            this.configuration.Save();
        }
    }
}
