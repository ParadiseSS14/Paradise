using Content.Client.UserInterface.Tweens.Delegates;

namespace Content.Client.UserInterface.Tweens.Extensions;

public static class ValidationExtensions
{
    public static readonly ValidationDelegates.Validation AlwaysValid = () => true;
}
