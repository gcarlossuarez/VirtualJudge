#!/bin/bash
# cleanup_docker.sh
# Script para limpiar imágenes, contenedores, redes y volúmenes no utilizados de Docker
# Útil para liberar espacio en disco periódicamente

# Listar espacio en disco 
echo "Espacio disonible en disco, antes de a la limpieza"
df -h

# Limpiar imágenes, contenedores y redes no utilizados
# -a: elimina todas las imágenes no referenciadas
# -f: fuerza la eliminación sin pedir confirmación
docker system prune -a -f

# Limpiar volúmenes no utilizados
# -f: fuerza la eliminación sin pedir confirmación
docker volume prune -f

# Listar espacio en disco 
echo "Espacio disonible en disco, después de la limpieza"
df -h

echo "Limpieza de Docker completada."
