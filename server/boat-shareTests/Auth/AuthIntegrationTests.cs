using System.Net;
using System.Net.Http.Json;
using boat_share.DTOs;
using boat_shareTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace boat_shareTests.Auth;

[TestClass]
public class AuthIntegrationTests : IntegrationTestBase
{
    [TestMethod]
    public async Task Auth_Test_Endpoint_Works()
    {
        var resp = await Client.GetAsync("/api/Auth/test");
        Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
    }

    [TestMethod]
    public async Task Create_Test_User_Then_Login_Succeeds()
    {
        // Create test user via endpoint
        var createResp = await Client.PostAsync("/api/Auth/create-test-user", content: null);
        Assert.AreEqual(HttpStatusCode.OK, createResp.StatusCode);

        // Attempt login
        var loginResp = await Client.PostAsJsonAsync("/api/Auth/login", new LoginDTO
        {
            Email = "test@test.com",
            Password = "test"
        });

        Assert.AreEqual(HttpStatusCode.OK, loginResp.StatusCode);
        var auth = await loginResp.Content.ReadFromJsonAsync<AuthResponseDTO>();
        Assert.IsNotNull(auth);
        Assert.IsFalse(string.IsNullOrWhiteSpace(auth!.Token));
        Assert.AreEqual("test@test.com", auth.User.Email);
    }
}
