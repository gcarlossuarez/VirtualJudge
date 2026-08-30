@echo off
setlocal enabledelayedexpansion

:: 1. Crear el directorio OUT si no existe
if not exist OUT mkdir OUT

:: 2. Obtener el nombre del directorio actual
for %%I in ("%cd%") do set "APP_NAME=%%~nxI"

:: 3. Definir la ruta del ejecutable (.net 10.0)
set "EXE_PATH=.\bin\Debug\net10.0\!APP_NAME!.exe"

:: Verificar si el ejecutable realmente existe
if not exist "!EXE_PATH!" (
    echo Error: No se encontro el ejecutable en !EXE_PATH!
    pause
    exit /b
)

:: 4. Listar y procesar cada archivo .txt en IN
for %%F in (IN\*.txt) do (
    echo Procesando: %%~nxF
    
    :: Ejecutar con entrada (<) y salida (>) redireccionadas
    "!EXE_PATH!" < "%%F" > "OUT\Output_%%~nxF"
)

echo Proceso terminado con exito!
pause

:: ## Explicación paso a paso de los comandos
 
:: * if not exist OUT mkdir OUT: Revisa si la carpeta existe. Si no está, la crea automáticamente.
:: * %%~nxI (dentro del primer ciclo): Obtiene solo el nombre y la extensión de la carpeta actual (eliminando toda la ruta larga de atrás).
:: * !APP_NAME!.exe: Usa el nombre de la carpeta para armar el nombre del programa ejecutable de .NET 10.0.
:: * for %%F in (IN\*.txt): Busca todos los archivos que terminen en .txt exclusivamente dentro de la carpeta IN.
:: * < "%%F": Redirecciona el flujo de entrada. Envía todo el contenido de ese archivo de texto directamente hacia el programa.
:: * > "OUT\Output_%%~nxF": Redirecciona el flujo de salida. Graba todo lo que el programa responda dentro de OUT, agregando el prefijo Output_ al nombre del archivo original (%%~nxF).
