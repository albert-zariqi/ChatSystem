# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ChatSystem.Chat.API/ChatSystem.Chat.API.csproj", "ChatSystem.Chat.API/"]
COPY ["ChatSystem.Caching/ChatSystem.Caching.csproj", "ChatSystem.Caching/"]
COPY ["ChatSystem.Chat.Common/ChatSystem.Chat.Common.csproj", "ChatSystem.Chat.Common/"]
COPY ["ChatSystem.Messaging/ChatSystem.Messaging.csproj", "ChatSystem.Messaging/"]
COPY ["ChatSystem.Utils/ChatSystem.Utils.csproj", "ChatSystem.Utils/"]
RUN dotnet restore "./ChatSystem.Chat.API/ChatSystem.Chat.API.csproj"
COPY . .
WORKDIR "/src/ChatSystem.Chat.API"
RUN dotnet build "./ChatSystem.Chat.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ChatSystem.Chat.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatSystem.Chat.API.dll"]