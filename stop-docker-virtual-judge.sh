#!/bin/bash
set -e

echo "🛑 Stopping VirtualJudge containers..."
docker compose down

echo "✅ VirtualJudge has been stopped and containers removed."

