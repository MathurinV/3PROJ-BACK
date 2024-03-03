using Newtonsoft.Json.Linq;

namespace Test;

public class GqlResultList<T>
{
    public GqlResultList(string serviceResultJson, string queryName)
    {
        var rawResultJObject = JObject.Parse(serviceResultJson);

        var dataJObject = rawResultJObject["data"];
        var dataArray = dataJObject == null ? new JArray() : (JArray)rawResultJObject["data"]?[queryName]!;
        Data = dataArray.ToObject<IList<T>>() ?? throw new InvalidOperationException();
        var graphQlErrorsJArray = (JArray)rawResultJObject["errors"]!;
        GraphQlError = graphQlErrorsJArray?.ToObject<IList<GraphQlError>>()![0];
    }

    public IList<T> Data { get; set; }
    public GraphQlError? GraphQlError { get; set; }
}