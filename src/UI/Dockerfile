# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project file (when building from src/UI as build context)
COPY ["UI.csproj", "./"]
RUN dotnet restore "./UI.csproj"

# Copy everything and publish
COPY . .
RUN dotnet publish "./UI.csproj" -c Release -o /app/publish

# Runtime stage - nginx
FROM nginx:alpine AS runtime
WORKDIR /etc/nginx/html

# Copy published Blazor WASM files
COPY --from=build /app/publish/wwwroot .
#COPY --from=build /app/publish/ .

# Copy nginx configuration (nginx.conf is at the build context root)
COPY nginx.conf /etc/nginx/nginx.conf

# Stelle sicher, dass nginx Zugriff hat
RUN chown -R nginx:nginx /usr/share/nginx/html || true

EXPOSE 80
ENTRYPOINT ["nginx", "-g", "daemon off;"]
