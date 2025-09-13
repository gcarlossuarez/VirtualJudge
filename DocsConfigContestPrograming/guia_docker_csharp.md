# Guía: Configuración de Docker y Ejecución de Programas C# en un Contenedor Seguro (tipo ICPC)

Este documento describe paso a paso cómo preparar un entorno **Linux +
Docker** para compilar y ejecutar programas **C#** en un contenedor
aislado, ideal para exámenes tipo ICPC.

Incluye **comentarios adicionales** sobre las instrucciones `bash` para
quienes no están familiarizados con shell scripting.

------------------------------------------------------------------------

## 1. Instalación de Docker en Linux

En Ubuntu/Debian:

``` bash
sudo apt update
sudo apt upgrade -y
sudo apt install -y docker.io
sudo systemctl enable docker
sudo systemctl start docker
```

Verificación:

``` bash
docker run hello-world
```

Si aparece `permission denied`:

``` bash
sudo usermod -aG docker $USER
newgrp docker
docker run hello-world
```

------------------------------------------------------------------------

## 2. Directorio del Runner

Crear un directorio `cs-single-runner/` con dos archivos: `Dockerfile` y
`run_single.sh`.

### `Dockerfile`

``` dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Variables de entorno para evitar mensajes molestos de .NET
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_CLI_WORKLOAD_UPDATE_ADVERTISING_DISABLED=1

# Crear usuario sandbox (no root)
RUN useradd -ms /bin/bash sandbox
WORKDIR /home/sandbox

# Instalar comando 'time' (para medir tiempo/memoria)
RUN apt-get update && apt-get install -y --no-install-recommends time \
 && rm -rf /var/lib/apt/lists/*

# Crear plantilla de consola y restaurar durante la build
RUN dotnet new console -n App -o /home/sandbox/template/App --force \
 && dotnet restore /home/sandbox/template/App

# Copiar el script runner
COPY run_single.sh /home/sandbox/run_single.sh
RUN chown -R sandbox:sandbox /home/sandbox \
 && chmod 755 /home/sandbox/run_single.sh

USER sandbox
ENTRYPOINT ["/home/sandbox/run_single.sh"]
```

### `run_single.sh` (con comentarios)

``` bash
#!/usr/bin/env bash
set -euo pipefail
# -e : salir si algún comando falla
# -u : error si se usa una variable no definida
# -o pipefail : errores en pipelines hacen fallar el script

SRC_DIR="${1:-/home/sandbox/in}"   # Primer argumento: directorio de entrada, o valor por defecto
TIME_LIMIT="${2:-6}"               # Segundo argumento: tiempo límite en segundos, por defecto 6

if [ ! -d "$SRC_DIR" ]; then
  echo "ERROR: No existe el directorio fuente: $SRC_DIR"
  exit 2
fi

cd /home/sandbox

# Limitar CPU y procesos dentro del contenedor
ulimit -t "$TIME_LIMIT"   # Tiempo máximo de CPU
ulimit -u 256             # Número máximo de procesos/hilos

# Copiar la plantilla restaurada
rm -rf proj
cp -r /home/sandbox/template/App proj

# Buscar archivos .cs del alumno
shopt -s nullglob   # evita que '*.cs' quede como string literal si no hay coincidencias
CS_FILES=("$SRC_DIR"/*.cs)
if [ ${#CS_FILES[@]} -eq 0 ]; then
  echo "ERROR: No se encontró ningún .cs en $SRC_DIR"
  exit 3
fi

# Reemplazar Program.cs por el archivo del alumno
rm -f proj/Program.cs
if [ ${#CS_FILES[@]} -eq 1 ]; then
  cp "${CS_FILES[0]}" proj/Program.cs
else
  cp "${CS_FILES[@]}" proj/
fi

BUILD_LOG=build.log
RUN_LOG=run.log

# Compilar sin restore (offline)
set +e
( cd proj && dotnet build -c Release --nologo --no-restore ) >"$BUILD_LOG" 2>&1
BUILD_RC=$?
set -e

# Determinar estado de compilación
STATUS_BUILD="error"
if grep -q "Build succeeded." "$BUILD_LOG"; then
  STATUS_BUILD="ok"
fi
if [ "$BUILD_RC" -eq 0 ] && [ "$STATUS_BUILD" != "ok" ]; then
  # '!=' significa "no es igual a"
  # Aquí: si STATUS_BUILD no es "ok" pero el código de retorno fue 0, lo forzamos a "ok"
  STATUS_BUILD="ok"
fi

OUTDIR="proj/bin/Release"
DLL=$(find "$OUTDIR" -type f -name "*.dll" | head -n1 || true)

# Condición: si STATUS_BUILD NO es "ok"  ||  si DLL está vacío
if [ "$STATUS_BUILD" != "ok" ] || [ -z "${DLL:-}" ]; then
  # -z STRING : cierto si STRING está vacío
  echo "===BUILD==="; cat "$BUILD_LOG" || true
  echo "===RUN==="; echo "No se generó salida (.dll)" >"$RUN_LOG"; cat "$RUN_LOG"
  echo "===SUMMARY==="; echo "build:error"; echo "run:error"
  exit 0
fi

# Ejecutar el programa con timeout y medir recursos
TIME_BIN="/usr/bin/time"
if [ -f "$SRC_DIR/input.txt" ]; then
  { "$TIME_BIN" -f "TIME=%E\nMEM=%MKB" timeout "${TIME_LIMIT}s" dotnet "$DLL" < "$SRC_DIR/input.txt"; } >"$RUN_LOG" 2>&1 || true
else
  { "$TIME_BIN" -f "TIME=%E\nMEM=%MKB" timeout "${TIME_LIMIT}s" dotnet "$DLL"; } >"$RUN_LOG" 2>&1 || true
fi

STATUS_RUN="ok"
# grep -qiE : busca patrones de error en la salida para marcar como fallo
grep -qiE "timed out|timeout|Killed|Unhandled|Exception" "$RUN_LOG" && STATUS_RUN="error"

# Mostrar resultados
echo "===BUILD==="; cat "$BUILD_LOG" || true
echo "===RUN==="; cat "$RUN_LOG" || true
echo "===SUMMARY==="; echo "build:$STATUS_BUILD"; echo "run:$STATUS_RUN"
```

------------------------------------------------------------------------

## 3. Construir la Imagen

``` bash
cd cs-single-runner
docker build -t cs-single-runner:1 .
```

------------------------------------------------------------------------

## 4. Probar un Programa Simple

``` bash
mkdir -p ~/icpc_test
cat > ~/icpc_test/solucion.cs <<'CS'
using System;
class A { static void Main() { Console.WriteLine("Hola ICPC!"); } }
CS

docker run --rm   --network=none   --cpus=1   --memory=1536m   --pids-limit=256   -v ~/icpc_test:/home/sandbox/in:ro   cs-single-runner:1 /home/sandbox/in 6
```

Salida esperada:

    ===BUILD===
    Build succeeded.
    ...
    ===RUN===
    Hola ICPC!
    TIME=0:00.01
    MEM=35000KB
    ===SUMMARY===
    build:ok
    run:ok

------------------------------------------------------------------------

## 5. Probar con Entrada Estándar

``` csharp
using System;
class A {
    static void Main() {
        var p = Console.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Console.WriteLine(int.Parse(p[0]) + int.Parse(p[1]));
    }
}
```

Ejecución:

``` bash
printf "7 35\n" | docker run --rm   --network=none   --cpus=1   --memory=1536m   --pids-limit=256   -i   -v ~/icpc_test:/home/sandbox/in:ro   cs-single-runner:1 /home/sandbox/in 6
```

Salida esperada:

    ===BUILD===
    Build succeeded.
    ...
    ===RUN===
    42
    TIME=0:00.01
    MEM=36000KB
    ===SUMMARY===
    build:ok
    run:ok

------------------------------------------------------------------------

## 6. Conclusiones

-   `!= "ok"` significa "distinto de 'ok'".\
-   `-z "${DLL:-}"` significa "la variable está vacía".\
-   Se usó `||` (OR lógico) para verificar condiciones alternativas.\
-   Los bloques `if ... fi` en `bash` permiten ejecutar secciones
    condicionales.

Con esto, tienes un flujo reproducible para **compilar y ejecutar
programas C# aislados** en Docker, con explicación de cada paso del
script.
