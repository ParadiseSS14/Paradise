using Robust.Shared.Serialization;

namespace Content.Shared.Chat._Paradise;

[Serializable, NetSerializable]
public sealed class TelepathicTargetsListState : BoundUserInterfaceState
{
    public List<(NetEntity Uid, string Name)> Targets;

    public TelepathicTargetsListState(List<(NetEntity Uid, string Name)> targets)
    {
        Targets = targets;
    }

}

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
    public readonly String Message;

    public TelepathicTextEnteredMsg(String message)
    {
        Message = message;
    }
}

[Serializable, NetSerializable]
public enum TelepathicChatUiKey : byte
{
    Send,
    Receive,
    Compose
}
