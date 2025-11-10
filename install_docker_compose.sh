#!/bin/bash
set -e

echo "🚀 Updating system..."
sudo apt update -y
sudo apt upgrade -y

echo "📦 Installing prerequisites..."
sudo apt install -y ca-certificates curl gnupg lsb-release apt-transport-https software-properties-common

echo "🔑 Adding Docker’s official GPG key..."
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | \
  sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg

echo "📂 Setting up Docker repository..."
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] \
  https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

echo "🔄 Updating apt with Docker repo..."
sudo apt update -y

echo "🐳 Installing Docker Engine and Compose v2..."
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

echo "👤 Adding current user to docker group..."
sudo usermod -aG docker $USER

echo "✅ Docker and Compose installation finished!"
echo "⚠️ Please log out and log back in (or reboot) for group changes to take effect."
echo
docker --version
docker compose version


# 1️⃣ Descargar la versión más reciente estable
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" \
  -o /usr/local/bin/docker-compose

# 2️⃣ Dar permisos de ejecución
sudo chmod +x /usr/local/bin/docker-compose

# 3️⃣ Verificar
docker-compose version

