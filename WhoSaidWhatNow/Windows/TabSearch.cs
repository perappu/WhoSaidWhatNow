using ImGuiNET;
using WhoSaidWhatNow;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using WhoSaidWhatNow.Windows;
using WhoSaidWhatNow.Utils;



public class TabSearch
{
    public TabSearch(MainWindow main)
    {

        main.toggleWindow(true);

        if (ImGui.BeginTabItem("Chat Search"))
        {
            ImGui.InputTextWithHint("", "Filter by message content...", ref Plugin.FilterSearch, 40);

            ImGui.BeginChild(MainWindow.ID_PANEL_RIGHT, new Vector2(0, 0), true);
            ImGui.BeginGroup();

            // for all chat entries where the Sender or Message contain a match for our filter;
            foreach (var c in Plugin.ChatEntries.Where(c => c.Value.Message.ToLower().Contains(Plugin.FilterSearch.ToLower()) || c.Value.Sender.ToString()!.ToLower().Contains(Plugin.FilterSearch.ToLower())))
            {
                try
                {
                    // if we are displaying this type of message;
                    if (Plugin.Config.ChannelToggles[c.Value.Type] == true)
                    {
                        ChatUtils.ShowMessage(c);
                    }
                }
                catch
                {
                    // trying to access a player that doesn't exist, just ignore
                }
            }
            ImGui.EndGroup();
            ImGui.EndChild();

            ImGui.EndTabItem();

        }


    }
}