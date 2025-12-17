# RESUMEN DE LA FUNCIONALIDAD
# Parte de la imagen oficial dotnet/sdk:8.0.
# Configura variables de entorno para evitar telemetría.
# Crea un usuario sin privilegios (sandbox).
# Instala time para medir ejecución.
# Prepara un template .NET Console para que las builds posteriores sean rápidas (ya 
# cacheadas).
# Copia el script run_single.sh que será el que compila y corre el programa del estudiante.
# Cambia al usuario sandbox y usa ese script como ENTRYPOINT.

FROM mcr.microsoft.com/dotnet/sdk:8.0

ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_CLI_WORKLOAD_UPDATE_ADVERTISING_DISABLED=1

# Crear usuario y carpeta home
#RUN useradd -ms /bin/bash sandbox
# Crear usuario con UID/GID fijos (sandbox:1655)
RUN groupadd -g 1655 sandbox && \
    useradd -u 1655 -g 1655 -m -s /bin/bash sandbox
WORKDIR /home/sandbox

# Instalar dependencias necesarias
RUN apt-get update && apt-get install -y --no-install-recommends \
      time \
      g++ \
      python3 \
      openjdk-17-jdk \
 && rm -rf /var/lib/apt/lists/*

# === Crear plantilla base restaurada (evita errores NETSDK1004) ===
RUN dotnet new console -n App -o /home/sandbox/template/App --force \
 && dotnet restore /home/sandbox/template/App \
 && dotnet build /home/sandbox/template/App -c Release --no-restore
#ENV NUGET_PACKAGES=/root/.nuget/packages
#RUN dotnet new console -n App -o /home/sandbox/template/App --force \
# && dotnet add /home/sandbox/template/App package Microsoft.CodeAnalysis.CSharp --version 4.8.0 \
# && dotnet restore /home/sandbox/template/App \
# && dotnet build /home/sandbox/template/App -c Release --no-restore



# Crear carpetas necesarias y asignar propiedad
RUN mkdir -p /home/sandbox/tmp \
    /home/sandbox/in/IN \
    /home/sandbox/in/OUT \
    /home/sandbox/in/VALIDATOR \
 && chown -R sandbox:sandbox /home/sandbox

# Copiar runner y ajustar permisos
COPY run_single.sh /home/sandbox/run_single.sh
RUN chown sandbox:sandbox /home/sandbox/run_single.sh \
 && chmod 755 /home/sandbox/run_single.sh

# Cambiar usuario final
USER sandbox

ENTRYPOINT ["/home/sandbox/run_single.sh"]

