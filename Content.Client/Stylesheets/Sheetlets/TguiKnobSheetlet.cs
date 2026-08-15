using System.Numerics;
using Content.Client.Stylesheets.Colorspace;
using Content.Client.Stylesheets.SheetletConfigs;
using Content.Client.Stylesheets.Stylesheets;
using Content.Client.UserInterface;
using Content.Client.UserInterface.Controls;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Stylesheets.Sheetlets;

[CommonSheetlet]
public sealed class TguiKnobSheetlet<T> : Sheetlet<T> where T : PalettedStylesheet, ICheckboxConfig
{
    public override StyleRule[] GetRules(T sheet, object config)
    {
        ICheckboxConfig checkboxCfg = sheet;

        ResPath monochromeCheckboxUncheckedPath = new("Monotone/monotone_checkbox_unchecked.svg.96dpi.png");
        ResPath monochromeCheckboxCheckedPath = new("Monotone/monotone_checkbox_checked.svg.96dpi.png");

        var uncheckedTex = sheet.GetTextureOr(monochromeCheckboxUncheckedPath, NanotrasenStylesheet.TextureRoot);
        var checkedTex = sheet.GetTextureOr(monochromeCheckboxCheckedPath, NanotrasenStylesheet.TextureRoot);

        var uncheckedBoxBackground = new StyleBoxSDFBox()
        {
            BorderThickness = sheet.PanelPalette.PanelBorderThickness,
            BorderColor = sheet.PanelPalette.PanelBorderColor,
            BackgroundColor = sheet.PanelPalette.PanelSecondary,
            CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius * 2),
        };
        var checkedBoxBackground = new StyleBoxSDFBox()
        {
            BorderThickness = sheet.PanelPalette.PanelBorderThickness,
            BorderColor = sheet.PositivePalette.Element,
            BackgroundColor = sheet.PositivePalette.Background.WithAlpha(.50f).NudgeLightness(-0.1f),
            CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius * 2),
        };

        var selectedHandleBox = new StyleBoxSDFBox()
        {
            BackgroundColor = sheet.PanelPalette.PanelBorderColor.NudgeLightness(0.2f),
        };

        var roundedconfig = new Vector4(
            sheet.PanelPalette.PanelCornerRadius * 3,
            sheet.PanelPalette.PanelCornerRadius * 3,
            0,
            0);

        var tabContainerBoxActive = new StyleBoxSDFBox(sheet.PanelPalette.PanelPrimary)
        {
            CornerRadius = roundedconfig,
            BorderColor = sheet.PanelPalette.PanelBorderColor.NudgeLightness(0.2f),
            BorderThickness = (sheet.PanelPalette.PanelBorderThickness),

        };
        var tabContainerBoxInactive = new StyleBoxSDFBox(sheet.PanelPalette.PanelPrimary)
        {
            CornerRadius =
                roundedconfig,
        };


        return
        [
            E<TguiKnob>()
                .Prop(TguiKnob.StylePropertyFgColor, sheet.PrimaryPalette.Element)
                .Prop(TguiKnob.StylePropertyBgColor, sheet.PrimaryPalette.Element.NudgeLightness(-0.25f).NudgeChroma(-0.1f))
                .Prop(TguiKnob.StylePropertyKnobColor, sheet.PrimaryPalette.Element.NudgeChroma(-1f).NudgeLightness(-0.2f))
                .Prop(TguiKnob.StylePropertyKnobHighlightColor, sheet.PrimaryPalette.Element.NudgeChroma(-1f).NudgeLightness(0.1f)),
        ];
    }
}
