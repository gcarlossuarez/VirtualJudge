# RESUMEN DE LA FUNCIONALIDAD
# Parte de la imagen oficial dotnet/sdk:8.0.
# Configura variables de entorno para evitar telemetría.
# Crea un usuario sin privilegios (sandbox).
# Instala time para medir ejecución.
# Prepara un template .NET Console para que las builds posteriores sean rápidas (ya 
# cacheadas).
# Copia el script run_single.sh que será el que compila y corre el programa del estudiante.
# Cambia al usuario sandbox y usa ese script como ENTRYPOINT.


# NOTA . - La imagen se construye una sola vez y luego, por cada petición de un estudiante, se crea un contenedor nuevo
# a partir de esa imagen. Cada contenedor es como una “mini máquina virtual” (más ligera y rápida), con los paquetes y 
# configuraciones definidos en este Dockerfile.
#
# Esto permite que cada ejecución sea aislada, segura y con el mismo entorno reproducible, sin afectar a otros procesos ni 
# al sistema principal. Cuando termina la ejecución, el contenedor se puede eliminar, garantizando limpieza y seguridad.


# Usa la imagen oficial de .NET SDK 8.0, que incluye herramientas para compilar y ejecutar aplicaciones .NET.
FROM mcr.microsoft.com/dotnet/sdk:8.0


# Desactiva telemetría, mensajes de bienvenida y publicidad para que el entorno sea más limpio y rápido.
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_CLI_WORKLOAD_UPDATE_ADVERTISING_DISABLED=1


# Crea un usuario sin privilegios llamado sandbox con UID/GID fijo (1655) y establece su carpeta de trabajo.
#RUN useradd -ms /bin/bash sandbox
# Crear usuario con UID/GID fijos (sandbox:1655)
RUN groupadd -g 1655 sandbox && \
    useradd -u 1655 -g 1655 -m -s /bin/bash sandbox
WORKDIR /home/sandbox


# Instala utilidades necesarias para medir tiempo (time), compilar C++ (g++), ejecutar Python (python3) y Java (openjdk-17-jdk).
RUN apt-get update && apt-get install -y --no-install-recommends \
      time \
      g++ \
      python3 \
      openjdk-17-jdk \
 && rm -rf /var/lib/apt/lists/*


# Crea y compila un proyecto de consola .NET de ejemplo para que las futuras compilaciones sean más rápidas (cacheadas).
# === Crear plantilla base restaurada (evita errores NETSDK1004) ===
RUN dotnet new console -n App -o /home/sandbox/template/App --force \
 && dotnet restore /home/sandbox/template/App \
 && dotnet build /home/sandbox/template/App -c Release --no-restore
 
# La creación del template ValidatorApp ya no es necesaria porque el validador se compila fuera del contenedor y solo se monta el DLL.
# Por compatibilidad y limpieza, se comenta la sección siguiente:
# RUN dotnet new console -n ValidatorApp -o /home/sandbox/template/ValidatorApp --force --no-restore
# RUN dotnet restore /home/sandbox/template/ValidatorApp/ValidatorApp.csproj --no-cache --force-evaluate --disable-parallel --ignore-failed-sources
# RUN dotnet build /home/sandbox/template/ValidatorApp/ValidatorApp.csproj -c Release --no-restore


# Crea carpetas para archivos temporales, entrada, salida y validación, y asigna propiedad al usuario sandbox.
RUN mkdir -p /home/sandbox/tmp \
    /home/sandbox/in/IN \
    /home/sandbox/in/OUT \
    /home/sandbox/in/VALIDATOR \
 && chown -R sandbox:sandbox /home/sandbox

# Copia el script que compila y ejecuta el código del estudiante, ajustando permisos para que el usuario sandbox pueda ejecutarlo.
COPY run_single.sh /home/sandbox/run_single.sh
RUN chown sandbox:sandbox /home/sandbox/run_single.sh \
 && chmod 755 /home/sandbox/run_single.sh


# Asegura que el usuario sandbox tenga acceso total a los templates.
RUN chown -R sandbox:sandbox /home/sandbox/template


# Ejecuta el contenedor como el usuario sin privilegios y define el script como punto de entrada.
USER sandbox
ENTRYPOINT ["/home/sandbox/run_single.sh"]

