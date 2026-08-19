# PLAN DE IMPLEMENTACIÓN - Multi-Semestre y Multi-Grupo

## 📅 Fecha: 17 de agosto de 2026

### Estado Actual
- **Semestre Anterior (2025-2)**: Agosto 4 - Diciembre 20, 2025 (IsActive=false)
  - Estudiantes: En tabla `Students`, asociados a `Submissions`
  - Paralelo: 1
  - Total: ~14 estudiantes (según últimas consultas)

- **Semestre Actual (2026-2)**: Agosto 3 - Diciembre 19, 2026 (IsActive=true)
  - Estudiantes: En CSVs (2 grupos)
  - `Lista_de_estudiantes_Paralelo_1.csv`: ~14 estudiantes
  - `Lista_de_estudiantes_Paralelo_2.csv`: ~20 estudiantes
  - Total: ~34 estudiantes nuevos

### Arquitectura Target
- Tabla `Semesters` con información de cada semestre
- Tabla `StudentSemesters` con relación M:N (un estudiante puede estar en múltiples semestres)
- Tabla `Students` simplificada (solo StudentId y Name)
- Tabla `Contests` asociada a Semester e IsActive

---

## 🔧 IMPLEMENTACIÓN PASO A PASO

### FASE 1: PREPARACIÓN (5 minutos)

#### Paso 1.1: Backup de Base de Datos
```bash
# Copiar archivo de backup de seguridad
cp submissions.db submissions.db.bkp.2026-08-17
cp submissions.db submissions.db.bkp.2026-08-17.sql
```

**Archivos involucrados**:
- `/home/virtualbox/VirtualJudge/CsJudgeApi/submissions.db`

#### Paso 1.2: Verificar Datos Actuales
```sql
-- Verificar estructura actual
sqlite3 submissions.db ".schema Students"
sqlite3 submissions.db ".schema Contests"

-- Verificar cantidad de estudiantes
sqlite3 submissions.db "SELECT COUNT(*) FROM Students;"

-- Verificar estudiantes únicos en Submissions
sqlite3 submissions.db "SELECT COUNT(DISTINCT StudentId) FROM Submissions;"
```

**Validación esperada**:
- Tabla `Students` existe con: StudentId, Name
- Tabla `Contests` existe con: ContestId, Name, ...
- Tabla `Submissions` existe con registros históricos

---

### FASE 2: CREACIÓN DE MODELOS (10 minutos)

#### Paso 2.1: Crear `Semester.cs`
**Archivo**: `/home/virtualbox/VirtualJudge/CsJudgeApi/Models/Semester.cs`

```csharp
using System;
using System.Collections.Generic;

namespace CsJudgeApi.Models;

public class Semester
{
    public int SemesterId { get; set; }
    public int SemesterCode { get; set; }          // 20262, 20252
    public string Name { get; set; } = string.Empty;  // "2-2026", "2-2025"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    // Relaciones
    public ICollection<StudentSemester> Students { get; set; } = new List<StudentSemester>();
    public ICollection<Contest> Contests { get; set; } = new List<Contest>();
}
```

#### Paso 2.2: Crear `StudentSemester.cs`
**Archivo**: `/home/virtualbox/VirtualJudge/CsJudgeApi/Models/StudentSemester.cs`

```csharp
using System;

namespace CsJudgeApi.Models;

public class StudentSemester
{
    public long StudentId { get; set; }
    public int SemesterId { get; set; }
    public int Group { get; set; }                 // 1 o 2
    public string ParallelName { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; } = DateTime.Now;

    // Navegación
    public Student? Student { get; set; }
    public Semester? Semester { get; set; }
}
```

#### Paso 2.3: Actualizar `Student.cs`
**Archivo**: `/home/virtualbox/VirtualJudge/CsJudgeApi/Models/Student.cs`

```csharp
using System;
using System.Collections.Generic;

namespace CsJudgeApi.Models;

public class Student
{
    public long StudentId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Relaciones
    public ICollection<StudentSemester> Semesters { get; set; } = new List<StudentSemester>();
    public ICollection<ContestStudent> Contests { get; set; } = new List<ContestStudent>();
}
```

#### Paso 2.4: Actualizar `Contest.cs`
**Archivo**: `/home/virtualbox/VirtualJudge/CsJudgeApi/Models/Contest.cs`

En la clase Contest, cambiar:
```csharp
// ❌ ELIMINAR si existen:
// public int Semester { get; set; }
// public int Group { get; set; }
// public string ParallelName { get; set; }

// ✅ AGREGAR:
public int SemesterId { get; set; }        // FK a Semester
public int Group { get; set; }             // 1 o 2

// ✅ AGREGAR Navegación:
public Semester? Semester { get; set; }
```

---

### FASE 3: CREAR MIGRACIÓN (15 minutos)

#### Paso 3.1: Generar Migración EF Core
```bash
cd /home/virtualbox/VirtualJudge/CsJudgeApi
dotnet ef migrations add AddSemesterAndStudentSemesterSupport
```

**Validación**:
```bash
ls -la Migrations/ | grep AddSemesterAndStudentSemesterSupport
```

#### Paso 3.2: Actualizar `AppDbContext.cs`
**Archivo**: `/home/virtualbox/VirtualJudge/CsJudgeApi/Data/AppDbContext.cs`

En la clase `AppDbContext`, agregar:
```csharp
public DbSet<Semester> Semesters { get; set; } = null!;
public DbSet<StudentSemester> StudentSemesters { get; set; } = null!;
```

En el método `OnModelCreating()`, agregar al final:
```csharp
// ========== NUEVA CONFIGURACIÓN M:N ==========

// PK compuesta en StudentSemester
modelBuilder.Entity<StudentSemester>()
    .HasKey(ss => new { ss.StudentId, ss.SemesterId });

// Relaciones StudentSemester -> Student
modelBuilder.Entity<StudentSemester>()
    .HasOne(ss => ss.Student)
    .WithMany(s => s.Semesters)
    .HasForeignKey(ss => ss.StudentId)
    .OnDelete(DeleteBehavior.Cascade);

// Relaciones StudentSemester -> Semester
modelBuilder.Entity<StudentSemester>()
    .HasOne(ss => ss.Semester)
    .WithMany(s => s.Students)
    .HasForeignKey(ss => ss.SemesterId)
    .OnDelete(DeleteBehavior.Cascade);

// Índices de búsqueda
modelBuilder.Entity<StudentSemester>()
    .HasIndex(ss => new { ss.SemesterId, ss.Group })
    .HasDatabaseName("IX_StudentSemester_SemesterGroup");

// Relación Contest -> Semester
modelBuilder.Entity<Contest>()
    .HasOne(c => c.Semester)
    .WithMany(s => s.Contests)
    .HasForeignKey(c => c.SemesterId)
    .OnDelete(DeleteBehavior.Cascade);

// Índice en Contest
modelBuilder.Entity<Contest>()
    .HasIndex(c => new { c.SemesterId, c.Group })
    .HasDatabaseName("IX_Contest_SemesterGroup");

// ========== SEED DATA: SEMESTERS ==========
modelBuilder.Entity<Semester>().HasData(
    new Semester 
    { 
        SemesterId = 1,
        SemesterCode = 20252,
        Name = "2-2025",
        StartDate = new DateTime(2025, 8, 4),
        EndDate = new DateTime(2025, 12, 20),
        IsActive = false
    },
    new Semester 
    { 
        SemesterId = 2,
        SemesterCode = 20262,
        Name = "2-2026",
        StartDate = new DateTime(2026, 8, 3),
        EndDate = new DateTime(2026, 12, 19),
        IsActive = true
    }
);
```

#### Paso 3.3: Aplicar Migración
```bash
cd /home/virtualbox/VirtualJudge/CsJudgeApi
dotnet ef database update
```

**Validación**:
```bash
sqlite3 submissions.db ".schema StudentSemesters"
sqlite3 submissions.db ".schema Semesters"
sqlite3 submissions.db "SELECT * FROM Semesters;"
```

---

### FASE 4: MIGRACIÓN DE DATOS (20 minutos)

#### Paso 4.1: Migrar Estudiantes Existentes (2025-2)

**Archivo Script**: `/home/virtualbox/VirtualJudge/CsJudgeApi/Migrations/XXXXXXX_MigrateHistoricalStudents.cs` (en la última migración, agregar en el método `Up()`):

```csharp
// Migrar estudiantes existentes al semestre 2025-2 (Paralelo 1)
migrationBuilder.Sql(@"
    INSERT INTO StudentSemesters (StudentId, SemesterId, [Group], ParallelName, EnrollmentDate)
    SELECT DISTINCT s.StudentId, 1, 1, 'Paralelo 1', datetime('now')
    FROM Students s
    WHERE NOT EXISTS (
        SELECT 1 FROM StudentSemesters ss 
        WHERE ss.StudentId = s.StudentId AND ss.SemesterId = 1
    );
");
```

O crear un script SQL separado y ejecutarlo:
```bash
sqlite3 submissions.db << 'EOF'
INSERT INTO StudentSemesters (StudentId, SemesterId, [Group], ParallelName, EnrollmentDate)
SELECT DISTINCT StudentId, 1, 1, 'Paralelo 1', datetime('now')
FROM Submissions
WHERE StudentId NOT IN (
    SELECT DISTINCT StudentId FROM StudentSemesters
);
EOF
```

**Validación**:
```bash
sqlite3 submissions.db "SELECT COUNT(*) FROM StudentSemesters WHERE SemesterId = 1;"
```

---

### FASE 5: IMPORTACIÓN DE ESTUDIANTES NUEVOS (2026-2) (10 minutos)

#### Opción A: Manual con SQL (RÁPIDO)

**Script SQL**:
```bash
python3 << 'EOF'
import csv
import sqlite3

conn = sqlite3.connect('submissions.db')
cursor = conn.cursor()

# Paralelo 1
with open('Lista_de_estudiantes_Paralelo_1.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    for row in reader:
        student_id = int(row['Documento Identidad'])
        name = row['Nombre Estudiante']
        
        # Insertar estudiante si no existe
        cursor.execute(
            'INSERT OR IGNORE INTO Students (StudentId, Name) VALUES (?, ?)',
            (student_id, name)
        )
        
        # Insertar StudentSemester
        cursor.execute(
            'INSERT OR IGNORE INTO StudentSemesters (StudentId, SemesterId, Group, ParallelName, EnrollmentDate) VALUES (?, 2, 1, ?, datetime("now"))',
            (student_id, 'Paralelo 1')
        )

# Paralelo 2
with open('Lista_de_estudiantes_Paralelo_2.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    for row in reader:
        student_id = int(row['Documento Identidad'])
        name = row['Nombre Estudiante']
        
        # Insertar estudiante si no existe
        cursor.execute(
            'INSERT OR IGNORE INTO Students (StudentId, Name) VALUES (?, ?)',
            (student_id, name)
        )
        
        # Insertar StudentSemester
        cursor.execute(
            'INSERT OR IGNORE INTO StudentSemesters (StudentId, SemesterId, Group, ParallelName, EnrollmentDate) VALUES (?, 2, 2, ?, datetime("now"))',
            (student_id, 'Paralelo 2')
        )

conn.commit()
conn.close()
print("✅ Estudiantes importados exitosamente")
EOF
```

**Validación**:
```bash
sqlite3 submissions.db "SELECT COUNT(*) FROM StudentSemesters WHERE SemesterId = 2;"
sqlite3 submissions.db "SELECT COUNT(*) FROM StudentSemesters WHERE SemesterId = 2 AND Group = 1;"
sqlite3 submissions.db "SELECT COUNT(*) FROM StudentSemesters WHERE SemesterId = 2 AND Group = 2;"
```

#### Opción B: Crear Servicio CsvImportService (Futuro)
Ver AGENTS.md TASK 5 para implementación

---

### FASE 6: ACTUALIZAR QUERIES (15 minutos)

#### Paso 6.1: Actualizar `StudentsController.cs`

**Encontrar el método que obtiene estudiantes** (típicamente `GetStudents()`):

```csharp
// ❌ ANTES:
[HttpGet]
public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
{
    var students = await _context.Students.ToListAsync();
    return Ok(students);
}

// ✅ DESPUÉS:
[HttpGet]
public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
{
    var currentSemester = await _context.Semesters
        .FirstOrDefaultAsync(s => s.IsActive);
    
    if (currentSemester == null)
        return NotFound("No active semester found");

    var students = await _context.StudentSemesters
        .Where(ss => ss.SemesterId == currentSemester.SemesterId)
        .Include(ss => ss.Student)
        .Select(ss => ss.Student)
        .Distinct()
        .ToListAsync();

    return Ok(students);
}
```

**Validación**:
```bash
# Compilar y verificar que no hay errores
dotnet build

# Ejecutar servidor
dotnet run

# En otra terminal, probar endpoint
curl http://localhost:5000/api/students
```

---

### FASE 7: VERIFICACIÓN FINAL (10 minutos)

#### Paso 7.1: Verificar Integridad de Datos

```bash
# Verificar que no hay estudiantes duplicados
sqlite3 submissions.db "SELECT StudentId, COUNT(*) FROM Students GROUP BY StudentId HAVING COUNT(*) > 1;"

# Verificar conteo por semestre
sqlite3 submissions.db "
SELECT 
    s.Name,
    COUNT(ss.StudentId) as TotalEstudiantes,
    COUNT(CASE WHEN ss.Group = 1 THEN 1 END) as Paralelo1,
    COUNT(CASE WHEN ss.Group = 2 THEN 1 END) as Paralelo2
FROM Semesters s
LEFT JOIN StudentSemesters ss ON s.SemesterId = ss.SemesterId
GROUP BY s.SemesterId;
"

# Verificar que Contests están asociados a Semesters
sqlite3 submissions.db "SELECT COUNT(*) FROM Contests WHERE SemesterId IS NULL;"
```

#### Paso 7.2: Probar Queries Principales

```bash
# Obtener estudiantes del semestre actual
sqlite3 submissions.db "
SELECT s.StudentId, s.Name, ss.Group
FROM Students s
JOIN StudentSemesters ss ON s.StudentId = ss.StudentId
JOIN Semesters sem ON ss.SemesterId = sem.SemesterId
WHERE sem.IsActive = 1
LIMIT 10;
"

# Obtener estudiantes por grupo
sqlite3 submissions.db "
SELECT s.StudentId, s.Name, sem.Name as Semestre, ss.Group, ss.ParallelName
FROM Students s
JOIN StudentSemesters ss ON s.StudentId = ss.StudentId
JOIN Semesters sem ON ss.SemesterId = sem.SemesterId
WHERE sem.SemesterCode = 20262 AND ss.Group = 1
LIMIT 5;
"
```

#### Paso 7.3: Verificar API

```bash
# Endpoint de estudiantes
curl -X GET http://localhost:5000/api/students

# Debería retornar solo estudiantes del semestre actual (2-2026)
# Total esperado: ~34 estudiantes
```

---

## 📊 RESUMEN DE CAMBIOS

| Componente | Cambios | Archivo |
|-----------|---------|---------|
| **Modelos** | +2 nuevos: Semester, StudentSemester | Models/Semester.cs, Models/StudentSemester.cs |
| **Student** | Simplificado (sin Semester, Group) | Models/Student.cs |
| **Contest** | SemesterId reemplaza Semester/Group | Models/Contest.cs |
| **DbContext** | +2 DbSets + relaciones M:N | Data/AppDbContext.cs |
| **Migraciones** | +1 nueva migración | Migrations/ |
| **Controller** | Queries filtran por semestre actual | Controllers/StudentsController.cs |
| **BD** | +2 tablas nuevas + modificaciones | submissions.db |

---

## 🎯 RESULTADO FINAL ESPERADO

### Base de Datos
- ✅ Tabla `Semesters` con 2 registros (2025-2 e 2026-2)
- ✅ Tabla `StudentSemesters` con ~48 registros (~14 del 2025-2 + ~34 del 2026-2)
- ✅ Tabla `Students` con ~48 registros únicos
- ✅ Tabla `Contests` con SemesterId e Group asignados

### API
- ✅ GET `/api/students` retorna solo estudiantes del semestre actual (2026-2)
- ✅ Todos los endpoints funcionan con datos filtrados por semestre

### Capacidad Futura
- ✅ Un estudiante puede repetir en diferentes semestres sin conflicto de clave primaria
- ✅ Fácil de extender a más grupos o semestres
- ✅ Historial completo de estudiantes por semestre

---

## ⏱️ TIEMPO TOTAL ESTIMADO
- **Fase 1**: 5 minutos
- **Fase 2**: 10 minutos
- **Fase 3**: 15 minutos
- **Fase 4**: 20 minutos
- **Fase 5**: 10 minutos
- **Fase 6**: 15 minutos
- **Fase 7**: 10 minutos
- **TOTAL**: ~85 minutos (1.5 horas)

---

## 🚨 ROLLBACK (en caso de problemas)

```bash
# 1. Restaurar backup
cp submissions.db.bkp.2026-08-17 submissions.db

# 2. Revertir migración
dotnet ef migrations remove

# 3. Restaurar modelos a su estado anterior
git checkout Models/Student.cs
git checkout Models/Contest.cs
git checkout Data/AppDbContext.cs

# 4. Compilar
dotnet build
```

---

**Documento creado**: 17 de agosto de 2026
**Responsable**: Dev Team
**Estado**: 🟢 Listo para ejecutar
