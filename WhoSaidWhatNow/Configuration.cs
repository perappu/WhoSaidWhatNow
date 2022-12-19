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

        //Autoscrolling on the chat log window
        public bool AutoScroll { get; set; } = false;

        //TODO: Channel toggles
        public IDictionary<XivChatType, bool> ChannelToggles = new Dictionary<XivChatType, bool>()
        {
            { XivChatType.Say, true},
            { XivChatType.TellIncoming, true},
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
            { XivChatType.CrossLinkShell7, true}
        };

        //TODO: Custom chat color values, ideally using ingame colors

        //Default chat color values
        public static readonly IDictionary<XivChatType, Vector4> ChatColors = new Dictionary<XivChatType, Vector4>()
        {
            { XivChatType.Say, new Vector4(0.969f,0.969f,0.961f, 1f)},
            { XivChatType.TellIncoming, new Vector4(1f,0.784f,0.929f, 1f) },
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
