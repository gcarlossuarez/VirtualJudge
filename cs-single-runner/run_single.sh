#!/usr/bin/env bash
echo "Iniciando run_single.sh"
# NOTA. - "set -euo pipefail" Es una configuración de Bash que hace que el script falle rápido y sea más seguro:
# -e → si cualquier comando devuelve un error (código distinto de 0), el script termina inmediatamente.
# 👉 evita seguir ejecutando pasos si ya falló uno.
# -u → trata como error el uso de variables no definidas.
# 👉 si escribes $VAR y no existe, en vez de dejarla vacía, aborta el script.
# -o pipefail → en un pipeline (cmd1 | cmd2 | cmd3), el código de salida será el del primer comando que falle, no solo el último.
# 👉 útil porque normalmente Bash solo devuelve el estado de cmd3.
#⚡ En resumen: garantiza que si algo falla en cualquier parte del pipeline, el script no siga como si nada.
set -euo pipefail

# 🔹 SRC_DIR="${1:-/home/sandbox/in}"
# Esto es una expansión de parámetros en Bash:
# $1 = el primer argumento con que llamas al script.
# ${1:-/home/sandbox/in} significa:
# usa $1 si está definido y no vacío; de lo contrario, usa "/home/sandbox/in" como valor por defecto.
# Entonces:
# Si se ejecuta ./run_single.sh /tmp/codigo, SRC_DIR=/tmp/codigo.
# Si se ejecuta solo ./run_single.sh, SRC_DIR=/home/sandbox/in.
SRC_DIR="${1:-/home/sandbox/in}"
echo SRC_DIR=$SRC_DIR

TIME_LIMIT="${2:-6}" # $2 = segundo argumento al script (un número de segundos máximo de ejecución). Si no se pasa, usa 6 como valor por defecto.

# 🔹 VALIDATOR_SRC: ruta al código fuente del validador.
VALIDATOR_SRC="${3:-}"
echo "VALIDATOR_SRC=$VALIDATOR_SRC"


# Directorio temporal para trabajar
WORK_DIR="/home/sandbox/tmp"


if [ ! -d "$SRC_DIR" ]; then
  echo "ERROR: No existe el directorio fuente: $SRC_DIR"
  exit 2
fi

cd /home/sandbox

# Límites razonables (sin limitar memoria virtual)
ulimit -t "$TIME_LIMIT"   # CPU time (s)
ulimit -u 256             # procesos/hilos

# Copiar la plantilla (ya restaurada en la build) a una carpeta de trabajo
rm -rf "$WORK_DIR/proj"
cp -r /home/sandbox/template/App "$WORK_DIR/proj"


# Copiar .cs de entrada
shopt -s nullglob
CS_FILES=("$SRC_DIR"/*.cs)
if [ ${#CS_FILES[@]} -eq 0 ]; then
  echo "ERROR: No se encontró ningún .cs en $SRC_DIR"
  exit 3
fi
echo Copiado=$CS_FILES 
rm -f "$WORK_DIR/proj/Program.cs"
if [ ${#CS_FILES[@]} -eq 1 ]; then
  cp "${CS_FILES[0]}" "$WORK_DIR/proj/Program.cs"
else
  cp "${CS_FILES[@]}" "$WORK_DIR/proj/"
fi

BUILD_LOG="$WORK_DIR/build.log"
RUN_LOG="$WORK_DIR/run.log"

# Compilar SIN restore (offline). No abortar el script si falla la compilación.
set +e # Para que no aborte en caso de timeout
( cd "$WORK_DIR/proj" && dotnet build -c Release --nologo --no-restore ) >"$BUILD_LOG" 2>&1
BUILD_RC=$?
set -e

# Determinar estado de build:
# - Preferimos "Build succeeded." como indicador robusto.
# - Evita falsos positivos por "0 Error(s)".
STATUS_BUILD="error"
if grep -q "Build succeeded." "$BUILD_LOG"; then
  STATUS_BUILD="ok"
fi
# Si el retorno fue 0 y no encontramos "Build succeeded.", lo marcamos como ok igualmente.
if [ "$BUILD_RC" -eq 0 ] && [ "$STATUS_BUILD" != "ok" ]; then
  STATUS_BUILD="ok"
fi

OUTDIR="$WORK_DIR/proj/bin/Release"
DLL=$(find "$OUTDIR" -type f -name "*.dll" | head -n1 || true)


if [ "$STATUS_BUILD" != "ok" ] || [ -z "${DLL:-}" ]; then
  echo "===BUILD==="; cat "$BUILD_LOG" || true
  echo "===RUN==="; echo "No se generó salida (.dll) - revisar errores de compilación." >"$RUN_LOG"; cat "$RUN_LOG"
  echo "===SUMMARY==="; echo "build:error"; echo "run:error"
  exit 0
fi

# Directorios de datasets
IN_DIR="$SRC_DIR/IN"
OUT_DIR="$SRC_DIR/OUT"
GEN_DIR="$WORK_DIR/proj/gen"
mkdir "$GEN_DIR"

STATUS_RUN="ok"
DETAILS=""

# 🔹 Preparar validador (si existe)
VALIDATOR_DLL=""
if [ -n "$VALIDATOR_SRC" ] && [ -f "$VALIDATOR_SRC" ]; then
  echo "Compilando validador desde: $VALIDATOR_SRC (archivo verificado)"
  
  rm -rf "$WORK_DIR/proj_validator"
  cp -r /home/sandbox/template/App "$WORK_DIR/proj_validator" # Copia el contenido de App dentro de una carpeta nueva llamada proj_validator.
  cp "$VALIDATOR_SRC" "$WORK_DIR/proj_validator/Program.cs"

  VALIDATOR_BUILD_LOG="$WORK_DIR/validator_build.log"
  ( cd "$WORK_DIR/proj_validator" && dotnet build -c Release --nologo --no-restore ) >"$VALIDATOR_BUILD_LOG" 2>&1 || true
  
  VALIDATOR_DLL=$(find "$WORK_DIR/proj_validator/bin/Release" -type f -name "*.dll" | head -n1 || true)

  if [ -z "$VALIDATOR_DLL" ]; then
    echo "ERROR: no se pudo compilar el validador"
    exit 5
  fi
fi

# Iterar datasets si existen
if [ -d "$IN_DIR" ]; then
  shopt -s nullglob
  for infile in "$IN_DIR"/datos*.txt; do
    echo infile=$infile
    base=$(basename "$infile")
    
    expected="$OUT_DIR/Output_${base}"
    actual="$GEN_DIR/Output_${base}"

    # Ejecutar programa del estudiante
    set +e
    { /usr/bin/time -f "TIME=%E\nMEM=%MKB" timeout "${TIME_LIMIT}s" dotnet "$DLL" < "$infile"; } \
      >"$actual" 2> "$GEN_DIR/metrics_${base}.log"
    RC=$?
    set -e
    echo ">>> RC=$RC"

    if [ $RC -eq 124 ]; then
      STATUS_RUN="error"
      DETAILS+="Dataset $base: ⏱ Timeout (excedió ${TIME_LIMIT}s)\n"
      continue
    elif [ $RC -ne 0 ]; then
      STATUS_RUN="error"
      DETAILS+="Dataset $base: ❌ ejecución falló (RC=$RC)\n"
      continue
    fi
    
    # 🔹 Si hay validador, se ejecuta en lugar del diff
    if [ -n "$VALIDATOR_DLL" ]; then
      echo "Ejecutando validador sobre $actual"
      set +e
      VAL_OUT=$(dotnet "$VALIDATOR_DLL" "$infile" "$expected" "$actual")
      VAL_RC=$?
      set -e
      if [ $VAL_RC -eq 0 ] && [[ "$VAL_OUT" == OK* ]]; then
        DETAILS+="Dataset $base: ✅ correcto (validador)\n"
      else
        STATUS_RUN="error"
        DETAILS+="Dataset $base: ❌ incorrecto (validador)\n$VAL_OUT\n"
      fi
    else
      # Comparar contra salida esperada (modo clásico)
      if [ -f "$expected" ]; then
        if diff -q <(tr -d '\r' < "$expected") <(tr -d '\r' < "$actual") >/dev/null; then
          DETAILS+="Dataset $base: ✅ correcto\n"
        else
          STATUS_RUN="error"
          DETAILS+="Dataset $base: ❌ incorrecto\n"
        fi
      else
        DETAILS+="Dataset $base: ⚠ sin expected\n"
      fi
    fi
  done
else
  DETAILS="Modo simple ejecutado\n"
fi

# Mostrar resultados
echo "===BUILD==="; cat "$BUILD_LOG" || true
echo "===RUN==="
if [ -d "$GEN_DIR" ]; then
  for f in "$GEN_DIR"/*; do
    echo "--- $(basename "$f") ---"
    cat "$f"
    echo
  done
fi
cat "$RUN_LOG" 2>/dev/null || true
echo -e "$DETAILS"

echo "===SUMMARY==="
echo "build:$STATUS_BUILD"
echo "run:$STATUS_RUN"
echo -e "DETAILS:\n$DETAILS"

