using System.Text;
using Bogus;
using DAL.Models.Expenses;
using DAL.Models.Groups;
using DAL.Models.Messages;
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

    private string? AddExpense(ExpensePrevisualizationInput expensePrevisualizationInput)
    {
        var fakeDescription = new Faker().Lorem.Sentence();

        var userAmountsListString = "[";
        foreach (var userAmount in expensePrevisualizationInput.UserAmountsList)
            userAmountsListString += $"{{key: \"{userAmount.Key}\" }},";
        userAmountsListString = userAmountsListString.Remove(userAmountsListString.Length - 1);
        userAmountsListString += "]";

        var addUserExpenseMutation = @"
        mutation {
            addUserExpense(
                expenseInsertInput: {
                    amount: " + expensePrevisualizationInput.Amount + @",
                    description: """ + fakeDescription + @""",
                    groupId: """ + expensePrevisualizationInput.GroupId + $@""",
                    userAmountsList: {userAmountsListString}
                }}
            ) {{
                amount
                paidAt
            }}
        }}";

        var addUserExpenseMutationObject = new { query = addUserExpenseMutation };
        var serializedAddUserExpenseMutation = JsonConvert.SerializeObject(addUserExpenseMutationObject);
        var addUserExpenseContent =
            new StringContent(serializedAddUserExpenseMutation, Encoding.UTF8, "application/json");
        var addUserExpenseResponse = _client.PostAsync(GraphQlUrl, addUserExpenseContent).Result;
        var addUserExpenseResponseString = addUserExpenseResponse.Content.ReadAsStringAsync().Result;
        _testOutputHelper.WriteLine(addUserExpenseResponseString);
        var addUserExpenseJsonResponse = JObject.Parse(addUserExpenseResponseString);
        var amount = addUserExpenseJsonResponse["data"]?["addUserExpense"]?[0]?["amount"];
        return amount?.ToString();
    }

    private string? PrevisualizeExpense(ExpensePrevisualizationInput expensePrevisualizationInput)
    {
        var userAmountsListString = "[";
        foreach (var userAmount in expensePrevisualizationInput.UserAmountsList)
            userAmountsListString += $"{{key: \"{userAmount.Key}\" }},";
        userAmountsListString = userAmountsListString.Remove(userAmountsListString.Length - 1);
        userAmountsListString += "]";

        var previsualizeEpenseQuery = $@"
            {{previsualizeUserExpenses(expensePrevisualizationInput: {{
                amount:{expensePrevisualizationInput.Amount}
                groupId:""{expensePrevisualizationInput.GroupId}""
                userAmountsList: {userAmountsListString}
            }}){{
                key
                value
            }}}}";

        var previsualizeExpenseObject = new { query = previsualizeEpenseQuery };
        var serializedPrevisualizeExpense = JsonConvert.SerializeObject(previsualizeExpenseObject);
        var previsualizeExpenseContent =
            new StringContent(serializedPrevisualizeExpense, Encoding.UTF8, "application/json");
        var previsualizeExpenseResponse = _client.PostAsync(GraphQlUrl, previsualizeExpenseContent).Result;
        var previsualizeExpenseResponseString = previsualizeExpenseResponse.Content.ReadAsStringAsync().Result;
        _testOutputHelper.WriteLine(previsualizeExpenseResponseString);
        var previsualizeExpenseJsonResponse = JObject.Parse(previsualizeExpenseResponseString);
        var previsualizeExpense = previsualizeExpenseJsonResponse["data"]?["previsualizeUserExpenses"];
        return previsualizeExpense?.ToString();
    }

    private string? SendMessage(Guid receiverId)
    {
        var messageFaker = new Faker<MessageInsertInput>()
            .RuleFor(m => m.Content, f => f.Lorem.Sentence());

        var currentMessageInsertInput = messageFaker.Generate();
        currentMessageInsertInput.ReceiverId = receiverId;

        var sendMessageMutation = @"
            mutation{
                sendMessage(messageInsertInput: {
                    content:""" + currentMessageInsertInput.Content + @""",
                    receiverId:""" + receiverId + @"""
                }){
                    sentAt
                }
            }";

        var sendMessageMutationObject = new { query = sendMessageMutation };
        var serializedSendMessageMutation = JsonConvert.SerializeObject(sendMessageMutationObject);
        var sendMessageContent = new StringContent(serializedSendMessageMutation, Encoding.UTF8, "application/json");
        var sendMessageResponse = _client.PostAsync(GraphQlUrl, sendMessageContent).Result;
        var sendMessageResponseString = sendMessageResponse.Content.ReadAsStringAsync().Result;
        _testOutputHelper.WriteLine(sendMessageResponseString);
        var sendMessageJsonResponse = JObject.Parse(sendMessageResponseString);
        var sentAt = sendMessageJsonResponse["data"]?["sendMessage"]?["sentAt"];
        return sentAt?.ToString();
    }

    private string? SignIn(string userName, string password, bool rememberMe)
    {
        var signInMutation = @"
            mutation{
                signIn(appUserLoginDto: {
                    password:""" + password + @""",
                    rememberMe:" + rememberMe.ToString().ToLower() + @",
                    username:""" + userName + @"""
                }){
                    succeeded
                }
            }";
        var signInMutationObject = new { query = signInMutation };
        var serializedSignInMutation = JsonConvert.SerializeObject(signInMutationObject);
        var signInContent = new StringContent(serializedSignInMutation, Encoding.UTF8, "application/json");
        var signInResponse = _client.PostAsync(GraphQlUrl, signInContent).Result;
        var signInResponseString = signInResponse.Content.ReadAsStringAsync().Result;
        _testOutputHelper.WriteLine(signInResponseString);
        var signInJsonResponse = JObject.Parse(signInResponseString);
        var succeeded = signInJsonResponse["data"]?["signIn"]?["succeeded"];
        return succeeded?.ToString();
    }

    [Fact]
    public async void Creates4UsersInAGroup()
    {
        var usersIds = new List<Guid>();

        var userFaker = new Faker<AppUserInsertDto>("fr")
            .RuleFor(u => u.UserName, f => f.Internet.UserName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.UserName))
            .RuleFor(u => u.Password, "P@ssw0rd")
            .RuleFor(u => u.Role, f => "User");

        var groupFaker = new Faker<GroupInsertInput>()
            .RuleFor(g => g.Name, f => f.Internet.UserName())
            .RuleFor(g => g.Description, f => f.Lorem.Sentence());

        var users = new List<AppUserInsertDto>();

        for (var i = 0; i < 4; i++) users.Add(userFaker.Generate());

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
            var id = jsonResponse["data"]?["createUser"]?["id"];

            Assert.NotNull(id);
            usersIds.Add(Guid.Parse(id.ToString()));
        }

        //login to user1
        var loginStatus = SignIn(users[0].UserName, users[0].Password, false);
        Assert.True(loginStatus != null && bool.Parse(loginStatus));

        //create group
        var newGroup = groupFaker.Generate();
        var createGroupMutation = @"
            mutation{
                createGroup(groupInsertInput: {
                    description: """ + newGroup.Description + @""",
                    name: """ + newGroup.Name + @"""
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
        var groupId = createGroupJsonResponse["data"]?["createGroup"]?["id"];
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
            var invitedAt = inviteUserJsonResponse["data"]?["inviteUser"]?["invitedAt"];
            Assert.NotNull(invitedAt);
        }

        // for each invited user, login and accept the invitation
        for (var i = 2; i < 5; i++)
        {
            loginStatus = SignIn(users[i - 1].UserName, users[i - 1].Password, false);
            Assert.True(loginStatus != null && bool.Parse(loginStatus));

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
            var joinedAt = acceptInvitationJsonResponse["data"]?["joinGroup"]?["joinedAt"];
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
        var userGroups = getGroupJsonResponse["data"]?["groupById"]?["userGroups"];
        if (userGroups != null) Assert.Equal(4, userGroups.Count());

        // signs in user1
        loginStatus = SignIn(users[0].UserName, users[0].Password, false);
        Assert.True(loginStatus != null && bool.Parse(loginStatus));

        var userAmountsList = new List<KeyValuePair<Guid, decimal?>>();
        foreach (var userId in usersIds) userAmountsList.Add(new KeyValuePair<Guid, decimal?>(userId, null));

        var randomAmount = new Faker().Finance.Amount();

        var fakeExpense = new ExpensePrevisualizationInput
        {
            GroupId = Guid.Parse(groupId.ToString()),
            Amount = randomAmount,
            UserAmountsList = userAmountsList
        };

        var previsualize = PrevisualizeExpense(fakeExpense);
        Assert.NotNull(previsualize);

        var addExpense = AddExpense(fakeExpense);
        Assert.NotNull(addExpense);

        // creates new messages
        // logs in with sender credentials and send message to created users
        for (var i = 0; i < 4; i++)
        {
            loginStatus = SignIn(users[i].UserName, users[i].Password, false);
            Assert.True(loginStatus != null && bool.Parse(loginStatus));

            var sentAt = SendMessage(usersIds[(i + 1) % 4]);
            Assert.True(sentAt != null);
        }
    }
}