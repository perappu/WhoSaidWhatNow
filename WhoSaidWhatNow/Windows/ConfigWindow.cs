using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;
using Dalamud.DrunkenToad.Extensions;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Lumina.Excel.Sheets;
using WhoSaidWhatNow.Objects;
using WhoSaidWhatNow.Utils;
using FFXIVClientStructs.FFXIV.Client.UI;
using static Dalamud.Interface.Utility.Raii.ImRaii;

namespace WhoSaidWhatNow.Windows;

public class ConfigWindow : Window, IDisposable
{
    private string newName = string.Empty;
    private int newServer = 0;
    private Vector4 newColor = Vector4.One;
    internal const String ID_PANEL_LEFT = "###WhoSaidWhatNowConfig_LeftPanel_Child";
    private readonly Plugin plugin;
    private readonly string[] worldNames = GetWorldNames(Plugin.DataManager);

    private readonly Sounds[] soundsList =
    ((Sounds[])Enum.GetValues(typeof(Sounds))).Where(s => s != Sounds.Unknown).ToArray();

    private static string[] GetWorldNames(IDataManager dataManager)
    {
        return dataManager.GetExcelSheet<World>().Where(world => world.IsPublic).Select(world => world.Name.ToString())
                          .OrderBy(worldName => worldName).ToArray();
    }

    public ConfigWindow(Plugin plugin) : base(
        "Who Said What Now - Settings", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.plugin = plugin;
        Size = new Vector2(600, 500);
        SizeCondition = ImGuiCond.Appearing;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // design philosophy for us right now is we save automatically
        // if we have more options we may change later, but honestly I think some larger plugins also do this so we're fine

        ImGui.BeginTabBar("###WhoSaidWhatNowConfig_Tab_Bar");

        //GENERAL SETTINGS TAB
        if (ImGui.BeginTabItem("General"))
        {
            // replace the existing panels by using the same IDs.
            ImGui.BeginChild(ID_PANEL_LEFT, new Vector2(0, 0), true);

            //plugin on/off
            var enabled = Plugin.Config.Enabled;
            if (ImGui.Checkbox("Plugin On/Off", ref enabled))
            {
                Plugin.Config.Enabled = enabled;
                Plugin.Config.Save();
            }

            ImGui.Separator();

            //show timestamps
            var showTimestamp = Plugin.Config.ShowTimestamp;
            if (ImGui.Checkbox("Show timestamps", ref showTimestamp))
            {
                Plugin.Config.ShowTimestamp = showTimestamp;
                Plugin.Config.Save();
            }

            //show server in names
            var showServer = Plugin.Config.ShowServer;
            if (ImGui.Checkbox("Show servers", ref showServer))
            {
                Plugin.Config.ShowServer = showServer;
                Plugin.Config.Save();
            }

            //plugin autoscroll
            var autoscroll = Plugin.Config.AutoscrollOnOpen;
            if (ImGui.Checkbox("Autoscroll to bottom when re-opening a log", ref autoscroll))
            {
                Plugin.Config.AutoscrollOnOpen = autoscroll;
                Plugin.Config.Save();
            }

            ImGui.Separator();

            //plugin autoscroll
            var playSound = Plugin.Config.PlaySound;
            if (ImGui.Checkbox("Play a notification sound on new message in currently open log", ref playSound))
            {
                Plugin.Config.PlaySound = playSound;
                Plugin.Config.Save();
            }

            //selected sound
            var selectedSound = Plugin.Config.SelectedSound;
            if (ImGui.BeginCombo("##selectedSound", selectedSound.ToName()))
            {
                foreach (var sound in soundsList)
                {
                    bool isSelected = selectedSound == sound;
                    if (ImGui.Selectable($"{sound.ToName()}##selectedSound", isSelected))
                    {
                        selectedSound = sound;
                        Plugin.Config.SelectedSound = selectedSound;
                        Plugin.Config.Save();
                        if (isSelected)
                        {
                            ImGui.SetItemDefaultFocus();
                        }
                    }
                }

                ImGui.EndCombo();
            }
            ImGui.SameLine();
            if (ImGui.Button("Play"))
            {
                UIGlobals.PlaySoundEffect((uint)selectedSound);
            }

            ImGui.Separator();

            //plugin set config colors
            ImGui.TextWrapped(
                "This button will match WhoWhat's colors to Character Configuration > Log Window Settings.");
            if (ImGui.Button("Set Colors to Character Log Text Colors"))
            {
                ConfigurationUtils.SetConfigColors();
                Plugin.Config.Save();
            }

            ImGui.EndChild();
            ImGui.EndTabItem();
        }

        // ALWAYS TRACKED TAB
        if (ImGui.BeginTabItem("Favorite Players"))
        {
            ImGui.BeginChild(ID_PANEL_LEFT, new Vector2(0, 0), true);

            ImGui.Text("Any players added here will always be tracked and marked with .");
            ImGui.Text("They can only be removed via this page.");
            ImGui.NewLine();

            if (ImGui.BeginTable("alwaysTrackedPlayers", 5, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("");
                ImGui.TableSetupColumn("Color", ImGuiTableColumnFlags.WidthStretch, 30);
                ImGui.TableSetupColumn("Player Name", ImGuiTableColumnFlags.WidthStretch, 150);
                ImGui.TableSetupColumn("Server", ImGuiTableColumnFlags.WidthStretch, 150);
                ImGui.TableSetupColumn("");

                ImGui.TableHeadersRow();
                ImGui.TableNextRow();

                //build table of existing data
                //TODO: sometimes remove raises System.InvalidOperationException because it doesn't like you editing the list it's iterating through
                foreach (var player in Plugin.Config.AlwaysTrackedPlayers)
                {
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    var v = player.Color;
                    if (ImGui.ColorEdit4($"##picker{player.Name}{player.Server}", ref v,
                                         ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
                    {
                        PlayerUtils.ColorTrackedPlayer(player, v);
                    }

                    ImGui.PushStyleColor(ImGuiCol.Text, player.Color);
                    ImGui.TableNextColumn();
                    ImGui.Text(player.Name);
                    ImGui.TableNextColumn();
                    ImGui.Text(player.Server);
                    ImGui.TableNextColumn();
                    ImGui.PopStyleColor();

                    if (ImGui.Button($"Remove##{player.Name}{player.Server}"))
                    {
                        PlayerUtils.RemoveTrackedPlayer(player);
                        PlayerUtils.CheckTrackedPlayers();
                    }

                    ImGui.TableNextColumn();
                    ImGui.TableNextRow();
                }

                //ui elements for adding new player
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                var newCol = newColor;
                if (ImGui.ColorEdit4("##pickerNewName", ref newCol,
                                     ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
                {
                    newColor = newCol;
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(-1);
                ImGui.InputText("##inputNewName", ref newName, 100);
                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(-1);
                ImGui.Combo("##servers", ref newServer, worldNames, worldNames.Length);
                ImGui.TableNextColumn();
                if (ImGui.Button("Add"))
                {
                    if (newName.IsValidCharacterName() && !newServer.Equals(""))
                    {
                        PlayerUtils.AddTrackedPlayer(new TrackedPlayer(newName, worldNames[newServer], newColor));
                        newName = string.Empty;
                        newServer = 0;
                        newColor = Vector4.One;
                    }
                }

                ImGui.EndTable();
            }

            ImGui.EndChild();
            ImGui.EndTabItem();
        }

        //ENABLED CHANNELS TAB
        if (ImGui.BeginTabItem("Channels"))
        {
            ImGui.BeginChild(ID_PANEL_LEFT, new Vector2(0, 0), true);

            var parts = Plugin.Config.ChannelToggles.Split(Plugin.Config.ChannelToggles.Count / 2 - 2);
            //generate checkbox for each chat channel

            if (ImGui.BeginTable("channelsEnabled", 3, ImGuiTableFlags.SizingFixedSame))
            {
                ImGui.TableSetupColumn("###col1", ImGuiTableColumnFlags.WidthStretch, 300);
                ImGui.TableSetupColumn("###col2", ImGuiTableColumnFlags.WidthStretch, 300);
                ImGui.TableSetupColumn("###col3", ImGuiTableColumnFlags.WidthStretch, 300);

                ImGui.TableNextRow();

                foreach (var part in parts)
                {
                    foreach (var chan in part)
                    {
                        ImGui.TableNextColumn();
                        var val = chan.Value;
                        if (ImGui.Checkbox($"##chk{chan.Key.ToString()}", ref val))
                        {
                            Plugin.Config.ChannelToggles[chan.Key] = val;
                            Plugin.Config.Save();
                        }

                        ImGui.SameLine();
                        var color = Plugin.Config.ChatColors[chan.Key];
                        if (ImGui.ColorEdit4($"##picker{chan.Key.ToString()}", ref color,
                                             ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
                        {
                            Plugin.Config.ChatColors[chan.Key] = color;
                            Plugin.Config.Save();
                        }

                        ImGui.SameLine();
                        ImGui.PushStyleColor(ImGuiCol.Text, Plugin.Config.ChatColors[chan.Key]);
                        ImGui.TextUnformatted(chan.Key.ToString());
                        ImGui.PopStyleColor();
                    }

                    ImGui.TableNextRow();
                }
            }

            ImGui.EndTable();

            ImGui.EndChild();
            ImGui.EndTabItem();
        }
    }
}
