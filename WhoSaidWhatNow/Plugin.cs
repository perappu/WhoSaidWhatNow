using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud.Interface.Windowing;
using WhoSaidWhatNow.Windows;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.Logging;
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
        private const string COMMAND = "/whowhat";
        public static Configuration Config = null!;
        public static Player? SelectedPlayer = null;
        public static List<Player> Players = new List<Player>();
        public static IDictionary<String, List<Player>> Groups = new Dictionary<String, List<Player>>();
        public static SortedList<DateTime, ChatEntry> ChatEntries = new SortedList<DateTime, ChatEntry>();
        public static TargetManager TargetManager = null!;
        private static ChatListener s_chatListener = null!;
        private static CommandManager s_commandManager = null!;
        public static ClientState ClientState = null!;

        private static DalamudPluginInterface s_pluginInterface = null!;
        private static WindowSystem WindowSystem = new("WhoSaidWhatNow");

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
            s_pluginInterface = pluginInterface;
            s_commandManager = commandManager;
            TargetManager = targetManager;

            // initiatize our configuration
            Config = s_pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Config.Initialize(s_pluginInterface);

            // create the listener
            s_chatListener = new ChatListener(chatGui);

            //add our windows
            WindowSystem.AddWindow(new ConfigWindow());
            WindowSystem.AddWindow(new MainWindow());

            //TODO: add a command for the config window?
            //and one for on/off toggle
            s_commandManager.AddHandler(COMMAND, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open settings"
            });

            s_pluginInterface.UiBuilder.Draw += DrawUI;
            s_pluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            Plugin.ClientState = clientState;
            Plugin.ClientState.Login += OnLogin;
            Plugin.ClientState.Logout += OnLogout;

        }

        //TODO: make sure we're disposing of everything we need to appropriately
        public void Dispose()
        {
            s_chatListener.Dispose();
            WindowSystem.RemoveAllWindows();
            s_commandManager.RemoveHandler(COMMAND);
            Plugin.ClientState.Login -= OnLogin;
        }

        //may be better suited somewhere else?
        void SetCurrentPlayer()
        {
            if (Config.CurrentPlayer == null)
            {
                Config.CurrentPlayer = ClientState.LocalPlayer!.Name.ToString();
                Players.Add(new Player(ClientState.LocalPlayer!));
                PluginLog.LogDebug("Currently Logged In Player was null. Set: " + Config.CurrentPlayer);
            }
            //if switched characters, remove old character and replace with new one
            //adds to top of list with insert
            else if (!Config.CurrentPlayer.ToString().Equals(ClientState.LocalPlayer!.Name.ToString()))
            {
                PluginLog.LogDebug("Currently Logged In Player was changed. Old: " + Config.CurrentPlayer);
                Players.Remove(Players.Find(x => Config.CurrentPlayer.Contains(x.Name)));
                Config.CurrentPlayer = ClientState.LocalPlayer!.Name.ToString();
                Players.Insert(0, new Player(ClientState.LocalPlayer!));
                PluginLog.LogDebug("Currently Logged In Player was changed. New: " + Config.CurrentPlayer);
            }
        }

        private void OnCommand(string command, string args)
        {

            if (args.Equals("on"))
            {
                Config.Enabled = true;
            }
            else if (args.Equals("off"))
            {
                Config.Enabled = false;
            }
            else if (args.Equals("reload"))
            {
                WindowSystem.GetWindow("Who Said What Now")!.IsOpen = false;
                SetCurrentPlayer();
                WindowSystem.GetWindow("Who Said What Now")!.IsOpen = true;
            } else
            {
                WindowSystem.GetWindow("Who Said What Now")!.IsOpen = !WindowSystem.GetWindow("Who Said What Now")!.IsOpen;
            }
        }

        //set the current player when logging in
        void OnLogin(object? sender, EventArgs e)
        {
            PluginLog.LogDebug("onlogin");
            SetCurrentPlayer();
        }

        //close all windows when logging out so that the windows refresh
        void OnLogout(object? sender, EventArgs e)
        {
            WindowSystem.GetWindow("Who Said What Now")!.IsOpen = false;
        }

        private static void DrawUI()
        {
            WindowSystem.Draw();
        }

        public static void DrawConfigUI()
        {
            WindowSystem.GetWindow("Who Said What Now - Settings")!.IsOpen = true;
        }
    }
}
