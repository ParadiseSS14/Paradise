using Content.Client.Stylesheets.Palette;

namespace Content.Client.Stylesheets.Stylesheets;

public partial class SystemStylesheet
{
    public override ColorPalette PrimaryPalette => ColorPalette.FromHexBase("#3E6189");
    public override ColorPalette SecondaryPalette => ColorPalette.FromHexBase("#80bfff");
    public override ColorPalette PositivePalette => Palettes.Green;
    public override ColorPalette NegativePalette => Palettes.Red;
    public override ColorPalette HighlightPalette => ColorPalette.FromHexBase("#FFFFFF");
    // public override ColorPalette ButtonPalette => PrimaryPalette;

    public override ModalPalette PanelPalette => ModalPalette.FromHexBases("#202020FF", "#00000080", "#ECF6F9", panelBorderThickness:0.75f, hexPanelBorder:"#3D3D3D3A", panelCornerRadius:2f, hexPanelSunken:"#0D0D0DFF");
    public override SemanticPalette SemanticPalette => SemanticPalette.FromHexBases(hexNotice: "#cea257", hexInfo: "#2a79c8", hexDanger:"#d92626");
}
