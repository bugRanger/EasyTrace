using System.Collections.Concurrent;
using System.Text;

namespace EasyTrace.Tests.Export.TestData;

public class TestHttpServerCache
{
    private readonly ConcurrentDictionary<string, List<string>> _cache = new();

    public void Set(string key, byte[] value)
    {
        var valueStr = Convert.ToHexStringLower(value);
        if (_cache.TryGetValue(key, out var list))
        {
            list.Add(valueStr);
            return;
        }

        _cache[key] = [valueStr];
    }

    public string GetValues()
    {
        var result = new StringBuilder();
        result.Append("[\n");
        foreach (var item in _cache)
        {
            result.Append("  {\n");
            result.AppendFormat($"    \"key\": \"{item.Key}\",\n");
            result.AppendFormat($"    \"value\": \"{string.Join(", \n", item.Value)}\",\n");
            result.Append("  },\n");
        }

        result.Append("]\n");
        return result.ToString();
    }
}