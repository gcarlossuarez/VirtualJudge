#!/bin/bash
set -e

echo "🚀 Building VirtualJudge images..."
docker compose build

echo "📦 Starting VirtualJudge containers..."
docker compose up -d

echo "🔍 Checking running containers..."
docker ps --filter "name=virtualjudge"

echo
echo "✅ VirtualJudge is up and running!"
echo "🌐 Access the backend at: http://localhost:8080"

