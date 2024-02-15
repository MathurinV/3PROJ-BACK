using Newtonsoft.Json.Linq;

namespace Test;

public class GraphQlResult<T>
{
    public T Data { get; set; }
    public GraphQlError GraphQlError { get; set; }

    public GraphQlResult(string serviceResultJson, string queryName)
    {
        var rawResultObject = JObject.Parse(serviceResultJson);
        var dataJObject = rawResultObject["data"];
        Data = (dataJObject == null ? default(T) : dataJObject[queryName]!.ToObject<T>()) ?? throw new InvalidOperationException();
        var graphQlErrorsArray = (JArray?) rawResultObject["errors"];
        GraphQlError = graphQlErrorsArray?.ToObject<IList<GraphQlError>>()![0] ?? throw new InvalidOperationException();
    }
}