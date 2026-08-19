# 🚀 RESUMEN EJECUTIVO - Plan de Multi-Semestre y Multi-Grupo

## 📌 Situación Actual

### Base de Datos
```
Students (tabla actual)
├── StudentId (PK)
├── Name
└── Relación directa con Contests/Submissions

Problema: No hay separación por semestre/grupo
```

### Estudiantes por Semestre
- **2025-2 (Paralelo 1)**: ~14 estudiantes → En `Submissions` histórico
- **2026-2 (Paralelo 1)**: ~14 estudiantes → `Lista_de_estudiantes_Paralelo_1.csv`
- **2026-2 (Paralelo 2)**: ~20 estudiantes → `Lista_de_estudiantes_Paralelo_2.csv`

---

## 🎯 Arquitectura Nueva (M:N)

```
┌─────────────────────────────────────────────────────────────┐
│                        SEMESTERS                            │
├─────────────────────────────────────────────────────────────┤
│ SemesterId │ Code  │ Name    │ StartDate  │ EndDate │Active│
├─────────────────────────────────────────────────────────────┤
│     1      │20252  │ 2-2025  │2025-08-04  │2025-12-20│False│
│     2      │20262  │ 2-2026  │2026-08-03  │2026-12-19│True │
└─────────────────────────────────────────────────────────────┘
           ▲                            ▲
           │                            │
      (FK)─┼────────────────────────────┼─(FK)
           │                            │
┌──────────┴────────────────┐    ┌──────┴──────────────────┐
│   STUDENT_SEMESTERS       │    │     CONTESTS           │
├───────────────────────────┤    ├───────────────────────┤
│StudentId (FK) │SemesterId │    │ContestId │SemesterId   │
│   (FK) │ Group│Parallel   │    │   (FK)   │  Group      │
├───────────────────────────┤    └───────────────────────┘
│ 123    │  1   │    1      │
│ 123    │  2   │    1      │  ◄─ Mismo estudiante
│ 456    │  2   │    2      │     en 2 semestres
│ 789    │  2   │    2      │
└───────────────────────────┘
      ▲
      │ (FK)
      │
┌─────┴──────────────┐
│     STUDENTS       │
├────────────────────┤
│StudentId │ Name    │
├────────────────────┤
│   123    │ Juan    │
│   456    │ María   │
│   789    │ Carlos  │
└────────────────────┘
```

---

## 📋 Cambios en Modelos

### Student.cs (SIMPLIFICADO)
```csharp
❌ ELIMINAR: Semester, Group, ParallelName
✅ AGREGAR: ICollection<StudentSemester> Semesters
```

### StudentSemester.cs (NUEVO)
```csharp
✅ NUEVA TABLA: StudentId + SemesterId (PK compuesta)
   Group, ParallelName, EnrollmentDate
```

### Semester.cs (NUEVO)
```csharp
✅ NUEVA TABLA: SemesterId, SemesterCode, Name, Dates, IsActive
```

### Contest.cs (MODIFICADO)
```csharp
❌ CAMBIAR: Semester (int) → SemesterId (int FK)
✅ AGREGAR: Semester? (navegación)
```

---

## 📊 Datos por Fase

### Fase 1-3: Creación de Estructura
- ✅ Crear 2 nuevos modelos
- ✅ Modificar 2 modelos existentes
- ✅ Generar y aplicar migración EF Core
- ✅ Actualizar AppDbContext

### Fase 4-5: Carga de Datos
| Acción | Semestre | Grupo | Cantidad | Origen |
|--------|----------|-------|----------|--------|
| Migrar | 2025-2 | 1 | ~14 | Tabla Submissions |
| Importar | 2026-2 | 1 | ~14 | Lista_de_estudiantes_Paralelo_1.csv |
| Importar | 2026-2 | 2 | ~20 | Lista_de_estudiantes_Paralelo_2.csv |
| **TOTAL** | - | - | **~48** | - |

### Fase 6: Actualización de API
- ✅ Query de `GetStudents()` filtra por semestre **actual** (IsActive=true)
- ✅ Resultado: solo ~34 estudiantes (2026-2) retornados

---

## 🔄 Comparación: Antes vs Después

### ANTES (Problema)
```csharp
// Query sin filtro
var students = await _context.Students.ToListAsync();
// Retorna: todos los estudiantes (~48) mezclados
// Problema: No hay separación por semestre/grupo
// Riesgo: Conflicto si un estudiante repite (duplicate key)
```

### DESPUÉS (Solución)
```csharp
// Query con filtro automático
var currentSemester = await _context.Semesters
    .FirstOrDefaultAsync(s => s.IsActive);

var students = await _context.StudentSemesters
    .Where(ss => ss.SemesterId == currentSemester.SemesterId)
    .Include(ss => ss.Student)
    .Select(ss => ss.Student)
    .Distinct()
    .ToListAsync();
// Retorna: solo 34 estudiantes del semestre 2026-2
// Ventaja: Si repite en 2027-1, crea nuevo registro en StudentSemesters
```

---

## 📁 Archivos a Crear/Modificar

### Crear (4 archivos)
```
✅ Models/Semester.cs (nuevo modelo)
✅ Models/StudentSemester.cs (nuevo modelo)
✅ Services/CsvImportService.cs (servicio de importación)
✅ semestres.csv (datos de semestres)
```

### Modificar (4 archivos)
```
✅ Models/Student.cs (eliminar Semester, Group)
✅ Models/Contest.cs (cambiar Semester → SemesterId)
✅ Data/AppDbContext.cs (agregar DbSets + configuración)
✅ Controllers/StudentsController.cs (actualizar query)
```

### Generar (1 archivo)
```
✅ Migrations/XXXXXXXXX_AddSemesterAndStudentSemesterSupport.cs (automático)
```

---

## ⏱️ Timeline

```
TOTAL: ~85 minutos (1.5 horas)

├─ Fase 1 (5 min):  Backup
├─ Fase 2 (10 min): Crear modelos
├─ Fase 3 (15 min): Migración EF Core
├─ Fase 4 (20 min): Migrar datos históricos
├─ Fase 5 (10 min): Importar CSVs nuevos
├─ Fase 6 (15 min): Actualizar API
└─ Fase 7 (10 min): Verificación final
```

---

## ✅ Criterios de Éxito

- [ ] Tabla `Semesters` tiene 2 registros con datos correctos
- [ ] Tabla `StudentSemesters` tiene ~48 registros (relaciones M:N)
- [ ] GET `/api/students` retorna ~34 estudiantes (solo 2026-2)
- [ ] No hay conflictos de clave primaria
- [ ] Historial de 2025-2 se mantiene intacto
- [ ] API compila sin errores
- [ ] Estudiante puede estar en múltiples semestres sin problema

---

## 🔒 Seguridad y Backup

```bash
# Backup automático
cp submissions.db submissions.db.bkp.2026-08-17

# Si algo falla:
# 1. Restaurar: cp submissions.db.bkp.2026-08-17 submissions.db
# 2. Revertir: dotnet ef migrations remove
# 3. Todo vuelve al estado anterior
```

---

## 📚 Documentación Adjunta

- **AGENTS.md**: Especificación técnica detallada (392 líneas)
- **PLAN_DE_IMPLEMENTACION.md**: Paso a paso ejecutable
- **ARCHITECTURE.md**: Decisiones de diseño y justificación
- **semestres.csv**: Datos de semestres a cargar

---

**Próximo Paso**: Ejecutar PLAN_DE_IMPLEMENTACION.md Fase 1-7 en orden

---

**Creado**: 17 de agosto de 2026
**Estado**: 🟢 LISTO PARA IMPLEMENTAR
