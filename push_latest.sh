#!/usr/bin/bash
# ==========================================
# Script: push_latest.sh
# Autor:  VirtualJudge DevOps
# Descripción:
#   Realiza un commit automático con fecha/hora y lo sube al repo remoto.
# ==========================================

cd "$(dirname "$0")" || exit 1

# Verificar que sea un repo Git
if ! git rev-parse --is-inside-work-tree >/dev/null 2>&1; then
  echo "❌ Este directorio no es un repositorio Git."
  exit 1
fi

# Mostrar cambios
echo "🔍 Verificando cambios pendientes..."
git status --short

# Si no hay cambios, salir
if git diff --quiet && git diff --cached --quiet; then
  echo "✅ No hay cambios nuevos para subir."
  exit 0
fi

# Agregar todo
echo "🧩 Agregando cambios..."
git add .

# Crear commit con timestamp
MSG="Auto-commit $(date '+%Y-%m-%d %H:%M:%S')"
echo "📝 Commit: $MSG"
git commit -m "$MSG"

# Subir a GitHub
echo "🚀 Subiendo a GitHub..."
if git push origin main; then
  echo "✅ Subida completada correctamente."
else
  echo "⚠️ Error al subir a GitHub. Verifica tu token o conexión."
fi

