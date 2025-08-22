using System.Net;
using System.Net.Http.Json;
using boat_share.DTOs;
using boat_shareTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace boat_shareTests.Boats;

[TestClass]
public class BoatsIntegrationTests : IntegrationTestBase
{
    [TestMethod]
    public async Task Get_All_Boats_Works()
    {
        SetTestUser(1, role: "Admin");
        var resp = await Client.GetAsync("/api/Boats");
        Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        var boats = await resp.Content.ReadFromJsonAsync<List<BoatDTO>>();
        Assert.IsNotNull(boats);
    }

    [TestMethod]
    public async Task Admin_Can_Create_Get_Update_Delete_Boat()
    {
        SetTestUser(1, role: "Admin");

        // Create
        var create = new BoatCreateDTO
        {
            Name = "API Boat",
            Type = "Motor",
            Description = "Nice boat",
            Location = "Pier 1",
            Capacity = 5,
            HourlyRate = 120
        };
        var createResp = await Client.PostAsJsonAsync("/api/Boats", create);
        Assert.AreEqual(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.Content.ReadFromJsonAsync<BoatDTO>();
        Assert.IsNotNull(created);
        var id = created!.BoatId;

        // Get by id
        var getResp = await Client.GetAsync($"/api/Boats/{id}");
        Assert.AreEqual(HttpStatusCode.OK, getResp.StatusCode);

        // Update
        var update = new BoatUpdateDTO { Name = "API Boat Updated", Capacity = 6 };
        var updateResp = await Client.PutAsJsonAsync($"/api/Boats/{id}", update);
        Assert.AreEqual(HttpStatusCode.OK, updateResp.StatusCode);
        var updated = await updateResp.Content.ReadFromJsonAsync<BoatDTO>();
        Assert.AreEqual("API Boat Updated", updated!.Name);
        Assert.AreEqual(6, updated.Capacity);

        // Delete
        var deleteResp = await Client.DeleteAsync($"/api/Boats/{id}");
        Assert.AreEqual(HttpStatusCode.NoContent, deleteResp.StatusCode);

        // Verify not found
        var getDeleted = await Client.GetAsync($"/api/Boats/{id}");
        Assert.AreEqual(HttpStatusCode.NotFound, getDeleted.StatusCode);
    }

    [TestMethod]
    public async Task Admin_Can_Assign_User_To_Boat()
    {
        SetTestUser(1, role: "Admin");

        // Create a boat first
        var create = new BoatCreateDTO { Name = "Assign Boat", Type = "Motor", Capacity = 4, HourlyRate = 80 };
        var createResp = await Client.PostAsJsonAsync("/api/Boats", create);
        Assert.AreEqual(HttpStatusCode.Created, createResp.StatusCode);
        var boat = await createResp.Content.ReadFromJsonAsync<BoatDTO>();
        Assert.IsNotNull(boat);

        // Create a user to assign via Users API
        var userCreate = new boat_share.DTOs.UserCreateDTO
        {
            Email = "assign@user.com",
            Name = "Assign User",
            Password = "password123",
            BoatId = boat!.BoatId,
            Role = "Member"
        };
        var userCreateResp = await Client.PostAsJsonAsync("/api/Users", userCreate);
        Assert.AreEqual(HttpStatusCode.Created, userCreateResp.StatusCode);
        var userInfo = await userCreateResp.Content.ReadFromJsonAsync<boat_share.DTOs.UserInfoDTO>();
        Assert.IsNotNull(userInfo);

        // Assign (even though boat is already set, this exercises endpoint)
        var assignResp = await Client.PostAsync($"/api/Boats/{boat.BoatId}/assign/{userInfo!.UserId}", content: null);
        Assert.AreEqual(HttpStatusCode.OK, assignResp.StatusCode);
    }
}
