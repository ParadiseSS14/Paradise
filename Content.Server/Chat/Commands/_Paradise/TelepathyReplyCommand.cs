using Content.Server.Chat._Paradise;
using Content.Shared.Chat._Paradise;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Chat.Commands._Paradise;

[AnyCommand]
internal sealed partial class TelepathyReplyCommand : LocalizedEntityCommands
{
    public const string CommandName = "_telepathy_reply";

    [Dependency] private EntityManager _entMan = default!;
    [Dependency] private TelepathyAbilitySystem _telepathy = default!;

    public override string Command => CommandName;

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1 || shell.Player?.AttachedEntity is not { } receiver)
            return;

        if (!NetEntity.TryParse(args[0], out var senderNet))
            return;

        var sender = _entMan.GetEntity(senderNet);

        if (!_entMan.HasComponent<TelepathyAbilityComponent>(sender))
        {
            _telepathy.LogInvalidReply(receiver, args[0]);
            return;
        }

        // Using a simple token to prevent command spam
        if (!_telepathy.CheckOfferValid(sender, receiver))
        {
            return;
        }

        _telepathy.OpenComposeFor(receiver, sender);
    }
}
