using EasyTrace.Export.Batch.Buffer;

namespace EasyTrace.Activity;

public sealed class TraceActivityTagList : ICopiable<TraceActivityTagList>
{
    // TODO: Add configure from in builder.
    private const int MaxListLen = 10;

    private int _length;
    // TODO: Take list from pool. > Not all actions require the use of tags; for this reason, a tag pool should be used..
    private readonly TraceActivityTag[] _tags =
    [
        .. Enumerable.Repeat(0, MaxListLen).Select(_ => new TraceActivityTag()),
    ];

    public uint Dropped { get; private set; }

    public ReadOnlySpan<TraceActivityTag> GetItems() => _tags.AsSpan(0, _length);

    internal void Add(ReadOnlySpan<char> key, ReadOnlySpan<char> value)
    {
        // TODO: Rent list from pool.
        if (_length == MaxListLen)
        {
            Dropped++;
            return;
        }

        var reusableTag = _tags[_length++];
        reusableTag.Write(key, value);
    }

    internal void Clear()
    {
        _length = 0;
        Dropped = 0;

        // TODO: Return list from pool.
        foreach (var reusableTag in _tags)
        {
            reusableTag.Clear();
        }
    }

    public void CopyFrom(TraceActivityTagList source)
    {
        source.CopyTo(this);
    }

    public void CopyTo(TraceActivityTagList destination)
    {
        destination.Clear();
        foreach (var reusableTag in GetItems())
        {
            destination.Add(reusableTag.Name, reusableTag.Value);
        }
    }
}