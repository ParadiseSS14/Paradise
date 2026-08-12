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
public enum TelepathicChatUiKey : byte
{
    Send,
    Receive,
    Compose
}
