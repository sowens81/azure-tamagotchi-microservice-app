using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Tamagotchi.Backend.Users.Api.E2E.Tests.Model;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Tamagotchi.Backend.Users.Api.E2E.Tests;

public class UsersApiTests : IClassFixture<HttpClientFixture>
{
    private readonly HttpClient _client;

    private string _testUserId = string.Empty;

    public UsersApiTests(HttpClientFixture fixture)
    {
        _client = fixture.Client; // Get the shared HttpClient instance
    }

    private string RandomGuid(int length)
    {
        return $"{Guid.NewGuid().ToString("N").Substring(0, length)}";
    }

    [Fact]
    public async Task Live_HealthCheck_ShouldReturn_Ok()
    {
        var liveHealthCheckResponse = await _client.GetAsync("/api/healthz/live");
        Assert.True(
            liveHealthCheckResponse.StatusCode == HttpStatusCode.OK,
            $"Live Healthz endpoint return 200 OK, but got {liveHealthCheckResponse.StatusCode.ToString()}"
        );
    }

    [Fact]
    public async Task Ready_HealthCheck_ShouldReturn_Ok()
    {
        var readyHealthCheckResponse = await _client.GetAsync("/api/healthz/ready");
        Assert.True(
            readyHealthCheckResponse.StatusCode == HttpStatusCode.OK,
            $"Ready Healthz endpoint return 200 OK, but got {readyHealthCheckResponse.StatusCode.ToString()}"
        );
    }

    [Fact]
    public async Task Validate_RegisterUser_Endpoint_Bad_Request_Functionality()
    {
        var randomString = RandomGuid(8);
        var username = $"testuser{randomString}";
        var email = $"testuser{randomString}@example.com";
        var firstName = "john";
        var lastName = "smith";
        var badPassword = RandomGuid(8);
        var goodPassword = RandomGuid(15);

        var emptyPayload = new { };

        var badEmailPayload = new
        {
            Username = username,
            Email = "email",
            Password = goodPassword,
            ConfirmPassword = goodPassword,
            FirstName = firstName,
            LastName = lastName,
        };

        var badPasswordPayload = new
        {
            Username = username,
            Email = email,
            Password = badPassword,
            ConfirmPassword = badPassword,
            FirstName = firstName,
            LastName = lastName,
        };

        var nonMatchingPasswordPayload = new
        {
            Username = username,
            Email = email,
            Password = goodPassword,
            ConfirmPassword = badPassword,
            FirstName = firstName,
            LastName = lastName,
        };

        var responseEmptyPayload = await _client.PostAsJsonAsync(
            "api/auth/register",
            emptyPayload
        );

        var responseBadEmailPayload = await _client.PostAsJsonAsync(
            "api/auth/register",
            badEmailPayload
        );

        var responseBadPasswordPayload = await _client.PostAsJsonAsync(
            "api/auth/register",
            badPasswordPayload
        );

        var responseNonMatchingPasswordPayload = await _client.PostAsJsonAsync(
            "api/auth/register",
            nonMatchingPasswordPayload
        );

        Assert.True(
            responseEmptyPayload.StatusCode == HttpStatusCode.BadRequest,
            "Empty payload or missing elements in payload should return 400 Bad Request"
        );

        Assert.True(
            responseBadEmailPayload.StatusCode == HttpStatusCode.BadRequest,
            "Invalid email format should return 400 Bad Request"
        );

        Assert.True(
            responseBadPasswordPayload.StatusCode == HttpStatusCode.BadRequest,
            "Weak password should return 400 Bad Request"
        );

        Assert.True(
            responseNonMatchingPasswordPayload.StatusCode == HttpStatusCode.BadRequest,
            "Non-matching passwords should return 400 Bad Request"
        );
    }

    [Fact]
    public async Task Validate_RegisterUser_Endpoint_Good_Request_Functionality()
    {
        // Register User Request
        var randomString = RandomGuid(8);
        var randomPassword = RandomGuid(15);
        var user = new
        {
            Username = $"testuser{randomString}",
            Email = $"testuser{randomString}@example.com",
            Password = randomPassword,
            ConfirmPassword = randomPassword,
            FirstName = "test",
            LastName = "user{randomString}",
        };

        var registerUserResponse = await _client.PostAsJsonAsync("api/auth/register", user);

        Assert.True(
            registerUserResponse.StatusCode == HttpStatusCode.Created,
            $"Valid user registration should return 201 Created, but got {registerUserResponse.StatusCode.ToString()}"
        );

        var registerUserResponseContent =
            await registerUserResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(
            registerUserResponseContent.TryGetProperty("data", out var rurcData),
            "Response should contain 'data' after successful registration"
        );

        Assert.True(
            rurcData.TryGetProperty("userId", out var rurcDataUserId)
                && null != rurcDataUserId.GetString(),
            "Response should contain 'data.userId' after successful registration"
        );

        Assert.True(
            rurcData.TryGetProperty("username", out var rurcDataUserName)
                && rurcDataUserName.GetString() == user.Username,
            "Response should contain 'data.username' after successful registration"
        );

        Assert.True(
            rurcData.TryGetProperty("createdAt", out var rurcCreatedAt),
            "Response should contain 'data.createdAt' after successful registration"
        );

        // Delete User Request
        var deleteUserResponse = await _client.DeleteAsync(
            $"api/users/{rurcDataUserId.GetString()}"
        );

        Assert.True(
            deleteUserResponse.StatusCode == HttpStatusCode.Accepted,
            $"DeleteUserById should return status code 202 when user id {rurcDataUserId.GetString()} is deleted, but Returned: {deleteUserResponse.StatusCode.ToString()}"
        );
    }

    [Fact]
    public async Task Validate_GetAllUsers_Endpoint_Good_Request_Functionality()
    {
        var totalUsers = 5;
        var registeredUsers = new List<RegisteredUser>();
        // Register User Request

        for (int i = 0; i < totalUsers; i++)
        {
            var randomString = RandomGuid(8);
            var randomPassword = RandomGuid(15);
            var registerUser = new
            {
                Username = $"testuser{randomString}",
                Email = $"testuser{randomString}@example.com",
                Password = randomPassword,
                ConfirmPassword = randomPassword,
                FirstName = "test",
                LastName = "user{randomString}",
            };

            var registerUserResponse = await _client.PostAsJsonAsync(
                "api/auth/register",
                registerUser
            );

            Assert.True(
                registerUserResponse.StatusCode == HttpStatusCode.Created,
                "Valid user registration should return 201 Created"
            );

            var registerUserResponseContent =
                await registerUserResponse.Content.ReadFromJsonAsync<JsonElement>();

            Assert.True(
                registerUserResponseContent.TryGetProperty("data", out var rurcData),
                "Response should contain 'data' after successful registration"
            );

            Assert.True(
                rurcData.TryGetProperty("userId", out var rurcDataUserId)
                    && null != rurcDataUserId.GetString(),
                "Response should contain 'data.userId' after successful registration"
            );

            Assert.True(
                rurcData.TryGetProperty("username", out var rurcDataUserName)
                    && rurcDataUserName.GetString() == registerUser.Username,
                "Response should contain 'data.username' after successful registration"
            );

            Assert.True(
                rurcData.TryGetProperty("createdAt", out var rurcCreatedAt),
                "Response should contain 'data.createdAt' after successful registration"
            );

            var registeredUser = new RegisteredUser()
            {
                UserId = rurcDataUserId.GetString(),
                Username = rurcDataUserName.GetString(),
                CreatedAt = rurcCreatedAt.GetString(),
            };

            registeredUsers.Add(registeredUser);
        }

        // Get All Users Request

        var getAllUsersResponse = await _client.GetAsync("api/users/");
        Assert.True(
            getAllUsersResponse.StatusCode == HttpStatusCode.OK,
            "GetAllUsers should return status code 200 when list of any number of users returned."
        );

        var getAllUsersResponseContent =
            await getAllUsersResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(
            getAllUsersResponseContent.TryGetProperty("data", out var gaurcData),
            "Response should contain 'data' after successful registration"
        );

        Assert.True(
            gaurcData.GetArrayLength() == (registeredUsers.Count),
            $"Response should contain {registeredUsers.Count} but contains {gaurcData.GetArrayLength()}"
        );

        // Delete All Users Request
        foreach (RegisteredUser registeredUser in registeredUsers)
        {
            // Delete User Request
            var deleteUserResponse = await _client.DeleteAsync(
                $"api/users/{registeredUser.UserId}"
            );

            Assert.True(
                deleteUserResponse.StatusCode == HttpStatusCode.Accepted,
                $"DeleteUserById should return status code 202 when user id {registeredUser.UserId} is deleted, but Returned: {deleteUserResponse.StatusCode.ToString()}"
            );
        }
    }

    [Fact]
    public async Task Validate_GetUser_Endpoint_NotExists_Request_Functionality()
    {
        //Get User Request

        var getUserResponse = await _client.GetAsync($"api/users/abc123");

        Assert.True(
            getUserResponse.StatusCode == HttpStatusCode.NotFound,
            $"GetUserById should return status code 404 when user not exists, but returned {getUserResponse.StatusCode.ToString()}"
        );
    }

    [Fact]
    public async Task Validate_GetUser_Endpoint_Good_Request_Functionality()
    {
        // Register User Request
        var randomString = RandomGuid(8);
        var randomPassword = RandomGuid(15);
        var user = new
        {
            Username = $"testuser{randomString}",
            Email = $"testuser{randomString}@example.com",
            Password = randomPassword,
            ConfirmPassword = randomPassword,
            FirstName = "test",
            LastName = "user{randomString}",
        };

        var registerUserResponse = await _client.PostAsJsonAsync("api/auth/register", user);

        Assert.True(
            registerUserResponse.StatusCode == HttpStatusCode.Created,
            $"Valid user registration should return 201 Created, but got {registerUserResponse.StatusCode.ToString()}"
        );

        var registerUserResponseContent =
            await registerUserResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(
            registerUserResponseContent.TryGetProperty("data", out var rurcData),
            "Response should contain 'data' after successful registration"
        );

        Assert.True(
            rurcData.TryGetProperty("userId", out var rurcDataUserId)
                && null != rurcDataUserId.GetString(),
            "Response should contain 'data.userId' after successful registration"
        );

        Assert.True(
            rurcData.TryGetProperty("username", out var rurcDataUserName)
                && rurcDataUserName.GetString() == user.Username,
            "Response should contain 'data.username' after successful registration"
        );

        Assert.True(
            rurcData.TryGetProperty("createdAt", out var rurcCreatedAt),
            "Response should contain 'data.createdAt' after successful registration"
        );

        //Get User Request

        var getUserResponse = await _client.GetAsync($"api/users/{rurcDataUserId.GetString()}");

        Assert.True(
            getUserResponse.StatusCode == HttpStatusCode.OK,
            $"GetUserById should return status code 200 when users returned, but returned {getUserResponse.StatusCode.ToString()}"
        );

        var getUserResponseContent =
            await getUserResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(
            getUserResponseContent.TryGetProperty("data", out var gurcData),
            "Response should contain 'data'."
        );

        Assert.True(
            gurcData.TryGetProperty("userId", out var gurcDataUserId)
                && null != gurcDataUserId.GetString(),
            $"Response should contain 'data.userId', but returned {gurcDataUserId}"
        );

        Assert.True(
            gurcData.TryGetProperty("username", out var gurcDataUsername)
                && gurcDataUsername.GetString() == user.Username,
            $"Response should contain 'data.username', but returned {gurcDataUsername}"
        );

        Assert.True(
            gurcData.TryGetProperty("email", out var gurcDataEmail)
                && gurcDataEmail.GetString() == user.Email,
            $"Response should contain 'data.email', but returned {gurcDataEmail}"
        );

        Assert.True(
            gurcData.TryGetProperty("firstName", out var gurcDataFirstName)
                && gurcDataFirstName.GetString() == user.FirstName,
            $"Response should contain 'data.firstName', but returned {gurcDataFirstName}"
        );

        Assert.True(
            gurcData.TryGetProperty("lastName", out var gurcDataLastName)
                && gurcDataLastName.GetString() == user.LastName,
            $"Response should contain 'data.lastName', but returned {gurcDataLastName}"
        );

        Assert.True(
            gurcData.TryGetProperty("createdAt", out var gurcDataCreatedAt),
            $"Response should contain 'data.createdAt', but returned {gurcDataCreatedAt}"
        );

        Assert.True(
            gurcData.TryGetProperty("updatedAt", out var gurcDataUpdatedAt),
            $"Response should contain 'data.updatedAt', but returned {gurcDataUpdatedAt}"
        );


        // Delete User Request
        var deleteUserResponse = await _client.DeleteAsync(
            $"api/users/{rurcDataUserId.GetString()}"
        );

        Assert.True(
            deleteUserResponse.StatusCode == HttpStatusCode.Accepted,
            $"DeleteUserById should return status code 202 when user id {rurcDataUserId.GetString()} is deleted, but Returned: {deleteUserResponse.StatusCode.ToString()}"
        );
    }

    [Fact]
    public async Task Validate_GetUserByEmail_Endpoint_Bad_Request_Functionality()
    {
        //Get User Request

        var getUserResponse = await _client.GetAsync($"api/users/email?email=notanemail.com");

        Assert.True(
            getUserResponse.StatusCode == HttpStatusCode.BadRequest,
            $"GetUserByEmail should return status code 400 when not proper email returned, but returned {getUserResponse.StatusCode.ToString()}"
        );

        var getUserResponseContent =
            await getUserResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(
            getUserResponseContent.TryGetProperty("message", out var gurcMessage) && gurcMessage.GetString() == "One or more validation errors occurred.",
            $"Response should contain 'data.message', but returned {gurcMessage}"
        );
    }

    [Fact]
    public async Task Validate_GetUserByEmail_Endpoint_NotExists_Request_Functionality()
    {
        //Get User Request

        var getUserResponse = await _client.GetAsync($"api/users/email?email=test.user@yahoo.com");

        Assert.True(
            getUserResponse.StatusCode == HttpStatusCode.NotFound,
            $"GetUserByEmail should return status code 404 when user not exists, but returned {getUserResponse.StatusCode.ToString()}"
        );
    }

    [Fact]
    public async Task Validate_GetUserByEmail_Endpoint_Good_Request_Functionality()
    {
        // Register User Request
        var randomString = RandomGuid(8);
        var randomPassword = RandomGuid(15);
        var user = new
        {
            Username = $"testuser{randomString}",
            Email = $"testuser{randomString}@example.com",
            Password = randomPassword,
            ConfirmPassword = randomPassword,
            FirstName = "test",
            LastName = "user{randomString}",
        };

        var registerUserResponse = await _client.PostAsJsonAsync("api/auth/register", user);

        Assert.True(
            registerUserResponse.StatusCode == HttpStatusCode.Created,
            $"Valid user registration should return 201 Created, but got {registerUserResponse.StatusCode.ToString()}"
        );

        var registerUserResponseContent =
            await registerUserResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(
            registerUserResponseContent.TryGetProperty("data", out var rurcData),
            "Response should contain 'data' after successful registration"
        );

        Assert.True(
            rurcData.TryGetProperty("userId", out var rurcDataUserId)
                && null != rurcDataUserId.GetString(),
            "Response should contain 'data.userId' after successful registration"
        );

        Assert.True(
            rurcData.TryGetProperty("username", out var rurcDataUserName)
                && rurcDataUserName.GetString() == user.Username,
            "Response should contain 'data.username' after successful registration"
        );

        Assert.True(
            rurcData.TryGetProperty("createdAt", out var rurcCreatedAt),
            "Response should contain 'data.createdAt' after successful registration"
        );

        //Get User Request

        var getUserResponse = await _client.GetAsync($"api/users/email?email={user.Email}");

        Assert.True(
            getUserResponse.StatusCode == HttpStatusCode.OK,
            $"GetUserByEmail should return status code 200 when users returned, but returned {getUserResponse.StatusCode.ToString()}"
        );

        var getUserResponseContent =
            await getUserResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(
            getUserResponseContent.TryGetProperty("data", out var gurcData),
            "Response should contain 'data'."
        );

        Assert.True(
            gurcData.TryGetProperty("userId", out var gurcDataUserId)
                && null != gurcDataUserId.GetString(),
            $"Response should contain 'data.userId', but returned {gurcDataUserId}"
        );

        Assert.True(
            gurcData.TryGetProperty("username", out var gurcDataUsername)
                && gurcDataUsername.GetString() == user.Username,
            $"Response should contain 'data.username', but returned {gurcDataUsername}"
        );

        Assert.True(
            gurcData.TryGetProperty("email", out var gurcDataEmail)
                && gurcDataEmail.GetString() == user.Email,
            $"Response should contain 'data.email', but returned {gurcDataEmail}"
        );

        Assert.True(
            gurcData.TryGetProperty("firstName", out var gurcDataFirstName)
                && gurcDataFirstName.GetString() == user.FirstName,
            $"Response should contain 'data.firstName', but returned {gurcDataFirstName}"
        );

        Assert.True(
            gurcData.TryGetProperty("lastName", out var gurcDataLastName)
                && gurcDataLastName.GetString() == user.LastName,
            $"Response should contain 'data.lastName', but returned {gurcDataLastName}"
        );

        Assert.True(
            gurcData.TryGetProperty("createdAt", out var gurcDataCreatedAt),
            $"Response should contain 'data.createdAt', but returned {gurcDataCreatedAt}"
        );

        Assert.True(
            gurcData.TryGetProperty("updatedAt", out var gurcDataUpdatedAt),
            $"Response should contain 'data.updatedAt', but returned {gurcDataUpdatedAt}"
        );

        // Delete User Request
        var deleteUserResponse = await _client.DeleteAsync(
            $"api/users/{rurcDataUserId.GetString()}"
        );

        Assert.True(
            deleteUserResponse.StatusCode == HttpStatusCode.Accepted,
            $"DeleteUserById should return status code 202 when user id {rurcDataUserId.GetString()} is deleted, but Returned: {deleteUserResponse.StatusCode.ToString()}"
        );
    }

    [Fact]
    public async Task Validate_GetUserByUsername_Endpoint_NotExists_Request_Functionality()
    {
        //Get User Request

        var getUserResponse = await _client.GetAsync($"api/users/username?username=abc123");

        Assert.True(
            getUserResponse.StatusCode == HttpStatusCode.NotFound,
            $"GetUserByUsername should return status code 404 when user not exists, but returned {getUserResponse.StatusCode.ToString()}"
        );
    }

    [Fact]
    public async Task Validate_GetUserByUser_Endpoint_Good_Request_Functionality()
    {
        // Register User Request
        var randomString = RandomGuid(8);
        var randomPassword = RandomGuid(15);
        var user = new
        {
            Username = $"testuser{randomString}",
            Email = $"testuser{randomString}@example.com",
            Password = randomPassword,
            ConfirmPassword = randomPassword,
            FirstName = "test",
            LastName = "user{randomString}",
        };

        var registerUserResponse = await _client.PostAsJsonAsync("api/auth/register", user);

        Assert.True(
            registerUserResponse.StatusCode == HttpStatusCode.Created,
            $"Valid user registration should return 201 Created, but got {registerUserResponse.StatusCode.ToString()}"
        );

        var registerUserResponseContent =
            await registerUserResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(
            registerUserResponseContent.TryGetProperty("data", out var rurcData),
            "Response should contain 'data' after successful registration"
        );

        Assert.True(
            rurcData.TryGetProperty("userId", out var rurcDataUserId)
                && null != rurcDataUserId.GetString(),
            "Response should contain 'data.userId' after successful registration"
        );

        Assert.True(
            rurcData.TryGetProperty("username", out var rurcDataUserName)
                && rurcDataUserName.GetString() == user.Username,
            "Response should contain 'data.username' after successful registration"
        );

        Assert.True(
            rurcData.TryGetProperty("createdAt", out var rurcCreatedAt),
            "Response should contain 'data.createdAt' after successful registration"
        );

        //Get User Request

        var getUserResponse = await _client.GetAsync($"api/users/username?username={user.Username}");

        Assert.True(
            getUserResponse.StatusCode == HttpStatusCode.OK,
            $"GetUserByEmail should return status code 200 when users returned, but returned {getUserResponse.StatusCode.ToString()}"
        );

        var getUserResponseContent =
            await getUserResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(
            getUserResponseContent.TryGetProperty("data", out var gurcData),
            "Response should contain 'data'."
        );

        Assert.True(
            gurcData.TryGetProperty("userId", out var gurcDataUserId)
                && null != gurcDataUserId.GetString(),
            $"Response should contain 'data.userId', but returned {gurcDataUserId}"
        );

        Assert.True(
            gurcData.TryGetProperty("username", out var gurcDataUsername)
                && gurcDataUsername.GetString() == user.Username,
            $"Response should contain 'data.username', but returned {gurcDataUsername}"
        );

        Assert.True(
            gurcData.TryGetProperty("email", out var gurcDataEmail)
                && gurcDataEmail.GetString() == user.Email,
            $"Response should contain 'data.email', but returned {gurcDataEmail}"
        );

        Assert.True(
            gurcData.TryGetProperty("firstName", out var gurcDataFirstName)
                && gurcDataFirstName.GetString() == user.FirstName,
            $"Response should contain 'data.firstName', but returned {gurcDataFirstName}"
        );

        Assert.True(
            gurcData.TryGetProperty("lastName", out var gurcDataLastName)
                && gurcDataLastName.GetString() == user.LastName,
            $"Response should contain 'data.lastName', but returned {gurcDataLastName}"
        );

        Assert.True(
            gurcData.TryGetProperty("createdAt", out var gurcDataCreatedAt),
            $"Response should contain 'data.createdAt', but returned {gurcDataCreatedAt}"
        );

        Assert.True(
            gurcData.TryGetProperty("updatedAt", out var gurcDataUpdatedAt),
            $"Response should contain 'data.updatedAt', but returned {gurcDataUpdatedAt}"
        );

        // Delete User Request
        var deleteUserResponse = await _client.DeleteAsync(
            $"api/users/{rurcDataUserId.GetString()}"
        );

        Assert.True(
            deleteUserResponse.StatusCode == HttpStatusCode.Accepted,
            $"DeleteUserById should return status code 202 when user id {rurcDataUserId.GetString()} is deleted, but Returned: {deleteUserResponse.StatusCode.ToString()}"
        );
    }

    // [Fact]
    //public async Task Validate_UpdateUser_Endpoint_Good_Request_Functionality()
    //{
    //    // Register User Request
    //    var randomString = RandomGuid(8);
    //    var randomPassword = RandomGuid(15);
    //    var user = new
    //    {
    //        Username = $"testuser{randomString}",
    //        Email = $"testuser{randomString}@example.com",
    //        Password = randomPassword,
    //        ConfirmPassword = randomPassword,
    //        FirstName = "test",
    //        LastName = "user{randomString}",
    //    };

    //    var registerUserResponse = await _client.PostAsJsonAsync("api/auth/register", user);

    //    Assert.True(
    //        registerUserResponse.StatusCode == HttpStatusCode.Created,
    //        $"Valid user registration should return 201 Created, but got {registerUserResponse.StatusCode.ToString()}"
    //    );

    //    var registerUserResponseContent =
    //        await registerUserResponse.Content.ReadFromJsonAsync<JsonElement>();

    //    Assert.True(
    //        registerUserResponseContent.TryGetProperty("data", out var rurcData),
    //        "Response should contain 'data' after successful registration"
    //    );

    //    Assert.True(
    //        rurcData.TryGetProperty("userId", out var rurcDataUserId)
    //            && null != rurcDataUserId.GetString(),
    //        "Response should contain 'data.userId' after successful registration"
    //    );

    //    Assert.True(
    //        rurcData.TryGetProperty("username", out var rurcDataUserName)
    //            && rurcDataUserName.GetString() == user.Username,
    //        "Response should contain 'data.username' after successful registration"
    //    );

    //    Assert.True(
    //        rurcData.TryGetProperty("createdAt", out var rurcCreatedAt),
    //        "Response should contain 'data.createdAt' after successful registration"
    //    );

    //    //Update User Request
    //    var userUpdate = new
    //    {
    //        Username = $"updateduser{randomString}",
    //        FirstName = "updated",
    //        LastName = "updateduser{randomString}",
    //    };

    //    var userUpdateJson = JsonContent.Create(userUpdate);

    //    // Perform the PUT request with the user update data
    //    var updateUserResponse = await _client.PutAsync(
    //        $"api/users/{rurcDataUserId.GetString()}",
    //        userUpdateJson
    //    );

    //    Assert.True(
    //        updateUserResponse.StatusCode == HttpStatusCode.OK,
    //        $"UpdateUserById should return status code 200 when user id {rurcDataUserId.GetString()} is updated, but Returned: {updateUserResponse.StatusCode.ToString()}"
    //    );

    //    // Delete User Request
    //    var deleteUserResponse = await _client.DeleteAsync(
    //        $"api/users/{rurcDataUserId.GetString()}"
    //    );

    //    Assert.True(
    //        deleteUserResponse.StatusCode == HttpStatusCode.Accepted,
    //        $"DeleteUserById should return status code 202 when user id {rurcDataUserId.GetString()} is deleted, but Returned: {deleteUserResponse.StatusCode.ToString()}"
    //    );
    //}

    [Fact]
    public async Task Validate_DeleteUser_Endpoint_NotExists_Request_Functionality()
    {
        // Delete User Request
        var fakeUserId = "FAKE12345";

        var deleteUserResponse = await _client.DeleteAsync($"api/users/{fakeUserId}");

        Assert.True(
            deleteUserResponse.StatusCode == HttpStatusCode.NotFound,
            $"DeleteUserById should return status code 404 when user id {fakeUserId} not found, but Returned: {deleteUserResponse.StatusCode.ToString()}"
        );
    }

    [Fact]
    public async Task Validate_DeleteUser_Endpoint_Good_Request_Functionality()
    {
        // Register User Request
        var randomString = RandomGuid(8);
        var randomPassword = RandomGuid(15);
        var user = new
        {
            Username = $"testuser{randomString}",
            Email = $"testuser{randomString}@example.com",
            Password = randomPassword,
            ConfirmPassword = randomPassword,
            FirstName = "test",
            LastName = "user{randomString}",
        };

        var registerUserResponse = await _client.PostAsJsonAsync("api/auth/register", user);

        Assert.True(
            registerUserResponse.StatusCode == HttpStatusCode.Created,
            "Valid user registration should return 201 Created"
        );

        var registerUserResponseContent =
            await registerUserResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(
            registerUserResponseContent.TryGetProperty("data", out var rurcData),
            "Response should contain 'data' after successful registration"
        );

        Assert.True(
            rurcData.TryGetProperty("userId", out var rurcDataUserId)
                && null != rurcDataUserId.GetString(),
            "Response should contain 'data.userId' after successful registration"
        );

        Assert.True(
            rurcData.TryGetProperty("username", out var rurcDataUserName)
                && rurcDataUserName.GetString() == user.Username,
            "Response should contain 'data.username' after successful registration"
        );

        Assert.True(
            rurcData.TryGetProperty("createdAt", out var rurcCreatedAt),
            "Response should contain 'data.createdAt' after successful registration"
        );

        // Delete User Request
        var deleteUserResponse = await _client.DeleteAsync(
            $"api/users/{rurcDataUserId.GetString()}"
        );

        Assert.True(
            deleteUserResponse.StatusCode == HttpStatusCode.Accepted,
            $"DeleteUserById should return status code 202 when user id {rurcDataUserId.GetString()} is deleted, but Returned: {deleteUserResponse.StatusCode.ToString()}"
        );
    }
}
