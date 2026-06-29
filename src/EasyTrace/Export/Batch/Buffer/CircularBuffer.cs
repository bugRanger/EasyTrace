using System.Runtime.CompilerServices;

namespace EasyTrace.Export.Batch.Buffer;

/// <summary>
/// Lock-free implementation of single-consumer multi-producer circular buffer.
/// </summary>
/// <remarks>
/// The buffer does not store a reference to the object, but only rewrites its state into a free slot.
/// </remarks>
public sealed class CircularBuffer<T>(uint capacity)
    where T : class, ICopiable<T>, new()
{
    private readonly CircularBufferSlot<T>[] _slots = new CircularBufferSlot<T>[capacity];
    private ulong _head;
    private ulong _tail;

    public ulong Capacity { get; } = capacity;

    /// <summary>
    /// Gets the number of items contained in the <see cref="CircularBuffer{T}"/>.
    /// </summary>
    public ulong Count
    {
        get
        {
            var tail = Volatile.Read(ref _tail);
            var head = Volatile.Read(ref _head);
            return head - tail;
        }
    }

    /// <summary>
    /// Try push the specified item to the buffer.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <param name="maxSpinCount">The maximum allowed spin count, when set to a negative zero, will spin indefinitely.</param>
    /// <returns>
    /// Returns <c>true</c> if the item was added to the buffer successfully;
    /// <c>false</c> if the buffer is full.
    /// </returns>
    public bool Push<TIn>(in TIn value, uint maxSpinCount = 0)
        where TIn : ICopiable<T>, allows ref struct
    {
        var spinCounter = maxSpinCount;

        while (true)
        {
            var tail = Volatile.Read(ref _tail);
            var head = Volatile.Read(ref _head);

            if (head - tail >= Capacity)
            {
                // buffer is full.
                return false;
            }

            if (Interlocked.CompareExchange(ref _head, head + 1, head) != head)
            {
                if (spinCounter-- == 0)
                {
                    return false;
                }

                continue;
            }

            while (true)
            {
                var slot = _slots[GetIndex(head)];
                if (!slot.IsEmpty())
                {
                    continue;
                }

                slot.CopyFrom(value);
                break;
            }

            return true;
        }
    }

    /// <summary>
    /// Reads an items from the <see cref="CircularBuffer{T}"/>.
    /// </summary>
    /// <remarks>
    /// This function is not reentrant-safe, only one reader is allowed at any given time.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<T> Next(ulong count)
    {
        while (count > 0)
        {
            if (Next(out var slot))
            {
                count--;
                yield return slot.Item;
                slot.Clear();
            }
            else
            {
                yield break;
            }
        }
    }

    /// <summary>
    /// Reads an item slot from the <see cref="CircularBuffer{T}"/>.
    /// </summary>
    /// <remarks>
    /// This function is not reentrant-safe, only one reader is allowed at any given time.
    /// </remarks>
    /// <returns>
    /// Returns <c>true</c> if the item was read from the buffer successfully; <c>false</c> if the buffer is empty.
    /// </returns>
    private bool Next(out CircularBufferSlot<T> slot)
    {
        while (true)
        {
            var head = Volatile.Read(ref _head);
            var tail = Volatile.Read(ref _tail);

            if (head - tail == 0)
            {
                slot = default;
                return false;
            }

            var index = GetIndex(tail);
            if (_slots[index].IsEmpty())
            {
                continue;
            }

            Volatile.Write(ref _tail, tail + 1);
            slot = _slots[index];

            return true;
        }
    }

    private int GetIndex(ulong value)
    {
        return (int)(value % Capacity);
    }
}