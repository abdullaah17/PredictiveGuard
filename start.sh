#!/bin/bash
set -e

# Start the API backend in the background on port 5000
echo "Starting PredictiveGuard API..."
cd /app/api
dotnet PredictiveGuard.API.dll --urls "http://127.0.0.1:5000" &

# Wait a moment for the API and Database seeder to initialize
sleep 3

# Start the Web Dashboard in the foreground on the assigned PORT (Render provides this)
PORT=${PORT:-80}
echo "Starting PredictiveGuard Web on port $PORT..."
cd /app/web
dotnet PredictiveGuard.Web.dll --urls "http://0.0.0.0:${PORT}"
