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

# Copy nginx config as template (envsubst replaces ${NGINX_PORT} at startup)
COPY nginx.conf /etc/nginx/nginx.conf.template

# Copy entrypoint script
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

RUN chown -R nginx:nginx /usr/share/nginx/html || true

ENV NGINX_PORT=80

EXPOSE 80
ENTRYPOINT ["/docker-entrypoint.sh"]
