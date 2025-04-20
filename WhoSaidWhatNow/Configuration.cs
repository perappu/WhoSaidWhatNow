using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Numerics;
using WhoSaidWhatNow.Objects;

namespace WhoSaidWhatNow
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        /// <summary>
        /// Start plugin as on by default for better UX. Improved UX outweighs performance issue for now
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Whether the chat log window should autoscroll when opening log.
        /// </summary>
        public bool AutoscrollOnOpen { get; set; } = false;

        /// <summary>
        /// Show user's server in messages.
        /// </summary>
        public bool ShowServer { get; set; } = true;

        /// <summary>
        /// Show timestamp in messages.
        /// </summary>
        public bool ShowTimestamp { get; set; } = true;

        /// <summary>
        /// Play a sound if there's a new message in the currently selected window.
        /// </summary>
        public bool PlaySound { get; set; } = false;

        /// <summary>
        /// Player IDs that should always be tracked.
        /// </summary>
        public List<TrackedPlayer> AlwaysTrackedPlayers = new List<TrackedPlayer>();

        public string CurrentPlayer = String.Empty;

        // CHANNEL CONFIGURATION //
        //Channel visibility toggle
        public IDictionary<XivChatType, bool> ChannelToggles = new Dictionary<XivChatType, bool>()
        {
            { XivChatType.Say, true},
            { XivChatType.TellIncoming, true},
            { XivChatType.TellOutgoing, true},
            { XivChatType.StandardEmote, true},
            { XivChatType.CustomEmote, true},
            { XivChatType.Shout, true},
            { XivChatType.Yell, true},
            { XivChatType.Party, true},
            { XivChatType.CrossParty, true},
            { XivChatType.Alliance, true},
            { XivChatType.FreeCompany, true},
            { XivChatType.Ls1, true},
            { XivChatType.Ls2, true},
            { XivChatType.Ls3, true},
            { XivChatType.Ls4, true},
            { XivChatType.Ls5, true},
            { XivChatType.Ls6, true},
            { XivChatType.Ls7, true},
            { XivChatType.Ls8, true},
            { XivChatType.CrossLinkShell1, true},
            { XivChatType.CrossLinkShell2, true},
            { XivChatType.CrossLinkShell3, true},
            { XivChatType.CrossLinkShell4, true},
            { XivChatType.CrossLinkShell5, true},
            { XivChatType.CrossLinkShell6, true},
            { XivChatType.CrossLinkShell7, true},
            { XivChatType.CrossLinkShell8, true},
            { XivChatType.NoviceNetwork, true},
            { XivChatType.PvPTeam, true}
        };

        //Default chat color values
        public IDictionary<XivChatType, Vector4> ChatColors = new Dictionary<XivChatType, Vector4>()
        {
            { XivChatType.Say, new Vector4(0.969f,0.969f,0.961f, 1f)},
            { XivChatType.TellIncoming, new Vector4(1f,0.784f,0.929f, 1f) },
            { XivChatType.TellOutgoing, new Vector4(1f,0.784f,0.929f, 1f) },
            { XivChatType.StandardEmote, new Vector4(0.353f,0.878f,0.725f, 1f) },
            { XivChatType.CustomEmote, new Vector4(0.353f,0.878f,0.725f, 1f) },
            { XivChatType.Shout, new Vector4(1f,0.729f,0.486f, 1f) },
            { XivChatType.Yell, new Vector4(1f, 1f, 0f, 1f) },
            { XivChatType.Party, new Vector4(0.259f,0.784f,0.859f, 1f) },
            { XivChatType.CrossParty, new Vector4(0.259f,0.784f,0.859f, 1f) },
            { XivChatType.Alliance, new Vector4(1f,0.616f,0.125f, 1f) },
            { XivChatType.FreeCompany, new Vector4(0.624f, 0.816f, 0.839f, 1f) },
            { XivChatType.Ls1, new Vector4(0.863f, 0.961f, 0.431f, 1f) },
            { XivChatType.Ls2, new Vector4(0.863f, 0.961f, 0.431f, 1f) },
            { XivChatType.Ls3, new Vector4(0.863f, 0.961f, 0.431f, 1f) },
            { XivChatType.Ls4, new Vector4(0.863f, 0.961f, 0.431f, 1f) },
            { XivChatType.Ls5, new Vector4(0.863f, 0.961f, 0.431f, 1f) },
            { XivChatType.Ls6, new Vector4(0.863f, 0.961f, 0.431f, 1f) },
            { XivChatType.Ls7, new Vector4(0.863f, 0.961f, 0.431f, 1f) },
            { XivChatType.Ls8, new Vector4(0.863f, 0.961f, 0.431f, 1f) },
            { XivChatType.CrossLinkShell1, new Vector4(0.863f, 0.961f, 0.431f, 1f) },
            { XivChatType.CrossLinkShell2, new Vector4(0.863f, 0.961f, 0.431f, 1f) },
            { XivChatType.CrossLinkShell3, new Vector4(0.863f, 0.961f, 0.431f, 1f) },
            { XivChatType.CrossLinkShell4, new Vector4(0.863f, 0.961f, 0.431f, 1f) },
            { XivChatType.CrossLinkShell5, new Vector4(0.863f, 0.961f, 0.431f, 1f) },
            { XivChatType.CrossLinkShell6, new Vector4(0.863f, 0.961f, 0.431f, 1f) },
            { XivChatType.CrossLinkShell7, new Vector4(0.863f, 0.961f, 0.431f, 1f) },
            { XivChatType.CrossLinkShell8, new Vector4(0.863f, 0.961f, 0.431f, 1f) },
            { XivChatType.NoviceNetwork, new Vector4(0.863f, 0.961f, 0.431f, 1f) },
            { XivChatType.PvPTeam, new Vector4(0.624f, 0.816f, 0.839f, 1f) }
        };

        //Channel format for when printing message
        public readonly IDictionary<XivChatType, Tuple<string, string>> GUIFormats = new Dictionary<XivChatType, Tuple<string, string>>()
        {
            { XivChatType.Say, new Tuple<string, string>(" ",": {0}") },
            { XivChatType.TellIncoming, new Tuple<string, string>(" "," >> {0}")},
            { XivChatType.TellOutgoing, new Tuple<string, string>(">> ",": {0}") },
            { XivChatType.StandardEmote, new Tuple<string, string>(" ","{0}") },
            { XivChatType.CustomEmote, new Tuple<string, string>(" ","{0}") },
            { XivChatType.Shout, new Tuple<string, string>(" "," shouts: {0}") },
            { XivChatType.Yell, new Tuple<string, string>(" "," yells: {0}")},
            { XivChatType.Party, new Tuple<string, string>("(",") {0}") },
            { XivChatType.CrossParty, new Tuple<string, string>("(",") {0}") },
            { XivChatType.Alliance, new Tuple<string, string>("((",")) {0}") },
            { XivChatType.FreeCompany, new Tuple<string, string>("[FC]<","> {0}") },
            { XivChatType.Ls1, new Tuple<string, string>("[LS1]<","> {0}")},
            { XivChatType.Ls2, new Tuple<string, string>("[LS2]<","> {0}")},
            { XivChatType.Ls3, new Tuple<string, string>("[LS3]<","> {0}")},
            { XivChatType.Ls4, new Tuple<string, string>("[LS4]<","> {0}")},
            { XivChatType.Ls5, new Tuple < string, string >("[LS5]<", "> {0}")},
            { XivChatType.Ls6, new Tuple < string, string >("[LS6]<", "> {0}")},
            { XivChatType.Ls7, new Tuple < string, string >("[LS7]<", "> {0}")},
            { XivChatType.Ls8, new Tuple < string, string >("[LS8]<", "> {0}")},
            { XivChatType.CrossLinkShell1, new Tuple < string, string >("[CWLS1]<","> {0}")},
            { XivChatType.CrossLinkShell2, new Tuple < string, string >("[CWLS2]<","> {0}")},
            { XivChatType.CrossLinkShell3, new Tuple < string, string >("[CWLS3]<","> {0}")},
            { XivChatType.CrossLinkShell4, new Tuple < string, string >("[CWLS4]<","> {0}")},
            { XivChatType.CrossLinkShell5, new Tuple < string, string >("[CWLS5]<","> {0}")},
            { XivChatType.CrossLinkShell6, new Tuple < string, string >("[CWLS6]<","> {0}")},
            { XivChatType.CrossLinkShell7, new Tuple < string, string >("[CWLS7]<","> {0}")},
            { XivChatType.CrossLinkShell8, new Tuple < string, string >("[CWLS8]<","> {0}")},
            { XivChatType.NoviceNetwork, new Tuple < string, string >("[NOVICE] ",": {0}")},
            { XivChatType.PvPTeam, new Tuple < string, string >("[PvP] ",": {0}")}
        };

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private IDalamudPluginInterface? PluginInterface;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;
        }

        public void Save()
        {
            PluginInterface!.SavePluginConfig(this);
        }
    }
}
