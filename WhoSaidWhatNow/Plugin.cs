using Dalamud.DrunkenToad.Extensions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
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
        public static String? SelectedGroup = null;
        public static Player? CurrentPlayer = null;
        public static List<Player> Players = new();
        public static string FilterPlayers = "";
        public static string FilterSearch = "";

        public static Dictionary<String, (String NAME, Dictionary<Player, Boolean> PLAYERS)> Groups = new()
            { { "1", ("Group 1", Players.ToDictionary(p => p, p => false)) } };

        public static SortedList<DateTime, ChatEntry> ChatEntries = new();

        private WindowSystem WindowSystem { get; set; }
        public static MainWindow MainWindow { get; private set; } = null!;
        public static ConfigWindow ConfigWindow { get; private set; } = null!;

        [PluginService]
        public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

        [PluginService]
        public static ICommandManager CommandManager { get; private set; } = null!;

        [PluginService]
        public static IDataManager DataManager { get; private set; } = null!;

        [PluginService]
        public static ITargetManager TargetManager { get; private set; } = null!;

        [PluginService]
        public static IClientState ClientState { get; private set; } = null!;

        [PluginService]
        public static IChatGui ChatGui { get; private set; } = null!;

        [PluginService]
        public static IObjectTable ObjectTable { get; private set; } = null!;

        [PluginService]
        internal static IFramework Framework { get; private set; } = null!;

        [PluginService]
        public static IGameConfig GameConfig { get; private set; } = null!;

        [PluginService]
        public static IPluginLog Logger { get; private set; } = null!;

        public static FileDialogManager FileDialogManager { get; set; } = new FileDialogManager();

        internal ChatService ChatListener { get; private set; } = null!;

        public PlayerUtils PlayerService { get; set; } = null!;

        public Plugin()
        {
            // initiatize our configuration
            try
            {
                Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
                Logger.Debug("Config file loaded successfully.");
                Config.Initialize(PluginInterface);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load config so creating new one.", ex);
                ChatGui.PluginPrint("Error loading config file. New one made.");
                Config = new Configuration();
                Config.Save();
                Config.Initialize(PluginInterface);
            }

            ConfigHelper = new ConfigurationUtils();

            // setup UI
            MainWindow = new MainWindow(this);
            ConfigWindow = new ConfigWindow(this);

            WindowSystem = new WindowSystem("WhoSaidWhatNow");
            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
            PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
            PluginInterface.UiBuilder.Draw += FileDialogManager.Draw;

            // add events/listeners
            ClientState.Login += OnLogin;
            ClientState.Logout += OnLogout;
            ChatListener = new ChatService(ChatGui);
            PlayerService = new PlayerUtils();
            Framework.Update += OnFrameworkUpdate;

            // commands
            CommandManager.AddHandler(COMMAND, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open main window"
            });

            try
            {
                if (ClientState.IsLoggedIn)
                {
                    Players.Clear();
                    Config.CurrentPlayer = string.Empty;
                    // PlayerUtils.SetCurrentPlayer();
                    PlayerUtils.CheckTrackedPlayers();
                }
            }
            catch
            {
                Logger.Error("Plugin loaded and thought there was a character logged in, but there wasn't.");
            }
        }

        //TODO: make sure we're disposing of everything we need to appropriately
        public void Dispose()
        {
            ChatListener.Dispose();

            PluginInterface.UiBuilder.Draw -= DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUI;
            PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUI;
            PluginInterface.UiBuilder.Draw -= FileDialogManager.Draw;

            WindowSystem.RemoveAllWindows();
            ConfigWindow.Dispose();
            MainWindow.Dispose();

            CommandManager.RemoveHandler(COMMAND);

            ClientState.Login -= OnLogin;
            ClientState.Logout -= OnLogout;
            Framework.Update -= OnFrameworkUpdate;
        }

        private void OnCommand(string command, string args)
        {
            if (args.Equals("on"))
            {
                ChatGui.Print("WhoWhat is ON.", "WhoWhat");
                Config.Enabled = true;
            }
            else if (args.Equals("off"))
            {
                ChatGui.Print("WhoWhat is OFF.", "WhoWhat");
                Config.Enabled = false;
            }
            else if (args.Equals("refresh"))
            {
                MainWindow.IsOpen = false;
                ConfigurationUtils.refresh();
                MainWindow.IsOpen = true;
                ChatGui.Print("WhoWhat refreshed. All temporary tracked players removed.", "WhoWhat");
            }
            else if (args.Equals("reset"))
            {
                MainWindow.IsOpen = false;
                ConfigWindow.IsOpen = false;
                ConfigurationUtils.reset();
                MainWindow.IsOpen = true;
                ConfigWindow.IsOpen = true;
                ChatGui.Print("WhoWhat refreshed. Most settings reset.", "WhoWhat");
            }
            else if (args.Equals("config"))
            {
                ConfigWindow.Toggle();
            }
            else
            {
                MainWindow.Toggle();
            }
        }

        //set the current player when logging in
        private void OnLogin()
        {
            Players.Clear();
            Config.CurrentPlayer = string.Empty;
            PlayerUtils.CheckTrackedPlayers();
        }

        //close all windows when logging out so that the windows refresh
        private void OnLogout(int type, int code)
        {
            MainWindow.IsOpen = false;
            SelectedPlayer = null;
            Players.Clear();
        }

        private void DrawUI() => WindowSystem.Draw();
        public void ToggleConfigUI() => ConfigWindow.Toggle();
        public void ToggleMainUI() => MainWindow.Toggle();

        private async void OnFrameworkUpdate(IFramework framework)
        {
            if (ClientState.LocalPlayer != null)
            {
                if (!ClientState.LocalPlayer.Name.ToString().Equals(Config.CurrentPlayer))
                {
                    Config.CurrentPlayer = await framework.RunOnTick(() => ClientState.LocalPlayer?.Name.ToString()) ??
                                   Config.CurrentPlayer;
                    CurrentPlayer = new Player(ClientState.LocalPlayer, true);
                }
            }
        }
    }
}
