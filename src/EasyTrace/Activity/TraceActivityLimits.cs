namespace EasyTrace.Activity;

public class TraceActivityLimits
{
    public int AttributeCount { get; init; } = 10;
    public int AttributeNameLen { get; init; } = 255;
    public int AttributeValueLen { get; init; } = 255;
}