# Build Frontend (Blazor WASM)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-ui
WORKDIR /src
COPY ["UI/UI.csproj", "UI/"]
RUN dotnet restore "./UI/UI.csproj"
COPY . .
WORKDIR "/src/UI"
RUN dotnet publish "./UI.csproj" -c Release -o /app/publish

# Build Backend (API)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-api
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["API/API.csproj", "API/"]
RUN dotnet restore "./API/API.csproj"
COPY . .
WORKDIR "/src/API"
RUN dotnet build "./API.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "./API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy Backend
COPY --from=build-api /app/publish .

# Copy Frontend to wwwroot (served by the Backend)
COPY --from=build-ui /app/publish/wwwroot ./wwwroot

USER $APP_UID
ENTRYPOINT ["dotnet", "API.dll"]
