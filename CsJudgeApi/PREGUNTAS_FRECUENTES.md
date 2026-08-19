# ❓ PREGUNTAS FRECUENTES - Implementación Multi-Semestre

## 🤔 PREGUNTAS GENERALES

### P1: ¿Cuál es el problema que resolvemos?

**R:** Actualmente, los estudiantes están almacenados en la tabla `Students` sin separación por semestre/grupo. Esto causa:
- ❌ No se puede saber a qué semestre pertenece cada estudiante
- ❌ Si un estudiante repite, hay conflicto de clave primaria
- ❌ Las queries retornan estudiantes de todos los semestres mezclados
- ❌ No hay forma de filtrar por grupo

**Solución:** Crear una tabla `StudentSemesters` (M:N) que relacione estudiantes con semestres.

---

### P2: ¿Esto afecta mi código actual?

**R:** SÍ, pero de forma controlada:

```csharp
// ❌ ANTES: Retorna todos los estudiantes
var students = await _context.Students.ToListAsync();

// ✅ DESPUÉS: Retorna solo del semestre actual
var currentSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.IsActive);
var students = await _context.StudentSemesters
    .Where(ss => ss.SemesterId == currentSemester.SemesterId)
    .Include(ss => ss.Student)
    .Select(ss => ss.Student)
    .Distinct()
    .ToListAsync();
```

Recomendación: **Actualizar solo la query de GetStudents() en StudentsController**.

---

### P3: ¿Pierdo datos históricos?

**R:** NO. Los datos históricos de 2025-2 se migran a la tabla `StudentSemesters`:

```sql
-- Los ~14 estudiantes de 2025-2 se copian a StudentSemesters
SELECT COUNT(*) FROM StudentSemesters WHERE SemesterId = 1;
-- Resultado: 14 estudiantes (históricos preservados)
```

---

### P4: ¿Cuánto tiempo toma?

**R:** 
- **Lectura**: 30 minutos
- **Implementación**: 85 minutos (1.5 horas)
- **Validación**: 15 minutos
- **TOTAL**: 2 horas

---

### P5: ¿Qué pasa si algo falla?

**R:** Rollback de 30 segundos:

```bash
# 1. Restaurar backup
cp submissions.db.bkp.2026-08-17 submissions.db

# 2. Revertir migración
dotnet ef migrations remove

# 3. Limpiar cambios
git checkout Models/Student.cs Data/AppDbContext.cs

# Vuelve todo al estado anterior
```

---

## 🏗️ PREGUNTAS TÉCNICAS

### P6: ¿Qué es una relación M:N?

**R:** Muchos-a-Muchos. Un estudiante puede estar en muchos semestres, un semestre puede tener muchos estudiantes:

```
Student (1) ──────M─────→ StudentSemester ←─────M──── Semester (1)
                    
Juan puede estar en:
  - 2025-2 (histórico)
  - 2026-2 (actual)
  - 2027-1 (futuro)
```

---

### P7: ¿Por qué no simplemente agregar columnas a Students?

**R:** Porque causaría este problema:

```
Estudiante: Juan (ID: 1001)

❌ OPCIÓN INCORRECTA (agregar Semester + Group a Students):
  StudentId | Name | Semester | Group
  1001      | Juan | 20252    | 1        ← 2025-2
  1001      | Juan | 20262    | 1        ← 2026-2 (CONFLICTO PK!)

✅ OPCIÓN CORRECTA (relación M:N):
  Students:
    StudentId | Name
    1001      | Juan
  
  StudentSemesters:
    StudentId | SemesterId | Group
    1001      | 1          | 1        ← 2025-2
    1001      | 2          | 1        ← 2026-2 (SIN CONFLICTO)
```

---

### P8: ¿Cómo funciona la clave primaria compuesta?

**R:** Una PK compuesta significa que la unicidad se valida en MÚLTIPLES columnas:

```sql
-- PK Compuesta: (StudentId, SemesterId)
-- Esto significa:
-- ✅ PERMITIDO: Mismo estudiante en diferentes semestres
-- ✅ PERMITIDO: Mismo semestre con diferentes estudiantes
-- ❌ NO PERMITIDO: Mismo estudiante EN EL MISMO semestre dos veces

-- Ejemplos:
-- ✅ OK: (1001, 20252) - Juan en 2025-2
-- ✅ OK: (1001, 20262) - Juan en 2026-2
-- ❌ NO: (1001, 20262) duplicado - Juan DOS VECES en 2026-2
```

---

### P9: ¿Cómo funciona el seeding?

**R:** El seed data se inserta automáticamente con la migración:

```csharp
// En OnModelCreating():
modelBuilder.Entity<Semester>().HasData(
    new Semester { SemesterId = 1, SemesterCode = 20252, Name = "2-2025", ... },
    new Semester { SemesterId = 2, SemesterCode = 20262, Name = "2-2026", ... }
);

// Cuando ejecutas: dotnet ef database update
// Automáticamente inserta estos datos
```

---

### P10: ¿Qué son los índices?

**R:** Permiten búsquedas más rápidas:

```sql
-- Índice en StudentSemesters:
CREATE INDEX IX_StudentSemester_SemesterGroup 
ON StudentSemesters(SemesterId, Group);

-- Sin índice:
  SELECT * FROM StudentSemesters WHERE SemesterId = 2 AND Group = 1
  -- Scande toda la tabla (lento)

-- Con índice:
  SELECT * FROM StudentSemesters WHERE SemesterId = 2 AND Group = 1
  -- Va directo al índice (rápido)
```

---

## 📊 PREGUNTAS DE DATOS

### P11: ¿Dónde van los estudiantes históricos (2025-2)?

**R:** Se migran automáticamente:

```bash
# Fase 4 del PLAN:
# Los ~14 estudiantes históricos se copian de Submissions a StudentSemesters:

INSERT INTO StudentSemesters (StudentId, SemesterId, Group, ParallelName)
SELECT DISTINCT StudentId, 1, 1, 'Paralelo 1'
FROM Submissions
WHERE StudentId NOT IN (SELECT DISTINCT StudentId FROM StudentSemesters);

# Resultado: ~14 registros en StudentSemesters con SemesterId=1
```

---

### P12: ¿Dónde van los estudiantes nuevos (2026-2)?

**R:** Se importan desde CSVs en Fase 5:

```bash
# De Lista_de_estudiantes_Paralelo_1.csv: ~14 estudiantes
# De Lista_de_estudiantes_Paralelo_2.csv: ~20 estudiantes
# Total: ~34 estudiantes nuevos en semestre 2

# En StudentSemesters:
# - Los 14 de Paralelo 1 con SemesterId=2, Group=1
# - Los 20 de Paralelo 2 con SemesterId=2, Group=2
```

---

### P13: ¿El tamaño de la BD aumenta mucho?

**R:** NO, muy poco:

```
Estimación:
  - 48 registros en Students: ~5 KB
  - 48 registros en StudentSemesters: ~3 KB
  - 2 registros en Semesters: <1 KB
  - Total nuevo: ~10 KB
  
Tamaño actual: ~50 MB
Aumento: ~10 KB (0.02%)

La BD SQLite soporta hasta 140 TB teóricamente.
```

---

### P14: ¿Qué pasa con Submissions antiguo?

**R:** NO se toca:

```sql
-- Submissions se mantiene igual
SELECT * FROM Submissions;
-- Sigue teniendo:
--   SubmissionId, StudentId, ProblemId, SourceCode, 
--   OutputExpected, OutputActual, IsCorrect, CreatedAt, etc.

-- Solo se usa para MIGRAR estudiantes en Fase 4:
SELECT DISTINCT StudentId FROM Submissions;
-- Esos IDs se copian a StudentSemesters
```

---

## 🔍 PREGUNTAS DE VALIDACIÓN

### P15: ¿Cómo verifico que todo está bien?

**R:** Sigue el CHECKLIST.md Fase 7:

```bash
# 1. Verificar estructuras
sqlite3 submissions.db ".schema StudentSemesters"
sqlite3 submissions.db ".schema Semesters"

# 2. Contar registros
sqlite3 submissions.db "SELECT COUNT(*) FROM StudentSemesters;"
# Esperado: ~48

# 3. Por semestre/grupo
sqlite3 submissions.db << 'EOF'
SELECT sem.Name, COUNT(ss.StudentId) as Total
FROM Semesters sem
LEFT JOIN StudentSemesters ss ON sem.SemesterId = ss.SemesterId
GROUP BY sem.SemesterId;
EOF
# Esperado:
# 2-2025|14
# 2-2026|34

# 4. Probar API
curl http://localhost:5000/api/students
# Esperado: ~34 estudiantes (solo 2026-2)
```

---

### P16: ¿Cómo sé si la migración funcionó?

**R:** Verificar con EF:

```bash
# Ver migraciones aplicadas
dotnet ef migrations list

# Ver cambios pendientes
dotnet ef migrations add --dry-run

# Aplicar migraciones
dotnet ef database update

# Si todo está bien: "Done" sin errores
```

---

## 🛠️ PREGUNTAS DE IMPLEMENTACIÓN

### P17: ¿Qué cambios debo hacer al código?

**R:** Solo 3 archivos principales:

1. **Crear 2 nuevos modelos**:
   - `Models/Semester.cs`
   - `Models/StudentSemester.cs`

2. **Modificar 2 modelos existentes**:
   - `Models/Student.cs` (simplificar)
   - `Models/Contest.cs` (agregar SemesterId)

3. **Actualizar DbContext**:
   - `Data/AppDbContext.cs` (agregar DbSets + config)

4. **Actualizar 1 Controller**:
   - `Controllers/StudentsController.cs` (cambiar query GetStudents)

**Total de archivos**: 6

---

### P18: ¿Puedo hacerlo en partes?

**R:** NO recomendado. Debes hacer TODO junto porque:

- ❌ Si creas modelos pero no la migración: errores de compilación
- ❌ Si aplicas migración pero no actualizas controller: errores de runtime
- ❌ Si importas datos pero el contexto no está actualizado: FK constraint errors

**Recomendación**: Sigue el PLAN_DE_IMPLEMENTACION.md Fase 1-7 de forma secuencial.

---

### P19: ¿Necesito cambiar otros controllers?

**R:** Por ahora NO necesario, pero en el futuro sí:

```
Ahora (Fase 1-7):
  - Solo actualizar StudentsController.GetStudents()

Futuro (Fase 8-10, opcional):
  - ContestsController: agregar filtro por semestre
  - SubmissionsController: validar semestre/grupo
  - DashboardController: mostrar selectores
  - Nuevos endpoints para filtrado
```

---

### P20: ¿Cuál es el orden correcto de ejecución?

**R:** EXACTO este orden:

```
1. ✅ Leer documentación (30 min)
   └─ COMIENZA_AQUI + PLAN_DE_IMPLEMENTACION

2. ✅ Preparación (5 min, Fase 1)
   └─ Crear backup

3. ✅ Crear modelos (10 min, Fase 2)
   └─ Crear Semester.cs, StudentSemester.cs
   └─ Actualizar Student.cs, Contest.cs

4. ✅ Migración (15 min, Fase 3)
   └─ dotnet ef migrations add
   └─ Actualizar DbContext
   └─ dotnet ef database update

5. ✅ Migrar datos (20 min, Fase 4)
   └─ Script SQL históricos

6. ✅ Importar CSVs (10 min, Fase 5)
   └─ Python script Paralelo 1
   └─ Python script Paralelo 2

7. ✅ Actualizar API (15 min, Fase 6)
   └─ Cambiar StudentsController.GetStudents()

8. ✅ Validar (10 min, Fase 7)
   └─ Queries SQL
   └─ Curl a API
   └─ Verificación completa

TOTAL: 85 minutos
```

---

## 🚨 PREGUNTAS DE TROUBLESHOOTING

### P21: ¿Qué hago si dotnet build falla?

**R:** Hay varias causas:

```bash
# 1. Limpiar cache
rm -rf obj/ bin/
dotnet clean

# 2. Restaurar packages
dotnet restore

# 3. Verificar errores específicos
dotnet build --verbosity:diagnostic

# 4. Común: Falta using
# Agrega al inicio del archivo:
using CsJudgeApi.Models;

# 5. Restaurar y reintentar
git status
# Si hay cambios incorrectos:
git checkout [archivo]
```

---

### P22: ¿Qué hago si dotnet ef database update falla?

**R:** Generalmente es un conflicto de FK:

```bash
# 1. Verificar integridad
sqlite3 submissions.db "PRAGMA foreign_keys=ON; SELECT 1;"

# 2. Si hay FK huérfanas, limpiar:
sqlite3 submissions.db << 'EOF'
DELETE FROM Contests WHERE SemesterId NOT IN (SELECT SemesterId FROM Semesters);
EOF

# 3. Revertir migración
dotnet ef migrations remove

# 4. Restaurar backup
cp submissions.db.bkp.2026-08-17 submissions.db

# 5. Comenzar desde Fase 3
```

---

### P23: ¿Qué hago si el endpoint retorna 500?

**R:** Verificar logs:

```bash
# 1. En terminal de "dotnet run" verás el error
# 2. Errores comunes:
#    - No active semester found → Verificar IsActive en Semesters
#    - Include + Select mal combinados → Verificar CODIGO_EXACTO.md
#    - DbContext no actualizado → Verificar AppDbContext.cs

# 3. Verificar en SQLite:
sqlite3 submissions.db "SELECT * FROM Semesters WHERE IsActive=1;"

# 4. Si IsActive es 0:
sqlite3 submissions.db "UPDATE Semesters SET IsActive=1 WHERE SemesterId=2;"

# 5. Reintentar
curl http://localhost:5000/api/students
```

---

### P24: ¿Cómo hago rollback total?

**R:** En 2 minutos:

```bash
# Paso 1: Parar servidor
# Ctrl+C en terminal de "dotnet run"

# Paso 2: Restaurar BD
cp submissions.db.bkp.2026-08-17 submissions.db

# Paso 3: Revertir código
git checkout Models/Student.cs
git checkout Models/Contest.cs
git checkout Data/AppDbContext.cs
git checkout Controllers/StudentsController.cs

# Paso 4: Eliminar migrations y modelos nuevos
rm Models/Semester.cs
rm Models/StudentSemester.cs
rm Migrations/*AddSemesterAndStudentSemesterSupport*

# Paso 5: Limpiar
dotnet clean
dotnet build

# Estado: Vuelto a lo anterior
```

---

## 📚 PREGUNTAS DE DOCUMENTACIÓN

### P25: ¿Cuál documento debo leer?

**R:** Depende de tu rol:

| Rol | Lee | Tiempo |
|-----|-----|--------|
| PM/Gerente | RESUMEN_EJECUTIVO | 5 min |
| Desarrollador | PLAN + CODIGO | 30 min |
| Arquitecto | AGENTS + ARCHITECTURE | 45 min |
| QA | CHECKLIST (Fase 7) | 15 min |

---

### P26: ¿Los documentos están en .gitignore?

**R:** NO:

```
Estos SÍ están en .gitignore:
  - AGENTS.md (NO subir, es referencia local)
  
Estos NO están (subir a git):
  - PLAN_DE_IMPLEMENTACION.md (instrucciones)
  - CODIGO_EXACTO.md (código)
  - README actualizado (documentación)
```

---

## 🎯 ÚLTIMA PREGUNTA

### P27: ¿Ya estoy listo para comenzar?

**R:** SÍ si:

- ✅ Leíste al menos COMIENZA_AQUI + PLAN_DE_IMPLEMENTACION
- ✅ Creaste backup: `cp submissions.db submissions.db.bkp.2026-08-17`
- ✅ Tienes CODIGO_EXACTO.md y CHECKLIST.md a mano
- ✅ Tienes 2 horas bloqueadas
- ✅ Compilaste: `dotnet build` (sin errores)

**Entonces:**

```bash
# Abre dos terminales/monitores:

# Terminal 1: Seguir CHECKLIST.md
# Terminal 2: Ejecutar comandos

# Sigue paso a paso
# Valida cada fase
# ¡Listo en 2 horas!
```

---

**¿Más preguntas? Consulta los 9 documentos en INDEX.md**

**¿Listo? Abre CHECKLIST.md y comienza 🚀**
