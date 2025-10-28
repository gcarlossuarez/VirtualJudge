#!/usr/bin/bash

# NOTA. - Para facil acceso de nsjail, utilizar el directorio temporal tmp (de paso, se garantiza limpieza auotmatica).
# Es importante haber creado, desde el programa principal, el subdirectorio temporal con los datos del usuiario, obedeciendo
# a esta estructura:
# /tmp
#    -- /temp-workdir
#          -- /id<nro. aleatorio>
#                  -- /in
#                         -- Program.cs
#                         -- /IN
#                              datos0001.txt .....
#                         -- /OUT
#                              Output_datos0001.txt ....
#          -- /template
#                -- /App
#                       --  Solution.csproj


######################################################################################################################
# NOTA. - Paquetes extra necesitados para .Net (OJO. - Da problemas en .NET 8. Se debería instalar .NET9)
#sudo apt update
#sudo apt install libicu74    # Runtime principal para .NET 8
#sudo apt install libicu-dev  # Si se necesita desarrollo/compilación extra (opcional, pero recomendado para builds)
######################################################################################################################


echo "Iniciando run_single.sh - NSJAIL con formato compatible"

set -euo pipefail

# 🔹 Parámetros
TMP_WORK_DIR="${1:-/tmp/sandbox/in}"
SRC_DIR="${2:-/tmp/sandbox/in}"
TIME_LIMIT="${3:-6}"
LANGUAGE="${4:-}"
VALIDATOR_SRC="${5:-}"
SHOW_OUTPUT="${6:-0}"
#USE_CSC_IN_DOTNET="${7:-0}"

# 🔹 Configuración  
SESSION_ID="juez_$$_$RANDOM"
WORK_DIR="$TMP_WORK_DIR/$SESSION_ID"

echo "SESSION_ID=$SESSION_ID" >&2
mkdir -p "$WORK_DIR"

cleanup() {
    echo "Limpiando sesión $SESSION_ID" >&2
    rm -rf "$WORK_DIR"
}
trap cleanup EXIT

if [ ! -d "$SRC_DIR" ]; then
  echo "ERROR: No existe el directorio fuente: $SRC_DIR"
  exit 2
fi

# === COPIA DE ARCHIVOS ===
echo "Copiando los archivos"
if [ "$LANGUAGE" = "dotnet" ]; then
   rm -rf "$WORK_DIR/proj"
   cp -r "$TMP_WORK_DIR/template/App" "$WORK_DIR/proj"

   CS_FILES=("$SRC_DIR"/*.cs)
   if [ ${#CS_FILES[@]} -eq 0 ]; then
      echo "ERROR: No se encontró ningún .cs en $SRC_DIR"
      exit 3
   fi

   if [ ${#CS_FILES[@]} -eq 1 ]; then
       cp "${CS_FILES[0]}" "$WORK_DIR/proj/Program.cs"
   else
       cp "${CS_FILES[@]}" "$WORK_DIR/proj/"
   fi

elif [ "$LANGUAGE" = "g++" ]; then
    
    CPP_FILES=("$SRC_DIR"/*.cpp)
    if [ ${#CPP_FILES[@]} -eq 0 ]; then
      echo "ERROR: No se encontró ningún .cpp en $SRC_DIR"
      exit 3
    fi

    if [ ${#CPP_FILES[@]} -eq 1 ]; then
        cp "${CPP_FILES[0]}" "$WORK_DIR/solucion.cpp"
    else
        cp "${CPP_FILES[@]}" "$WORK_DIR/"
    fi
else
    echo "❌ Lenguaje no soportado: $LANGUAGE"
    exit 1
fi

BUILD_LOG="$WORK_DIR/build.log"

# === COMPILACIÓN FUERA DEL SANDBOX ===
BUILD_RC=0
if [ "$LANGUAGE" = "dotnet" ]; then
    echo "Compilando en dotnet"
    
    # 🔹Restaurar paquetes NuGet primero
    # Ver archivo ./dotnet-restore-explanation.md para una mejor explicacion de por que se necesita dotnet restore
    ( cd "$WORK_DIR/proj" && dotnet restore --no-cache ) >>"$BUILD_LOG" 2>&1
    RESTORE_RC=$?
    echo "DEBUG: dotnet restore terminó con RC=$RESTORE_RC" >&2
    
    # Luego compilar
    ( cd "$WORK_DIR/proj" && dotnet build -c Release --nologo --no-restore ) >>"$BUILD_LOG" 2>&1
    BUILD_RC=$?
    echo "Termino de compilar en dotnet con RC=$BUILD_RC" >&2

elif [ "$LANGUAGE" = "g++" ]; then
    echo "Compilando en g++"
    ( g++ -O2 -std=c++17 "$WORK_DIR/solucion.cpp" -o "$WORK_DIR/solucion" ) >"$BUILD_LOG" 2>&1
    BUILD_RC=$?
    echo "BUILD_RC=$BUILD_RC"
    chmod +x "$WORK_DIR/solucion"
fi

echo "Determinado estado de BUILD"
# Determinar estado de build
STATUS_BUILD="error"
if [ "$LANGUAGE" = "dotnet" ]; then
  if grep -q "Build succeeded." "$BUILD_LOG"; then
    STATUS_BUILD="ok"
  fi
elif [ "$LANGUAGE" = "g++" ]; then
  if [ "$BUILD_RC" -eq 0 ]; then
    STATUS_BUILD="ok"
  fi
fi

if [ "$BUILD_RC" -eq 0 ] && [ "$STATUS_BUILD" != "ok" ]; then
  STATUS_BUILD="ok"
fi
echo "STATUS_BUILD=$STATUS_BUILD"
#echo "ls de $WORK_DIR/proj/bin/Release/net8.0" 
#ls $WORK_DIR/proj/bin/Release/net8.0

# === EJECUCIÓN DENTRO DE NSJAIL ===
if [ "$LANGUAGE" = "dotnet" ]; then
    DLL=$(find "$WORK_DIR/proj/bin/Release" -type f -name "*.dll" | head -n1)
    # Ruta DENTRO del sandbox (relativa al bindmount)
    DLL_SANDBOX_PATH="/tmp/sandbox/proj/bin/Release/net8.0/$(basename "$DLL")"
    echo "DEBUG: DLL real: $DLL" >&2
    echo "DEBUG: DLL sandbox: $DLL_SANDBOX_PATH" >&2
elif [ "$LANGUAGE" = "g++" ]; then
    DLL=$(find "$WORK_DIR/solucion")
    EXEC_CMD="/tmp/sandbox/solucion"  # Relativa al bindmount (no "$TMP_WORK_DIR/solucion")
fi

if [ "$STATUS_BUILD" != "ok" ] || [ -z "${DLL:-}" ]; then
  echo "===BUILD==="; cat "$BUILD_LOG" || true
  echo "===RUN==="; echo "No se generó ejecutable - errores de compilación." 
  echo "===SUMMARY==="; echo "build:error"; echo "run:error"
  exit 0
fi

# Directorios de datasets
IN_DIR="$SRC_DIR/IN"
OUT_DIR="$SRC_DIR/OUT"
GEN_DIR="$WORK_DIR/gen"
mkdir -p "$GEN_DIR"

STATUS_RUN="ok"
DETAILS=""
RUN_OUTPUT=""

# 🔹 Preparar validador (si existe)
VALIDATOR_DLL=""
if [ -n "$VALIDATOR_SRC" ] && [ -f "$VALIDATOR_SRC" ]; then
  echo "Compilando validador desde: $VALIDATOR_SRC" >&2
  
  rm -rf "$WORK_DIR/proj_validator"
  cp -r "$TMP_WORK_DIR/template/App" "$WORK_DIR/proj_validator"
  cp "$VALIDATOR_SRC" "$WORK_DIR/proj_validator/Program.cs"
  echo "Archivo en directorio work para Validator"
  ls "$WORK_DIR/proj_validator"
  VALIDATOR_BUILD_LOG="$WORK_DIR/validator_build.log"
  
  # 1. RESTORE (OBLIGATORIO)
  echo "Restaurando paquetes del validador (restore)..." >&2
  # Ver archivo ./dotnet-restore-explanation.md para una mejor explicacion de por que se necesita dotnet restore
  if ! (cd "$WORK_DIR/proj_validator" && dotnet restore --no-cache >>"$VALIDATOR_BUILD_LOG" 2>&1); then
    echo "ERROR: dotnet restore falló en validador" >&2
    cat "$VALIDATOR_BUILD_LOG" >&2
  else
    # 2. BUILD
    echo "Compilando validador (build)..." >&2
    if ! (cd "$WORK_DIR/proj_validator" && dotnet build -c Release --nologo --no-restore >>"$VALIDATOR_BUILD_LOG" 2>&1); then
      echo "ERROR: Falló compilación del validador" >&2
      cat "$VALIDATOR_BUILD_LOG" >&2
    else
      # 3. BUSCAR DLL
      VALIDATOR_DLL=$(find "$WORK_DIR/proj_validator/bin/Release/net8.0" -name "*.dll" -type f | head -n1)
      if [ -z "$VALIDATOR_DLL" ]; then
        echo "ERROR: No se encontró DLL del validador" >&2
        find "$WORK_DIR/proj_validator/bin/Release" -type f >&2 || true
      else
        echo "Validador compilado correctamente: $VALIDATOR_DLL" >&2
      fi
    fi
  fi
fi

# Iterar datasets
if [ -d "$IN_DIR" ]; then
  # Normalizar todos los archivos de entrada y salida esperada
  echo "Normalizando archivos de entrada y salida (quitando \r)" >&2
  for f in "$IN_DIR"/*.txt "$OUT_DIR"/*.txt; do
    if [ -f "$f" ]; then
        # Crear versión limpia sin \r
        tr -d '\r' < "$f" > "$f.clean"
        mv "$f.clean" "$f"
    fi
  done
  
  shopt -s nullglob
  for infile in "$IN_DIR"/datos*.txt; do
    #if [ $SHOW_OUTPUT -eq 1 ]; then
    #echo "Procesando: $infile" >&2
    #fi
    base=$(basename "$infile")
    #echo "Contenido del archivo: $infile"
    #cat $infile
    
    expected="$OUT_DIR/Output_${base}"
    actual="$GEN_DIR/Output_${base}"

    # 🔹 EJECUTAR EN NSJAIL - COMANDO DIRECTO
    set +e
    
    # Configurar comando nsjail según el lenguaje
    if [ "$LANGUAGE" = "dotnet" ]; then
        # Construir comando como array para dotnet (mantiene lo tuyo)
        set +e

        nsjail_cmd=(
	    sudo
	    nsjail
	    -M o
	    --time_limit $((TIME_LIMIT + 5))
	    --rlimit_cpu $TIME_LIMIT
	    --rlimit_as inf  # Ilimitado para .NET (o baja si pruebas muestran menos uso)
	    --rlimit_nproc 16
	    --rlimit_fsize 10485760
	    --rlimit_nofile 1024  # Fix: 1024 FDs en vez de inf (evita EPERM en user ns) (fix "Too many open files")
	    --disable_proc
	    --iface_no_lo
	    --user 65534
	    --group 65534
	    --cwd /tmp
	    --bindmount_ro /bin
	    --bindmount_ro /lib
	    --bindmount_ro /lib64
	    --bindmount_ro /usr
	    --bindmount_ro /etc
	    --bindmount_ro /usr/share/dotnet
	    --bindmount_ro /usr/lib/dotnet
	    --bindmount_ro /proc
	    --bindmount_ro /sys
	    --bindmount /dev
	    --bindmount_ro /var
	    --bindmount_ro /run
	    --tmpfsmount /dev/shm:size=1073741824  # 1GB para .NET temp/GC
	    --bindmount "$WORK_DIR:/tmp/sandbox"
	    --env "COMPlus_EnableDiagnostics=0"
	    --env "DOTNET_EnableDiagnostics=0"
            --env "DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1"  # Nuevo: Fix ICU crash en runtime del programa
	    #--quiet  # Comentar si se necesita ver los logs
	    #--really_quiet  # En vez de --quiet, suprimer, incluso, los warnings innecesarios de nsjail
	    --
	    /usr/bin/dotnet
	    "$DLL_SANDBOX_PATH"
        )

        #echo "DEBUG: Ejecutando nsjail para C#..." >&2
        #echo "DEBUG: Ejecutando comando array..." >&2
        #echo "DEBUG: Comando array: ${nsjail_cmd[*]}" >&2
        "${nsjail_cmd[@]}" < "$infile" > "$actual" 2> "$GEN_DIR/stderr_${base}.log"
        RC=$?
        #echo "DEBUG: Código de retorno: $RC" >&2
        #echo "DEBUG: Stderr contenido:" >&2
        if [ $RC -ne 0 ]; then 
           cat "$GEN_DIR/stderr_${base}.log" >&2 
        fi
        
        set -e

    else  # Para g++ (C++): Usa array similar para consistencia, con límites ajustados
        set +e

        nsjail_cmd=(
	    sudo
	    nsjail
	    -M o
	    --time_limit $((TIME_LIMIT + 5))
	    --rlimit_cpu $TIME_LIMIT
	    --rlimit_as 268435456  # 256MB para C++ (suficiente para la mayoría de envíos de estudiantes)
	    --rlimit_nproc 16
	    --rlimit_fsize 10485760
	    --disable_proc
	    --iface_no_lo
	    --user 65534
	    --group 65534
	    --cwd /tmp
	    --bindmount_ro /bin
	    --bindmount_ro /lib
	    --bindmount_ro /lib64
	    --bindmount_ro /usr
	    --bindmount_ro /etc
	    --bindmount_ro /proc  # Útil para info de procesos en C++
	    --bindmount_ro /sys   # Para syscalls básicas
	    --bindmount /dev      # RW para I/O si el código usa devices
	    --bindmount_ro /var
	    --bindmount_ro /run
	    --tmpfsmount /dev/shm:size=67108864  # 64MB shm para C++ (mucho menos que .NET, ajusta si OOM)
	    --bindmount "$WORK_DIR:/tmp/sandbox"
	    #--quiet  # Comentar si se necesita ver los logs
	    --really_quiet  # En vez de --quiet, suprimer, incluso, los warnings innecesarios de nsjail
	    --
	    "$EXEC_CMD"
        )

        #echo "DEBUG: Ejecutando nsjail para C++..." >&2
        #echo "DEBUG: Ejecutando comando array..." >&2
        #echo "DEBUG: Comando array: ${nsjail_cmd[*]}" >&2
        "${nsjail_cmd[@]}" < "$infile" > "$actual" 2> "$GEN_DIR/stderr_${base}.log"
        RC=$?
        #echo "DEBUG: Código de retorno: $RC" >&2
        #echo "DEBUG: Stderr contenido:" >&2
        
        if [ $RC -ne 0 ]; then 
           cat "$GEN_DIR/stderr_${base}.log" >&2
        fi
        set -e
    fi
    
    if [ $SHOW_OUTPUT -eq 1 ]; then
        echo "Contenido del archivo generado como resultado de ejecutar el programa"
        cat "$actual"
    fi
    
    # Probar el programa COMPILADO fuera de nsjail
    #echo "=== PRUEBA FUERA DE NSJAIL ==="
    #DLL=$(find "$WORK_DIR/proj/bin/Release" -type f -name "*.dll" | head -n1)
    #echo "ls DLL"
    #ls $DLL
    #cat "$infile" | dotnet "$DLL"
    #echo "Código de retorno: $?"

    # Capturar la salida real del programa para RUN_OUTPUT
    if [ -f "$actual" ]; then
        RUN_OUTPUT+="--- $base ---"$'\n'
        RUN_OUTPUT+="$(cat "$actual")"$'\n'
    fi

    if [ $RC -eq 124 ]; then
      if [ $SHOW_OUTPUT -eq 1 ]; then
         echo "✅  Cumple if [ $RC -eq 124 ]; then"
      fi
      STATUS_RUN="error"
      DETAILS+="Dataset $base: ⏱ Timeout\n"
    elif [ $RC -ne 0 ]; then
      if [ $SHOW_OUTPUT -eq 1 ]; then
         echo "✅ Cumple elif [ $RC -ne 0 ]; then"
      fi
      STATUS_RUN="error"
      DETAILS+="Dataset $base: ❌ Ejecución falló (RC=$RC)\n"
    else
      if [ $SHOW_OUTPUT -eq 1 ]; then
         echo "✅ Cumple else va a verificar si se utiliza validador"
      fi
      # 🔹 Validación con validador o diff
      if [ -n "$VALIDATOR_DLL" ]; then
        if [ $SHOW_OUTPUT -eq 1 ]; then
           echo "👷‍♂️ Ejecuta validador"
        
      	   echo "IN:  $(od -c "$infile" | head -n1)" >&2
	   echo "EXP: $(od -c "$expected" | head -n1)" >&2
	   echo "ACT: $(od -c "$actual" | head -n1)" >&2
           echo "Ejecutando validador sobre $actual" >&2
        fi
        set +e
        # Workaround: Modo invariant (evita ICU crash)
        set +e
	unset DOTNET_SYSTEM_GLOBALIZATION_INVARIANT 2>/dev/null || true
        VAL_OUT=$(DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 dotnet "$VALIDATOR_DLL" "$infile" "$expected" "$actual")
        VAL_RC=$?
        set -e
        if [ $VAL_RC -eq 0 ] && [[ "$VAL_OUT" == OK* ]]; then
            DETAILS+="Dataset $base: ✅ correcto (validador)\n"
        else
            STATUS_RUN="error"
            DETAILS+="Dataset $base: ❌ incorrecto (validador)\n$VAL_OUT\n"
        fi
        if [ $SHOW_OUTPUT -eq 1 ]; then
           echo "Post-ejecución: VAL_OUT=$VAL_OUT" >&2  # Ver output
        fi
      else
        if [ $SHOW_OUTPUT -eq 1 ]; then
           echo "👷‍♂️ Comparar contra salida esperada (modo clásico)"
        fi
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
    fi
  done
else
  # Modo simple: ejecutar sin datasets
  set +e
  if [ "$LANGUAGE" = "dotnet" ]; then
      cmd="sudo nsjail -M o --time_limit $((TIME_LIMIT + 5)) --rlimit_cpu $TIME_LIMIT --rlimit_as 1073741824 --rlimit_nproc 16 --rlimit_fsize 10485760 --disable_proc --iface_no_lo --user 65534 --group 65534 --cwd /tmp --bindmount_ro /bin --bindmount_ro /lib --bindmount_ro /lib64 --bindmount_ro /usr --bindmount_ro /etc --bindmount_ro /usr/share/dotnet --bindmount_ro /usr/lib/dotnet --bindmount $WORK_DIR:/tmp/sandbox --quiet -- /usr/bin/dotnet $DLL_SANDBOX_PATH"
  else
      cmd="sudo nsjail -M o --time_limit $((TIME_LIMIT + 5)) --rlimit_cpu $TIME_LIMIT --rlimit_as 268435456 --rlimit_nproc 16 --rlimit_fsize 10485760 --disable_proc --iface_no_lo --user 65534 --group 65534 --cwd /tmp --bindmount_ro /bin --bindmount_ro /lib --bindmount_ro /lib64 --bindmount_ro /usr --bindmount_ro /etc --bindmount $WORK_DIR:/tmp/sandbox --quiet -- $EXEC_CMD"
  fi
  
  eval "$cmd" > "$GEN_DIR/simple_output.txt" 2> "$GEN_DIR/stderr_simple.log"
  RC=$?
  set -e
  
  if [ -f "$GEN_DIR/simple_output.txt" ]; then
    RUN_OUTPUT+="$(cat "$GEN_DIR/simple_output.txt")"$'\n'
  fi
  
  if [ $RC -eq 124 ]; then
    STATUS_RUN="error"
    DETAILS+="Ejecución simple: ⏱ Timeout\n"
  elif [ $RC -ne 0 ]; then
    STATUS_RUN="error" 
    DETAILS+="Ejecución simple: ❌ falló (RC=$RC)\n"
  else
    DETAILS+="Ejecución simple: ✅ completada\n"
  fi
fi

# 🔹 GENERAR SALIDA EN FORMATO COMPATIBLE
echo "===BUILD==="
cat "$BUILD_LOG" || true
echo "===RUN==="
echo -n "$RUN_OUTPUT"
echo "===SUMMARY==="
echo "build:$STATUS_BUILD"
echo "run:$STATUS_RUN"
echo "DETAILS:"
echo -e "$DETAILS"
