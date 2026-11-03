using Robust.Client.Graphics;

namespace Content.Client.Stylesheets;

/// <summary>
/// Gives each draw in a frame its own shader instance, then reuses them on later frames.
/// </summary>
/// <remarks>
/// Clyde reads shader parameters when it flushes a batch, so draws can't share an instance.
/// Making a new one per draw works, but causes GC hitches.
/// </remarks>
public sealed class FrameShaderInstancePool<TParams> where TParams : struct, IEquatable<TParams>
{
    private readonly ShaderPrototype _prototype;
    private readonly List<(ShaderInstance Shader, TParams Params)> _slots = new();
    private uint _frame;
    private int _next;

    public FrameShaderInstancePool(ShaderPrototype prototype)
    {
        _prototype = prototype;
    }

    /// <summary>
    /// Gets the instance for the next draw in <paramref name="frame"/>.
    /// </summary>
    /// <returns>True if the instance's parameters need to be set.</returns>
    public bool Next(uint frame, in TParams parameters, out ShaderInstance shader)
    {
        if (frame != _frame)
        {
            _frame = frame;
            _next = 0;
        }

        if (_next == _slots.Count)
        {
            shader = _prototype.InstanceUnique();
            _slots.Add((shader, parameters));
            _next++;
            return true;
        }

        var slot = _slots[_next];
        shader = slot.Shader;
        var changed = !slot.Params.Equals(parameters);
        if (changed)
            _slots[_next] = (shader, parameters);

        _next++;
        return changed;
    }
}
