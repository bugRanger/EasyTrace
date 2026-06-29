namespace EasyTrace.Export.Batch.Buffer;


internal class CircularBufferSlot<T>
    where T : class, ICopiable<T>, new()
{
    private bool _isEmpty = true;

    /// <summary>
    /// Checking that the slot has been cleared and is ready for reuse.
    /// </summary>
    /// <returns>
    /// Returns <c>true</c> if the slot was successfully cleared for the buffer; otherwise, <c>false</c>.
    /// </returns>
    public bool IsEmpty()
    {
        return _isEmpty;
    }

    /// <summary>
    /// Get item from the buffer.
    /// </summary>
    public T Item { get; } = new();

    /// <summary>
    /// Copy from <see cref="value"/>
    /// </summary>
    /// <param name="value">Source for copying data</param>
    public void CopyFrom<TIn>(in TIn value)
        where TIn : ICopiable<T>, allows ref struct
    {
        value.CopyTo(Item);
        Volatile.Write(ref _isEmpty, false);
    }

    /// <summary>
    /// Clear the buffer for reuse.
    /// </summary>
    public void Clear()
    {
        Volatile.Write(ref _isEmpty, true);
    }
}