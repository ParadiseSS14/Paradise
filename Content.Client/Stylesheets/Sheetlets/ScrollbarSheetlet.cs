using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Stylesheets.Sheetlets;

[CommonSheetlet]
public sealed class ScrollbarSheetlet : Sheetlet<PalettedStylesheet>
{
    public const int DefaultGrabberSize = 10;

    public override StyleRule[] GetRules(PalettedStylesheet sheet, object config)
    {
        // TODO: hardcoded colors!!!
        var vScrollBarGrabberNormal = new StyleBoxFlat
        {
            BackgroundColor = Color.Gray.WithAlpha(0.35f), ContentMarginLeftOverride = DefaultGrabberSize,
            ContentMarginTopOverride = DefaultGrabberSize,
        };
        var vScrollBarGrabberHover = new StyleBoxFlat
        {
            BackgroundColor = new Color(140, 140, 140).WithAlpha(0.35f), ContentMarginLeftOverride = DefaultGrabberSize,
            ContentMarginTopOverride = DefaultGrabberSize,
        };

        var vScrollBarGrabberGrabbed = new StyleBoxFlat
        {
            BackgroundColor = new Color(160, 160, 160).WithAlpha(0.35f), ContentMarginLeftOverride = DefaultGrabberSize,
            ContentMarginTopOverride = DefaultGrabberSize,
        };

        var hScrollBarGrabberNormal = new StyleBoxFlat
        {
            BackgroundColor = Color.Gray.WithAlpha(0.35f), ContentMarginTopOverride = DefaultGrabberSize,
        };

        var hScrollBarGrabberHover = new StyleBoxFlat
        {
            BackgroundColor = new Color(140, 140, 140).WithAlpha(0.35f), ContentMarginTopOverride = DefaultGrabberSize,
        };

        var hScrollBarGrabberGrabbed = new StyleBoxFlat
        {
            BackgroundColor = new Color(160, 160, 160).WithAlpha(0.35f), ContentMarginTopOverride = DefaultGrabberSize,
        };

        // oasis
        var scrollbarAccented = new StyleBoxSDFBox()
        {
            BackgroundColor = sheet.PrimaryPalette.Base,
        };
        var scrollbarAccentedHovered = new StyleBoxSDFBox()
        {
            BackgroundColor = sheet.PrimaryPalette.HoveredElement,
        };
        var scrollbarAccentedGrabbed = new StyleBoxSDFBox()
        {
            BackgroundColor = sheet.PrimaryPalette.PressedElement,
        };
        //end oasis

        return
        [
            E<VScrollBar>().Prop(ScrollBar.StylePropertyGrabber, scrollbarAccented),
            E<VScrollBar>().PseudoHovered().Prop(ScrollBar.StylePropertyGrabber, scrollbarAccentedHovered),
            E<VScrollBar>().PseudoPressed().Prop(ScrollBar.StylePropertyGrabber, scrollbarAccentedGrabbed),
            E<HScrollBar>().Prop(ScrollBar.StylePropertyGrabber, scrollbarAccented),
            E<HScrollBar>().PseudoHovered().Prop(ScrollBar.StylePropertyGrabber, scrollbarAccentedHovered),
            E<HScrollBar>().PseudoPressed().Prop(ScrollBar.StylePropertyGrabber, scrollbarAccentedGrabbed),
        ];
    }
}
