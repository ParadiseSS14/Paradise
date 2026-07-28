using System.Numerics;
using Content.Client.Stylesheets.Colorspace;
using Content.Client.Stylesheets.SheetletConfigs;
using Content.Client.Stylesheets.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Stylesheets.Sheetlets;

[CommonSheetlet]
public sealed class LineEditSheetlet<T> : Sheetlet<T> where T : PalettedStylesheet, ILineEditConfig
{
    public override StyleRule[] GetRules(T sheet, object config)
    {
        ILineEditConfig lineEditCfg = sheet;

        var lineEditStylebox = new StyleBoxSDFBox()
        {
            BackgroundColor = sheet.PanelPalette.PanelSunken,
            ContentMarginTopOverride = 3,
            ContentMarginBottomOverride = 2,
            ContentMarginLeftOverride = 4,
            ContentMarginRightOverride = 4,
            CornerRadius = new Vector4(2f),
            BorderThickness = 0.75f,
            BorderColor = sheet.SecondaryPalette.Element,
        };
        return
        [
            E<LineEdit>()
                .Prop(LineEdit.StylePropertyStyleBox, lineEditStylebox)
                .Prop("font-color", sheet.SecondaryPalette.Element),
            // TODO: Hardcoded colors bad, kill.
            E<LineEdit>()
                .Class(LineEdit.StyleClassLineEditNotEditable)
                .Prop("font-color", new Color(192, 192, 192)),
            E<LineEdit>()
                .Pseudo(LineEdit.StylePseudoClassPlaceholder)
                .Prop("font-color", Color.Gray),
            E<TextEdit>()
                .Pseudo(TextEdit.StylePseudoClassPlaceholder)
                .Prop("font-color", Color.Gray),
        ];
    }
}
