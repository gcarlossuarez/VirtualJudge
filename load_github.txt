#!/bin/bash
# Script para inicializar y subir VirtualJudge a GitHub

git init
git branch -M main
git remote add origin https://github.com/TU_USUARIO/VirtualJudge.git

git config --global user.name "German Carlos Suarez"
git config --global user.email "TU_CORREO_DE_GITHUB"

cat <<EOF > .gitignore
bin/
obj/
*.dll
*.exe
__pycache__/
*.pyc
*.sqlite
EOF

git add .
git commit -m "Versión inicial del Juez Virtual"
git push -u origin main
