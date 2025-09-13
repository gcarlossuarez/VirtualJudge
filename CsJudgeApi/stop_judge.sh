#!/bin/bash
# ===============================
# Script para apagar CsJudgeApi + ngrok
# ===============================

PUERTO=5000

echo "=== 🛑 Apagando Juez Online ==="

# Buscar backend (dotnet run en puerto 5000)
PID_BACKEND=$(lsof -t -i:$PUERTO)
if [ ! -z "$PID_BACKEND" ]; then
  echo "⚠️ Matando backend en puerto $PUERTO (PID $PID_BACKEND)..."
  kill -9 $PID_BACKEND
else
  echo "✅ No hay backend corriendo en puerto $PUERTO"
fi

# Buscar ngrok
PID_NGROK=$(pgrep -f "ngrok http $PUERTO")
if [ ! -z "$PID_NGROK" ]; then
  echo "⚠️ Matando ngrok (PID $PID_NGROK)..."
  kill -9 $PID_NGROK
else
  echo "✅ No hay ngrok corriendo"
fi

echo "=== ✅ Juez apagado ==="

