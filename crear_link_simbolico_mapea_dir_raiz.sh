#!/bin/bash

# Borra el enlace simbólico si existe
sudo rm -rf /home/vboxuser/VirtualJudge

# Crea el enlace simbólico
sudo ln -s /home/docente/VirtualJudge/VirtualJudge /home/vboxuser/VirtualJudge

# Verificar que sí sea un link simbolico (symlink)
ls -l /home/vboxuser
# Deberia salir algo como:
#VirtualJudge -> /home/docente/VirtualJudge/VirtualJudge


# Verificar
echo "Verificando link simbólico (symlink)"
ls /home/vboxuser/VirtualJudge/Utilitarios
