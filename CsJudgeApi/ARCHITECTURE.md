# Arquitectura de Múltiples Semestres y Grupos

## Contexto del Proyecto
- **Sistema**: CsJudgeApi (Juez Virtual)
- **Semestre anterior**: 2-2050 (Agosto - Diciembre 2025)
- **Semestre actual**: 2-2026 (Agosto - Diciembre 2026)
- **Grupos en semestre actual**: 2 (Paralelo 1 y Paralelo 2)
- **Base de datos**: SQLite
- **CSV de entrada**:
  - `Lista_de_EstudiantesParalelo_1.csv`
  - `Lista_de_estudiantes_Paralelo_2.csv`

## Decisión de Arquitectura

### Opción Seleccionada: Archivo SQLite Único con Separación por Semestre/Grupo

**Justificación:**
- ✅ Mantiene datos históricos valiosos para análisis comparativo
- ✅ Menor complejidad operacional (un solo archivo DB)
- ✅ Bajo tamaño: ~5-10 MB por grupo, ~40-50 MB total con histórico
- ✅ SQLite soporta sin problemas (límite teórico: 140 TB)
- ✅ Facilita backups únicos
- ✅ No requiere migración de datos entre archivos

## Cambios en el Modelo de Datos

### 1. Tabla `Students` - Nuevos Campos

```csharp
public class Student
{
    public long StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // ✨ Nuevos campos para multi-semestre/grupo
    public int Semester { get; set; }        // Formato: 20252 (semestre 2-2025)
    public int Group { get; set; }           // 1 o 2
    public string ParallelName { get; set; } // "Paralelo 1" o "Paralelo 2"
    
    public ICollection<ContestStudent> Contests { get; set; } = new List<ContestStudent>();
}
```

**Formato de Semester:**
- Antiguo: `2025-2` → Nuevo: `20252` (año + semestre sin guión)
- Ejemplo: `20252` = 2-2025, `20262` = 2-2026

### 2. Tabla `Contests` - Nuevos Campos

```csharp
public class Contest
{
    public int ContestId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // ✨ Nuevos campos para asociar contest a semestre/grupo
    public int Semester { get; set; }
    public int Group { get; set; }
    
    // ... resto del código existente
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<ContestStudent> Students { get; set; } = new List<ContestStudent>();
}
```

### 3. Cambios en `AppDbContext`

Agregar restricción de clave única en `Students`:
```csharp
modelBuilder.Entity<Student>()
    .HasIndex(s => new { s.Semester, s.Group, s.Name })
    .IsUnique()
    .HasDatabaseName("IX_Student_SemesterGroupName");
```

Agregar índices para queries frecuentes:
```csharp
// Búsquedas rápidas por semestre/grupo
modelBuilder.Entity<Student>()
    .HasIndex(s => new { s.Semester, s.Group })
    .HasDatabaseName("IX_Student_SemesterGroup");

modelBuilder.Entity<Contest>()
    .HasIndex(c => new { c.Semester, c.Group })
    .HasDatabaseName("IX_Contest_SemesterGroup");
```

## Estimación de Datos

### Tamaño esperado por grupo (40 estudiantes, semestre completo):
- Estudiantes: 40 registros
- Contests: ~15 preguntas
- Submissions promedio: 5 por estudiante = 3,000 registros
- **Tamaño DB: 5-10 MB**

### Total con histórico:
- Semestre 2-2025: ~20 MB (dos grupos)
- Semestre 2-2026: ~20 MB (dos grupos)
- **Total estimado: 40-50 MB**

### Capacidad SQLite:
- Límite teórico: 140 TB
- Nuestro caso: 50 MB = 0.000036% del límite
- **Conclusión: ✅ Sin problemas**

## Pasos de Implementación

### Fase 1: Migración de Base de Datos
1. ✅ Crear migración EF Core para agregar campos `Semester` y `Group`
2. ✅ Actualizar `Student.cs` con nuevos campos
3. ✅ Actualizar `Contest.cs` con nuevos campos
4. ✅ Actualizar `AppDbContext` con índices

### Fase 2: Importación de Datos
1. ✅ Crear script de importación de CSVs
   - Parsear `Lista_de_EstudiantesParalelo_1.csv` → `Semester: 20262, Group: 1`
   - Parsear `Lista_de_estudiantes_Paralelo_2.csv` → `Semester: 20262, Group: 2`
2. ✅ Manejo de duplicados (estudiantes que repiten grupo)
3. ✅ Validación de datos

### Fase 3: Actualización de Controllers/Services
1. ✅ Agregar filtros por `Semester` y `Group` en todas las queries
2. ✅ Crear endpoints parametrizados para semestre/grupo
3. ✅ Actualizar Dashboard para mostrar datos por grupo

### Fase 4: Historización de datos antiguo (2-2025)
1. ✅ Migrar datos antiguos con `Semester: 20252`
2. ✅ Asignar `Group` basado en análisis de datos históricos
3. ✅ Validar integridad de datos

## Ejemplo de Queries Filtradas

```csharp
// Obtener estudiantes del paralelo 1, semestre 2-2026
var paralelo1_2026 = dbContext.Students
    .Where(s => s.Semester == 20262 && s.Group == 1)
    .ToList();

// Obtener todos los contests del grupo 2
var contests_group2 = dbContext.Contests
    .Where(c => c.Semester == 20262 && c.Group == 2)
    .Include(c => c.Questions)
    .ToList();

// Obtener submissions de un estudiante específico
var submissions = dbContext.Submissions
    .Where(s => s.StudentId == 123)
    .OrderByDescending(s => s.CreatedAt)
    .ToList();
```

## Beneficios de esta Arquitectura

| Aspecto | Beneficio |
|--------|----------|
| **Historial** | Analizar evolución de estudiantes entre semestres |
| **Comparativa** | Comparar desempeño Paralelo 1 vs Paralelo 2 |
| **Escalabilidad** | Fácil agregar más grupos/semestres sin migrar datos |
| **Performance** | Índices optimizan búsquedas por semestre/grupo |
| **Simplicidad** | Un único archivo SQLite, sin complejidad operacional |
| **Mantenibilidad** | Código limpio con filtros consistentes |

## Notas Importantes

1. **Conversión de Semester**: 
   - Antiguo formato en DB: `"2025-2"` (string)
   - Nuevo formato: `20252` (int)
   - Migración: transformar durante la migración EF Core

2. **Group por defecto**: Para datos históricos sin información de grupo, usar `Group: 0` o requerir asignación manual

3. **Backups**: Mantener un backup separado del `submissions.db.Bkp.sql` antes de agregar nuevos campos

4. **Validación en importación**: Verificar que no hay duplicados StudentId dentro del mismo semestre/grupo

## Referencias
- Archivo actual: `/home/virtualbox/VirtualJudge/CsJudgeApi/submissions.db`
- CSVs de entrada:
  - `/home/virtualbox/VirtualJudge/CsJudgeApi/Lista_de_EstudiantesParalelo_1.csv`
  - `/home/virtualbox/VirtualJudge/CsJudgeApi/Lista_de_estudiantes_Paralelo_2.csv`
