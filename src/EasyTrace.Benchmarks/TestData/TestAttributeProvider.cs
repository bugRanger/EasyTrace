using System;
using System.Collections.Generic;
using System.Linq;
using EasyTrace.Activity;

namespace EasyTrace.Benchmarks.TestData;

public class TestAttributeProvider
{
    public const string None = "-";
    public const string Int32 = nameof(Int32);
    public const string Double = nameof(Double);
    public const string String = nameof(String);

    private readonly KeyValuePair<string, int>[] _attributeInt;
    private readonly KeyValuePair<string, double>[] _attributeDouble;
    private readonly KeyValuePair<string, string>[] _attributeString;

    public TestAttributeProvider(int attributeCount)
    {
        _attributeInt = Enumerable.Range(0, attributeCount)
            .Select(idx => new KeyValuePair<string, int>($"Tag-{idx}", idx))
            .ToArray();

        _attributeDouble = _attributeInt
            .Select(kvp => new KeyValuePair<string, double>(kvp.Key, kvp.Value + 0.123456789))
            .ToArray();

        _attributeString = _attributeInt
            .Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.ToString()))
            .ToArray();
    }

    public void SetAttributes(string attributeType, in TraceActivityScope? activity)
    {
        if (!activity.HasValue)
        {
            return;
        }

        switch (attributeType)
        {
            case None:
                break;

            case Int32:
                foreach (var (name, value) in _attributeInt)
                {
                    activity?.SetAttribute(name, value);
                }

                break;
            case Double:
                foreach (var (name, value) in _attributeDouble)
                {
                    activity?.SetAttribute(name, value);
                }

                break;
            case String:
                foreach (var (name, value) in _attributeString)
                {
                    activity?.SetAttribute(name, value);
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(attributeType), attributeType, null);
        }
    }

    public void SetTags(string attributeType, System.Diagnostics.Activity? activity)
    {
        if (activity == null)
        {
            return;
        }

        switch (attributeType)
        {
            case None:
                break;

            case Int32:
                foreach (var (name, value) in _attributeInt)
                {
                    activity?.SetTag(name, value);
                }

                break;
            case Double:
                foreach (var (name, value) in _attributeDouble)
                {
                    activity?.SetTag(name, value);
                }

                break;
            case String:
                foreach (var (name, value) in _attributeString)
                {
                    activity?.SetTag(name, value);
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(attributeType), attributeType, null);
        }
    }
}