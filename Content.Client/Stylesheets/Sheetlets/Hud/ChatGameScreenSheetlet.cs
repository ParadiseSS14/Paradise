using Content.Client.UserInterface.Screens;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Stylesheets.Sheetlets.Hud;

[CommonSheetlet]
public sealed class ChatGameScreenSheetlet : Sheetlet<PalettedStylesheet>
{
    public override StyleRule[] GetRules(PalettedStylesheet sheet, object config)
    {
        // Tojo - I'm not a fan of palettes. They encourage poor design in my opinion.
        return
        [
            E()
                .Class(SeparatedChatGameScreen.StyleClassChatContainer)
                .Panel(new StyleBoxSDFBox(sheet.PanelPalette.PanelPrimary)),
            E<OutputPanel>()
                .Class(SeparatedChatGameScreen.StyleClassChatOutput)
                .Panel(new StyleBoxSDFBox(sheet.PanelPalette.PanelPrimary)),
        ];
    }
}
