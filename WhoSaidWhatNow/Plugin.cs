using Dalamud.Data;
using Dalamud.DrunkenToad;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using WhoSaidWhatNow.Objects;
using WhoSaidWhatNow.Services;
using WhoSaidWhatNow.Utils;
using WhoSaidWhatNow.Windows;

namespace WhoSaidWhatNow
{
    public sealed class Plugin : IDalamudPlugin
    {

        public string Name => "Who Said What Now";
        private const string COMMAND = "/whowhat";
        public static Configuration Config = null!;
        public static ConfigurationUtils ConfigHelper = null!;

        public static Player? SelectedPlayer = null;
        public static List<Player> Players = new List<Player>();
        public static Dictionary<String, (String NAME, Dictionary<Player, Boolean> PLAYERS)> Groups = new Dictionary<String, (String, Dictionary<Player, Boolean>)> { { "1", ("Group 1", Players.ToDictionary(p => p, p => false)) } };
        public static SortedList<DateTime, ChatEntry> ChatEntries = new SortedList<DateTime, ChatEntry>();

        private WindowSystem WindowSystem { get; set; }
        public MainWindow MainWindow { get; }
        public ConfigWindow ConfigWindow { get; }

        [PluginService]
        [RequiredVersion("1.0")]
        public static DalamudPluginInterface PluginInterface { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static CommandManager CommandManager { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static DataManager DataManager { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static TargetManager TargetManager { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static ClientState ClientState { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static ChatGui ChatGui { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static ObjectTable ObjectTable { get; private set; } = null!;

        public FileDialogManager FileDialogManager { get; set; } = null!;

        internal ChatService ChatListener { get; private set; } = null!;

        public PlayerUtils PlayerService { get; set; } = null!;

        public Plugin()
        {

            // initiatize our configuration
            Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Config.Initialize(PluginInterface);

            ConfigHelper = new ConfigurationUtils();

            // setup UI
            this.MainWindow = new MainWindow(this);
            this.ConfigWindow = new ConfigWindow(this);
            this.FileDialogManager = new FileDialogManager();

            this.WindowSystem = new WindowSystem("WhoSaidWhatNow");
            this.WindowSystem.AddWindow(this.ConfigWindow);
            this.WindowSystem.AddWindow(this.MainWindow);

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            PluginInterface.UiBuilder.Draw += FileDialogManager.Draw;

            // add events/listeners
            Plugin.ClientState.Login += OnLogin;
            Plugin.ClientState.Logout += OnLogout;
            this.ChatListener = new ChatService(ChatGui);
            this.PlayerService = new PlayerUtils();

            // commands
            CommandManager.AddHandler(COMMAND, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open main window"
            });

        }

        //TODO: make sure we're disposing of everything we need to appropriately
        public void Dispose()
        {
            ChatListener.Dispose();
            PluginInterface.UiBuilder.Draw -= DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
            PluginInterface.UiBuilder.Draw -= FileDialogManager.Draw;
            WindowSystem.RemoveAllWindows();
            CommandManager.RemoveHandler(COMMAND);
            Plugin.ClientState.Login -= OnLogin;
            Plugin.ClientState.Logout -= OnLogout;
        }

        private void OnCommand(string command, string args)
        {

            if (args.Equals("on"))
            {
                ChatGuiExtensions.PluginPrint(Plugin.ChatGui, "WhoWhat is ON.");
                Config.Enabled = true;
            }
            else if (args.Equals("off"))
            {
                ChatGuiExtensions.PluginPrint(Plugin.ChatGui, "WhoWhat is OFF.");
                Config.Enabled = false;
            }
            else if (args.Equals("refresh"))
            {
                this.MainWindow.IsOpen = false;
                ConfigurationUtils.refresh();
                this.MainWindow.IsOpen = true;
                ChatGuiExtensions.PluginPrint(Plugin.ChatGui, "WhoWhat refreshed. All temporary tracked players removed.");
            }
            else if (args.Equals("reset"))
            {
                this.MainWindow.IsOpen = false;
                this.ConfigWindow.IsOpen = false;
                ConfigurationUtils.reset();
                this.MainWindow.IsOpen = true;
                this.ConfigWindow.IsOpen = true;
                ChatGuiExtensions.PluginPrint(Plugin.ChatGui, "WhoWhat refreshed. Most settings reset.");
            }

            else if (args.Equals("config"))
            {
                this.ConfigWindow.IsOpen = !this.ConfigWindow.IsOpen;
            }
            else
            {
                this.MainWindow.IsOpen = !this.MainWindow.IsOpen;
            }
        }

        //set the current player when logging in
        void OnLogin(object? sender, EventArgs e)
        {
            Plugin.Players.Clear();
            Plugin.Config.CurrentPlayer = string.Empty;
            PlayerUtils.SetCurrentPlayer();
            PlayerUtils.CheckTrackedPlayers();
        }

        //close all windows when logging out so that the windows refresh
        void OnLogout(object? sender, EventArgs e)
        {
            this.MainWindow.IsOpen = false;
            SelectedPlayer = null;
            Plugin.Players.Clear();
        }

        private void DrawUI()
        {
            WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            this.ConfigWindow.IsOpen = true;
        }

        public void ToggleConfigUI()
        {
            this.ConfigWindow.IsOpen = !this.ConfigWindow.IsOpen;
        }

    }
}
