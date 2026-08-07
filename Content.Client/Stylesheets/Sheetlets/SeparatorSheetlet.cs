using System.Numerics;
using Content.Client.Stylesheets.SheetletConfigs;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Stylesheets.Sheetlets;

[CommonSheetlet]
public sealed class SeperatorSheetlet<T> : Sheetlet<T> where T : PalettedStylesheet, IButtonConfig
{
    public override StyleRule[] GetRules(T sheet, object config)
    {
        IButtonConfig buttonCfg = sheet;

        //Oasis

        var seperatorAccent = new StyleBoxSDFBox()
        {
            BackgroundColor = sheet.PrimaryPalette.Base,
            CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius),
        };

        //Oasis End

        return
        [
            E<PanelContainer>()
                .Class(StyleClass.SeperatorAccented)
                .Panel(seperatorAccent),
        ];
    }
}
