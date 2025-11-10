#!/bin/bash
# ======================================================
# 🐳 Instalador automatizado de Docker CE en Ubuntu
# Compatible con Ubuntu 20.04, 22.04, 24.04
# ======================================================

set -e

echo "=== 🚀 Actualizando sistema y preparando dependencias ==="
sudo apt update -y
sudo apt install -y ca-certificates curl gnupg lsb-release

echo "=== 🔐 Agregando clave GPG de Docker ==="
sudo mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

echo "=== 📦 Agregando repositorio oficial de Docker ==="
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] \
  https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

echo "=== ⏬ Instalando Docker CE y utilitarios ==="
sudo apt update -y
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

echo "=== 🔧 Habilitando y arrancando servicio Docker ==="
sudo systemctl enable docker
sudo systemctl start docker

echo "=== 🧪 Verificando estado del servicio ==="
sudo systemctl status docker --no-pager || true

echo "=== 👥 Agregando usuario actual al grupo docker ==="
sudo usermod -aG docker $USER

echo
echo "✅ Instalación completada. Reinicia la sesión o ejecuta: newgrp docker"
echo "Luego prueba con: docker run hello-world"
echo "======================================================"

