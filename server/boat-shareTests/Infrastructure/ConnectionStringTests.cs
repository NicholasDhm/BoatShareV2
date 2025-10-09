using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace boat_share.Tests.Infrastructure;

[TestClass]
public class ConnectionStringTests
{
    [TestMethod]
    [DataRow(
        "postgresql://postgres:password123@localhost:5432/testdb",
        "Host=localhost;Port=5432;Database=testdb;Username=postgres;Password=password123;SSL Mode=Prefer;Trust Server Certificate=true")]
    [DataRow(
        "postgres://user:pass@db.example.com:5433/myapp",
        "Host=db.example.com;Port=5433;Database=myapp;Username=user;Password=pass;SSL Mode=Prefer;Trust Server Certificate=true")]
    [DataRow(
        "postgresql://postgres:crJzCVfJdhKuEHlrggifuzEOyhyihFqb@postgres-yrxp.railway.internal:5432/railway",
        "Host=postgres-yrxp.railway.internal;Port=5432;Database=railway;Username=postgres;Password=crJzCVfJdhKuEHlrggifuzEOyhyihFqb;SSL Mode=Prefer;Trust Server Certificate=true")]
    public void ConvertPostgresUrlToConnectionString_ValidUrl_ReturnsCorrectConnectionString(string postgresUrl, string expected)
    {
        // Act
        var result = ConvertPostgresUrlToConnectionString(postgresUrl);

        // Assert
        Assert.AreEqual(expected, result);
    }

    // Copy of the helper method from Program.cs for testing
    private static string ConvertPostgresUrlToConnectionString(string postgresUrl)
    {
        var uri = new Uri(postgresUrl);
        var userInfo = uri.UserInfo.Split(':');

        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');
        var username = userInfo.Length > 0 ? userInfo[0] : "";
        var password = userInfo.Length > 1 ? userInfo[1] : "";

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Prefer;Trust Server Certificate=true";
    }
}
