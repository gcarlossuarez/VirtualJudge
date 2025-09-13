#!/bin/bash
# ===============================
# Dashboard de monitoreo del Juez Online
# ===============================

echo "=== 🚦 Monitoreo del Juez Online ==="
echo "Presiona Ctrl+C para salir"
echo

while true; do
    clear
    echo "=== 📊 Recursos Generales ==="
    # Uso general de CPU y RAM
    free -h
    echo
    echo "=== 🔎 Top 5 procesos por RAM ==="
    ps -eo pid,user,%mem,%cpu,command --sort=-%mem | head -n 6
    echo
    echo "=== 🔎 Top 5 procesos por CPU ==="
    ps -eo pid,user,%cpu,%mem,command --sort=-%cpu | head -n 6
    echo
    echo "=== 🐳 Contenedores Docker ==="
    docker stats --no-stream --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}"
    echo
    sleep 5
done

