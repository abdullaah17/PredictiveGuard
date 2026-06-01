# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for caching restore
COPY PredictiveGuard.slnx .
COPY PredictiveGuard.API/PredictiveGuard.API.csproj PredictiveGuard.API/
COPY PredictiveGuard.Web/PredictiveGuard.Web.csproj PredictiveGuard.Web/
COPY PredictiveGuard.Data/PredictiveGuard.Data.csproj PredictiveGuard.Data/

# Restore dependencies
RUN dotnet restore PredictiveGuard.slnx

# Copy the rest of the source code
COPY . .

# Publish API
WORKDIR /src/PredictiveGuard.API
RUN dotnet publish -c Release -o /app/publish/api

# Publish Web
WORKDIR /src/PredictiveGuard.Web
RUN dotnet publish -c Release -o /app/publish/web

# Final Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create a data directory for the shared SQLite database
RUN mkdir -p /app/data

# Copy published binaries to separate folders
COPY --from=build /app/publish/api /app/api
COPY --from=build /app/publish/web /app/web

# Copy and set up the startup script
COPY start.sh /app/
RUN chmod +x /app/start.sh

# The standard port Render will provide via $PORT
EXPOSE 80

# Entrypoint is the unified script
ENTRYPOINT ["/app/start.sh"]
