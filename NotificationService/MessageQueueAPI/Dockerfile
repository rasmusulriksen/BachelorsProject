# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# This COPY uses the build context of current directory due to above docker-compose change
COPY ["./MessageQueueAPI.csproj", "./"]
RUN dotnet restore "./MessageQueueAPI.csproj"

# Copy the entire source
COPY . .

WORKDIR "/app"
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MessageQueueAPI.dll"]