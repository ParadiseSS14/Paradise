using System.Numerics;
using Content.Client.Stylesheets.Colorspace;
using Content.Client.Stylesheets.Palette;
using Content.Client.Stylesheets.SheetletConfigs;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Stylesheets.Sheetlets;

[CommonSheetlet]
public sealed class PanelSheetlet<T> : Sheetlet<T> where T : PalettedStylesheet, IButtonConfig
{
    public override StyleRule[] GetRules(T sheet, object config)
    {
        IButtonConfig buttonCfg = sheet;

        var boxLight = new StyleBoxFlat()
        {
            BackgroundColor = sheet.SecondaryPalette.BackgroundLight,
        };
        var boxDark = new StyleBoxFlat()
        {
            BackgroundColor = sheet.SecondaryPalette.BackgroundDark,
        };
        var boxPositive = new StyleBoxFlat { BackgroundColor = sheet.PositivePalette.Background };
        var boxNegative = new StyleBoxFlat { BackgroundColor = sheet.NegativePalette.Background };
        var boxHighlight = new StyleBoxFlat { BackgroundColor = sheet.HighlightPalette.Background };


        //Paradise

        var panelBackground = new StyleBoxSDFBox()
        {
            BackgroundColor = sheet.PanelPalette.Background,
            BorderThickness = 0,
        };
        var panelChatboxBackground = new StyleBoxSDFBox()
        {
            BackgroundColor = sheet.PanelPalette.Background.NudgeLightness(-0.05f),
            BorderThickness = 0,
        };

        var panelPrimary = new StyleBoxSDFBox()
        {
            BackgroundColor = sheet.PanelPalette.PanelPrimary,
            BorderColor = sheet.PanelPalette.PanelBorderColor,
            BorderThickness = sheet.PanelPalette.PanelBorderThickness,
            CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius),
        };

        var panelSecondary = new StyleBoxSDFBox()
        {
            BackgroundColor = sheet.PanelPalette.PanelSecondary,
            BorderColor = sheet.PanelPalette.PanelBorderColor,
            BorderThickness = sheet.PanelPalette.PanelBorderThickness,
            CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius),
        };

        var panelTertiary = new StyleBoxSDFBox()
        {
            BackgroundColor = sheet.PanelPalette.PanelTertiary,
            BorderColor = sheet.PanelPalette.PanelBorderColor,
            BorderThickness = sheet.PanelPalette.PanelBorderThickness,
            CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius),
        };

        var listViewPanel = new StyleBoxSDFBox()
        {
            BackgroundColor = ModalPalette.Shift(sheet.PanelPalette.PanelPrimary, 0.06f, 0.00f, -20).WithAlpha(0.30f),
            BorderColor = sheet.PanelPalette.PanelBorderColor,
            BorderThickness = sheet.PanelPalette.PanelBorderThickness,
            CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius),
        };

        var roundedButton = new StyleBoxSDFBox()
        {
            CornerRadius = new Vector4(2f),
            BackgroundColor = sheet.PrimaryPalette.Base,
            ContentMarginTopOverride = 2,
            ContentMarginBottomOverride = 2,
            ContentMarginLeftOverride = 4,
            ContentMarginRightOverride = 4,
        };

        //Oasis End

        return
        [
            E<PanelContainer>().Class(StyleClass.PanelLight).Panel(boxLight),
            E<PanelContainer>().Class(StyleClass.PanelDark).Panel(boxDark),

            E<PanelContainer>().Class(StyleClass.Positive).Panel(boxPositive),
            E<PanelContainer>().Class(StyleClass.Negative).Panel(boxNegative),
            E<PanelContainer>().Class(StyleClass.Highlight).Panel(boxHighlight),

            // TODO: this should probably be cleaned up but too many UIs rely on this hardcoded color so I'm scared to touch it
            E<PanelContainer>()
                .Class("BackgroundDark")
                .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat(Color.FromHex("#25252A"))),


            E<PanelContainer>()
                .Class(StyleClass.SurfacePrimary)
                .Panel(panelPrimary),

            E<PanelContainer>()
                .Class(StyleClass.SurfaceSecondary)
                .Panel(panelSecondary),

            E<PanelContainer>()
                .Class(StyleClass.SurfaceTertiary)
                .Panel(panelTertiary),

            E<PanelContainer>()
                .Class(StyleClass.SurfaceTertiary)
                .Panel(panelTertiary),

            E<PanelContainer>()
                .Class(StyleClass.SurfaceBackground)
                .Panel(panelBackground),
            E<PanelContainer>()
                .Class(StyleClass.ChatboxDarkenedBackground)
                .Panel(panelChatboxBackground),

            E<PanelContainer>()
                .Class(StyleClass.ListView)
                .Panel(listViewPanel),

            E<PanelContainer>().Class(Button.StyleClassButton).Prop(PanelContainer.StylePropertyPanel, roundedButton),

            // panels that have the same corner bezels as buttons
            E()
                .Class(StyleClass.BackgroundPanel)
                .Panel(panelBackground),
            E()
                .Class("BackgroundDark")
                .Class(StyleClass.BackgroundPanel)
                .Panel(panelBackground),
             E()
                .Class(StyleClass.BackgroundPanelOpenLeft)
                .Prop(PanelContainer.StylePropertyPanel, StyleBoxHelpers.OpenLeftStyleBox(sheet))
                .Modulate(sheet.SecondaryPalette.Background),
            E()
                .Class(StyleClass.BackgroundPanelOpenRight)
                .Prop(PanelContainer.StylePropertyPanel, StyleBoxHelpers.OpenRightStyleBox(sheet))
                .Modulate(sheet.SecondaryPalette.Background),
        ];
    }
}
