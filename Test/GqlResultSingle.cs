using Newtonsoft.Json.Linq;

namespace Test;

public class GqlResultSingle<T>
{
    public GqlResultSingle(string json, string propertyName)
    {
        var jObject = JObject.Parse(json);
        var data = jObject["data"];
        var property = data[propertyName];
        Data = property.ToObject<T>();
    }

    public T Data { get; set; }
}