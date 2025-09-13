# Guía de tu script `compile_run_in_nsjail.sh` (C# + nsjail, offline y seguro)

> Objetivo: compilar y ejecutar código **C# (.NET 8)** dentro de **nsjail** sin acceso a Internet, con filesystem de solo lectura y tmpfs para trabajo, minimizando riesgos y evitando `restore`/NuGet.

---

## Resumen rápido (TL;DR)

- **Aislamiento**: user namespace (root interno mapeado a `nobody` en el host), chroot `/tmp`, system dirs **RO**, `/work` y `/tmp` en **tmpfs** `1777`.
- **.NET**: se usa el SDK **local** (`/usr/lib/dotnet`) y se compila **offline** con `csc.dll` + referencias locales.
- **Sin “first-time experience”** ni workloads; variables de entorno ajustadas para no escribir fuera de `/work`/`/tmp`.
- **Resultado**: `app.exe` se ejecuta dentro del jail con `dotnet /work/app.exe`.

---

## Sección por sección

### 1) Cabecera y “modo estricto” del shell

```sh
set -eu
# Activa pipefail solo si es bash
if [ -n "${BASH_VERSION-}" ]; then
  set -o pipefail
fi
```

- `set -e`: sale ante el **primer error**.
- `set -u`: error si usas variables **no definidas**.
- `set -o pipefail` (solo en bash): si una etapa de un **pipe** falla, el pipeline se considera fallido. Como `sh`/`dash` no lo soporta, lo activas solo si existe `BASH_VERSION`.

**Consejo**: ejecuta el script con `bash` (o pon `#!/usr/bin/env bash` al inicio).

---

### 2) Parámetros y archivos de ejemplo

```sh
SRC="${1:-/tmp/nsjail-test/hello.cs}"
OUTNAME="${2:-hello}"
mkdir -p /tmp/nsjail-test
[ -f "$SRC" ] || cat > /tmp/nsjail-test/hello.cs <<'CS'
using System;
class Program { static void Main() => Console.WriteLine("Hello, World!"); }
CS
```

- `SRC`: ruta del **.cs** (por defecto uno de ejemplo).
- `OUTNAME`: nombre base de salida (no imprescindible si usas `/work/app.exe`).
- Si no existe `SRC`, se crea un **Hello World**.

#### `hello.runtimeconfig.json` mínimo

```json
{
  "runtimeOptions": {
    "tfm": "net8.0",
    "framework": { "name": "Microsoft.NETCore.App", "version": "8.0.0" }
  }
}
```

- Indica al host de .NET que la app es **net8.0** y usa el framework `Microsoft.NETCore.App`.
- Como la ejecución es **offline**, lo mantenemos simple (sin `restore`).

---

### 3) Lanzar **nsjail** (modo _standalone once_)

```sh
sudo nsjail -Mo \
  --time_limit=60 \
  --rlimit_cpu=20 \
  --rlimit_as=1073741824 \
  --rlimit_core=0 \
  --rlimit_fsize=inf \
  --rlimit_nofile=1024 \
  --user 0:65534:1 --group 0:65534:1 \
  --chroot /tmp \
  --bindmount_ro /usr \
  --bindmount_ro /lib \
  --bindmount_ro /lib64 \
  --bindmount_ro /etc \
  --bindmount_ro /dev \
  --bindmount_ro /usr/lib/dotnet \
  -m none:/work:tmpfs:mode=1777,size=1073741824 \
  -m none:/tmp:tmpfs:mode=1777,size=268435456 \
  --bindmount_ro "$SRC":/work/src.cs \
  --bindmount_ro /tmp/nsjail-test/hello.runtimeconfig.json:/work/app.runtimeconfig.json \
  -- /usr/bin/bash -s <<'EOF'
  ...
EOF
```

**Claves de seguridad y rendimiento:**

- `-Mo`: **STANDALONE_ONCE** (una ejecución y termina).
- `--time_limit=60` / `--rlimit_cpu=20`: límites de **tiempo total** y **CPU** del proceso.
- `--rlimit_as=1073741824`: **1 GiB** de límite de memoria virtual (ajustable).
- `--rlimit_core=0`: **sin core dumps**.
- `--rlimit_fsize=inf`: permite archivos grandes (p.ej. compilación). En entornos muy restringidos, puedes bajarlo; si ves `File size limit exceeded`, súbelo.
- `--rlimit_nofile=1024`: aumenta descriptores abiertos (el CLI de .NET abre bastantes archivos).
- `--user 0:65534:1 --group 0:65534:1`: **mapeo seguro**: root **dentro** → `nobody` **fuera**.
- `--chroot /tmp`: raíz del jail.
- `--bindmount_ro` a **sistemas**: `/usr`, `/lib`, `/lib64`, `/etc`, `/dev` en **solo lectura**.
- `--bindmount_ro /usr/lib/dotnet`: SDK/host/runtime locales (sin Internet).
- `-m none:/work:tmpfs:mode=1777,...` y `-m none:/tmp:tmpfs:mode=1777,...`: áreas **RW** seguras; `1777` imita permisos de `/tmp` (sticky bit).
- `--bindmount_ro "$SRC":/work/src.cs`: inyecta tu **fuente** al jail (RO).
- `--bindmount_ro ...runtimeconfig.json`: inyecta el runtimeconfig para la app.

> **Tip**: si `bash` está en otra ruta (p.ej. `/bin/bash` vs `/usr/bin/bash`), ajusta el binario tras `--`.

---

### 4) Script **dentro** del jail (HEREDOC)

```sh
set -euo pipefail
export DOTNET_ROOT=/usr/lib/dotnet
export HOME=/work
export TMPDIR=/tmp
export DOTNET_CLI_HOME=/work
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE=1
export DOTNET_NOLOGO=1
```

- `DOTNET_ROOT`: base del SDK/host (en tu Ubuntu: `/usr/lib/dotnet`).
- `HOME`, `TMPDIR`, `DOTNET_CLI_HOME`: fuerzan escrituras a **/work**/**/tmp** (tmpfs).
- `DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1`: evita tareas de “primer uso” (descargas/config).
- `DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE=1`: silencia avisos de workloads.
- `DOTNET_NOLOGO=1`: CLI más limpio.
- Puedes añadir `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1` para ignorar ICU si faltara.

#### Detección de rutas del SDK/runtime/refs

```sh
SDKDIR=$(ls -d /usr/lib/dotnet/sdk/* 2>/dev/null | sort -V | tail -1)
RUNTIMEDIR=$(ls -d /usr/lib/dotnet/shared/Microsoft.NETCore.App/* 2>/dev/null | sort -V | tail -1)
REFDIR=$(ls -d /usr/lib/dotnet/packs/Microsoft.NETCore.App.Ref/*/ref/net8.0 2>/dev/null | sort -V | tail -1 || true)

CSC="$SDKDIR/Roslyn/bincore/csc.dll"
```

- Toma la **última versión** disponible del SDK y del runtime netcore.
- **`csc.dll`** es el compilador Roslyn que usa el host `dotnet`.

#### Referencias para compilar

```sh
REFS=()
if [ -n "${REFDIR:-}" ] && [ -d "$REFDIR" ]; then
  for f in "$REFDIR"/*.dll; do REFS+=("-r:$f"); done
else
  # Fallback: usar runtime (menos ideal pero válido offline)
  for f in "$RUNTIMEDIR"/*.dll; do REFS+=("-r:$f"); done
fi
```

- Preferimos el **Reference Pack** (`Microsoft.NETCore.App.Ref`) → diagnósticos más correctos y ABI estable.
- Si no está instalado, se cae al **runtime** (`shared/Microsoft.NETCore.App/...`). Es válido **offline**.

#### Compilar y ejecutar

```sh
dotnet "$CSC" /work/src.cs -out:/work/app.exe "${REFS[@]}"
dotnet /work/app.exe
```

- La **compilación** se hace totalmente **offline** (sin `restore`).
- La **ejecución** usa el host local de .NET dentro del jail, con el `runtimeconfig.json` que montaste.

---

## Variantes y ajustes útiles

- **Más memoria / menos memoria**: cambia `--rlimit_as` (y tamaños de tmpfs).
- **Más tiempo**: ajusta `--time_limit` y/o `--rlimit_cpu`.
- **TLS/HTTPS** en apps reales: añade certificados en RO
  ```
  --bindmount_ro /etc/ssl --bindmount_ro /usr/share/ca-certificates
  ```
- **Sin Ref Pack** instalado**:** tu fallback al runtime ya cubre el caso (lo estás usando con éxito).

---

## Diagnóstico rápido

- Dentro del jail:
  - `ulimit -a` → confirma límites efectivos.
  - `dotnet --info` → confirma SDK/host/runtimes visibles.
- Si ves `File size limit exceeded (core dumped)`: suele ser un **crash previo** + shell message.
  - Asegura `--rlimit_core=0` y usa `--rlimit_fsize=inf` al compilar.
- Verifica rutas:
  - `which nsjail` / `nsjail -h | head -n 50`
  - `ls /usr/lib/dotnet/{sdk,shared,packs} -1`

---

## Cheatsheet (corta y al grano)

- **User mapping seguro**: `--user 0:65534:1 --group 0:65534:1` (root adentro ⇒ `nobody` afuera).
- **Dirs RO**: `--bindmount_ro /usr /lib /lib64 /etc /dev /usr/lib/dotnet`.
- **Trabajo RW**: `-m none:/work:tmpfs:mode=1777,...` y `-m none:/tmp:tmpfs:mode=1777,...`.
- **.NET variables**: `DOTNET_ROOT=/usr/lib/dotnet`, `DOTNET_CLI_HOME=/work`, `TMPDIR=/tmp`.
- **Compilar offline**: `dotnet "$SDKDIR/Roslyn/bincore/csc.dll" fuente.cs -out:app.exe -r:...`.
- **Ejecutar**: `dotnet /work/app.exe` (con `app.runtimeconfig.json` montado).
- **Límites**: `--rlimit_core=0`, `--rlimit_fsize=inf`, `--rlimit_nofile=1024`.

---

