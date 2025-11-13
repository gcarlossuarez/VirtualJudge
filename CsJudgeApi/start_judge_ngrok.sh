#!/bin/bash
# ===============================
# Script para levantar CsJudgeApi + ngrok
# ===============================

# Puerto donde corre el juez
PUERTO=${1:-5000}  # Puerto por param, default 5000


# Ruta absoluta de tu index.html
INDEX_HTML="/home/virtualbox/VirtualJudge/CsJudgeApi/wwwroot/index.html"

echo "=== 🚀 Iniciando Juez Online en puerto $PUERTO ==="

# 1) Matar cualquier proceso viejo en ese puerto
PID=$(lsof -t -i:$PUERTO)
if [ ! -z "$PID" ]; then
  echo "⚠️ Puerto $PUERTO ocupado por PID $PID, matando proceso..."
  sudo kill -9 $PID
fi


# 2) Arrancar el backend en segundo plano. Se inicia en modo sudo, para no
# tener problemas si se quiere utilizar como contenedoir a nsjail (investigar
# si se puede hacer de otro modo).

#sudo dotnet run --urls "http://localhost:$PUERTO" &
#BACKEND_PID=$!
#echo "✅ Backend levantado con PID $BACKEND_PID"

# Función para matar proceso en puerto
kill_port() {
    local puerto=$1
    echo "🔍 Chequeando puerto $puerto..."
    local pids=$(lsof -ti:$puerto)  # PIDs usando el puerto (lsof del sistema)
    if [ -n "$pids" ]; then
        echo "⚠️ Puerto $puerto ocupado por PIDs: $pids. Matando..."
        sudo kill -TERM $pids  # Graceful kill (SIGTERM)
        sleep 2  # Espera liberación
        if lsof -ti:$puerto >/dev/null 2>&1; then
            sudo kill -KILL $pids  # Force kill si persiste
        fi
        echo "✅ Puerto liberado."
    else
        echo "✅ Puerto $puerto libre."
    fi
}

# 2.1. Mata proceso anterior
kill_port $PUERTO

# 2.2. Arranca en sudo (como root, nsjail no pide pass)
echo "🚀 Levantando backend en modo sudo (para no tener problemas si se usa njail) puerto $PUERTO..."
sudo dotnet run --urls "http://0.0.0.0:$PUERTO" &
BACKEND_PID=$!

#dotnet run --urls "http://0.0.0.0:$PUERTO" &
BACKEND_PID=$!


# Espera un poco para que arranqeu el backend y hacer el bind
sleep 8

# Chequea si levantó
#if curl -f http://localhost:$PUERTO/health 2>/dev/null || curl -f http://localhost:$PUERTO 2>/dev/null; then
#    echo "✅ Backend levantado con PID $BACKEND_PID en http://localhost:$PUERTO"
#else
#    echo "❌ Falló el bind. Chequea logs."
#    sudo kill $BACKEND_PID
#    exit 1
#fi

# Opcional: Guarda PID para stop futuro
echo $BACKEND_PID > /tmp/juez_backend.pid


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
  # Reemplaza la URL base del servidor (funciona aunque ya haya sido reemplazada antes)
  sed -i "s|const SERVER_BASE_URL = \"http[s]*://[^\"]*\"|const SERVER_BASE_URL = \"$URL\"|g" "$INDEX_HTML"
  
  # Reemplaza la URL de compile-run (si existe el patrón viejo)
  sed -i "s|http://localhost:$PUERTO/compile-run|$URL/compile-run|g" "$INDEX_HTML"
  
  echo "✅ index.html actualizado con la URL pública: $URL"
else
  echo "⚠️ No se encontró $INDEX_HTML, omitiendo actualización."
fi


# 6) Guardar la URL en un archivo (útil para compartir)
echo Url de ngrok="$URL"
echo Url de ngrok="$URL"/index.html para acceder a la pagina principal del sitio
echo "$URL"/index.html > url.txt
echo "📄 URL guardada en url.txt"

echo "=== ✅ Juez Online listo ==="

