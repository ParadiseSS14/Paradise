using Robust.Shared.Serialization;

namespace Content.Shared._Paradise.Weapons.Ranged.Events;

/// <summary>
/// Raised on the client to request a change of aiming status.
/// </summary>
[Serializable, NetSerializable]
public sealed class AimStatusChangeAttemptEvent : EntityEventArgs
{
    public NetEntity Gun;

    public NetEntity User;

    public bool Aim;
}
