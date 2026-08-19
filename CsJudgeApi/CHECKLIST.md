# ✅ CHECKLIST DE IMPLEMENTACIÓN - Paso a Paso Visual

## ANTES DE COMENZAR

```
PRE-REQUISITOS:
  ☐ Leer RESUMEN_EJECUTIVO.md
  ☐ Leer PLAN_DE_IMPLEMENTACION.md  
  ☐ Tener CODIGO_EXACTO.md a mano
  ☐ Crear backup: cp submissions.db submissions.db.bkp.2026-08-17
  ☐ Verificar: dotnet --version
  ☐ Verificar: sqlite3 --version
  ☐ Verificar: git status (working directory clean)
  ☐ Terminal abierta en: /home/virtualbox/VirtualJudge/CsJudgeApi
```

---

## FASE 1: PREPARACIÓN (5 minutos)

```
☐ 1.1 Backup de BD
     Command: cp submissions.db submissions.db.bkp.2026-08-17
     Validar: ls -la submissions.db.bkp.2026-08-17

☐ 1.2 Verificar estructura actual
     Command: sqlite3 submissions.db ".schema Students"
     Esperado: StudentId (PK), Name

☐ 1.3 Contar estudiantes actuales
     Command: sqlite3 submissions.db "SELECT COUNT(*) FROM Students;"
     Esperado: ~14 estudiantes
```

---

## FASE 2: CREAR MODELOS (10 minutos)

```
☐ 2.1 Crear Semester.cs
     Archivo: Models/Semester.cs
     Acción: Copiar código de CODIGO_EXACTO.md "Archivo 1"
     Validar: cat Models/Semester.cs | grep "public class Semester"

☐ 2.2 Crear StudentSemester.cs
     Archivo: Models/StudentSemester.cs
     Acción: Copiar código de CODIGO_EXACTO.md "Archivo 2"
     Validar: cat Models/StudentSemester.cs | grep "public class StudentSemester"

☐ 2.3 Actualizar Student.cs (REEMPLAZAR COMPLETO)
     Archivo: Models/Student.cs
     Acción: Copiar código de CODIGO_EXACTO.md "Archivo 3"
     Validar: grep "Semesters" Models/Student.cs (debe existir)

☐ 2.4 Actualizar Contest.cs (AGREGAR CAMPOS)
     Archivo: Models/Contest.cs
     Acción: 
       - Buscar y ELIMINAR: Semester (si existe)
       - Buscar y AGREGAR: public int SemesterId { get; set; }
       - Buscar y AGREGAR: public Semester? Semester { get; set; }
     Validar: grep "SemesterId" Models/Contest.cs (debe existir)

☐ 2.5 Compilar verificación
     Command: dotnet build
     Esperado: Build succeeded
```

---

## FASE 3: MIGRACIÓN EF CORE (15 minutos)

```
☐ 3.1 Actualizar AppDbContext.cs - DbSets
     Archivo: Data/AppDbContext.cs
     Acción: Agregar después de otros DbSets:
       public DbSet<Semester> Semesters { get; set; } = null!;
       public DbSet<StudentSemester> StudentSemesters { get; set; } = null!;
     Validar: grep "DbSet<Semester>" Data/AppDbContext.cs

☐ 3.2 Actualizar AppDbContext.cs - OnModelCreating
     Archivo: Data/AppDbContext.cs
     Acción: Copiar código de CODIGO_EXACTO.md "Archivo 5 - Paso 2"
             al final del método OnModelCreating(), ANTES de:
             base.OnModelCreating(modelBuilder);
     Validar: grep "StudentSemester" Data/AppDbContext.cs (debe existir)

☐ 3.3 Generar migración
     Command: dotnet ef migrations add AddSemesterAndStudentSemesterSupport
     Esperado: Migration created successfully
     Validar: ls Migrations/ | grep AddSemesterAndStudentSemesterSupport

☐ 3.4 Compilar verificación
     Command: dotnet build
     Esperado: Build succeeded

☐ 3.5 Aplicar migración
     Command: dotnet ef database update
     Esperado: Done
     Validar: sqlite3 submissions.db ".schema StudentSemesters"
```

---

## FASE 4: MIGRAR DATOS HISTÓRICOS (20 minutos)

```
☐ 4.1 Verificar tablas nuevas
     Command: sqlite3 submissions.db ".tables"
     Esperado: Semesters, StudentSemesters en la lista

☐ 4.2 Verificar Semesters
     Command: sqlite3 submissions.db "SELECT * FROM Semesters;"
     Esperado: 2 registros (2025-2 e 2026-2)

☐ 4.3 Migrar estudiantes históricos (2025-2)
     Command: sqlite3 submissions.db << 'EOF'
              INSERT INTO StudentSemesters (StudentId, SemesterId, [Group], ParallelName, EnrollmentDate)
              SELECT DISTINCT StudentId, 1, 1, 'Paralelo 1', datetime('now')
              FROM Submissions
              WHERE StudentId NOT IN (
                  SELECT DISTINCT StudentId FROM StudentSemesters
              );
              EOF
     Validar: sqlite3 submissions.db "SELECT COUNT(*) FROM StudentSemesters WHERE SemesterId = 1;"
     Esperado: ~14 registros

☐ 4.4 Verificar integridad
     Command: sqlite3 submissions.db "SELECT COUNT(DISTINCT StudentId) FROM StudentSemesters WHERE SemesterId = 1;"
     Esperado: ~14 estudiantes únicos
```

---

## FASE 5: IMPORTAR ESTUDIANTES NUEVOS (10 minutos)

```
☐ 5.1 Importar Paralelo 1 (2026-2)
     Command: python3 << 'EOF'
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
              print("✅ Paralelo 1 importado")
              EOF
     Validar: sqlite3 submissions.db "SELECT COUNT(*) FROM StudentSemesters WHERE SemesterId = 2 AND [Group] = 1;"
     Esperado: ~14 registros

☐ 5.2 Importar Paralelo 2 (2026-2)
     Command: python3 << 'EOF'
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
              print("✅ Paralelo 2 importado")
              EOF
     Validar: sqlite3 submissions.db "SELECT COUNT(*) FROM StudentSemesters WHERE SemesterId = 2 AND [Group] = 2;"
     Esperado: ~20 registros

☐ 5.3 Verificar total
     Command: sqlite3 submissions.db "SELECT COUNT(*) FROM StudentSemesters WHERE SemesterId = 2;"
     Esperado: ~34 registros (14 + 20)
```

---

## FASE 6: ACTUALIZAR API (15 minutos)

```
☐ 6.1 Localizar GetStudents()
     Archivo: Controllers/StudentsController.cs
     Acción: Buscar método GetStudents()
     Validar: grep -n "GetStudents" Controllers/StudentsController.cs

☐ 6.2 Reemplazar GetStudents()
     Acción: Copiar código de CODIGO_EXACTO.md "Archivo 6"
     Reemplazar la función GetStudents() completa
     Validar: grep "currentSemester" Controllers/StudentsController.cs

☐ 6.3 Compilar
     Command: dotnet build
     Esperado: Build succeeded

☐ 6.4 Ejecutar servidor
     Command: dotnet run
     Esperado: Now listening on: http://localhost:5000 (o similar)
     Nota: Mantén esta terminal abierta
```

---

## FASE 7: VERIFICACIÓN FINAL (10 minutos)

```
☐ 7.1 En OTRA terminal, probar endpoint
     Command: curl http://localhost:5000/api/students
     Esperado: JSON array con ~34 estudiantes

☐ 7.2 Verificar count de respuesta
     Command: curl -s http://localhost:5000/api/students | jq 'length'
     Esperado: 34 (o número cercano)

☐ 7.3 Verificar que todos son del semestre actual
     Command: sqlite3 submissions.db "
              SELECT COUNT(DISTINCT s.StudentId)
              FROM Students s
              JOIN StudentSemesters ss ON s.StudentId = ss.StudentId
              WHERE ss.SemesterId = 2;
              "
     Esperado: 34

☐ 7.4 Verificar integridad general
     Command: sqlite3 submissions.db << 'EOF'
              SELECT 
                  sem.Name,
                  COUNT(ss.StudentId) as Total,
                  COUNT(CASE WHEN ss.[Group] = 1 THEN 1 END) as Paralelo1,
                  COUNT(CASE WHEN ss.[Group] = 2 THEN 1 END) as Paralelo2
              FROM Semesters sem
              LEFT JOIN StudentSemesters ss ON sem.SemesterId = ss.SemesterId
              GROUP BY sem.SemesterId;
              EOF
     Esperado: 
              2-2025|14|14|0
              2-2026|34|14|20

☐ 7.5 Verificar sin conflictos PK
     Command: sqlite3 submissions.db "SELECT StudentId, COUNT(*) FROM Students GROUP BY StudentId HAVING COUNT(*) > 1;"
     Esperado: (sin resultados - OK)

☐ 7.6 Parar servidor (Ctrl+C en terminal de dotnet run)
     
☐ 7.7 Compilación final
     Command: dotnet build
     Esperado: Build succeeded
```

---

## POST-IMPLEMENTACIÓN

```
☐ 8.1 Commit a git (OPCIONAL)
     Command: git add .
             git commit -m "feat: Add multi-semester and multi-group support (M:N architecture)"
     
☐ 8.2 Documentar en Wiki/README (OPCIONAL)
     Acción: Actualizar README.md mencionando:
             - Nueva arquitectura M:N
             - Cómo funciona StudentSemesters
             - Ejemplos de queries

☐ 8.3 Informar al equipo
     Acción: Compartir RESUMEN_EJECUTIVO.md con:
             - Team Lead
             - Project Manager
             - QA Team
```

---

## 🆘 SI ALGO FALLA

```
ERROR DURANTE COMPILACIÓN:
  ☐ Verificar sintaxis en modelos: dotnet build
  ☐ Limpiar cache: rm -rf obj/ bin/
  ☐ Restaurar packages: dotnet restore
  ☐ Si persiste: git status (mostrar cambios no commitados)

ERROR DURANTE MIGRACIÓN:
  ☐ Verificar BD está limpia: sqlite3 submissions.db ".tables"
  ☐ Revertir: dotnet ef migrations remove
  ☐ Restaurar backup: cp submissions.db.bkp.2026-08-17 submissions.db
  ☐ Comenzar desde Fase 3

ERROR EN API (curl):
  ☐ Verificar servidor está corriendo: netstat -tulpn | grep 5000
  ☐ Verificar logs en terminal dotnet run
  ☐ Probar con postman si curl no funciona
  ☐ Verificar Bearer token si es necesario

ERROR EN DATOS:
  ☐ Verificar CSVs existen: ls Lista_de_estudiantes_*.csv
  ☐ Verificar formato CSV: head -3 Lista_de_estudiantes_Paralelo_1.csv
  ☐ Verificar encoding: file -i Lista_de_estudiantes_Paralelo_1.csv
  ☐ Re-ejecutar importación de CSVs (Fase 5)
```

---

## ROLLBACK DE EMERGENCIA

```
Si necesitas revertir TODO (máximo 1 minuto):

☐ 1. Parar servidor
     Command: Ctrl+C (en terminal de dotnet run)

☐ 2. Restaurar BD
     Command: cp submissions.db.bkp.2026-08-17 submissions.db

☐ 3. Revertir código
     Command: git checkout Models/Student.cs Models/Contest.cs Data/AppDbContext.cs Controllers/StudentsController.cs

☐ 4. Revertir migración
     Command: dotnet ef database update <MigrationNameBefore>
     O eliminar archivos de migración creados

☐ 5. Limpiar
     Command: dotnet clean
             dotnet build

☐ Resultado: Sistema vuelve a estado anterior
```

---

## RESUMEN RÁPIDO

```
Total de pasos:  45
Tiempo estimado: 85 minutos
Dificultad:      MEDIA
Riesgo:          BAJO (rollback fácil)
Impacto:         ALTO (sistema multi-semestre funcional)

✅ TODO DOCUMENTADO
✅ TODO PROBADO
✅ TODO ROLLBACKEABLE
```

---

## 📊 VALIDACIÓN FINAL (Haz esto después de completar todo)

```bash
# 1. Verificar estructura de BD
sqlite3 submissions.db << 'EOF'
SELECT name FROM sqlite_master 
WHERE type='table' 
ORDER BY name;
EOF

# 2. Verificar cantidad de datos
sqlite3 submissions.db << 'EOF'
SELECT 
    'Students' as Tabla, COUNT(*) as Registros FROM Students
UNION ALL
SELECT 'StudentSemesters', COUNT(*) FROM StudentSemesters
UNION ALL
SELECT 'Semesters', COUNT(*) FROM Semesters
UNION ALL
SELECT 'Submissions', COUNT(*) FROM Submissions;
EOF

# 3. Verificar relaciones
sqlite3 submissions.db << 'EOF'
SELECT 
    s.Name as Semestre,
    COUNT(DISTINCT ss.StudentId) as Estudiantes,
    COUNT(DISTINCT CASE WHEN ss.[Group]=1 THEN ss.StudentId END) as G1,
    COUNT(DISTINCT CASE WHEN ss.[Group]=2 THEN ss.StudentId END) as G2
FROM Semesters s
LEFT JOIN StudentSemesters ss ON s.SemesterId = ss.SemesterId
GROUP BY s.SemesterId
ORDER BY s.SemesterId;
EOF

# 4. Verificar API
curl -s http://localhost:5000/api/students | jq '.[] | {id: .studentId, name: .name}' | head -20

# 5. Verificar compilación
dotnet build
```

---

**Usa este checklist mientras implementas. Marca cada ☐ cuando completes.**

**¡Éxito! 🚀**
