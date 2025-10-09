# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy built app from build stage
COPY --from=build /app/out .

# Expose port (Railway will set PORT env variable)
EXPOSE 8080

# Run the app
ENTRYPOINT ["dotnet", "boat-share.dll"]
