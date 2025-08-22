using System.Net;
using System.Net.Http.Json;
using boat_share.DTOs;
using boat_shareTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using boat_share.Data;

namespace boat_shareTests.Users;

[TestClass]
public class UsersIntegrationTests : IntegrationTestBase
{
    [TestMethod]
    public async Task Admin_Can_List_And_Create_User()
    {
        SetTestUser(99, role: "Admin");
        // Need a boat to assign
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var boat = db.Boats.FirstOrDefault() ?? db.Boats.Add(new boat_share.Models.Boat { Name = "Boat A", Type = "Sail", Capacity = 4, HourlyRate = 50, IsActive = true }).Entity;
        db.SaveChanges();

        // Create
        var create = new UserCreateDTO
        {
            Email = "new@user.com",
            Name = "New User",
            Password = "password123",
            BoatId = boat.BoatId,
            Role = "Member"
        };
        var createResp = await Client.PostAsJsonAsync("/api/Users", create);
        Assert.AreEqual(HttpStatusCode.Created, createResp.StatusCode);

        // List
        var listResp = await Client.GetAsync("/api/Users");
        Assert.AreEqual(HttpStatusCode.OK, listResp.StatusCode);

    // Delete the created user
    var createdUser = await createResp.Content.ReadFromJsonAsync<UserInfoDTO>();
    Assert.IsNotNull(createdUser);
    var delResp = await Client.DeleteAsync($"/api/Users/{createdUser!.UserId}");
    Assert.AreEqual(HttpStatusCode.NoContent, delResp.StatusCode);
    }

    [TestMethod]
    public async Task Member_Cannot_List_Users_And_Can_Get_Self()
    {
    using var scope = Factory.Services.CreateScope();
    var (boat, user) = SeedBoatAndUser(scope, role: "Member");

        SetTestUser(user.UserId, role: "Member", email: user.Email, name: user.Name);

        // List forbidden
        var listResp = await Client.GetAsync("/api/Users");
        Assert.AreEqual(HttpStatusCode.Forbidden, listResp.StatusCode);

        // Get self OK
        var getSelf = await Client.GetAsync($"/api/Users/{user.UserId}");
        Assert.AreEqual(HttpStatusCode.OK, getSelf.StatusCode);

    // Get other forbidden
        var getOther = await Client.GetAsync($"/api/Users/{user.UserId + 1}");
        Assert.AreEqual(HttpStatusCode.Forbidden, getOther.StatusCode);

    // Member update should ignore role change
    var update = new UserUpdateDTO { Role = "Admin", Name = "New Name" };
    var updateResp = await Client.PutAsJsonAsync($"/api/Users/{user.UserId}", update);
    Assert.AreEqual(HttpStatusCode.OK, updateResp.StatusCode);
    var updated = await updateResp.Content.ReadFromJsonAsync<UserInfoDTO>();
    Assert.IsNotNull(updated);
    Assert.AreEqual("Member", updated!.Role); // role unchanged for member
    Assert.AreEqual("New Name", updated.Name);
    }
}
