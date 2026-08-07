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
