# Prueba del Endpoint: Restaurar Rutas de Validadores

## 1️⃣ **Método HTTP**: POST
## 2️⃣ **URL**: `http://localhost:5000/api/admin/restore-validator-paths`
## 3️⃣ **Content-Type**: `application/json`

### ✅ **Ejemplo 1: Usar backup por defecto**

```bash
curl -X POST http://localhost:5000/api/admin/restore-validator-paths \
  -H "Content-Type: application/json" \
  -d '{}' \
  -w "\n"
```



**Respuesta esperada:**
```json
{
  "success": true,
  "restored": 16,
  "failed": 2,
  "savedChanges": 16,
  "message": "Restauración completada exitosamente"
}
```


#### Análisis del Comando cURL

El comando ejecutado realiza una solicitud HTTP utilizando la herramienta de línea de comandos **cURL** con el objetivo de interactuar con una interfaz de programación de aplicaciones (API) local. Específicamente, invoca un proceso administrativo diseñado para la restauración de rutas de validación (`restore-validator-paths`).

##### Desglose de Parámetros

*   **`curl`**: Invoca la herramienta de transferencia de datos basada en protocolos de red.
*   **`-X POST`**: Especifica el uso del método HTTP POST. Esto indica al servidor que la solicitud tiene como fin enviar datos o desencadenar una acción que altera el estado del sistema.
*   **`http://localhost:5000/api/admin/restore-validator-paths`**: Corresponde a la dirección URL de destino (*endpoint*). Señala que el servicio se encuentra alojado en la máquina local (`localhost`), escucha a través del puerto `5000` y expone una ruta de nivel administrativo.
*   **`-H "Content-Type: application/json"`**: Define un encabezado (*header*) HTTP. Informa al servidor que la estructura de los datos contenidos en el cuerpo de la petición sigue el formato estandarizado JSON.
*   **`-d '{}'`**: Representa el cuerpo de la petición (*payload*). En este caso, se transmite un objeto JSON vacío, lo cual sugiere que el *endpoint* requiere una acción de tipo POST pero no necesita variables adicionales para su ejecución básica.
*   **`-w "\n"`**: Utiliza la opción *write-out* para formatear la salida en la consola. Añade un salto de línea al finalizar la recepción de la respuesta, optimizando la legibilidad en el entorno de la terminal.

##### Mecanismo de Funcionamiento

El flujo operativo se divide en los siguientes pasos:
1. El cliente establece una conexión local con el servidor en el puerto especificado.
2. Se transmite la petición POST junto con el encabezado de contenido y el objeto vacío.
3. El servidor recibe la instrucción en la ruta administrativa y ejecuta la lógica interna de restauración.
4. El servidor retorna un código de estado junto con la respuesta correspondiente, la cual es mostrada en la terminal de forma ordenada gracias al salto de línea final.

---

### ✅ **Ejemplo 2: Especificar ruta de backup personalizada**

```bash
curl -X POST http://localhost:5000/api/admin/restore-validator-paths \
  -H "Content-Type: application/json" \
  -d '{"backupPath": "/path/to/backup/submissions.db.bkp.2026-08-17"}' \
  -w "\n"
```

---

### ✅ **Ejemplo 3: Con PowerShell/Windows**

```powershell
$uri = "http://localhost:5000/api/admin/restore-validator-paths"
$body = @{"backupPath" = "submissions.db.bkp.2026-08-17"} | ConvertTo-Json

Invoke-WebRequest -Uri $uri `
  -Method POST `
  -ContentType "application/json" `
  -Body $body | ConvertTo-Json -Depth 10
```

---

### ✅ **Ejemplo 4: Con JavaScript/Fetch**

```javascript
const response = await fetch('http://localhost:5000/api/admin/restore-validator-paths', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    backupPath: 'submissions.db.bkp.2026-08-17'
  })
});

const result = await response.json();
console.log(result);
```

---

## 📊 **Respuesta del Endpoint**

### ✅ **Éxito**:
```json
{
  "success": true,
  "restored": 16,      // Preguntas con rutas restauradas
  "failed": 2,         // Preguntas donde el archivo no existe
  "savedChanges": 16,  // Cambios guardados en BD
  "message": "Restauración completada exitosamente"
}
```

### ❌ **Error**:
```json
{
  "success": false,
  "error": "Archivo de backup no encontrado: submissions.db.bkp.2026-08-17",
  "message": "Error al restaurar validadores"
}
```

---

## 🔍 **Verificar Resultados en BD**

Después de restaurar, verificar con SQLite:

```bash
# Ver cuántos validadores se restauraron
sqlite3 submissions.db "
  SELECT COUNT(*) as TotalQuestions, 
         COUNT(CASE WHEN FullPathValidatorSourceCode != '' THEN 1 END) as WithValidators
  FROM Questions;
"

# Ver ejemplos
sqlite3 submissions.db "
  SELECT QuestionId, Path, FullPathValidatorSourceCode 
  FROM Questions 
  WHERE FullPathValidatorSourceCode != '' 
  LIMIT 5;
"
```

---

## 📝 **Proceso Interno del Servicio**

1. ✅ Abre el archivo de backup
2. ✅ Lee todas las preguntas con validadores
3. ✅ Verifica que cada archivo EXISTE en el filesystem
4. ✅ Restaura solo los que existen
5. ✅ Reporta fallos para archivos que NO existen
6. ✅ Guarda cambios en la BD
7. ✅ Registra todo en logs

---

## 🚀 **Iniciar el Servidor**

```bash
cd /home/virtualbox/VirtualJudge/CsJudgeApi
dotnet run --urls "http://0.0.0.0:5000"
```

---

## 📊 **Comparativo Antes/Después**

### ANTES (Base de datos dañada):
- Total preguntas: 64
- Con validadores: 2 (89% pérdida)

### DESPUÉS (Después de restaurar):
- Total preguntas: 64 (sin cambios)
- Con validadores: 18+ (restauradas desde backup)
- Integridad: ✅ Recuperada
