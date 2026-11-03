using System.Numerics;
using Content.Shared.Colorspace;
using Content.Client.UserInterface.Controls.FancyTree;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Stylesheets.Sheetlets;

[CommonSheetlet]
public sealed class FancyTreeSheetlet : Sheetlet<PalettedStylesheet>
{
    public override StyleRule[] GetRules(PalettedStylesheet sheet, object config)
    {
        return
        [
            E<ContainerButton>()
                .Identifier(TreeItem.StyleIdentifierTreeButton)
                .Class(TreeItem.StyleClassEvenRow)
                .Prop(ContainerButton.StylePropertyStyleBox,
                    new StyleBoxSDFBox()
                    {
                        BorderThickness = sheet.PanelPalette.PanelBorderThickness * 0,
                        CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius),
                        BackgroundColor = sheet.PanelPalette.PanelSecondary,
                    }),
            E<ContainerButton>()
                .Identifier(TreeItem.StyleIdentifierTreeButton)
                .Class(TreeItem.StyleClassOddRow)
                .Prop(ContainerButton.StylePropertyStyleBox,
                    new StyleBoxSDFBox()
                    {
                        BorderThickness = sheet.PanelPalette.PanelBorderThickness * 0,
                        BackgroundColor = sheet.PanelPalette.PanelPrimary,
                    }),

            E<ContainerButton>()
                .Identifier(TreeItem.StyleIdentifierTreeButton)
                .Class(TreeItem.StyleClassSelected)
                .Prop(ContainerButton.StylePropertyStyleBox,
                    new StyleBoxSDFBox()
                    {
                        BackgroundColor = Color.InterpolateBetween(sheet.PanelPalette.PanelPrimary, sheet.SecondaryPalette.Element, 0.25f),
                        BorderThickness = 0.75f,
                        BorderColor = sheet.SecondaryPalette.Element,
                        CornerRadius = new Vector4(2f),
                    }),

            E<ContainerButton>()
                .Identifier(TreeItem.StyleIdentifierTreeButton)
                .Pseudo(ContainerButton.StylePseudoClassHover)
                .Prop(ContainerButton.StylePropertyStyleBox,
                    new StyleBoxSDFBox()
                    {
                        BackgroundColor = sheet.PanelPalette.PanelPrimary,
                        BorderThickness = 0.75f,
                        BorderColor = sheet.SecondaryPalette.Element.NudgeLightness(-0.1f),
                        CornerRadius = new Vector4(2f),
                    }),
        ];
    }
}
