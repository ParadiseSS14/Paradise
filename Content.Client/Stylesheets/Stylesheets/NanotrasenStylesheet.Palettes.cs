using Content.Client.Stylesheets.Palette;

namespace Content.Client.Stylesheets.Stylesheets;

public sealed partial class NanotrasenStylesheet
{
    public override ColorPalette PrimaryPalette => Palettes.Navy;
    public override ColorPalette SecondaryPalette => Palettes.Slate;
    public override ColorPalette PositivePalette => Palettes.Green;
    public override ColorPalette NegativePalette => Palettes.Red;
    public override ColorPalette HighlightPalette => Palettes.Gold;
    public override ModalPalette PanelPalette => ModalPalette.FromHexBases("#ff0000", "#ff0000", "#ff0000", "#ff0000");
    public override SemanticPalette SemanticPalette => SemanticPalette.FromHexBases(hexNotice: "#cea257", hexInfo: "#2a79c8", hexDanger:"#d92626");
}
