using Newtonsoft.Json.Linq;

namespace PatteLib;

public static class JsonUtils
{
    public static T GetValue<T>(JToken? jToken, bool throwOnNull = true, T defaultValue = default!)
    {
        if (jToken == null && throwOnNull)
        {
            throw new Exception();
        }
        return (jToken == null ? defaultValue : jToken.Value<T>())!;
    }
    
    public static IEnumerable<T> GetValues<T>(JToken? jToken, bool throwOnNull = true, IEnumerable<T> defaultValue = default!)
    {
        if (jToken == null && throwOnNull)
        {
            throw new Exception();
        }
        return (jToken == null ? defaultValue : jToken.Values<T>())!;
    }
}