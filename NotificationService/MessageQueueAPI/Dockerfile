# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy Common project with all its build files
COPY ["Common/Directory.Build.props", "Common/"]
COPY ["Common/Directory.Build.allprojects.targets", "Common/"]
COPY ["Common/stylecop.json", "Common/"]
COPY ["Common/stylecop.ruleset", "Common/"]
COPY ["Common/", "Common/"]

# Copy the MessageQueueAPI project
COPY ["BachelorsProject/NotificationService/MessageQueueAPI/", "BachelorsProject/NotificationService/MessageQueueAPI/"]

# Restore and build
RUN dotnet restore "BachelorsProject/NotificationService/MessageQueueAPI/MessageQueueAPI.csproj"
RUN dotnet publish "BachelorsProject/NotificationService/MessageQueueAPI/MessageQueueAPI.csproj" -c Debug -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Install debugger
RUN apt-get update \
    && apt-get install -y --no-install-recommends unzip procps curl \
    && rm -rf /var/lib/apt/lists/* \
    && curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l /vsdbg

ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 8080
EXPOSE 2222

ENTRYPOINT ["dotnet", "MessageQueueAPI.dll"]