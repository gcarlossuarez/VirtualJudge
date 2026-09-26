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
