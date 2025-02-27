FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ["NotificationAPI.csproj", "./"]
RUN dotnet restore "./NotificationAPI.csproj"

COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
COPY ["Config/notification-preferences.json", "./Config/"]
ENTRYPOINT ["dotnet", "NotificationAPI.dll"]
