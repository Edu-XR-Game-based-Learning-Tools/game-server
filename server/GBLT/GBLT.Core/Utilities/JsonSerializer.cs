using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Core.Utility;

public sealed class JsonSerializer
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new JsonContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
    };

    public sealed class JsonContractResolver : CamelCasePropertyNamesContractResolver
    {
    }

    public static string SerializeObject(object o)
    {
        return JsonConvert.SerializeObject(o, Formatting.Indented, Settings);
    }
}