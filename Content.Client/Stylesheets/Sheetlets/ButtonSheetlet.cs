using System.Numerics;
using Content.Client.Lathe.UI;
using Content.Client.Resources;
using Content.Client.Stylesheets.Colorspace;
using Content.Client.Stylesheets.Palette;
using Content.Client.Stylesheets.SheetletConfigs;
using Content.Client.Stylesheets.Stylesheets;
using Content.Client.UIControls;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Stylesheets.Sheetlets;

[CommonSheetlet]
public sealed class ButtonSheetlet<T> : Sheetlet<T> where T : PalettedStylesheet, IButtonConfig, IIconConfig
{
    public override StyleRule[] GetRules(T sheet, object config)
    {
        IButtonConfig buttonCfg = sheet;
        IIconConfig iconCfg = sheet;

        var crossTex = (sheet.ResCache.GetTexture("/Textures/Interface/Default/cross_ui.png"));
        var refreshTex = sheet.GetTextureOr(iconCfg.RefreshIconPath, NanotrasenStylesheet.TextureRoot);

        var roundedButton = new StyleBoxSDFBox()
        {
            CornerRadius = new Vector4(2f),
            BackgroundColor = sheet.PrimaryPalette.Base,
            ContentMarginTopOverride = 2,
            ContentMarginBottomOverride = 2,
            ContentMarginLeftOverride = 4,
            ContentMarginRightOverride = 4,
        };

        var transparentButton = new StyleBoxSDFBox()
        {
            CornerRadius = new Vector4(5f),
            BackgroundColor = Color.Transparent,
        };

        var closeButtonStyleBox = new StyleBoxIconBox(crossTex)
        {
            HighlightColor = sheet.PositivePalette.Element,
            UnhighlitColor = sheet.PanelPalette.Background,
            BackgroundColor = sheet.PanelPalette.Background,
        };

        var rules = new List<StyleRule>
        {
            // Set textures for the kinds of buttons

            CButton()
                .Box(roundedButton),
            CButton()
                .Class(StyleClass.ButtonSmall)
                .Box(StyleBoxHelpers.SmallStyleBox(sheet)),
            CButton()
                .Class(StyleClass.ButtonSmall)
                .ParentOf(E<Label>())
                .Font(sheet.BaseFont.GetFont(8)),
            CButton().Class(StyleClass.ButtonBig).ParentOf(E<Label>()).Font(sheet.BaseFont.GetFont(14)),

            // Cross Button (Red)
            E<TextureButton>()
                .Class(StyleClass.CrossButtonRed)
                .Prop(TextureButton.StylePropertyTexture, crossTex),

            // Refresh Button
            E<TextureButton>()
                .Class(StyleClass.RefreshButton)
                .Prop(TextureButton.StylePropertyTexture, refreshTex),

            // Ensure labels in buttons are aligned.
            E<Label>()
                // ReSharper disable once AccessToStaticMemberViaDerivedType
                .Class(Button.StyleClassButton)
                .AlignMode(Label.AlignMode.Center),

            CButton().Class("transparentButton").Box(transparentButton),
            CButton().Class("transparentButton").PseudoHovered().Box(roundedButton),

            E<CloseContainerButton>().Class("closeButton").ForkedBox(closeButtonStyleBox),
            E<CloseContainerButton>().Class("closeButton").Pseudo(ContainerButton.StylePseudoClassNormal)
                .Prop(Control.StylePropertyModulateSelf, Color.White),
            E<CloseContainerButton>().Class("closeButton").Pseudo(ContainerButton.StylePseudoClassHover)
                .Prop(Control.StylePropertyModulateSelf, Color.White),

            // Have disabled button's text be faded
            CButton().PseudoDisabled().ParentOf(E<Label>()).FontColor(Color.FromHex("#E5E5E581")),
            CButton().PseudoDisabled().ParentOf(E()).ParentOf(E<Label>()).FontColor(Color.FromHex("#E5E5E581")),
        };
        // Texture button modulation
        MakeButtonRules<TextureButton>(rules, Palettes.AlphaModulate, null);
        MakeButtonRules<TextureButton>(rules, sheet.NegativePalette, StyleClass.CrossButtonRed);

        MakeButtonRules(rules, sheet.PrimaryPalette, null);
        MakeButtonRules(rules, sheet.PrimaryPalette, StyleClass.Positive);
        MakeButtonRules(rules, sheet.PrimaryPalette, StyleClass.Negative);

        return rules.ToArray();
    }

    public static void MakeButtonRules<TC>(
        List<StyleRule> rules,
        ColorPalette palette,
        string? styleclass)
        where TC : Control
    {
        rules.AddRange([
            E<TC>().MaybeClass(styleclass).PseudoNormal().Modulate(Color.White),
            E<TC>().MaybeClass(styleclass).PseudoHovered().Modulate(Color.White.NudgeLightness(0.1f)),
            E<TC>().MaybeClass(styleclass).PseudoPressed().Modulate(Color.White.NudgeLightness(-0.05f)),
            E<TC>().MaybeClass(styleclass).PseudoDisabled().Modulate(Color.White.NudgeLightness(-0.15f)),
        ]);
    }

    public static void MakeButtonRules(
        List<StyleRule> rules,
        ColorPalette palette,
        string? styleclass)
    {
        rules.AddRange([
            E().MaybeClass(styleclass).PseudoNormal().Modulate(Color.White),
            E().MaybeClass(styleclass).PseudoHovered().Modulate(Color.White.NudgeLightness(0.1f)),
            E().MaybeClass(styleclass).PseudoPressed().Modulate(Color.White.NudgeLightness(-0.05f)),
            E().MaybeClass(styleclass).PseudoDisabled().Modulate(Color.White.NudgeLightness(-0.15f)),
        ]);
    }


    private static MutableSelectorElement CButton()
    {
        return E<ContainerButton>().Class(ContainerButton.StyleClassButton);
    }
}

// this is currently the only other "helper" type class, if any more crop up consider making a specific directory for them
public static class StyleBoxHelpers
{
    // TODO: Figure out a nicer way to store/represent these hardcoded margins. This is icky.
    public static StyleBoxTexture BaseStyleBox<T>(T sheet) where T : PalettedStylesheet, IButtonConfig
    {
        var baseBox = new StyleBoxTexture
        {
            Texture = sheet.GetTextureOr(sheet.BaseButtonPath, NanotrasenStylesheet.TextureRoot),
        };
        baseBox.SetPatchMargin(StyleBox.Margin.All, 10);
        baseBox.SetPadding(StyleBox.Margin.All, 1);
        baseBox.SetContentMarginOverride(StyleBox.Margin.Vertical, 2);
        baseBox.SetContentMarginOverride(StyleBox.Margin.Horizontal, 14);
        return baseBox;
    }

    public static StyleBoxTexture OpenLeftStyleBox<T>(T sheet) where T : PalettedStylesheet, IButtonConfig
    {
        var openLeftBox = new StyleBoxTexture(BaseStyleBox(sheet))
        {
            Texture = new AtlasTexture(sheet.GetTextureOr(sheet.OpenLeftButtonPath, NanotrasenStylesheet.TextureRoot),
                UIBox2.FromDimensions(new Vector2(10, 0), new Vector2(14, 24))),
        };
        openLeftBox.SetPatchMargin(StyleBox.Margin.Left, 0);
        openLeftBox.SetContentMarginOverride(StyleBox.Margin.Left, 8);
        // openLeftBox.SetPadding(StyleBox.Margin.Left, 1);
        return openLeftBox;
    }

    public static StyleBoxTexture OpenRightStyleBox<T>(T sheet) where T : PalettedStylesheet, IButtonConfig
    {
        var openRightBox = new StyleBoxTexture(BaseStyleBox(sheet))
        {
            Texture = new AtlasTexture(sheet.GetTextureOr(sheet.OpenRightButtonPath, NanotrasenStylesheet.TextureRoot),
                UIBox2.FromDimensions(new Vector2(0, 0), new Vector2(14, 24))),
        };
        openRightBox.SetPatchMargin(StyleBox.Margin.Right, 0);
        openRightBox.SetContentMarginOverride(StyleBox.Margin.Right, 8);
        openRightBox.SetPadding(StyleBox.Margin.Right, 1);
        return openRightBox;
    }

    public static StyleBoxTexture SquareStyleBox<T>(T sheet) where T : PalettedStylesheet, IButtonConfig
    {
        var openBothBox = new StyleBoxTexture(BaseStyleBox(sheet))
        {
            Texture = new AtlasTexture(sheet.GetTextureOr(sheet.OpenBothButtonPath, NanotrasenStylesheet.TextureRoot),
                UIBox2.FromDimensions(new Vector2(10, 0), new Vector2(3, 24))),
        };
        openBothBox.SetPatchMargin(StyleBox.Margin.Horizontal, 0);
        openBothBox.SetContentMarginOverride(StyleBox.Margin.Horizontal, 8);
        openBothBox.SetPadding(StyleBox.Margin.Horizontal, 1);
        return openBothBox;
    }

    public static StyleBoxTexture SmallStyleBox<T>(T sheet) where T : PalettedStylesheet, IButtonConfig
    {
        var smallBox = new StyleBoxTexture
        {
            Texture = sheet.GetTextureOr(sheet.SmallButtonPath, NanotrasenStylesheet.TextureRoot),
        };
        return smallBox;
    }
}
