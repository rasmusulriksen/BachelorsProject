# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Install debugger
RUN apt-get update \
    && apt-get install -y unzip \
    && curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l /vsdbg

# Copy the project file
COPY ["./NotificationAPI.csproj", "./"]

# Restore the dependencies
RUN dotnet restore "./NotificationAPI.csproj"

# Copy the entire source
COPY . .

# Build the project
WORKDIR "/app"
RUN dotnet publish -c Debug -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /vsdbg /vsdbg
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationAPI.dll"]