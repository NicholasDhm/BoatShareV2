using System.Net;
using System.Net.Http.Json;
using boat_share.DTOs;
using boat_shareTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using boat_share.Data;

namespace boat_shareTests.Reservations;

[TestClass]
public class ReservationsIntegrationTests : IntegrationTestBase
{
    [TestMethod]
    public async Task Member_Can_Create_Update_Confirm_And_Delete_Reservation()
    {
        using var scope = Factory.Services.CreateScope();
        var (boat, user) = SeedBoatAndUser(scope);

        SetTestUser(user.UserId, role: "Member", email: user.Email, name: user.Name);

        var start = DateTime.UtcNow.AddDays(10).Date.AddHours(9);
        var end = start.AddHours(2);

        // Create
        var create = new CreateReservationDTO
        {
            BoatId = boat.BoatId,
            StartTime = start,
            EndTime = end,
            ReservationType = "Standard",
            Notes = "Morning trip"
        };
        var createResp = await Client.PostAsJsonAsync("/api/Reservations", create);
        Assert.AreEqual(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.Content.ReadFromJsonAsync<ReservationResponseDTO>();
        Assert.IsNotNull(created);

        // Get by id
        var getResp = await Client.GetAsync($"/api/Reservations/{created!.ReservationId}");
        Assert.AreEqual(HttpStatusCode.OK, getResp.StatusCode);

        // Update
        var dto = new ReservationDTO
        {
            ReservationId = created.ReservationId,
            UserId = user.UserId,
            BoatId = boat.BoatId,
            StartTime = start,
            EndTime = end.AddHours(1),
            ReservationType = "Standard",
            Status = created.Status,
            Notes = "Extended"
        };
        var updateResp = await Client.PutAsJsonAsync($"/api/Reservations/{created.ReservationId}", dto);
        Assert.AreEqual(HttpStatusCode.OK, updateResp.StatusCode);

        // Confirm
        var confirmResp = await Client.PutAsJsonAsync("/api/Reservations/confirm-reservation", dto);
        Assert.AreEqual(HttpStatusCode.OK, confirmResp.StatusCode);
        var confirmed = await confirmResp.Content.ReadFromJsonAsync<ReservationResponseDTO>();
        Assert.AreEqual("Confirmed", confirmed!.Status);

        // Update statuses job
        var jobResp = await Client.PutAsync("/api/Reservations/update-reservations", content: null);
        Assert.AreEqual(HttpStatusCode.OK, jobResp.StatusCode);

    // Get by user
    var listByUser = await Client.GetAsync($"/api/Reservations/user/{user.UserId}");
    Assert.AreEqual(HttpStatusCode.OK, listByUser.StatusCode);

    // Get by boat
    var listByBoat = await Client.GetAsync($"/api/Reservations/boat/{boat.BoatId}");
    Assert.AreEqual(HttpStatusCode.OK, listByBoat.StatusCode);

    // Get by date and boat
    var dateResp = await Client.GetAsync($"/api/Reservations/by-date-and-boatId?day={start.Day}&month={start.Month}&year={start.Year}&boatId={boat.BoatId}");
    Assert.AreEqual(HttpStatusCode.OK, dateResp.StatusCode);

        // Delete
        var delResp = await Client.DeleteAsync($"/api/Reservations/{created.ReservationId}");
        Assert.AreEqual(HttpStatusCode.NoContent, delResp.StatusCode);
    }
}
