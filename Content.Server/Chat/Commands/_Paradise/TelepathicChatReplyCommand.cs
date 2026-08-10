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
        if (!NetEntity.TryParse(args[0], out var telepathNet))
        {
            return;
        }
        if (!Guid.TryParse(args[1], out var token))
        {
            return;
        }

        var telepath = _entMan.GetEntity(telepathNet);

        if (!_entMan.HasComponent<TelepathicChatComponent>(telepath))
        {
            return;
        }

        if (args.Length != 2 || shell.Player?.AttachedEntity is not { } target)
        {
            return;
        }

        // Using a simple token to prevent command spam
        if (!_telepathic.TryToken(telepath, target, token))
        {
            shell.WriteLine($"{target}: {Loc.GetString("telepathic-chat-token-invalid")}");
            return;
        }

        _telepathic.OpenComposeFor(target, telepathNet, telepath);
    }
}
