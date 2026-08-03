using Content.Server.Investigation;

namespace Content.Server.Chat.Systems;

public sealed partial class ChatSystem
{
    [Dependency] private readonly IInvestigationRecorder _investigation = default!;

    private void RecordInvestigationChat(EntityUid source, string channel, string message, string? speakerName)
    {
        if (!_investigation.IsRecording)
            return;

        _investigation.OnChat(source, channel, message, speakerName);
    }
}
