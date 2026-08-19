# 📝 CÓDIGO EXACTO A USAR - Copy-Paste Ready

## 🔧 Archivo 1: Models/Semester.cs (CREAR)

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

---

## 🔧 Archivo 2: Models/StudentSemester.cs (CREAR)

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

---

## 🔧 Archivo 3: Models/Student.cs (REEMPLAZAR COMPLETAMENTE)

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

---

## 🔧 Archivo 4: Models/Contest.cs (MODIFICAR)

**Encontrar y reemplazar** en la clase `Contest`:

### PASO 1: Eliminar estas líneas si existen:
```csharp
// ❌ ELIMINAR ESTAS LINEAS:
public int Semester { get; set; }
public int Group { get; set; }
public string ParallelName { get; set; }
```

### PASO 2: Agregar estas líneas:
```csharp
// ✅ AGREGAR ESTAS LINEAS después de propiedades principales:
public int SemesterId { get; set; }        // FK a Semester
public int Group { get; set; }             // 1 o 2

// ✅ AGREGAR NAVEGACIÓN (después de los DbSets):
public Semester? Semester { get; set; }
```

**Resultado esperado** en Contest.cs:
```csharp
public class Contest
{
    public int ContestId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // ... otras propiedades existentes ...
    
    public int SemesterId { get; set; }        // ← NUEVO
    public int Group { get; set; }             // ← NUEVO
    
    // Relaciones
    public Semester? Semester { get; set; }    // ← NUEVO
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<ContestStudent> Students { get; set; } = new List<ContestStudent>();
}
```

---

## 🔧 Archivo 5: Data/AppDbContext.cs (ACTUALIZAR)

### PASO 1: Agregar DbSets

En la clase `AppDbContext`, buscar donde están los DbSets existentes y agregar:

```csharp
public DbSet<Semester> Semesters { get; set; } = null!;
public DbSet<StudentSemester> StudentSemesters { get; set; } = null!;
```

**Ubicación** (después de los DbSets existentes):
```csharp
public class AppDbContext : DbContext
{
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Contest> Contests { get; set; } = null!;
    public DbSet<ContestStudent> ContestStudents { get; set; } = null!;
    public DbSet<Question> Questions { get; set; } = null!;
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Configuration> Configurations => Set<Configuration>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    
    // ✅ AGREGAR:
    public DbSet<Semester> Semesters { get; set; } = null!;
    public DbSet<StudentSemester> StudentSemesters { get; set; } = null!;
    
    // ... resto del código
}
```

### PASO 2: Actualizar OnModelCreating()

Al final del método `OnModelCreating()`, agregar (ANTES del `base.OnModelCreating(modelBuilder);`):

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

base.OnModelCreating(modelBuilder);
```

---

## 🔧 Archivo 6: Controllers/StudentsController.cs (ACTUALIZAR GetStudents)

Encontrar el método `GetStudents()` y reemplazarlo:

### ANTES:
```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
{
    var students = await _context.Students.ToListAsync();
    return Ok(students);
}
```

### DESPUÉS:
```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
{
    var currentSemester = await _context.Semesters
        .FirstOrDefaultAsync(s => s.IsActive);
    
    if (currentSemester == null)
        return NotFound(new { message = "No active semester found" });

    var students = await _context.StudentSemesters
        .Where(ss => ss.SemesterId == currentSemester.SemesterId)
        .Include(ss => ss.Student)
        .Select(ss => ss.Student)
        .Distinct()
        .ToListAsync();

    return Ok(students);
}
```

---

## 📊 Resumen de Comandos a Ejecutar

```bash
# 1. Compilar para verificar que no hay errores
dotnet build

# 2. Crear migración
dotnet ef migrations add AddSemesterAndStudentSemesterSupport

# 3. Aplicar migración
dotnet ef database update

# 4. (Opcional) Verificar estructura
sqlite3 submissions.db ".schema StudentSemesters"
sqlite3 submissions.db ".schema Semesters"

# 5. (Opcional) Migrar estudiantes existentes
sqlite3 submissions.db << 'EOF'
INSERT INTO StudentSemesters (StudentId, SemesterId, [Group], ParallelName, EnrollmentDate)
SELECT DISTINCT StudentId, 1, 1, 'Paralelo 1', datetime('now')
FROM Submissions
WHERE StudentId NOT IN (
    SELECT DISTINCT StudentId FROM StudentSemesters
);
EOF

# 6. (Opcional) Importar estudiantes nuevos - Paralelo 1
python3 << 'PYSCRIPT'
import csv, sqlite3
conn = sqlite3.connect('submissions.db')
cursor = conn.cursor()
with open('Lista_de_estudiantes_Paralelo_1.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    for row in reader:
        sid = int(row['Documento Identidad'])
        name = row['Nombre Estudiante']
        cursor.execute('INSERT OR IGNORE INTO Students VALUES (?, ?)', (sid, name))
        cursor.execute('INSERT OR IGNORE INTO StudentSemesters VALUES (?, 2, 1, ?, datetime("now"))', (sid, 'Paralelo 1'))
conn.commit()
conn.close()
PYSCRIPT

# 7. (Opcional) Importar estudiantes nuevos - Paralelo 2
python3 << 'PYSCRIPT'
import csv, sqlite3
conn = sqlite3.connect('submissions.db')
cursor = conn.cursor()
with open('Lista_de_estudiantes_Paralelo_2.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    for row in reader:
        sid = int(row['Documento Identidad'])
        name = row['Nombre Estudiante']
        cursor.execute('INSERT OR IGNORE INTO Students VALUES (?, ?)', (sid, name))
        cursor.execute('INSERT OR IGNORE INTO StudentSemesters VALUES (?, 2, 2, ?, datetime("now"))', (sid, 'Paralelo 2'))
conn.commit()
conn.close()
PYSCRIPT

# 8. Compilar y ejecutar
dotnet build
dotnet run

# 9. Probar endpoint (en otra terminal)
curl http://localhost:5000/api/students
```

---

## ✅ Checklist de Implementación

- [ ] Crear `Models/Semester.cs`
- [ ] Crear `Models/StudentSemester.cs`
- [ ] Actualizar `Models/Student.cs` (reemplazar)
- [ ] Actualizar `Models/Contest.cs` (agregar SemesterId, Group, Semester)
- [ ] Actualizar `Data/AppDbContext.cs` (DbSets + configuración + seed)
- [ ] Ejecutar: `dotnet ef migrations add AddSemesterAndStudentSemesterSupport`
- [ ] Ejecutar: `dotnet ef database update`
- [ ] Compilar: `dotnet build`
- [ ] (Opcional) Migrar estudiantes históricos
- [ ] (Opcional) Importar CSVs nuevos
- [ ] Actualizar `Controllers/StudentsController.cs` (GetStudents)
- [ ] Compilar nuevamente: `dotnet build`
- [ ] Ejecutar: `dotnet run`
- [ ] Probar: `curl http://localhost:5000/api/students`

---

**Versión**: 1.0
**Fecha**: 17 de agosto de 2026
**Estado**: Listo para usar
