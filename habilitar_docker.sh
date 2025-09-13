# 1) Verifica que Docker esté activo (solo por si acaso)
sudo systemctl status docker --no-pager

# 2) Asegura que exista el grupo 'docker' (normalmente ya existe)
sudo groupadd docker 2>/dev/null || true

# 3) Agrega tu usuario actual (vboxuser) al grupo docker
sudo usermod -aG docker $USER

# 4) Aplica la pertenencia al grupo SIN reiniciar sesión
newgrp docker

# 5) Prueba
docker run hello-world

