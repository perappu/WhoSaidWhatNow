using Dalamud.Game.Config;
using Dalamud.Game.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using WhoSaidWhatNow.Objects;


namespace WhoSaidWhatNow.Utils
{
    public class ConfigurationUtils
    {

        public static void refresh()
        {
            Plugin.Logger.Debug("refresh called");
            Plugin.Players.Clear();
            Plugin.Config.CurrentPlayer = string.Empty;
            PlayerUtils.SetCurrentPlayer();
            PlayerUtils.CheckTrackedPlayers();
        }

        public static void reset()
        {
            Plugin.Config.AlwaysTrackedPlayers = new List<TrackedPlayer>();
            Plugin.Players.Clear();
            Plugin.ChatEntries.Clear();
            Plugin.Config.CurrentPlayer = string.Empty;
            PlayerUtils.SetCurrentPlayer();
            PlayerUtils.CheckTrackedPlayers();
        }

        public static void SetConfigColors()
        {
            foreach (var chatColor in Plugin.Config.ChatColors)
            {
                Plugin.GameConfig.TryGet(ChatTypeToConfigColor(chatColor.Key), out uint color);
                Plugin.Config.ChatColors[chatColor.Key] = GenerateRgba(color);
            }
        }

        public static Vector4 GenerateRgba(uint color)
        {
            var c = Color.FromArgb(0xFF, Color.FromArgb((int)color));
            return new Vector4(c.R / 255f, c.G / 255f, c.B / 255f, 1f);
        }

        public static UiConfigOption ChatTypeToConfigColor(XivChatType chatType)
        {
            switch (chatType)
            {
                case XivChatType.Say: return UiConfigOption.ColorSay;
                case XivChatType.TellIncoming: return UiConfigOption.ColorTell;
                case XivChatType.TellOutgoing: return UiConfigOption.ColorTell;
                case XivChatType.StandardEmote: return UiConfigOption.ColorEmote;
                case XivChatType.CustomEmote: return UiConfigOption.ColorEmote;
                case XivChatType.Shout: return UiConfigOption.ColorShout;
                case XivChatType.Yell: return UiConfigOption.ColorYell;
                case XivChatType.Party: return UiConfigOption.ColorParty;
                case XivChatType.CrossParty: return UiConfigOption.ColorParty;
                case XivChatType.Alliance: return UiConfigOption.ColorAlliance;
                case XivChatType.FreeCompany: return UiConfigOption.ColorFCompany;
                case XivChatType.Ls1: return UiConfigOption.ColorLS1;
                case XivChatType.Ls2: return UiConfigOption.ColorLS2;
                case XivChatType.Ls3: return UiConfigOption.ColorLS3;
                case XivChatType.Ls4: return UiConfigOption.ColorLS4;
                case XivChatType.Ls5: return UiConfigOption.ColorLS5;
                case XivChatType.Ls6: return UiConfigOption.ColorLS6;
                case XivChatType.Ls7: return UiConfigOption.ColorLS7;
                case XivChatType.Ls8: return UiConfigOption.ColorLS8;
                case XivChatType.CrossLinkShell1: return UiConfigOption.ColorCWLS;
                case XivChatType.CrossLinkShell2: return UiConfigOption.ColorCWLS2;
                case XivChatType.CrossLinkShell3: return UiConfigOption.ColorCWLS3;
                case XivChatType.CrossLinkShell4: return UiConfigOption.ColorCWLS4;
                case XivChatType.CrossLinkShell5: return UiConfigOption.ColorCWLS5;
                case XivChatType.CrossLinkShell6: return UiConfigOption.ColorCWLS6;
                case XivChatType.CrossLinkShell7: return UiConfigOption.ColorCWLS7;
                case XivChatType.CrossLinkShell8: return UiConfigOption.ColorCWLS8;
                case XivChatType.NoviceNetwork: return UiConfigOption.ColorBeginner;
                case XivChatType.PvPTeam: return UiConfigOption.ColorPvPGroup;
            }
            return UiConfigOption.ColorSay;
        }

        public static string ChatTypeToFormat(XivChatType chatType)
        {
            switch (chatType)
            {
                case XivChatType.Say: return "{0}: {1}";
                case XivChatType.TellIncoming: return "{0} >> {1}";
                case XivChatType.TellOutgoing: return ">> {0}: {1}";
                case XivChatType.StandardEmote: return "{0} {1}";
                case XivChatType.CustomEmote: return "{0} {1}";
                case XivChatType.Shout: return "{0} shouts: {1}";
                case XivChatType.Yell: return "{0} yells: {1}";
                case XivChatType.Party: return "({0}) {1}";
                case XivChatType.CrossParty: return "({0}) {1}";
                case XivChatType.Alliance: return "(({0})) {1}";
                case XivChatType.FreeCompany: return "[FC]<{0}> {1}";
                case XivChatType.Ls1: return "[LS1]<{0}> {1}";
                case XivChatType.Ls2: return "[LS2]<{0}> {1}";
                case XivChatType.Ls3: return "[LS3]<{0}> {1}";
                case XivChatType.Ls4: return "[LS4]<{0}> {1}";
                case XivChatType.Ls5: return "[LS5]<{0}> {1}";
                case XivChatType.Ls6: return "[LS6]<{0}> {1}";
                case XivChatType.Ls7: return "[LS7]<{0}> {1}";
                case XivChatType.Ls8: return "[LS8]<{0}> {1}";
                case XivChatType.CrossLinkShell1: return "[CWLS1]<{0}> {1}";
                case XivChatType.CrossLinkShell2: return "[CWLS2]<{0}> {1}";
                case XivChatType.CrossLinkShell3: return "[CWLS3]<{0}> {1}";
                case XivChatType.CrossLinkShell4: return "[CWLS4]<{0}> {1}";
                case XivChatType.CrossLinkShell5: return "[CWLS5]<{0}> {1}";
                case XivChatType.CrossLinkShell6: return "[CWLS6]<{0}> {1}";
                case XivChatType.CrossLinkShell7: return "[CWLS7]<{0}> {1}";
                case XivChatType.CrossLinkShell8: return "[CWLS8]<{0}> {1}";
                case XivChatType.NoviceNetwork: return "[NOVICE]{0}: {1}";
                case XivChatType.PvPTeam: return "[PvP]{0}: {1}";
            }
            return "{0}: {1}";
        }
    }
}
