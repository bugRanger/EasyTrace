using EasyTrace.Export.Batch.Buffer;

namespace EasyTrace.Attribute;

public sealed class TraceAttributeList(int sizeLimit, int nameCapacity, int valueCapacity)
    : ICopiable<TraceAttributeList>
{
    private readonly TraceAttribute[] _attributes =
    [
        .. Enumerable.Repeat(0, sizeLimit).Select(_ => new TraceAttribute(nameCapacity, valueCapacity)),
    ];

    private int _size;

    public uint Dropped { get; private set; }

    public ReadOnlySpan<TraceAttribute> AsSpan() => _attributes.AsSpan(0, _size);

    public void CopyFrom(TraceAttributeList source)
    {
        source.CopyTo(this);
    }

    public void CopyTo(TraceAttributeList destination)
    {
        destination.Clear();
        destination.Dropped = Dropped;
        foreach (var attribute in AsSpan())
        {
            destination.Add(attribute.Name, attribute.Value);
        }
    }

    internal void Add(in TraceAttributeFieldRef key, in TraceAttributeFieldRef value)
    {
        if (_size == sizeLimit)
        {
            Dropped++;
            return;
        }

        var attribute = _attributes[_size++];
        attribute.Write(key, value);
    }

    internal void Clear()
    {
        _size = 0;
        Dropped = 0;

        foreach (var attribute in _attributes)
        {
            attribute.Clear();
        }
    }
}