#!/bin/bash
# ==========================================================
# 🚀 Iniciar CsJudgeApi + Cloudflare Tunnel (Quick Tunnel)
# ==========================================================

PUERTO=${1:-5000}
API_DLL="/home/virtualbox/VirtualJudge/CsJudgeApi/bin/Debug/net8.0/CsJudgeApi.dll"
LOG_CF="/tmp/cloudflare.log"
INDEX_HTML="/home/virtualbox/VirtualJudge/CsJudgeApi/wwwroot/index.html"

echo "=== 🚀 Iniciando Juez Virtual en puerto $PUERTO ==="

# 1) Limpiar procesos previos
echo "🧹 Limpiando procesos previos..."
sudo pkill cloudflared 2>/dev/null
sudo pkill -f "CsJudgeApi.dll" 2>/dev/null

# 2) Iniciar API
echo "🟢 Iniciando CsJudgeApi..."
sudo dotnet "$API_DLL" --urls "http://0.0.0.0:$PUERTO" &
PID_API=$!
sleep 3

if ! ps -p $PID_API >/dev/null; then
  echo "❌ La API falló al iniciar"
  exit 1
fi

echo "✅ CsJudgeApi corriendo con PID $PID_API"

# 3) Iniciar Cloudflare Tunnel EN BACKGROUND y CAPTURAR LOG
echo "🌐 Iniciando Cloudflare Tunnel..."
sudo rm -f "$LOG_CF"

cloudflared tunnel --protocol http2 --url "http://localhost:$PUERTO" 2>&1 | tee "$LOG_CF" &
PID_TUNNEL=$!

# 4) Extraer URL
echo "⏳ Esperando URL del túnel..."
URL=""
for i in {1..20}; do
    sleep 1
    URL=$(grep -Eo "https://[a-zA-Z0-9.-]+\.trycloudflare\.com" "$LOG_CF" | head -n 1)

    if [[ -n "$URL" ]]; then
        break
    fi
done

if [[ -z "$URL" ]]; then
    echo "❌ No se pudo detectar URL. Revisa: $LOG_CF"
    exit 1
fi

# 5) Actualizar index.html (excepto SANDBOX_DOWNLOAD_URL que es de Google Drive)
if [[ -f "$INDEX_HTML" ]]; then
    echo "🔧 Actualizando index.html con: $URL"

    # Reemplazar SERVER_BASE_URL pero NO la línea que contiene SANDBOX_DOWNLOAD_URL (Google Drive)
    sudo sed -i "/SANDBOX_DOWNLOAD_URL/!s|const SERVER_BASE_URL = \"http[s]*://[^\"]*\"|const SERVER_BASE_URL = \"$URL\"|g" "$INDEX_HTML"
    sudo sed -i "s|http://localhost:$PUERTO/compile-run|$URL/compile-run|g" "$INDEX_HTML"

    echo "✅ index.html actualizado (SANDBOX_DOWNLOAD_URL preservado - Google Drive)."
fi

# 6) Mostrar URL final
echo
echo "============================================================="
echo "🌐 URL DEL JUEZ VIRTUAL:"
echo "👉 $URL/index.html"
echo "============================================================="
echo

echo "📡 Para detener todo: ./stop_judge_cloudflare.sh"
echo "ℹ️ Logs del juez y del túnel continúan abajo:"
echo

# 7) Mantener script abierto
wait $PID_API $PID_TUNNEL

