using System.Numerics;
using Content.Client.Stylesheets.Colorspace;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Stylesheets.Sheetlets;

[CommonSheetlet]
public sealed class ItemListSheetlet : Sheetlet<PalettedStylesheet>
{
    private static StyleBoxFlat Box(Color c)
    {
        return new StyleBoxFlat(c)
            // TODO: dont hardcode these maybe
            {
                ContentMarginLeftOverride = 4,
                ContentMarginTopOverride = 2,
                ContentMarginRightOverride = 4,
                ContentMarginBottomOverride = 2,
            };
    }

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
        var boxDisabled = new StyleBoxSDFBox { BackgroundColor = sheet.PanelPalette.PanelSunken };

        // var boxBackground = new StyleBoxSDFBox { BackgroundColor = Color.Transparent };
        // var boxItemBackground = new StyleBoxSDFBox
        // {
        //     BackgroundColor = sheet.PanelPalette.PanelSunken.NudgeLightness(0.08f).WithAlpha(0.3f), ContentMarginTopOverride = 4,
        //     ContentMarginBottomOverride = 4, ContentMarginRightOverride = 4, ContentMarginLeftOverride = 4,
        //     CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius * 2),
        //     BorderThickness = 1f,
        //     BorderColor = sheet.PanelPalette.PanelBorderColor,
        // };
        // var boxSelected = new StyleBoxSDFBox
        // {
        //     BackgroundColor = sheet.PanelPalette.PanelSunken.NudgeLightness(0.12f).WithAlpha(0.4f), ContentMarginTopOverride = 4,
        //     ContentMarginBottomOverride = 4, ContentMarginRightOverride = 4, ContentMarginLeftOverride = 4,
        //     CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius * 2),
        //     BorderThickness = 1f,
        //     BorderColor = sheet.PanelPalette.PanelBorderColor,
        // };
        // var boxDisabled = new StyleBoxSDFBox { BackgroundColor = sheet.PanelPalette.PanelSunken };

        return
        [
            E<ItemList>()
                .Prop(ItemList.StylePropertyBackground, boxBackground)
                .Prop(ItemList.StylePropertyItemBackground, boxItemBackground)
                .Prop(ItemList.StylePropertyDisabledItemBackground, boxDisabled)
                .Prop(ItemList.StylePropertySelectedItemBackground, boxSelected),

            // these styles seem to be unused now
            // E<ItemList>().Class("transparentItemList")
            //     .Prop(ItemList.StylePropertyBackground, boxTransparent)
            //     .Prop(ItemList.StylePropertyItemBackground, boxTransparent)
            //     .Prop(ItemList.StylePropertyDisabledItemBackground, boxDisabled)
            //     .Prop(ItemList.StylePropertySelectedItemBackground, boxItemBackground),
            //
            // E<ItemList>().Class("transparentBackgroundItemList")
            //     .Prop(ItemList.StylePropertyBackground, boxTransparent)
            //     .Prop(ItemList.StylePropertyItemBackground, boxBackground)
            //     .Prop(ItemList.StylePropertyDisabledItemBackground, boxItemBackground)
            //     .Prop(ItemList.StylePropertySelectedItemBackground, boxSelected),
        ];
    }
}
