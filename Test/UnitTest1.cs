using System.Text;
using DAL.Models.Users;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace Test;

public class UnitTest1
{
    private readonly HttpClient _client = new();
    private readonly ITestOutputHelper _testOutputHelper;

    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static string GraphQlUrl { get; } = "http://localhost:3000/graphql";

    [Fact]
    public async void Creates4UsersInAGroup()
    {
        var usersIds = new List<Guid>();
        var users = new List<AppUserInsertDto>
        {
            new()
            {
                UserName = "user1", Email = "user1@test.com", Password = "P@ssw0rd", Role = "User"
            },
            new()
            {
                UserName = "user2", Email = "user2@test.com", Password = "P@ssw0rd", Role = "User"
            },
            new()
            {
                UserName = "user3", Email = "user3@test.com", Password = "P@ssw0rd", Role = "User"
            },
            new()
            {
                UserName = "user4", Email = "user4@test.com", Password = "P@ssw0rd", Role = "User"
            }
        };
        foreach (var currentUser in users)
        {
            var createUserMutation = @"
                mutation{
                    createUser(appUserInsertDto: {
                        email:""" + currentUser.Email + @""",
                        password:""" + currentUser.Password + @""",
                        role:""" + currentUser.Role + @""",
                        userName:""" + currentUser.UserName + @"""
                    }){
                        id
                    }
                }";

            var mutationObject = new
            {
                query = createUserMutation
            };

            var serializedMutation = JsonConvert.SerializeObject(mutationObject);

            var content = new StringContent(serializedMutation, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(GraphQlUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine(responseString);
            var jsonResponse = JObject.Parse(responseString);
            var id = jsonResponse["data"]["createUser"]["id"];

            Assert.NotNull(id);
            usersIds.Add(Guid.Parse(id.ToString()));
        }

        //login to user1
        var loginMutation = @"
            mutation{
                signIn(appUserLoginDto: {
                    password:""P@ssw0rd"",
                    rememberMe:false
                    username:""user1""
                }){
                    succeeded
                }
            }
            ";
        var loginMutationObject = new { query = loginMutation };
        var serializedLoginMutation = JsonConvert.SerializeObject(loginMutationObject);
        var loginContent = new StringContent(serializedLoginMutation, Encoding.UTF8, "application/json");
        var loginResponse = await _client.PostAsync(GraphQlUrl, loginContent);
        var loginResponseString = await loginResponse.Content.ReadAsStringAsync();
        _testOutputHelper.WriteLine(loginResponseString);
        var loginJsonResponse = JObject.Parse(loginResponseString);
        var loginSucceeded = loginJsonResponse["data"]["signIn"]["succeeded"];
        Assert.True((bool)loginSucceeded);

        //create group
        var createGroupMutation = @"
            mutation{
                createGroup(groupInsertInput: {
                    description: ""new test group""
                    name:""test group""
                }){
                    id
                }
            }";
        var createGroupMutationObject = new { query = createGroupMutation };
        var serializedCreateGroupMutation = JsonConvert.SerializeObject(createGroupMutationObject);
        var createGroupContent = new StringContent(serializedCreateGroupMutation, Encoding.UTF8, "application/json");
        var createGroupResponse = await _client.PostAsync(GraphQlUrl, createGroupContent);
        var createGroupResponseString = await createGroupResponse.Content.ReadAsStringAsync();
        _testOutputHelper.WriteLine(createGroupResponseString);
        var createGroupJsonResponse = JObject.Parse(createGroupResponseString);
        var groupId = createGroupJsonResponse["data"]["createGroup"]["id"];
        Assert.NotNull(groupId);

        // invite other users to the group
        for (var i = 1; i < 4; i++)
        {
            var inviteUserMutation = @"
                mutation{
                    inviteUser(invitationInsertDto: {
                        groupId:""" + groupId + @""",
                        userId:""" + usersIds[i] + @"""
                    }){
                        invitedAt
                    }
                }";
            var inviteUserMutationObject = new { query = inviteUserMutation };
            var serializedInviteUserMutation = JsonConvert.SerializeObject(inviteUserMutationObject);
            var inviteUserContent = new StringContent(serializedInviteUserMutation, Encoding.UTF8, "application/json");
            var inviteUserResponse = await _client.PostAsync(GraphQlUrl, inviteUserContent);
            var inviteUserResponseString = await inviteUserResponse.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine(inviteUserResponseString);
            var inviteUserJsonResponse = JObject.Parse(inviteUserResponseString);
            var invitedAt = inviteUserJsonResponse["data"]["inviteUser"]["invitedAt"];
            Assert.NotNull(invitedAt);
        }

        // for each invited user, login and accept the invitation
        for (var i = 2; i < 5; i++)
        {
            loginMutation = @"
                mutation{
                    signIn(appUserLoginDto: {
                        password:""P@ssw0rd"",
                        rememberMe:false
                        username:""user" + i + @"""
                    }){
                        succeeded
                    }
                }";
            loginMutationObject = new { query = loginMutation };
            serializedLoginMutation = JsonConvert.SerializeObject(loginMutationObject);
            loginContent = new StringContent(serializedLoginMutation, Encoding.UTF8, "application/json");
            loginResponse = await _client.PostAsync(GraphQlUrl, loginContent);
            loginResponseString = await loginResponse.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine(loginResponseString);
            loginJsonResponse = JObject.Parse(loginResponseString);
            loginSucceeded = loginJsonResponse["data"]["signIn"]["succeeded"];
            Assert.True((bool)loginSucceeded);

            var acceptInvitationMutation = @"
                mutation{
                    joinGroup(userGroupInsertInput: {
                        groupId:""" + groupId + @"""
                    }){
                        joinedAt
                    }
                }";
            var acceptInvitationMutationObject = new { query = acceptInvitationMutation };
            var serializedAcceptInvitationMutation = JsonConvert.SerializeObject(acceptInvitationMutationObject);
            var acceptInvitationContent =
                new StringContent(serializedAcceptInvitationMutation, Encoding.UTF8, "application/json");
            var acceptInvitationResponse = await _client.PostAsync(GraphQlUrl, acceptInvitationContent);
            var acceptInvitationResponseString = await acceptInvitationResponse.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine(acceptInvitationResponseString);
            var acceptInvitationJsonResponse = JObject.Parse(acceptInvitationResponseString);
            var joinedAt = acceptInvitationJsonResponse["data"]["joinGroup"]["joinedAt"];
            Assert.NotNull(joinedAt);
        }

        // ensures there are 4 users in the group
        var getGroupQuery = @"
                {
                    groupById(id: """ + groupId + @"""){
                        userGroups{
                            user{
                                id
                            }
                        }
                    }
                }";

        var getGroupQueryObject = new { query = getGroupQuery };
        var serializedGetGroupQuery = JsonConvert.SerializeObject(getGroupQueryObject);
        var getGroupContent = new StringContent(serializedGetGroupQuery, Encoding.UTF8, "application/json");
        var getGroupResponse = await _client.PostAsync(GraphQlUrl, getGroupContent);
        var getGroupResponseString = await getGroupResponse.Content.ReadAsStringAsync();
        _testOutputHelper.WriteLine(getGroupResponseString);
        var getGroupJsonResponse = JObject.Parse(getGroupResponseString);
        var userGroups = getGroupJsonResponse["data"]["groupById"][0]["userGroups"];
        Assert.Equal(4, userGroups.Count());
    }
}