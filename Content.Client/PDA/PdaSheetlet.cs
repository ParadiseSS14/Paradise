using System.Numerics;
using Content.Client.PDA;
using Content.Client.Resources;
using Content.Client.Stylesheets;
using Content.Client.Stylesheets.Colorspace;
using Content.Client.Stylesheets.Sheetlets;
using Content.Client.Stylesheets.SheetletConfigs;
using Content.Client.Stylesheets.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.PDA;

[CommonSheetlet]
public sealed class PdaSheetlet<T> : Sheetlet<T> where T: PalettedStylesheet, IPanelConfig, IButtonConfig
{
    public override StyleRule[] GetRules(T sheet, object config)
    {
        // TODO: This should have its own set of images, instead of using button cfg directly.
        var angleBorderRect =
            ResCache.GetTexture("/Textures/Interface/Nano/geometric_panel_border.svg.96dpi.png").IntoPatch(StyleBox.Margin.All, 10);

        var backgroundBox = new StyleBoxSDFBox()
        {
            BorderColor = sheet.PanelPalette.PanelBorderColor,
            BorderThickness = sheet.PanelPalette.PanelBorderThickness,
            CornerRadius = Vector4.Zero,
            DoGradient = true,
            BackgroundColor = sheet.PanelPalette.Background.NudgeLightness(0.02f),
            GradientBottomColor = sheet.PanelPalette.Background.NudgeLightness(-0.02f),
        };

        var titleBar = new StyleBoxSDFBox()
        {
            BackgroundColor = sheet.PanelPalette.Background.NudgeLightness(-0.05f),
        };

        var pdaListingButton = new StyleBoxSDFBox()
        {
            CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius),
            BackgroundColor = sheet.PanelPalette.PanelSecondary.NudgeLightness(0.2f).WithAlpha(0f),
        };

        var pdaListingButtonHovered = new StyleBoxSDFBox()
        {
            CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius),
            BackgroundColor = sheet.PanelPalette.PanelSecondary.NudgeLightness(0.4f),
        };

        return
        [
            //PDA - Backgrounds
            E<PanelContainer>()
                .Class("PdaContentBackground")
                .Prop(PanelContainer.StylePropertyPanel, backgroundBox)
                .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#25252a")),

            E<PanelContainer>()
                .Class("PdaBackgroundRect")
                .Prop(PanelContainer.StylePropertyPanel, StyleBoxHelpers.BaseStyleBox((sheet)))
                .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#717059")),

            //PDA - Buttons
            E<PdaSettingsButton>()
                .Pseudo(ContainerButton.StylePseudoClassNormal)
                .Prop(PdaSettingsButton.StylePropertyBgColor, Color.FromHex(PdaSettingsButton.NormalBgColor))
                .Prop(PdaSettingsButton.StylePropertyFgColor, Color.FromHex(PdaSettingsButton.EnabledFgColor)),

            E<PdaSettingsButton>()
                .Pseudo(ContainerButton.StylePseudoClassHover)
                .Prop(PdaSettingsButton.StylePropertyBgColor, Color.FromHex(PdaSettingsButton.HoverColor))
                .Prop(PdaSettingsButton.StylePropertyFgColor, Color.FromHex(PdaSettingsButton.EnabledFgColor)),

            E<PdaSettingsButton>()
                .Pseudo(ContainerButton.StylePseudoClassPressed)
                .Prop(PdaSettingsButton.StylePropertyBgColor, Color.FromHex(PdaSettingsButton.PressedColor))
                .Prop(PdaSettingsButton.StylePropertyFgColor, Color.FromHex(PdaSettingsButton.EnabledFgColor)),

            E<PdaSettingsButton>()
                .Pseudo(ContainerButton.StylePseudoClassDisabled)
                .Prop(PdaSettingsButton.StylePropertyBgColor, Color.FromHex(PdaSettingsButton.NormalBgColor))
                .Prop(PdaSettingsButton.StylePropertyFgColor, Color.FromHex(PdaSettingsButton.DisabledFgColor)),

            //PDA - Text
            E<Label>()
                .Class("PdaContentFooterText")
                .Prop(Label.StylePropertyFont, sheet.BaseFont.GetFont(10))
                .Prop(Label.StylePropertyFontColor, Color.FromHex("#757575")),

            E<Label>()
                .Class("PdaWindowFooterText")
                .Prop(Label.StylePropertyFont, sheet.BaseFont.GetFont(10))
                .Prop(Label.StylePropertyFontColor, Color.FromHex("#333d3b")),

            E<PanelContainer>().Class("PDAHeaderBar").Prop(PanelContainer.StylePropertyPanel, titleBar),

            E<ContainerButton>().Class("PDAHoverButton").Prop(Button.StylePropertyStyleBox, pdaListingButton),
            E<ContainerButton>().Class("PDAHoverButton").PseudoHovered().Prop(Button.StylePropertyStyleBox, pdaListingButtonHovered),
        ];
    }
}

