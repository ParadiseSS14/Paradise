using System.Numerics;
using Content.Client.Stylesheets.SheetletConfigs;
using Content.Client.Stylesheets.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Stylesheets.Sheetlets;

[CommonSheetlet]
public sealed class TabContainerSheetlet<T> : Sheetlet<T> where T : PalettedStylesheet, ITabContainerConfig
{
    public override StyleRule[] GetRules(T sheet, object config)
    {
        ITabContainerConfig tabCfg = sheet;

        var roundedconfig = new Vector4(sheet.PanelPalette.PanelCornerRadius * 3,
            0,
            sheet.PanelPalette.PanelCornerRadius * 3,
            0);

        var tabContainerPanel = new StyleBoxSDFBox(sheet.PanelPalette.PanelPrimary)
        {
            ContentMarginTopOverride = 8,
            ContentMarginBottomOverride = 8,
            ContentMarginLeftOverride = 8,
            ContentMarginRightOverride = 8,
            CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius),
        };

        var tabContainerBoxActive = new StyleBoxSDFBox(sheet.PanelPalette.PanelPrimary)
        {
            CornerRadius = roundedconfig,

        };
        tabContainerBoxActive.SetContentMarginOverride(StyleBox.Margin.All, 8);
        var tabContainerBoxInactive = new StyleBoxSDFBox(sheet.PanelPalette.PanelPrimary)
        {
            CornerRadius =
                roundedconfig,
        };
        tabContainerBoxInactive.SetContentMarginOverride(StyleBox.Margin.All, 8);

        return
        [
            E<TabContainer>()
                .Prop(TabContainer.StylePropertyPanelStyleBox, tabContainerPanel)
                .Prop(TabContainer.StylePropertyTabStyleBox, tabContainerBoxActive)
                .Prop(TabContainer.StylePropertyTabStyleBoxInactive, tabContainerBoxInactive),
            E<TabContainer>().Class("TGUITabContainer").Prop(TabContainer.stylePropertyTabFontColor, sheet.PanelPalette.Text),
        ];
    }
}
