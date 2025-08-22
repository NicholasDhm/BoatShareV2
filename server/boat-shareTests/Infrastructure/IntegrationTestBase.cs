using System.Net.Http.Headers;
using boat_share.Data;
using boat_share.Models;
using Microsoft.Extensions.DependencyInjection;

namespace boat_shareTests.Infrastructure;

public abstract class IntegrationTestBase : IDisposable
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase()
    {
        Factory = new CustomWebApplicationFactory();
        Client = Factory.CreateClient();
        // Default test auth header (scheme can be anything)
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestAuth");
    }

    protected void SetTestUser(int userId, string role = "Admin", string email = "test@test.com", string name = "Test User")
    {
        Client.DefaultRequestHeaders.Remove("X-Test-UserId");
        Client.DefaultRequestHeaders.Remove("X-Test-Role");
        Client.DefaultRequestHeaders.Remove("X-Test-Email");
        Client.DefaultRequestHeaders.Remove("X-Test-Name");

        Client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        Client.DefaultRequestHeaders.Add("X-Test-Role", role);
        Client.DefaultRequestHeaders.Add("X-Test-Email", email);
        Client.DefaultRequestHeaders.Add("X-Test-Name", name);
    }

    protected (Boat boat, User user) SeedBoatAndUser(IServiceScope? externalScope = null, string role = "Admin")
    {
        using var scope = externalScope ?? Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var boat = new Boat
        {
            Name = "Test Boat",
            Type = "Sailboat",
            Capacity = 6,
            HourlyRate = 100,
            IsActive = true
        };
        db.Boats.Add(boat);
        db.SaveChanges();

        var user = new User
        {
            Email = "test@test.com",
            Name = "Test User",
            Role = role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("test"),
            BoatId = boat.BoatId,
            StandardQuota = 10,
            SubstitutionQuota = 5,
            ContingencyQuota = 3,
            IsActive = true
        };
        db.Users.Add(user);
        db.SaveChanges();

        return (boat, user);
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
    }
}
