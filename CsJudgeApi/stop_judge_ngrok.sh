#!/bin/bash
# ===============================
# Script para apagar CsJudgeApi + ngrok
# MEJORADO: Shutdown graceful antes de forzar
# ===============================

PUERTO=5000
TIMEOUT=10

echo "=== 🛑 Apagando Juez Online ==="

# =====================================================
# 1️⃣ BACKEND: Shutdown graceful con timeout
# =====================================================
echo "\n📡 Solicitando shutdown graceful del backend en puerto $PUERTO..."
PID_BACKEND=$(lsof -t -i:$PUERTO 2>/dev/null)

if [ ! -z "$PID_BACKEND" ]; then
  echo "📡 Enviando SIGTERM a proceso $PID_BACKEND (tiempo máximo: ${TIMEOUT}s)..."
  kill -15 $PID_BACKEND 2>/dev/null
  
  # Esperar TIMEOUT segundos para que cierre elegantemente
  for i in $(seq 1 $TIMEOUT); do
    if ! kill -0 $PID_BACKEND 2>/dev/null; then
      echo "✅ Backend cerró gracefully en ${i}s"
      break
    fi
    echo "⏳ Esperando shutdown... ($i/$TIMEOUT)"
    sleep 1
  done
  
  # Si sigue corriendo después del timeout, ENTONCES forzar (SIGKILL)
  if kill -0 $PID_BACKEND 2>/dev/null; then
    echo "⚠️  Backend no respondió. Forzando cierre (SIGKILL)..."
    kill -9 $PID_BACKEND 2>/dev/null
    sleep 1
  fi
else
  echo "✅ No hay backend corriendo en puerto $PUERTO"
fi

# =====================================================
# 2️⃣ NGROK: Shutdown graceful
# =====================================================
echo "\n🛑 Deteniendo ngrok..."
PID_NGROK=$(pgrep -f "ngrok http $PUERTO" 2>/dev/null)

if [ ! -z "$PID_NGROK" ]; then
  echo "📡 Enviando SIGTERM a ngrok (PID $PID_NGROK)..."
  kill -15 $PID_NGROK 2>/dev/null
  sleep 2
  
  # Si sigue corriendo, forzar
  if kill -0 $PID_NGROK 2>/dev/null; then
    echo "⚠️  Forzando cierre de ngrok..."
    kill -9 $PID_NGROK 2>/dev/null
  else
    echo "✅ ngrok cerró gracefully"
  fi
else
  echo "✅ No hay ngrok corriendo"
fi

echo "\n=== ✅ Juez apagado ==="

