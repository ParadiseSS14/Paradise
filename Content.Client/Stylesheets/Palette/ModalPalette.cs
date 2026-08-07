using Content.Client.Stylesheets.Colorspace;

// ReSharper disable MemberCanBePrivate.Global

namespace Content.Client.Stylesheets.Palette;

/// <remarks>
///     Don't be afraid to add a lot of fields here! This class is made for readability.
/// </remarks>
public record ModalPalette(
    Color Base,

    float LightnessShift,
    float ChromaShift,

    Color Background,
    Color PanelPrimary,
    Color PanelSecondary,
    Color PanelTertiary,
    Color PanelSunken,

    Color PanelBorderColor,
    float PanelBorderThickness,
    float PanelCornerRadius,


    Color Text,
    Color TextDark
)
{
    /// <summary>
    /// Helper method for generating a ColorPalette from a specified base hex string, with the
    /// option to override specific parts of the palette
    /// </summary>
    public static ModalPalette FromHexBases(
        string hexBackground = "#000000",
        string hexPanel = "#FF0000",
        string hexText = "#FFFFFF",
        string hexPanelBorder = "#FF00FF",
        string hexPanelSunken = "#F0F0F0",
        float lightnessShift = 0.06f,
        float chromaShift = 0.00f,
        float panelBorderThickness = 0.00f,
        float panelCornerRadius = 0.00f,
        Color? element = null,
        Color? background = null
    )
    {
        var @base = Color.FromHex(hexBackground);
        var panel = Color.FromHex(hexPanel);
        var border = Color.FromHex(hexPanelBorder);
        var panelSunken = Color.FromHex(hexPanelSunken);
        var text = Color.FromHex(hexText);

        background ??= Shift(@base, lightnessShift, chromaShift, 0); //                     Shift(@base, -3)
        var panelPrimary = Shift(panel, lightnessShift, chromaShift, 0); //    Shift(@base, -2)
        var panelSecondary = Shift(panel, lightnessShift, chromaShift, -6, 0.75f); //    Shift(@base, -4)
        var panelTertiary = Shift(panel, lightnessShift, chromaShift, -12, 0.75f); //    Shift(@base, -4)
        var panelBorderColor = Shift(border, lightnessShift, chromaShift, 0);

        var textDark = Shift(text, lightnessShift, chromaShift, -2); //                Shift(@base, -1)

        return new ModalPalette(
            Base: @base,

            LightnessShift: lightnessShift,
            ChromaShift: chromaShift,

            Background: background.Value,
            PanelPrimary: panelPrimary,
            PanelSecondary: panelSecondary,
            PanelTertiary: panelTertiary,
            PanelBorderColor: panelBorderColor,
            PanelBorderThickness: panelBorderThickness,
            PanelCornerRadius: panelCornerRadius,
            PanelSunken: panelSunken,

            Text: text,
            TextDark: textDark
        );
    }

    public static Color Shift(Color from, float lightnessShift, float chromaShift, float factor, float alphaFactor = 1f)
    {
        return from.NudgeLightness(lightnessShift * factor).NudgeChroma(chromaShift * factor).WithAlpha(from.A * alphaFactor);
    }
}
