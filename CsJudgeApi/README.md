# CsJudgeApi (Minimal API)

API mínima que envuelve el contenedor `cs-single-runner:1` para compilar/ejecutar programas C# tipo ICPC.

## Requisitos
- .NET SDK 8.0+
- Docker con permisos para tu usuario
- Imagen `cs-single-runner:1` ya construida

## Ejecutar
```bash
dotnet restore
dotnet run
# la API queda en http://localhost:5000 (o http://localhost:5238 según puerto)
```

## Endpoints

### `POST /compile-run`

Formulario `multipart/form-data`:

- **code**: obligatorio. `.cs` o `.zip` (con uno o más `.cs`).
- **input**: opcional. Archivo `input.txt`.
- **stdin**: opcional. Texto crudo para usar como stdin (si no envías `input`).
- **timeLimit**: opcional. Entero (1-30). Por defecto 6.
- **keep**: opcional. `"true"` para conservar la carpeta temporal (debug).

Ejemplo con `curl`:
```bash
# .cs + stdin
curl -F "code=@/ruta/solucion.cs" \
     -F "stdin=7 35" \
     -F "timeLimit=6" \
     http://localhost:5000/compile-run

# ZIP con varios .cs + input.txt
curl -F "code=@/ruta/fuentes.zip" \
     -F "input=@/ruta/input.txt" \
     http://localhost:5000/compile-run
```

Respuesta:
```jsonc
{
  "id": "c3c7...",
  "exitCode": 0,
  "buildStatus": "ok",
  "runStatus": "ok",
  "time": "0:00.01",
  "memKB": "36000",
  "buildLog": "...",
  "runLog": "...",
  "stderrRaw": "",
  "stdoutRaw": "===BUILD===..."
}
```

### `GET /healthz`
Devuelve versión de Docker y estado.

## Tips de despliegue
- Levantar detrás de Nginx/Apache (reverse proxy).
- Limitar tamaño de subida en el proxy (p. ej. 10-20MB).
- Ejecutar con usuario de sistema y `Systemd` para autoinicio.
- No expongas el demonio Docker remoto.
- Rate limiting básico con un proxy o middleware si abres al público.

### Estructura de las preguntas
```textplain
/problems/
   ├── P001/
   │    ├── IN/
   │    │    ├── datos0001.txt
   │    │    ├── datos0002.txt
   │    └── OUT/
   │         ├── out_datos0001.txt
   │         ├── out_datos0002.txt
   ├── P002/
   │    ├── IN/...
   │    └── OUT/...
```
