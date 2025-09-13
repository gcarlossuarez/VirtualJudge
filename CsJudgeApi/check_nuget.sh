#!/bin/bash
set -eu pipefail

PKG_DIR="$HOME/nuget-local"

echo "📦 Verificando paquetes en $PKG_DIR ..."
cd "$PKG_DIR"

for pkg in *.nupkg; do
    echo "🔍 Revisando $pkg ..."
    if ! unzip -t "$pkg" >/dev/null 2>&1; then
        echo "⚠️  $pkg está corrupto. Re-descargando..."
        # Extraer nombre y versión del archivo
        name=$(echo "$pkg" | sed -E 's/(.+)\.([0-9]+\.[0-9]+\.[0-9]+.*)\.nupkg/\1/')
        version=$(echo "$pkg" | sed -E 's/.+\.(.+)\.nupkg/\1/')
        echo "   → Paquete: $name, versión: $version"
        rm -f "$pkg"
        wget -4 "https://www.nuget.org/api/v2/package/$name/$version" -O "$pkg"
        echo "✅ Re-descargado: $pkg"
    else
        echo "✅ $pkg está OK"
    fi
done

echo "🎉 Todos los paquetes verificados."

