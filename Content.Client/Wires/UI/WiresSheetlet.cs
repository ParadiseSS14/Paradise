using Content.Client.Stylesheets;
using Content.Client.Stylesheets.Colorspace;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Wires.UI;

[CommonSheetlet]
public sealed class WiresSheetlet : Sheetlet<PalettedStylesheet>
{
    public override StyleRule[] GetRules(PalettedStylesheet sheet, object config)
    {
        var boxBackground = new StyleBoxSDFBox { BackgroundColor = Color.Transparent };
        var boxItemBackground = new StyleBoxSDFBox
        {
            BackgroundColor = sheet.PanelPalette.PanelSunken.NudgeLightness(0.08f).WithAlpha(0.3f), ContentMarginTopOverride = 4,
            ContentMarginBottomOverride = 4, ContentMarginRightOverride = 4, ContentMarginLeftOverride = 4,
            BorderThickness = 1f,
            BorderColor = sheet.PanelPalette.PanelBorderColor,
        };
        var boxSelected = new StyleBoxSDFBox
        {
            BackgroundColor = sheet.PanelPalette.PanelSunken.NudgeLightness(0.12f).WithAlpha(0.4f), ContentMarginTopOverride = 4,
            ContentMarginBottomOverride = 4, ContentMarginRightOverride = 4, ContentMarginLeftOverride = 4,
            BorderThickness = 1f,
            BorderColor = sheet.PanelPalette.PanelBorderColor,
        };
        var contentBackground = new StyleBoxSDFBox { BackgroundColor = sheet.PanelPalette.PanelSunken };

        return
        [
            E<Button>().Class("ploopy").Prop(Button.StylePropertyStyleBox, boxItemBackground),
        ];
    }
}
