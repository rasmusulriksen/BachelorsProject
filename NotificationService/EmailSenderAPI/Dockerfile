FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy Common project with all its build files
COPY ["Common/Directory.Build.props", "Common/"]
COPY ["Common/Directory.Build.allprojects.targets", "Common/"]
COPY ["Common/stylecop.json", "Common/"]
COPY ["Common/stylecop.ruleset", "Common/"]
COPY ["Common/", "Common/"]

# Copy the EmailSenderAPI project
COPY ["BachelorsProject/NotificationService/EmailSenderAPI/", "BachelorsProject/NotificationService/EmailSenderAPI/"]

# Restore and build
RUN dotnet restore "BachelorsProject/NotificationService/EmailSenderAPI/EmailSenderAPI.csproj"
RUN dotnet publish "BachelorsProject/NotificationService/EmailSenderAPI/EmailSenderAPI.csproj" -c Debug -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EmailSenderAPI.dll"]