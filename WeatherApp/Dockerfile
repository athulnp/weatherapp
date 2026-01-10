# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy everything and restore dependencies
COPY . .
RUN dotnet restore "WeatherApp.csproj"

# Build and publish app
RUN dotnet publish "WeatherApp.csproj" -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose port 80 for HTTP traffic
EXPOSE 80

# Entry point
ENTRYPOINT ["dotnet", "WeatherApp.dll"]
