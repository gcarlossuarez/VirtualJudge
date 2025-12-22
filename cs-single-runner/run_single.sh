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
LANGUAGE="${3:-}"
echo "LANGUAGE=$LANGUAGE"

# 🔹 VALIDATOR_SRC: ruta al código fuente del validador.
VALIDATOR_SRC="${4:-}"
echo "VALIDATOR_SRC=$VALIDATOR_SRC"


# Directorio temporal para trabajar
WORK_DIR="/home/sandbox/tmp"


if [ ! -d "$SRC_DIR" ]; then
  echo "ERROR: No existe el directorio fuente: $SRC_DIR"
  exit 2
fi

cd /home/sandbox

echo "Contenido de /home/sandbox/in:"
ls -l /home/sandbox/in

echo "Contenido de /home/sandbox/in/OUT:"
ls -l /home/sandbox/in/OUT

echo "Contenido de /home/sandbox/in/VALIDATOR:"
ls -l /home/sandbox/in/VALIDATOR

# Límites razonables (sin limitar memoria virtual)
ulimit -t "$TIME_LIMIT"   # CPU time (s)
ulimit -u 256             # procesos/hilos

# Para que funcione bien, si no hay archivos.
# Ejemplo> si no hay .cs → FILES queda como un array vacío (()).
shopt -s nullglob
# Evita que CS_FILES=("$SRC_DIR"/*.cs) se llene con un string literal "$SRC_DIR/*.cs" 
# cuando no hay archivos.
# En cambio, queda un array vacío y se puede chequear con if [ ${#CS_FILES[@]} -eq 0 ]; then ... fi.
if [[ ( -n "$VALIDATOR_SRC" && -f "$VALIDATOR_SRC" ) || "$LANGUAGE" = "dotnet" ]]; then
   # =============================
   # 🔧 Asegurar que exista la plantilla base
   # =============================
   TEMPLATE_DIR="/home/sandbox/template/App"
   TEMPLATE_DIR_VALIDATOR="/home/sandbox/template/ValidatorApp"
   FALLBACK_TEMPLATE="/home/sandbox/tmp/template/App"

   if [ ! -d "$TEMPLATE_DIR" ]; then
     echo "⚠️ Plantilla base no encontrada (FS solo lectura). Creando plantilla temporal..."
     mkdir -p "$FALLBACK_TEMPLATE"
     
     FALLBACK_BASE_TMP_TEMPLATE="/home/sandbox/tmp/template" 
     cd "$FALLBACK_BASE_TMP_TEMPLATE"
     # Asegurar permisos de escritura en tmpfs
     if [ ! -w /home/sandbox/tmp ]; then
       echo "🔧 Ajustando permisos de /home/sandbox/tmp..."
       chmod 777 /home/sandbox/tmp 2>/dev/null || echo "ℹ️ (Aviso benigno) No se pudieron cambiar permisos de /home/sandbox/tmp"
     fi

     # =============================
     # Asegurar /home/sandbox/.dotnet escribible y existente
     # =============================
     if [ ! -d "/home/sandbox/.dotnet" ]; then
        echo "📁 Creando /home/sandbox/.dotnet (faltaba)"
        mkdir -p /home/sandbox/.dotnet
     fi
     chown -R "$(id -u)":"$(id -g)" /home/sandbox/.dotnet
     chmod 777 /home/sandbox/.dotnet || echo "ℹ️ (Aviso benigno) No se pudieron cambiar permisos de /home/sandbox/.dotnet"
     
     # Crear proyecto base dentro del tmpfs (que sí es escribible)
     echo "🧩 Creando proyecto base temporal (sin restore)..."
     dotnet new console -n App -o App --force --no-restore > /dev/null 2>&1

     # 🔧 Generar o copiar el project.assets.json necesario
     mkdir -p App/obj

     if [ -f "/home/sandbox/template/App/obj/project.assets.json" ]; then
        cp /home/sandbox/template/App/obj/project.assets.json App/obj/
        echo "🧩 project.assets.json copiado desde plantilla base"
     else
        echo "⚠️ No se encontró project.assets.json en la plantilla base. Creando uno local..."
        # Generar el archivo mediante un restore rápido local (solo si no existe)
        #dotnet restore App > /dev/null 2>&1 || echo "⚠️ Restore falló, pero se continuará con build offline"
        dotnet restore App --ignore-failed-sources --disable-parallel --no-cache > /dev/null 2>&1 || {
        echo "⚠️ No se pudo restaurar vía NuGet; generando project.assets.json vacío para modo offline..."
        mkdir -p App/obj
        cat > App/obj/project.assets.json <<'EOF'
{
  "version": 3,
  "targets": {
    "net8.0": {}
  },
  "libraries": {},
  "project": {
    "version": "1.0.0",
    "restore": { "projectUniqueName": "App" },
    "frameworks": {
      "net8.0": {
        "dependencies": {}
      }
    }
  }
}
EOF
}

     fi

     dotnet build App -c Release --nologo --no-restore > /dev/null 2>&1 || true

     echo "Contenido de App/obj/project.assets.json"
     cat App/obj/project.assets.json
     chown -R "$(id -u)":"$(id -g)" "$FALLBACK_TEMPLATE"
     echo "✅ Plantilla temporal creada y restaurada en $FALLBACK_TEMPLATE"

     TEMPLATE_DIR="$FALLBACK_TEMPLATE"
   fi
fi

# === Copiar archivos fuente según lenguaje ===
if [ "$LANGUAGE" = "dotnet" ]; then
   # Crear el directorio temporal de trabajo
   mkdir -p "$WORK_DIR"
   chmod 700 "$WORK_DIR" || echo "ℹ️ (Aviso benigno) No se pudieron cambiar permisos de $WORK_DIR"

   # Copiar la plantilla base al directorio temporal de proyecto
   rm -rf "$WORK_DIR/proj"
   cp -r "$TEMPLATE_DIR" "$WORK_DIR/proj"

   # Copiar .cs de entrada
   CS_FILES=("$SRC_DIR"/*.cs)
   if [ ${#CS_FILES[@]} -eq 0 ]; then
      echo "ERROR: No se encontró ningún .cs en $SRC_DIR"
      exit 3
   fi
   
   # Archivos C#
   if [ ${#CS_FILES[@]} -eq 1 ]; then
       cp "${CS_FILES[0]}" "$WORK_DIR/proj/Program.cs"
       echo "Copiado=${CS_FILES[0]} como $WORK_DIR/proj/Program.cs"
   else
       cp "${CS_FILES[@]}" "$WORK_DIR/proj/"
       echo "Copiado=$CS_FILES" 
   fi

elif [ "$LANGUAGE" = "g++" ]; then
    # Copiar .cpp de entrada
    CPP_FILES=("$SRC_DIR"/*.cpp)
    if [ ${#CPP_FILES[@]} -eq 0 ]; then
      echo "ERROR: No se encontró ningún .cpp en $SRC_DIR"
      exit 3
    fi
    echo Copiado=$CPP_FILES 

    # Archivos C++
    if [ ${#CPP_FILES[@]} -eq 1 ]; then
        cp "${CPP_FILES[0]}" "$WORK_DIR/solucion.cpp"
    else
        cp "${CPP_FILES[@]}" "$WORK_DIR/"
    fi

# 🚩 Aquí se pueden ir sumando más lenguajes
# elif [ "$LANGUAGE" = "python" ]; then
#     cp "${PY_FILES[0]}" "$WORK_DIR/solucion.py"

else
    echo "❌ Lenguaje no soportado en copia: $LANGUAGE"
    exit 1
fi

BUILD_LOG="$WORK_DIR/build.log"
RUN_LOG="$WORK_DIR/run.log"

# Compilar SIN restore (offline). No abortar el script si falla la compilación.
set +e # Para que no aborte en caso de timeout

BUILD_RC=0
if [ "$LANGUAGE" = "dotnet" ]; then
    # === Compilar C# ===
    cd "$WORK_DIR/proj"
    echo "Contenido de $WORK_DIR/proj"
    ls "$WORK_DIR/proj"

    # 🔧 Siempre regenerar obj limpio y copiar project.assets.json válido
    rm -rf "$WORK_DIR/proj/obj"
    mkdir -p "$WORK_DIR/proj/obj"
    if [ -f "$TEMPLATE_DIR/obj/project.assets.json" ]; then
       cp "$TEMPLATE_DIR/obj/project.assets.json" "$WORK_DIR/proj/obj/"
       echo "✅ Copiado project.assets.json desde el directorio $TEMPLATE_DIR al directorio $WORK_DIR/proj/obj/"
    else
       echo "⚠️ No se encontró project.assets.json base en $TEMPLATE_DIR; la compilación puede fallar."
    fi


    # Compilar
    dotnet build -c Release --nologo --no-restore >"$BUILD_LOG" 2>&1

    BUILD_RC=$?

elif [ "$LANGUAGE" = "g++" ]; then
    # === Compilar C++ ===
    ( g++ -O2 -std=c++17 "$WORK_DIR/solucion.cpp" -o "$WORK_DIR/solucion" ) >"$BUILD_LOG" 2>&1
    BUILD_RC=$?

# 🚩 futuros lenguajes
# elif [ "$LANGUAGE" = "python" ]; then
#     # Python no necesita build → marcar como éxito
#     BUILD_RC=0

else
    echo "❌ Lenguaje no soportado en compilación: $LANGUAGE"
    exit 1
fi

echo "✅ Proyecto del estudiante compilado"
echo "Contenido de $TEMPLATE_DIR_VALIDATOR"
ls $TEMPLATE_DIR_VALIDATOR
echo "Contenido de BUILD_LOG=$BUILD_LOG"
cat "$BUILD_LOG"

set -e

# Determinar estado de build:
STATUS_BUILD="error"
if [ "$LANGUAGE" = "dotnet" ]; then
  # - Preferimos "Build succeeded." como indicador robusto.
  # - Evita falsos positivos por "0 Error(s)".
  if grep -q "Build succeeded." "$BUILD_LOG"; then
    STATUS_BUILD="ok"
  fi
elif [ "$LANGUAGE" = "g++" ]; then
  if [ "$BUILD_RC" -eq 0 ]; then
    STATUS_BUILD="ok"
  fi
fi


# Si el retorno fue 0 y no encontramos "Build succeeded.", lo marcamos como ok igualmente.
if [ "$BUILD_RC" -eq 0 ] && [ "$STATUS_BUILD" != "ok" ]; then
  STATUS_BUILD="ok"
fi

OUTDIR="$WORK_DIR/proj/bin/Release"
if [ "$LANGUAGE" = "dotnet" ]; then
    OUTDIR="$WORK_DIR/proj/bin/Release"
    DLL=$(find "$OUTDIR" -type f -name "*.dll" | head -n1 || true)
    # EXEC no es un string, es un array → Bash sabe separar el binario dotnet y su argumento (App.dll).
    EXEC=(dotnet "$DLL")
elif [ "$LANGUAGE" = "g++" ]; then
    OUTDIR="$WORK_DIR"
    EXEC=("$WORK_DIR/solucion")
fi


# Resultado final
if [ "$STATUS_BUILD" != "ok" ] || [ -z "${EXEC:-}" ]; then
  echo "===BUILD==="; cat "$BUILD_LOG" || true
  echo "===RUN==="; echo "No se generó salida (.dll) - revisar errores de compilación." >"$RUN_LOG"; cat "$RUN_LOG"
  echo "===SUMMARY==="; echo "build:error"; echo "run:error"
  exit 0
fi

# Directorios de datasets
IN_DIR="$SRC_DIR/IN"
OUT_DIR="$SRC_DIR/OUT"
GEN_DIR="$WORK_DIR/proj/gen"
if [ "$LANGUAGE" = "dotnet" ]; then
    GEN_DIR="$WORK_DIR/proj/gen"
else
    GEN_DIR="$WORK_DIR/gen"
fi
mkdir -p "$GEN_DIR"

STATUS_RUN="ok"
DETAILS=""

# 🔹 Preparar validador (si existe)
# NOTA. - El validador, podria ser siempre en C#. como es nativo del que creo el DataSet, y el problema, no deberia ser un problema el lenguaje del validador
#VALIDATOR_DLL=""
#if [ -n "$VALIDATOR_SRC" ] && [ -f "$VALIDATOR_SRC" ]; then
#   echo "Compilando validador desde: $VALIDATOR_SRC (archivo verificado)"
#   echo "Contenido de $VALIDATOR_SRC"
#   cat $VALIDATOR_SRC
#   
#   echo "Contenido directorios del sandbox"
#   ls -l /home/sandbox/
#   ls -l /home/sandbox/template
#   echo "Contenido de $TEMPLATE_DIR_VALIDATOR"
#   ls $TEMPLATE_DIR_VALIDATOR
#  
#   rm -rf "$WORK_DIR/proj_validator"
#
#   # Usar la plantilla que esté activa (TEMPLATE_DIR_VALIDATOR apunta a la base o al fallback)
#   if [ -d "$TEMPLATE_DIR_VALIDATOR" ]; then
#     echo "👌 Se encontró plantilla base en $TEMPLATE_DIR_VALIDATOR, no se necesita crear proyecto validador temporal..."
#     cp -r "$TEMPLATE_DIR_VALIDATOR" "$WORK_DIR/proj_validator"
#     
#     # Eliminar el directorio obj y bin de la plantilla antes de copiarla o usarla como base para el validador.
#     # Así, cuando se compile el proyecto temporal, se generará un nuevo project.assets.json limpio, solo con los paquetes 
#     # realmente requeridos, par ano tener problemas de acceso a Internet, buscando paquetes no necesitados
#     #rm -rf "$WORK_DIR/proj_validator/obj" "$WORK_DIR/proj_validator/bin" # Genera problemas con la opci'on "--no-restore"
#     # del comando dotnet build, por no tener acceso a internet 
#     rm -rf "$WORK_DIR/proj_validator/bin"
#   else
#     echo "⚠️ No se encontró plantilla base en $TEMPLATE_DIR, creando proyecto validador temporal..."
#     mkdir -p "$WORK_DIR/proj_validator"
#     dotnet new console -n App -o "$WORK_DIR/proj_validator" --force --no-restore > /dev/null 2>&1
#     mkdir -p "$WORK_DIR/proj_validator/obj"
#     if [ -f "$WORK_DIR/proj/obj/project.assets.json" ]; then
#        cp "$WORK_DIR/proj/obj/project.assets.json" "$WORK_DIR/proj_validator/obj/"
#     fi
#   fi
#
#   cp "$VALIDATOR_SRC" "$WORK_DIR/proj_validator/Program.cs"
#
#   # find "$WORK_DIR/proj_validator" -type f -name "*.csproj"
#   # find: busca archivos y directorios recursivamente.
#   # "$WORK_DIR/proj_validator": directorio raíz donde va a buscar.
#   # -type f: solo archivos (no directorios).
#   # -name "*.csproj": archivos cuyo nombre termine en .csproj.
#   # 👉 Resultado: lista de todas las rutas de archivos .csproj dentro de "$WORK_DIR/proj_validator".
#   # | head -n1 👉 Toma solo la primera línea de la salida de find. Es decir, el primer archivo .csproj encontrado. 
#   # || true 👉 Si find ... | head -n1 devuelve código de salida distinto de 0 (por ejemplo, si no encuentra nada), el || true fuerza a que la subshell complete con éxito. Esto evita que el script se caiga por set -e u opciones similares.
#   CSPROJ=$(find "$WORK_DIR/proj_validator" -type f -name "*.csproj" | head -n1 || true)
#
#   #if [ -n "$CSPROJ" ]; then
#   #  # Solo si el csproj NO tiene aún Microsoft.CodeAnalysis.CSharp
#   #  if ! grep -q 'Microsoft.CodeAnalysis.CSharp' "$CSPROJ"; then # 👉 Verifica si ya tiene la referencia; si ya la tiene, no toca el archivo.
#   #    echo "Agregando referencia a Microsoft.CodeAnalysis.CSharp en $CSPROJ..."
#
#   #    TMPFILE=$(mktemp)
#
#   #    # Cuando encuentra </Project>, primero imprime el <ItemGroup> ... </ItemGroup>,
#   #    # y después, por la regla { print }, imprime la línea actual (</Project>).
#   #    # Resultado: el bloque <ItemGroup> queda insertado justo antes de </Project>.
#   #    awk '
#   #      /<\/Project>/ {
#   #        print "  <ItemGroup>"
#   #        print "    <PackageReference Include=\"Microsoft.CodeAnalysis.CSharp\" Version=\"4.8.0\" />"
#   #        print "  </ItemGroup>"
#   #      }
#   #      { print }
#   #    ' "$CSPROJ" > "$TMPFILE"
#   #
#   #    mv "$TMPFILE" "$CSPROJ"
#   #  fi
#   #else
#   #  echo "WARN: No se encontró ningún archivo .csproj en $WORK_DIR/proj_validator"
#   #fi
#   cat $CSPROJ
#
#   VALIDATOR_BUILD_LOG="$WORK_DIR/validator_build.log"
#   ( cd "$WORK_DIR/proj_validator" && dotnet build -c Release --nologo --no-restore ) >"$VALIDATOR_BUILD_LOG" 2>&1 || true
#   cat "$VALIDATOR_BUILD_LOG"
#   
#   VALIDATOR_DLL=$(find "$WORK_DIR/proj_validator/bin/Release" -type f -name "*.dll" | head -n1 || true)
#
#   if [ -z "$VALIDATOR_DLL" ]; then
#     echo "ERROR: no se pudo compilar el validador"
#     exit 5
#   fi
#fi
# --- NUEVO FLUJO: usar DLL ya montado ---
VALIDATOR_DLL=""
if [ -n "$VALIDATOR_SRC" ] && [ -f "$VALIDATOR_SRC" ]; then
  # [ -n "$VALIDATOR_SRC" ] Comprueba si la variable $VALIDATOR_SRCno está vacía (es decir, tiene algún valor asignado).
  # -n = "non-zero length string" (cadena con longitud mayor que cero).
  # &&
  # Operador lógico AND. Solo evalúa la segunda condición si la primera es verdadera.
  # [ -f "$VALIDATOR_SRC" ] Comprueba si existe un archivo regular (no un directorio, no un enlace, etc.) en la ruta almacenada en $VALIDATOR_SRC.
  # -f = "file exists and is a regular file".
  # ; then
  # Si ambas condiciones son verdaderas (la variable tiene valor y apunta a un archivo real), se ejecuta el bloque siguiente.
  echo "Usando validador precompilado pasado como argumento: $VALIDATOR_SRC"
  VALIDATOR_DLL="$VALIDATOR_SRC"
elif [ -d "/home/sandbox/validator" ]; then
  # Buscar cualquier DLL si no se pasó como argumento
  echo "[DEBUG] Contenido de /home/sandbox/validator/:"
  ls -l /home/sandbox/validator/
  echo "[DEBUG] VALIDATOR_SRC: $VALIDATOR_SRC"
  VALIDATOR_DLL=$(find /home/sandbox/validator -maxdepth 1 -type f -name "*.dll" | head -n1 || true)
  if [ -n "$VALIDATOR_DLL" ]; then
    echo "Usando validador encontrado en /home/sandbox/validator/: $VALIDATOR_DLL"
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
    # En Bash, cuando se guarda un comando en un array, se lo ejecuta expandiéndolo con:
    # EXEC=(dotnet /home/sandbox/proj/bin/Release/net8.0/App.dll)
    # "${EXEC[@]}" arg1 arg2
    # Esto se expande así:
    # dotnet /home/sandbox/proj/bin/Release/net8.0/App.dll arg1 arg2
    { /usr/bin/time -f "TIME=%E\nMEM=%MKB" timeout "${TIME_LIMIT}s" "${EXEC[@]}" < "$infile"; } \
      >"$actual" 2> "$GEN_DIR/metrics_${base}.log"
    RC=$?
    set -e
    echo ">>> RC=$RC"

    if [ $RC -eq 124 ]; then
      STATUS_RUN="error"
      DETAILS+="Dataset $base: ⏱ Timeout (excedió ${TIME_LIMIT}s)\n"
      break;
    elif [ $RC -ne 0 ]; then
      STATUS_RUN="error"
      DETAILS+="Dataset $base: ❌ ejecución falló (RC=$RC)\n"
      continue
    fi
    
    # 🔹 Si hay validador, se ejecuta en lugar del diff
    if [ -n "$VALIDATOR_DLL" ]; then
      echo "Ejecutando validador sobre $actual"
      set +e
      if [ "$LANGUAGE" = "dotnet" ]; then
        echo "Ejectuando validador con opción de verficación de firma"
      	VAL_OUT=$(dotnet "$VALIDATOR_DLL" "$infile" "$expected" "$actual" "$WORK_DIR/proj/Program.cs")
      else
        VAL_OUT=$(dotnet "$VALIDATOR_DLL" "$infile" "$expected" "$actual")
      fi
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

