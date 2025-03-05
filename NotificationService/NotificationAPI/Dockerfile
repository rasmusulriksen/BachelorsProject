# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# COPY
COPY ["./NotificationAPI.csproj", "./"]
RUN dotnet restore "./NotificationAPI.csproj"

COPY . .

WORKDIR "/app/"
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationAPI.dll"]