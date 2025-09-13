# Resumen de Actividades y Pasos para Configurar Ubuntu en VirtualBox

## Contexto
- Se configuró una máquina virtual (VM) con Ubuntu en VirtualBox en una laptop con Windows.
- El objetivo fue habilitar copiar/pegar texto, arrastrar/soltar archivos y compartir carpetas entre Windows (host) y Ubuntu (invitado).

## Pasos Realizados

### 1. Configuración Inicial
- Verificamos que el copiar/pegar texto ya estaba habilitado.
- Intentamos habilitar arrastrar/soltar y compartir archivos, enfrentando errores como `Drag and drop to guest not possible` (código `VBOX_E_DND_ERROR`).

### 2. Instalación de Guest Additions
- Intentamos instalar las Guest Additions con `sudo sh ./VBox_GAs_*.sh`, pero el archivo no existía.
- Corregimos el comando usando `sudo sh ./VBoxLinuxAdditions.run` tras identificar el archivo correcto en `/mnt/cdrom`.
- Instalamos dependencias necesarias: `build-essential`, `dkms`, y `linux-headers-$(uname -r)`.
- Reiniciamos la VM para aplicar los cambios.

### 3. Diagnóstico de Problemas
- Verificamos módulos cargados con `lsmod | grep vbox`, inicialmente solo `vboxguest` estaba presente.
- Identificamos que faltaba `vboxsf` para carpetas compartidas y soporte completo de arrastrar/soltar.
- Reinstalamos Guest Additions para asegurar compatibilidad y soporte completo.

### 4. Habilitación de Arrastrar y Soltar
- Configuramos en VirtualBox: **Dispositivos > Arrastrar y Soltar > Bidireccional**.
- Resolvimos errores revisando compatibilidad de versiones y reinstalando Guest Additions.

### 5. Configuración de Carpetas Compartidas
- Añadimos una carpeta compartida (e.g., `Compartido_Con_Virtual_Machine`) en **Configuración > Carpetas Compartidas**, con **Montar automáticamente** y **Hacer permanente**.
- Verificamos montaje con `df -h`, confirmando que aparecía en `/media/sf_Compartido_Con_Virtual_Machine`.
- Solucionamos el error `Permission denied` añadiendo el usuario al grupo `vboxsf` con `sudo usermod -aG vboxsf $USER` y reiniciando.

### 6. Resolución de Permisos
- Ajustamos permisos manualmente con `sudo mount -t vboxsf -o uid=$(id -u $USER),gid=$(id -g $USER) Compartido_Con_Virtual_Machine /media/sf_Compartido_Con_Virtual_Machine`.
- Opcionalmente, configuramos `/etc/fstab` para montaje permanente con permisos personalizados.

### 7. Verificación Final
- Confirmamos acceso a la carpeta compartida y visualización de archivos.
- Probamos transferencia bidireccional entre Windows y Ubuntu.

## Tareas Pendientes o Verificación
- **Verificar módulos**: Ejecutar `lsmod | grep vbox` para confirmar que `vboxsf` está cargado.
- **Comprobar versión**: Verificar versión de VirtualBox (`vboxmanage --version`) y Ubuntu (`lsb_release -a`) para compatibilidad.
- **Probar funcionalidad**: 
  - Arrastrar/soltar archivos entre host e invitado.
  - Crear/editar archivos en `/media/sf_Compartido_Con_Virtual_Machine` y verificar en Windows.
- **Optimizar montaje**: Asegurarse de que `/etc/fstab` esté configurado si se desea montaje automático con permisos correctos.
- **Documentar errores**: Revisar logs (`/var/log/vboxadd-setup.log`) si surgen problemas futuros.

## Notas Adicionales
- Mantener VirtualBox y Guest Additions actualizados.
- Usar carpetas compartidas como alternativa si arrastrar/soltar no funciona.
- Asegurarse de tener un entorno gráfico (e.g., GNOME) para funcionalidades completas.