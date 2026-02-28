# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080         
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ENV DOTNET_EnableWriteXorExecute=0
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# First copy only the project file and run restore (cache-friendly)
COPY ["API.csproj", "./"]
RUN dotnet restore "API.csproj"

# Then copy the rest of the project (including Program.cs etc.)
COPY . .

# Build
RUN dotnet build "API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN mkdir -p /app/LANdalf_Data /app/logs \
    && groupadd --system appgroup \
    && useradd --system --no-create-home -g appgroup appuser \
    && chown -R appuser:appgroup /app
COPY --from=publish /app/publish .
USER appuser
ENTRYPOINT ["dotnet", "API.dll"]
