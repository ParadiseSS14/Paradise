using Robust.Shared.GameStates;

namespace Content.Shared._Paradise.CustomColorableLayer;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]

public sealed partial class CustomColorableLayerComponent : Component
{
    [DataField]
    public ColorableVisualLayer AttachedColoredSpriteLayer = ColorableVisualLayer.CustomColor;

    [DataField]
    [AutoNetworkedField]
    public Color ColoredLayerColor = Color.White;

    [DataField]
    public TimeSpan TimeToPaint = TimeSpan.FromSeconds(20);
}

public enum ColorableVisualLayer : byte
{
    CustomBase = 0,
    CustomColor = 1
}
