using Robust.Shared.Serialization;

namespace Content.Shared.Chat._Paradise;
[Serializable, NetSerializable]
public sealed class TelepathyTargetSelectedMsg : BoundUserInterfaceMessage
{
    public readonly NetEntity? Target;
    public readonly string? Name;

    public TelepathyTargetSelectedMsg(NetEntity target, string name)
    {
        Target = target;
        Name = name;
    }
}

[Serializable, NetSerializable]
public sealed class TelepathyTextEnteredMsg : BoundUserInterfaceMessage
{
    public readonly string Message;

    public TelepathyTextEnteredMsg(string message)
    {
        Message = message;
    }
}

[Serializable, NetSerializable]
public sealed class TelepathyTargetUIState : BoundUserInterfaceState
{
    public readonly List<(NetEntity Uid, string Name)> TargetsList;

    public TelepathyTargetUIState(List<(NetEntity Uid, string Name)> targetsList)
    {
        TargetsList = targetsList;
    }
}

[Serializable, NetSerializable]
public enum TelepathyAbilityUiKey : byte
{
    Send,
    Offer,
    Compose
}
