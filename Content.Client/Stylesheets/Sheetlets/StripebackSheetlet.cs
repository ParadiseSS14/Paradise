using System.Numerics;
using Content.Client.Stylesheets.SheetletConfigs;
using Content.Client.Stylesheets.Stylesheets;
using Content.Client.UserInterface.Controls;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Stylesheets.Sheetlets;

[CommonSheetlet]
public sealed class StripebackSheetlet<T> : Sheetlet<T> where T : PalettedStylesheet, IStripebackConfig
{
    public override StyleRule[] GetRules(T sheet, object config)
    {
        IStripebackConfig stripebackCfg = sheet;

        var stripeBack = new StyleBoxSDFTextureBox
        {
            Texture = sheet.GetTextureOr(stripebackCfg.StripebackPath, NanotrasenStylesheet.TextureRoot),
            Modulate = sheet.PanelPalette.Background,
            CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius),
        };

        var warningStripeBack = new StyleBoxSDFTextureBox
        {
            Texture = sheet.GetTextureOr(stripebackCfg.StripebackPath, NanotrasenStylesheet.TextureRoot),
            Modulate = sheet.SemanticPalette.Notice,
            CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius),
        };
        var dangerStripeBack = new StyleBoxSDFTextureBox
        {
            Texture = sheet.GetTextureOr(stripebackCfg.StripebackPath, NanotrasenStylesheet.TextureRoot),
            Modulate = sheet.SemanticPalette.Danger,
            CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius),
        };
        var infoStripeBack = new StyleBoxSDFTextureBox
        {
            Texture = sheet.GetTextureOr(stripebackCfg.StripebackPath, NanotrasenStylesheet.TextureRoot),
            Modulate = sheet.SemanticPalette.Info,
            CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius),
        };

        return
        [
            E<StripeBack>()
                .Prop(StripeBack.StylePropertyBackground, stripeBack),
            E<StripeBack>().Class("WarningStripe").Prop(StripeBack.StylePropertyBackground, warningStripeBack),
            E<StripeBack>().Class("DangerStripe").Prop(StripeBack.StylePropertyBackground, dangerStripeBack),
            E<StripeBack>().Class("InfoStripe").Prop(StripeBack.StylePropertyBackground, infoStripeBack),
        ];
    }
}
