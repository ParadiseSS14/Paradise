using System.Numerics;
using Content.Client.Stylesheets.Fonts;
using Content.Client.Stylesheets.SheetletConfigs;
using Content.Client.Stylesheets.Stylesheets;
using Content.Client.UserInterface.Controls;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Stylesheets.Sheetlets;

[CommonSheetlet]
public sealed class MenuButtonSheetlet<T> : Sheetlet<T> where T : PalettedStylesheet, IButtonConfig, IIconConfig
{
    private static MutableSelectorElement CButton()
    {
        return E<MenuButton>();
    }

    public override StyleRule[] GetRules(T sheet, object config)
    {
        IButtonConfig cfg = sheet;


        var topButtonTransparent = new StyleBoxSDFBox()
        {
            BackgroundColor = Color.FromHex("#FFFFFF00"),
            CornerRadius = new Vector4(5f),
        };
        var topButtonTransparentHovered = new StyleBoxSDFBox()
        {
            BackgroundColor = Color.FromHex("#FFFFFF33"),
            CornerRadius = new Vector4(5f),
        };

        var rules = new List<StyleRule>
        {
            CButton().Box(topButtonTransparent).Prop("ColorNormal", sheet.PrimaryPalette.Base),
            CButton().PseudoHovered().Box(topButtonTransparentHovered),
            E<Label>()
                .Class(MenuButton.StyleClassLabelTopButton)
                .Prop(Label.StylePropertyFont, sheet.BaseFont.GetFont(14, FontKind.Bold)),
        };

        ButtonSheetlet<T>.MakeButtonRules<MenuButton>(rules, cfg.ButtonPalette, null);

        return rules.ToArray();
    }
}
