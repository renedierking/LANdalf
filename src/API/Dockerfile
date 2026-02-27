# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080         
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Zuerst nur die Projektdatei kopieren und restore ausführen (Cache-freundlich)
COPY ["API.csproj", "./"]
RUN dotnet restore "API.csproj"

# Dann den Rest des Projekts kopieren (inkl. Program.cs usw.)
COPY . .

# Build
RUN dotnet build "API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN mkdir -p /app/LANdalf_Data && chown -R $APP_UID /app/LANdalf_Data
USER $APP_UID
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]
