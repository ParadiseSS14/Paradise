using Content.Client.Stylesheets.Colorspace;

// ReSharper disable MemberCanBePrivate.Global

namespace Content.Client.Stylesheets.Palette;

/// <remarks>
///     Don't be afraid to add a lot of fields here! This class is made for readability.
/// </remarks>
public record SemanticPalette(
    float LightnessShift,
    float ChromaShift,

    Color Notice,
    Color Info,
    Color Danger
)
{
    /// <summary>
    /// Helper method for generating a ColorPalette from a specified base hex string, with the
    /// option to override specific parts of the palette
    /// </summary>
    public static SemanticPalette FromHexBases(
        string hexNotice = "#000000",
        string hexInfo = "#000000",
        string hexDanger = "#F0F0F0F",
        float lightnessShift = 0.06f,
        float chromaShift = 0.00f
    )
    {
        var colNotice = Color.FromHex(hexNotice);
        var colInfo = Color.FromHex(hexInfo);
        var colDanger = Color.FromHex(hexDanger);

        // var warning = Shift(colWarning, lightnessShift, chromaShift, 0);
        // var error = Shift(colError, lightnessShift, chromaShift, 0);

        return new SemanticPalette(
            LightnessShift: lightnessShift,
            ChromaShift: chromaShift,

            Notice: colNotice,
            Info: colInfo,
            Danger: colDanger
        );
    }

    public static Color Shift(Color from, float lightnessShift, float chromaShift, float factor, float alphaFactor = 1f)
    {
        return from.NudgeLightness(lightnessShift * factor).NudgeChroma(chromaShift * factor).WithAlpha(from.A * alphaFactor);
    }
}
