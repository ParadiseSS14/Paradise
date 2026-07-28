using Content.Client.Stylesheets.Palette;

namespace Content.Client.Stylesheets.Stylesheets;

public partial class SyndicateParadiseStylesheet
{
    public override ColorPalette PrimaryPalette => ColorPalette.FromHexBase("#3b783b");
    public override ColorPalette SecondaryPalette => ColorPalette.FromHexBase("#80FF80BF");
    public override ColorPalette PositivePalette => Palettes.Green;
    public override ColorPalette NegativePalette => Palettes.Red;
    public override ColorPalette HighlightPalette => ColorPalette.FromHexBase("#3b783b");
    // public override ColorPalette ButtonPalette => PrimaryPalette;

    public override ModalPalette PanelPalette => ModalPalette.FromHexBases("#600303", "#00000080", "#ECF6F9", panelBorderThickness:0.75f, hexPanelBorder:"#3D3D3D3A", panelCornerRadius:2f, hexPanelSunken:"#0D0D0DFF");
    public override SemanticPalette SemanticPalette => SemanticPalette.FromHexBases(hexNotice: "#cea257", hexInfo: "#2a79c8", hexDanger:"#d92626");
}
