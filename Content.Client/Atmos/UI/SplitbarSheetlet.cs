using System.Numerics;
using Content.Client.Stylesheets;
using Content.Client.Stylesheets.Colorspace;
using Content.Client.Stylesheets.Palette;
using Content.Client.Stylesheets.SheetletConfigs;
using Content.Client.Stylesheets.Sheetlets;
using Content.Client.UserInterface.Controls;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Atmos.UI;

[CommonSheetlet]
public sealed class SplitbarSheetlet<T> : Sheetlet<T> where T : PalettedStylesheet, IButtonConfig
{
    public override StyleRule[] GetRules(T sheet, object config)
    {
        IButtonConfig buttonCfg = sheet;

        //Paradise


        var roundedButton = new StyleBoxSDFBox()
        {
            CornerRadius = new Vector4(2f),
            BackgroundColor = Color.White,
            ContentMarginTopOverride = 2,
            ContentMarginBottomOverride = 2,
            ContentMarginLeftOverride = 4,
            ContentMarginRightOverride = 4,
        };

        return
        [
            E<PanelContainer>()
                .Class(SplitBar.SplitBarEntry)
                .Panel(roundedButton),
        ];
    }
}
