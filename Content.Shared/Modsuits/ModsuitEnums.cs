using Robust.Shared.Serialization;

namespace Content.Shared.Modsuits;
/// <summary>
/// Enum representing the different parts of a modsuit. Each part can have its own functionality and can be deployed or retracted independently.
/// </summary>
public enum ModsuitPartType
{
    Helmet,
    Chest,
    Gloves,
    Boots
}

[Serializable, NetSerializable]
public enum ModsuitUiKey
{
    Radial
}

[Serializable, NetSerializable]
public enum ModsuitVisuals : byte
{
    IconLayer
}
