using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Stylesheets.Sheetlets;

[CommonSheetlet]
public sealed class ProgressBarSheetlet : Sheetlet<PalettedStylesheet>
{
    public override StyleRule[] GetRules(PalettedStylesheet sheet, object config)
    {
        // TODO: 1) hardcoded colors, 2) yuck
        var progressBarBackground = new StyleBoxSDFBox()
        {
            BackgroundColor = sheet.PanelPalette.PanelSunken,
            CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius),
            BorderThickness = sheet.PanelPalette.PanelBorderThickness,
            BorderColor = sheet.PositivePalette.Element,
        };
        progressBarBackground.SetContentMarginOverride(StyleBox.Margin.Vertical, 14.5f);
        var progressBarForeground = new StyleBoxSDFBox()
        {
            BackgroundColor = sheet.PositivePalette.Element,
            CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius),
            BorderThickness = sheet.PanelPalette.PanelBorderThickness,
            BorderColor = sheet.PositivePalette.Element,
        };
        progressBarForeground.SetContentMarginOverride(StyleBox.Margin.Vertical, 14.5f);

        return
        [
            E<ProgressBar>()
                .Prop(ProgressBar.StylePropertyBackground, progressBarBackground)
                .Prop(ProgressBar.StylePropertyForeground, progressBarForeground),
        ];
    }
}
