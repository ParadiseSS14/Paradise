using System.Numerics;
using Content.Client.Stylesheets;
using Content.Client.UIControls;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.UserInterface.Controls.ColorPicker;

[CommonSheetlet]
public sealed class ColorPickerSheetlet : Sheetlet<PalettedStylesheet>
{
    public override StyleRule[] GetRules(PalettedStylesheet sheet, object config)
    {
        var sweepBox = new StyleBoxColorSweep(4);
        var hueSweep = new StyleBoxColorSweep(1);
        var satSweep = new StyleBoxColorSweep(2);
        var valSweep = new StyleBoxColorSweep(3);
        var colorPreviewBox = new StyleBoxSDFBox(Color.White)
        {
            CornerRadius = new Vector4(sheet.PanelPalette.PanelCornerRadius),
            BackgroundColor = Color.White,
            BorderThickness = 0f,
        };



        return
        [
            //PDA - Backgrounds
            E<ContainerForkedButton>()
                .Class("ColorPickerSweep")
                .Prop(ContainerForkedButton.StylePropertyStyleBox, sweepBox),
            E<ContainerForkedButton>()
                .Class("ColorPickerSweep")
                .PseudoNormal()
                .Modulate(Color.White),
            E<ContainerForkedButton>()
                .Class("ColorPickerSweep")
                .PseudoHovered()
                .Modulate(Color.White),
            E<ContainerForkedButton>()
                .Class("ColorPickerSweep")
                .PseudoPressed()
                .Modulate(Color.White),
            E<ContainerForkedButton>()
                .Class("ColorPickerHueSweep")
                .Prop(ContainerForkedButton.StylePropertyStyleBox, hueSweep),
            E<ContainerForkedButton>()
                .Class("ColorPickerHueSweep")
                .PseudoNormal()
                .Modulate(Color.White),
            E<ContainerForkedButton>()
                .Class("ColorPickerHueSweep")
                .PseudoHovered()
                .Modulate(Color.White),
            E<ContainerForkedButton>()
                .Class("ColorPickerHueSweep")
                .PseudoPressed()
                .Modulate(Color.White),
            E<PanelContainer>()
                .Class("RoundedColorPreview")
                .Prop(PanelContainer.StylePropertyPanel, colorPreviewBox),
        ];
    }
}
