using System.Text;
using DAL.Models.Users;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace Test;

public class UnitTest1
{
    public static string GraphQlUrl { get; } = "http://localhost:3000/graphql";
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _client = new();
    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void QueriesExistingUsers()
    {
        var param = new JObject();
        param["query"] = @"{
                                                users (){
                                                    id
                                                }
                                            }";
        var content = new StringContent(JsonConvert.SerializeObject(param), Encoding.UTF8, "application/json");
        var response = _client.PostAsync(GraphQlUrl, content).Result;
        var serviceResultJson = response.Content.ReadAsStringAsync().Result;
        _testOutputHelper.WriteLine(serviceResultJson);
        var gqlResult = new GqlResultList<AppUser>(serviceResultJson, "users");
        Assert.True(gqlResult.Data.Count == 0);
    }
}