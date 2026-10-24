namespace Content.Client.DoOnce;

/// <summary>
/// A system for clients to handle only doing things once per session easily. An example would be, for example, a UI opening animation.
/// </summary>
public sealed class DoOnceSystem
{
    public HashSet<string> CompletedKeys = new();



    /// <summary>
    /// If it hasn't already happened, allow an if statement to continue.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="key">Any name to track what is what.</param>
    /// <returns>A boolean saying if it has not occured yet.</returns>
    public bool TryDoOnce(string key)
    {
        return CompletedKeys.Add(key);
    }

    /// <summary>
    /// If it hasn't already happened, allow an if statement to continue.
    /// </summary>
    /// <param name="uid"></param>
    /// <typeparam name="T">The name of the class, so this only happens once per class.</typeparam>
    /// <returns>A boolean saying if it has not occured yet.</returns>
    public bool TryDoOnce<T>() where T : class
    {
        return CompletedKeys.Add(typeof(T).FullName!);
    }

    /// <summary>
    /// Taking a key, opens the gate for another DoOnce to occur. Common places to put this is on RemovedFromTree for UI.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="key">Any name to track what is what.</param>
    public void ClearDoOnce(string key)
    {
        CompletedKeys.Remove(key);
    }

    /// <summary>
    /// Taking a key, opens the gate for another DoOnce to occur. Common places to put this is on RemovedFromTree for UI.
    /// </summary>
    /// <param name="uid"></param>
    /// <typeparam name="T">The name of the class, so this only happens once per class.</typeparam>
    public void ClearDoOnce<T>() where T : class
    {
        CompletedKeys.Remove(typeof(T).FullName!);
    }
}
