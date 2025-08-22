# boat-shareTests

Integration test suite for the ASP.NET Core API. Uses WebApplicationFactory, an in-memory EF Core database in Testing environment, and a fake auth handler.

- Test runner: MSTest
- Target framework: .NET 9.0
- Key folders:
  - Infrastructure: test server factory, fake auth, and shared helpers
  - Auth/Boats/Users/Reservations: endpoint coverage

How to run

```
dotnet test server/boat-shareTests/boat-shareTests.csproj -v minimal
```

Notes
- The API switches to EFCore InMemory when ASPNETCORE_ENVIRONMENT=Testing.
- Fake auth headers you can set per request: X-Test-UserId, X-Test-Role, X-Test-Email, X-Test-Name.
- Legacy placeholder unit tests are marked [Ignore].