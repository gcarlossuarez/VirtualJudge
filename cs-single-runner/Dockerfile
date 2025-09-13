FROM mcr.microsoft.com/dotnet/sdk:8.0

# Evitar experiencias de "first run" y avisos de workloads
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_CLI_WORKLOAD_UPDATE_ADVERTISING_DISABLED=1

# Crear usuario y carpeta
RUN useradd -ms /bin/bash sandbox
WORKDIR /home/sandbox

# Instalar /usr/bin/time (paquete 'time')
RUN apt-get update && apt-get install -y --no-install-recommends time \
 && rm -rf /var/lib/apt/lists/*

# Crear plantilla y restaurar durante la build (con red)
RUN dotnet new console -n App -o /home/sandbox/template/App --force \
 && dotnet restore /home/sandbox/template/App

# Copiar runner y ajustar permisos
COPY run_single.sh /home/sandbox/run_single.sh
RUN chown -R sandbox:sandbox /home/sandbox \
 && chmod 755 /home/sandbox/run_single.sh

# 🔧 FIX: asegurar permisos de las carpetas de entrada/salida
#RUN mkdir -p /home/sandbox/in/IN /home/sandbox/out/OUT /home/sandbox/proj/gen \
# && chown -R sandbox:sandbox /home/sandbox/ \
# && chmod -R a+r /home/sandbox/in \
# && chmod 755 /home/sandbox/run_single.sh

USER sandbox
ENTRYPOINT ["/home/sandbox/run_single.sh"]

