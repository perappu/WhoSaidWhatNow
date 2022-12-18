using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace WhoSaidWhatNow
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        //Always start the plugin as off by default, to prevent crashes
        public bool IsOn { get; set; } = false;

        public static readonly IDictionary<XivChatType, Vector4> ChatColors = new Dictionary<XivChatType, Vector4>()
        {
            { XivChatType.Say, new Vector4(247, 247, 245, 255)},
            { XivChatType.TellIncoming, new Vector4(255, 200, 237, 255) },
            { XivChatType.StandardEmote, new Vector4(90, 224, 185, 255) },
            { XivChatType.CustomEmote, new Vector4(90, 224, 185, 255) },
            { XivChatType.Shout, new Vector4(255, 186, 124, 255) },
            { XivChatType.Yell, new Vector4(255, 255, 0, 255) },
            { XivChatType.Party, new Vector4(66, 200, 219, 255) },
            { XivChatType.CrossParty, new Vector4(66, 200, 219, 255) },
            { XivChatType.Alliance, new Vector4(255, 157, 32, 255) },
            { XivChatType.FreeCompany, new Vector4(159, 208, 214, 255) },
            { XivChatType.Ls1, new Vector4(220, 245, 110, 255) },
            { XivChatType.Ls2, new Vector4(220, 245, 110, 255) },
            { XivChatType.Ls3, new Vector4(220, 245, 110, 255) },
            { XivChatType.Ls4, new Vector4(220, 245, 110, 255) },
            { XivChatType.Ls5, new Vector4(220, 245, 110, 255) },
            { XivChatType.Ls6, new Vector4(220, 245, 110, 255) },
            { XivChatType.Ls7, new Vector4(220, 245, 110, 255) },
            { XivChatType.Ls8, new Vector4(220, 245, 110, 255) },
            { XivChatType.CrossLinkShell1, new Vector4(220, 245, 110, 255) },
            { XivChatType.CrossLinkShell2, new Vector4(220, 245, 110, 255) },
            { XivChatType.CrossLinkShell3, new Vector4(220, 245, 110, 255) },
            { XivChatType.CrossLinkShell4, new Vector4(220, 245, 110, 255) },
            { XivChatType.CrossLinkShell5, new Vector4(220, 245, 110, 255) },
            { XivChatType.CrossLinkShell6, new Vector4(220, 245, 110, 255) },
            { XivChatType.CrossLinkShell7, new Vector4(220, 245, 110, 255) },
        };

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}
