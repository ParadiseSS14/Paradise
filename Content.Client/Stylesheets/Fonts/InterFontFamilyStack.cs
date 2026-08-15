using Content.Client.Resources;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;

namespace Content.Client.Stylesheets.Fonts;

/// <summary>
///     This class should have a base type. The whole font system is currently kind of bad and completely temporary.
///     This class is just here because it does sort of work.
///     TODO: fix (once engine support is added for font properties?)
/// </summary>
/// <param name="resCache"></param>
/// <param name="variant"></param>
[PublicAPI]
public sealed class InterFontFamilyStack(IResourceCache resCache, string variant = "") : FontFamilyStack(resCache, variant)
{
    /// <summary>
    ///     The primary font path, with string substitution markers.
    /// </summary>
    /// <remarks>
    ///     If using the default GetFontPaths function, the substitutions are as follows:
    ///     0 is the font kind.
    ///     1 is the font kind with BoldItalic replaced with Bold when it occurs.
    /// </remarks>
    public override string _fontPrimary => $"/Fonts/Inter/Inter-{{0}}.ttf";

}
