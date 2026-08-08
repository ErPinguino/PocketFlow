# Use SDK image to build and publish the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["PocketFlow.csproj", "./"]
RUN dotnet restore "./PocketFlow.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "PocketFlow.csproj" -c Release -o /app/publish

# Use ASP.NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# The PORT environment variable is set by Render
# Entrypoint command
ENTRYPOINT ["dotnet", "PocketFlow.dll"]
