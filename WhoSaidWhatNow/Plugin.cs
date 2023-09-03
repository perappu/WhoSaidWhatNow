using Dalamud.Data;
using Dalamud.DrunkenToad;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Config;
using Dalamud.Game.Gui;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Logging;
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
        public static List<Player> Players = new();
        public static string FilterPlayers = "";
        public static string FilterSearch = "";
        public static Dictionary<String, (String NAME, Dictionary<Player, Boolean> PLAYERS)> Groups = new() { { "1", ("Group 1", Players.ToDictionary(p => p, p => false)) } };
        public static SortedList<DateTime, ChatEntry> ChatEntries = new();

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

        [PluginService]
        [RequiredVersion("1.0")]
        public static GameConfig GameConfig { get; private set; } = null!;

        public static FileDialogManager FileDialogManager { get; set; } = new FileDialogManager();

        internal ChatService ChatListener { get; private set; } = null!;

        public PlayerUtils PlayerService { get; set; } = null!;

        public Plugin()
        {

            // initiatize our configuration
            try
            {
                Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
                Logger.LogDebug("Config file loaded successfully.");
                Config.Initialize(PluginInterface);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to load config so creating new one.", ex);
                ChatGuiExtensions.PluginPrint(Plugin.ChatGui, "Error loading config file. New one made.");
                Config = new Configuration();
                Config.Save();
                Config.Initialize(PluginInterface);
            }

            ConfigHelper = new ConfigurationUtils();

            // setup UI
            this.MainWindow = new MainWindow(this);
            this.ConfigWindow = new ConfigWindow(this);

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

            try
            {
                if (ClientState.IsLoggedIn)
                {
                    Plugin.Players.Clear();
                    Plugin.Config.CurrentPlayer = string.Empty;
                    PlayerUtils.SetCurrentPlayer();
                    PlayerUtils.CheckTrackedPlayers();
                }
            }
            catch
            {
                PluginLog.LogError("Plugin loaded and thought there was a character logged in, but there wasn't.");
            }

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
