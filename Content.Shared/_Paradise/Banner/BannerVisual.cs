using Robust.Shared.Serialization;

namespace Content.Shared._Paradise.Banner;

[Serializable, NetSerializable]
public enum BannerVisuals : byte
{
    State,
    Burning,
}

[Serializable, NetSerializable]
public enum BannerVisualsState : byte
{
    Unrolled,
    Rolled,
}
