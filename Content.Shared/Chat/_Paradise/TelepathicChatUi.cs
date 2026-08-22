using Robust.Shared.Serialization;

namespace Content.Shared.Chat._Paradise;
[Serializable, NetSerializable]
public sealed class TelepathicTargetSelectedMsg : BoundUserInterfaceMessage
{
    public readonly NetEntity? Target;

    public TelepathicTargetSelectedMsg(NetEntity target)
    {
        Target = target;
    }
}

[Serializable, NetSerializable]
public sealed class TelepathicTextEnteredMsg : BoundUserInterfaceMessage
{
    public readonly string Message;

    public TelepathicTextEnteredMsg(string message)
    {
        Message = message;
    }
}

[Serializable, NetSerializable]
public sealed class TelepathicTargetUIState : BoundUserInterfaceState
{
    public readonly List<(NetEntity Uid, string Name)> TargetsList;

    public TelepathicTargetUIState(List<(NetEntity Uid, string Name)> targetsList)
    {
        TargetsList = targetsList;
    }
}

[Serializable, NetSerializable]
public enum TelepathicChatUiKey : byte
{
    Send,
    Offer,
    Compose
}
