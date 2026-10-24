using Content.Server.Chat._Paradise;
using Content.Shared.Chat._Paradise;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Chat.Commands._Paradise;

[AnyCommand]
internal sealed partial class TelepathicChatReplyCommand : LocalizedEntityCommands
{
    public const string CommandName = "telepathic_chat_reply";

    [Dependency] private EntityManager _entMan = default!;
    [Dependency] private TelepathicChatSystem _telepathic = default!;

    public override string Command => CommandName;

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (!NetEntity.TryParse(args[0], out var senderNet))
            return;

        var sender = _entMan.GetEntity(senderNet);

        if (!_entMan.HasComponent<TelepathicChatComponent>(sender))
            return;

        if (args.Length != 1 || shell.Player?.AttachedEntity is not { } receiver)
            return;

        // Using a simple token to prevent command spam
        if (!_telepathic.CheckOfferValid(sender, receiver))
        {
            shell.WriteLine($"{receiver}: {Loc.GetString("telepathic-chat-token-invalid")}");
            return;
        }

        _telepathic.OpenComposeFor(receiver, senderNet, sender);
    }
}
