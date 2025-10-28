# ¿Qué es `dotnet restore` y por qué es obligatorio?

> **Resumen:** `dotnet restore` descarga e instala todas las librerías externas que necesita tu proyecto .NET antes de compilar.

---

## ¿Para qué sirve?

Cuando se crea un proyecto .NET, se tiene un archivo `.csproj` como este:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>

Este archivo no incluye el código de .NET. Solo dice:

"Necesito .NET 8.0"

Pero .NET 8.0 tiene miles de archivos DLL que no vienen con tu código.

¿Qué hace dotnet restore?

Lee el .csproj
Ve qué versión de .NET necesitas (net8.0)
Descarga todos los paquetes necesarios desde nuget.org
Los guarda en una carpeta caché global:
text~/.nuget/packages/

Genera un archivo clave:
textobj/project.assets.json


Este archivo dice:

"Ya tengo todo lo que necesito para compilar"


¿Qué pasa si no haces restore?
bashdotnet build --no-restore
→ FALLA con:
texterror NETSDK1004: Assets file 'obj/project.assets.json' not found.
Run a NuGet package restore to generate this file.

Analogía simple


AcciónEquivalente en la vida real
dotnet restore
Ir al supermercado a comprar harina, huevos, azúcardotnet build
Hacer el pastel
dotnet build --no-restore
Hacer el pastel sin ir al super, asumiendo que ya se tiene todo
→ Si no se fue al super → no se puede hacer el pastel.

¿Cuándo usar cada comando?


Comando                     Cuándo usarlo    
dotnet restore              Siempre antes de la primera compilación
dotnet build                Hace restore + build (lento)
dotnet build --no-restore   Solo si ya hiciste restore antes (rápido)

Ejemplo correcto en un script
bash
#1. Restaurar paquetes (OBLIGATORIO)
dotnet restore --no-cache

# 2. Compilar (rápido, ya tiene todo)
dotnet build -c Release --no-restore


#Conclusión

dotnet restore es OBLIGATORIO antes de dotnet build --no-restore
Sin él → project.assets.json no existe → compilación falla
En entornos sandbox (jueces, CTF, servidores) → siempre hacer restore
