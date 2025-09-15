#!/bin/bash
# ===============================
# Script para levantar CsJudgeApi + ngrok
# ===============================

# Puerto donde corre el juez
PUERTO=5000

# Ruta absoluta de tu index.html
INDEX_HTML="/home/vboxuser/VirtualJudge/CsJudgeApi/wwwroot/index.html"

echo "=== 🚀 Iniciando Juez Online en puerto $PUERTO ==="

# 1) Matar cualquier proceso viejo en ese puerto
PID=$(lsof -t -i:$PUERTO)
if [ ! -z "$PID" ]; then
  echo "⚠️ Puerto $PUERTO ocupado por PID $PID, matando proceso..."
  kill -9 $PID
fi

# 2) Arrancar el backend en segundo plano
dotnet run --urls "http://localhost:$PUERTO" &
BACKEND_PID=$!
echo "✅ Backend levantado con PID $BACKEND_PID"

# Esperar unos segundos a que arranque
sleep 8

# 3) Levantar ngrok
ngrok http $PUERTO > ngrok.log 2>&1 &
NGROK_PID=$!
echo "✅ ngrok levantado con PID $NGROK_PID"

# Esperar a que ngrok publique la URL
sleep 10

# 4) Obtener la URL pública
URL=$(curl -s http://127.0.0.1:4040/api/tunnels \
      | grep -o 'https://[0-9a-zA-Z.-]*\.ngrok-free.app' \
      | head -n1)

if [ -z "$URL" ]; then
  echo "❌ No se pudo obtener la URL de ngrok"
  exit 1
fi

echo "🌍 URL pública de ngrok: $URL"

# 5) Actualizar index.html para apuntar al backend correcto
if [ -f "$INDEX_HTML" ]; then
  sed -i "s|http://localhost:$PUERTO/compile-run|$URL/compile-run|g" "$INDEX_HTML"
  echo "✅ index.html actualizado con la URL pública"
else
  echo "⚠️ No se encontró $INDEX_HTML, omitiendo actualización."
fi

# 6) Guardar la URL en un archivo (útil para compartir)
echo Url de ngrok="$URL"
echo Url de ngrok="$URL"/index.html para acceder a la pagina principal del sitio
echo "$URL"/index.html > url.txt
echo "📄 URL guardada en url.txt"

echo "=== ✅ Juez Online listo ==="

