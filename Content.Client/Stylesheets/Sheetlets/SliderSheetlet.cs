using System.Numerics;
using Content.Client.Stylesheets.SheetletConfigs;
using Content.Client.Stylesheets.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Stylesheets.Sheetlets;

[CommonSheetlet]
public sealed class SliderSheetlet<T> : Sheetlet<T> where T: PalettedStylesheet, ISliderConfig
{
    public override StyleRule[] GetRules(T sheet, object config)
    {
        ISliderConfig sliderCfg = sheet;

        var cornerRounding = 4f;

        var sliderFillBox = new StyleBoxSDFBox
        {
            CornerRadius = new Vector4(cornerRounding),
            BackgroundColor = sheet.PanelPalette.Text,
        };

        var sliderBackBox = new StyleBoxSDFBox()
        {
            CornerRadius = new Vector4(cornerRounding),
            BackgroundColor = sheet.PanelPalette.PanelSunken,
        };

        var sliderForeBox = new StyleBoxSDFBox
        {
            CornerRadius = new Vector4(cornerRounding),
            BackgroundColor = Color.Transparent,
            BorderColor = sheet.PanelPalette.PanelBorderColor,
            BorderThickness = sheet.PanelPalette.PanelBorderThickness,
        };

        var sliderGrabBox = new StyleBoxTexture
        {
            Texture = sheet.GetTextureOr(sliderCfg.SliderGrabber, NanotrasenStylesheet.TextureRoot),
        };
        
        sliderGrabBox.SetPatchMargin(StyleBox.Margin.All, 12);

        // var sliderFillGreen = new StyleBoxTexture(sliderFillBox) { Modulate = Color.LimeGreen };
        // var sliderFillRed = new StyleBoxTexture(sliderFillBox) { Modulate = Color.Red };
        // var sliderFillBlue = new StyleBoxTexture(sliderFillBox) { Modulate = Color.Blue };
        // var sliderFillWhite = new StyleBoxTexture(sliderFillBox) { Modulate = Color.White };

        return new StyleRule[]
        {
            E<Slider>()
                .Prop(Slider.StylePropertyBackground, sliderBackBox)
                .Prop(Slider.StylePropertyForeground, sliderForeBox)
                .Prop(Slider.StylePropertyGrabber, sliderGrabBox)
                .Prop(Slider.StylePropertyFill, sliderFillBox),
            // these styles seem to be unused now
            // E<ColorableSlider>()
            //     .Prop(ColorableSlider.StylePropertyFillWhite, sliderFillWhite)
            //     .Prop(ColorableSlider.StylePropertyBackgroundWhite, sliderFillWhite),
            //
            // E<Slider>().Class(StyleClass.StyleClassSliderRed)
            //     .Prop(Slider.StylePropertyFill, sliderFillRed),
            // E<Slider>().Class(StyleClass.StyleClassSliderBlue)
            //     .Prop(Slider.StylePropertyFill, sliderFillBlue),
            // E<Slider>().Class(StyleClass.StyleClassSliderGreen)
            //     .Prop(Slider.StylePropertyFill, sliderFillGreen),
            // E<Slider>().Class(StyleClass.StyleClassSliderWhite)
            //     .Prop(Slider.StylePropertyFill, sliderFillWhite),
        };
    }
}
