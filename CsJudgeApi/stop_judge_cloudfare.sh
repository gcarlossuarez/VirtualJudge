#!/bin/bash
# ===============================
# Script para apagar CsJudgeApi + ngrok
# ===============================
echo "🛑 Deteniendo Cloudflare Tunnel..."
sudo pkill cloudflared

echo "🛑 Deteniendo CsJudgeApi / procesos dotnet..."
sudo pkill -f CsJudgeApi.dll
sudo pkill dotnet

echo "✔ Todo detenido correctamente."

