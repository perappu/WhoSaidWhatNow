using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud.Interface.Windowing;
using WhoSaidWhatNow.Windows;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game;
using System.Collections.Generic;
using WhoSaidWhatNow.Objects;
using System;

namespace WhoSaidWhatNow
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Who Said What Now";
        private const string CommandName = "/whowhat";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        private ChatListener ChatListener { get; init; }
        public Configuration configuration { get; init; }
        public WindowSystem WindowSystem = new("WhoSaidWhatNow");
        public List<Player> trackedPlayers;
        public SortedList<DateTime, ChatEntry> chatEntries;

        //TODO: Make sure we're only actually declaring stuff we need
        //I went a little ham because of what I thought was required by onmessagehandled
        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] ClientState clientState,
            [RequiredVersion("1.0")] ChatGui chatGui,
            [RequiredVersion("1.0")] TargetManager targetManager,
            [RequiredVersion("1.0")] SigScanner sigScanner)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.trackedPlayers = new List<Player>();
            this.chatEntries = new SortedList<DateTime, ChatEntry>();

            //initiatize our configuration
            this.configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.configuration.Initialize(this.PluginInterface);

            //create the listener
            this.ChatListener = new ChatListener(chatEntries, trackedPlayers, chatGui, configuration, clientState, targetManager, sigScanner);

            //add our windows
            WindowSystem.AddWindow(new ConfigWindow(this));
            WindowSystem.AddWindow(new MainWindow(this, trackedPlayers, chatEntries, targetManager));

            //TODO: add a command for the config window?
            //and one for on/off toggle
            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open settings"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        //TODO: make sure we're disposing of everything we need to appropriately
        public void Dispose()
        {
            this.ChatListener.Dispose();
            this.WindowSystem.RemoveAllWindows();
            this.CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            WindowSystem.GetWindow("Who Said What Now").IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            WindowSystem.GetWindow("Who Said What Now - Settings").IsOpen = true;
        }
    }
}
