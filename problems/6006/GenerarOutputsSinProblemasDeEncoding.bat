@echo off
setlocal enabledelayedexpansion

set IN=IN
set OUT=OUT
set EXE=.\bin\Debug\net8.0\Solution.exe

if not exist %OUT% mkdir %OUT%

REM Para no tener probleas con el encoding
chcp 65001 >nul

echo === Procesando archivos en %IN% ===

for %%f in (%IN%\*.txt) do (
    set fname=%%~nxf
    echo Procesando %%f...
    %EXE% < "%%f" > "%OUT%\Output_!fname!"
)

echo === Proceso completado ===
pause
