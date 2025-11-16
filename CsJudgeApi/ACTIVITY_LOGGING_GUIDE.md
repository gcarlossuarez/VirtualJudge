# 📊 Sistema de Logging de Actividad - Guía de Uso

## ✅ Implementación Completa

Se ha implementado un sistema completo de logging de actividad con las siguientes características:

### 🗄️ Base de Datos

**Tabla:** `ActivityLogs`
- `Id` (PK autoincremental)
- `Timestamp` (fecha/hora automática)
- `Action` (enum de tipo de acción)
- `StudentId` (nullable)
- `ContestId` (nullable)
- `QuestionId` (nullable)
- `Metadata` (JSON para datos adicionales)
- `IpAddress`
- `UserAgent`

**Índices creados:**
- `IX_ActivityLogs_Action`
- `IX_ActivityLogs_ContestId`
- `IX_ActivityLogs_QuestionId`
- `IX_ActivityLogs_StudentId`
- `IX_ActivityLogs_Timestamp`

### 📝 Tipos de Acciones (EActivityAction)

1. **ContestLoaded** (1) - Cuando un estudiante carga la lista de problemas de un contest
2. **ProblemViewed** (2) - Cuando un estudiante ve la descripción de un problema
3. **SubmissionSent** (3) - Cuando un estudiante envía una solución
4. **LocalValidation** (4) - Validación local con sandbox (reservado)
5. **DatasetSynced** (5) - Sincronización de datasets (reservado)
6. **SdkDownloaded** (6) - Descarga del .NET SDK (reservado)
7. **ContestCreated** (7) - Cuando se crea un nuevo contest (admin)
8. **ContestEdited** (8) - Cuando se edita un contest (admin, reservado)
9. **StudentAdded** (9) - Cuando un estudiante se registra en un contest **por primera vez**
10. **StudentRemoved** (10) - Cuando se elimina un estudiante (admin, reservado)
11. **StudentLogin** (11) - **Cada vez que un estudiante entra al juez** (para estadísticas de uso)

---

## 🎯 Endpoints con Logging Automático

### 1. **POST /submit** - Envío de soluciones
**Registra:** `SubmissionSent`
```json
{
  "studentId": "123",
  "problemId": "1-2",
  "sourceCode": "...",
  "language": "csharp",
  "expected": "...",
  "actual": "..."
}
```
**Metadata registrada:**
```json
{
  "language": "csharp",
  "isCorrect": true,
  "codeLength": 245
}
```

---

### 2. **GET /questions/{id}/desc** - Ver descripción de problema
**Registra:** `ProblemViewed`

**Ejemplo:**
```bash
curl http://localhost:5000/questions/1/desc
```

---

### 3. **GET /contest/questions** - Cargar lista de problemas
**Registra:** `ContestLoaded`

**Ejemplo:**
```bash
curl http://localhost:5000/contest/questions
```

**Metadata registrada:**
```json
{
  "questionCount": 5
}
```

---

### 4. **POST /api/student-login** - Login/registro de estudiante
**Registra:** `StudentAdded` (solo para nuevos registros)

**Ejemplo:**
```bash
curl -X POST http://localhost:5000/api/student-login \
  -H "Content-Type: application/json" \
  -d '{
    "studentId": "123",
    "timestamp": "2025-11-16T10:30:00",
    "action": "login"
  }'
```

**Metadata registrada:**
```json
{
  "studentName": "Juan Pérez",
  "registrationIp": "192.168.1.100"
}
```

---

## 🔧 Nuevos Endpoints Administrativos

### 5. **POST /api/admin/contests** - Crear nuevo contest
**Registra:** `ContestCreated`

**Ejemplo:**
```bash
curl -X POST http://localhost:5000/api/admin/contests \
  -H "Content-Type: application/json" \
  -d '{
    "date": "2025-12-01T09:00:00"
  }'
```

**Respuesta:**
```json
{
  "success": true,
  "contestId": 42,
  "date": "2025-12-01T09:00:00",
  "message": "Contest 42 creado exitosamente"
}
```

---

### 6. **GET /api/admin/activity-stats** - Estadísticas de uso
**No registra actividad** (es de consulta)

**Parámetros:**
- `days` (opcional, default: 30) - Número de días hacia atrás

**Ejemplos:**
```bash
# Últimos 30 días (default)
curl http://localhost:5000/api/admin/activity-stats

# Últimos 7 días
curl http://localhost:5000/api/admin/activity-stats?days=7

# Últimas 24 horas
curl http://localhost:5000/api/admin/activity-stats?days=1
```

**Respuesta:**
```json
{
  "period": "Últimos 30 días",
  "fromDate": "2025-10-17T00:00:00Z",
  "totalActivities": 1547,
  "uniqueStudents": 45,
  "activityByType": [
    {
      "action": "SubmissionSent",
      "actionId": 3,
      "count": 823,
      "lastActivity": "2025-11-16T14:23:15Z"
    },
    {
      "action": "ProblemViewed",
      "actionId": 2,
      "count": 412,
      "lastActivity": "2025-11-16T14:20:10Z"
    },
    {
      "action": "ContestLoaded",
      "actionId": 1,
      "count": 156,
      "lastActivity": "2025-11-16T13:45:32Z"
    },
    {
      "action": "StudentAdded",
      "actionId": 9,
      "count": 45,
      "lastActivity": "2025-11-15T10:15:22Z"
    }
  ]
}
```

---

## 📊 Consultas SQL Útiles

### Ver todas las actividades recientes
```sql
SELECT 
  Id,
  datetime(Timestamp) as Fecha,
  Action,
  StudentId,
  ContestId,
  QuestionId,
  IpAddress
FROM ActivityLogs
ORDER BY Timestamp DESC
LIMIT 50;
```

### Contar actividades por tipo
```sql
SELECT 
  Action,
  COUNT(*) as Total
FROM ActivityLogs
GROUP BY Action
ORDER BY Total DESC;
```

### Ver actividad de un estudiante específico
```sql
SELECT 
  datetime(Timestamp) as Fecha,
  Action,
  ContestId,
  QuestionId,
  json_extract(Metadata, '$.language') as Lenguaje,
  json_extract(Metadata, '$.isCorrect') as Correcto
FROM ActivityLogs
WHERE StudentId = 123
ORDER BY Timestamp DESC;
```

### Actividad por día
```sql
SELECT 
  DATE(Timestamp) as Dia,
  Action,
  COUNT(*) as Total
FROM ActivityLogs
WHERE Timestamp >= datetime('now', '-7 days')
GROUP BY Dia, Action
ORDER BY Dia DESC, Total DESC;
```

### Estudiantes más activos
```sql
SELECT 
  StudentId,
  COUNT(*) as TotalActividades,
  COUNT(DISTINCT DATE(Timestamp)) as DiasActivos,
  MIN(Timestamp) as PrimeraActividad,
  MAX(Timestamp) as UltimaActividad
FROM ActivityLogs
WHERE StudentId IS NOT NULL
GROUP BY StudentId
ORDER BY TotalActividades DESC
LIMIT 10;
```

### **⭐ REPORTE DE ENTRADAS AL JUEZ (Para evidencia de uso)**
```sql
-- Ver todas las entradas de estudiantes con nombres
SELECT 
  datetime(al.Timestamp, 'localtime') as FechaHora,
  s.StudentId,
  s.Name as NombreEstudiante,
  al.ContestId,
  json_extract(al.Metadata, '$.isNewRegistration') as EsPrimeraVez,
  al.IpAddress
FROM ActivityLogs al
JOIN Students s ON al.StudentId = s.StudentId
WHERE al.Action = 11  -- StudentLogin
ORDER BY al.Timestamp DESC;
```

### **⭐ CONTEO DE ENTRADAS POR ESTUDIANTE (Para identificar quién NO entra)**
```sql
-- Cuántas veces ha entrado cada estudiante
SELECT 
  s.StudentId,
  s.Name as NombreEstudiante,
  COUNT(*) as TotalEntradas,
  COUNT(DISTINCT DATE(al.Timestamp)) as DiasDistintos,
  MIN(datetime(al.Timestamp, 'localtime')) as PrimeraEntrada,
  MAX(datetime(al.Timestamp, 'localtime')) as UltimaEntrada
FROM Students s
LEFT JOIN ActivityLogs al ON s.StudentId = al.StudentId AND al.Action = 11
WHERE s.StudentId IN (
  SELECT DISTINCT StudentId FROM ContestStudents WHERE ContestId = 1  -- Cambiar ContestId
)
GROUP BY s.StudentId, s.Name
ORDER BY TotalEntradas DESC;
```

### **⭐ ESTUDIANTES QUE NO HAN ENTRADO (Lista de ausentes)**
```sql
-- Estudiantes registrados en el contest pero que NUNCA han entrado
SELECT 
  s.StudentId,
  s.Name as NombreEstudiante,
  cs.DateParticipation as FechaRegistro
FROM ContestStudents cs
JOIN Students s ON cs.StudentId = s.StudentId
WHERE cs.ContestId = 1  -- Cambiar ContestId
  AND NOT EXISTS (
    SELECT 1 FROM ActivityLogs al 
    WHERE al.StudentId = s.StudentId 
      AND al.Action = 11  -- StudentLogin
      AND al.ContestId = cs.ContestId
  )
ORDER BY s.Name;
```

### Problemas más vistos
```sql
SELECT 
  QuestionId,
  COUNT(*) as Vistas,
  COUNT(DISTINCT StudentId) as EstudiantesUnicos
FROM ActivityLogs
WHERE Action = 2  -- ProblemViewed
  AND QuestionId IS NOT NULL
GROUP BY QuestionId
ORDER BY Vistas DESC;
```

---

## 🧪 Pruebas

### 1. Verificar que la tabla existe:
```bash
sqlite3 submissions.db ".tables"
# Debe aparecer ActivityLogs
```

### 2. Ver estructura de la tabla:
```bash
sqlite3 submissions.db "PRAGMA table_info(ActivityLogs);"
```

### 3. Simular actividad y verificar:
```bash
# Cargar contest
curl http://localhost:5000/contest/questions

# Ver si se registró
sqlite3 submissions.db "SELECT * FROM ActivityLogs ORDER BY Id DESC LIMIT 1;"
```

---

## 💡 Ventajas de esta Implementación

✅ **Tipado fuerte:** Enum previene errores de tipeo en queries  
✅ **Refactoring seguro:** Cambiar nombres no rompe el código  
✅ **Optimizado:** Índices en campos más consultados  
✅ **Flexible:** Metadata JSON para datos adicionales sin cambiar schema  
✅ **Eficiente:** ~100 bytes por log, escala bien  
✅ **No intrusivo:** Se registra automáticamente sin cambiar lógica de negocio  
✅ **Auditable:** IP y UserAgent para análisis de seguridad  

---

## 🔜 Próximos Pasos (Opcional)

1. **Dashboard en frontend:** Mostrar estadísticas en tiempo real
2. **Alertas:** Notificar actividad sospechosa (múltiples IPs, etc.)
3. **Exportar reportes:** Generar CSV/PDF de estadísticas
4. **Limpieza automática:** Archivar logs antiguos (>90 días)
5. **Más eventos:** DatasetSynced, SdkDownloaded, LocalValidation
