using Content.Server.Chat._Paradise;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Chat.Commands._Paradise;

[AnyCommand]
internal sealed partial class TelepathicChatReplyCommand : LocalizedEntityCommands
{
    public const string CommandName = "telepathic_chat_reply";

    [Dependency] private EntityManager _entman = default!;
    [Dependency] private TelepathicChatSystem _telepathic = default!;

    public override string Command => CommandName;

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2 || shell.Player?.AttachedEntity is not { } targetEnt)
            return;

        var telepath = args[0];
        if (!NetEntity.TryParse(telepath, out var telepathNet))
            return;
        if (!Guid.TryParse(args[1], out var token))
            return;
        var telepathEnt = _entman.GetEntity(telepathNet);

        // Using a simple token to prevent command spam
        if (!_telepathic.TryToken(telepathEnt, token))
        {
            return;
        }

        _telepathic.OpenComposeFor(telepathEnt, targetEnt, telepathNet);
    }
}
