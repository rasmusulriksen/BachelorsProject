# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy Common project with all its build files
COPY ["Common/Directory.Build.props", "Common/"]
COPY ["Common/Directory.Build.allprojects.targets", "Common/"]
COPY ["Common/stylecop.json", "Common/"]
COPY ["Common/stylecop.ruleset", "Common/"]
COPY ["Common/", "Common/"]

# Copy the NotificationAPI project
COPY ["BachelorsProject/NotificationService/NotificationAPI/", "BachelorsProject/NotificationService/NotificationAPI/"]

# Restore and build
RUN dotnet restore "BachelorsProject/NotificationService/NotificationAPI/NotificationAPI.csproj"
RUN dotnet publish "BachelorsProject/NotificationService/NotificationAPI/NotificationAPI.csproj" -c Debug -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationAPI.dll"]