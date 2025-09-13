# Guía rápida: dotnet-ef con SQLite

## 1. Verificar que `dotnet-ef` está disponible
```bash
dotnet ef --version
```

2. Crear una migración

(cuando defines o cambias tus clases de modelo)

```bash
dotnet ef migrations add InitialCreate
```


Esto genera una carpeta Migrations/ con el código para crear tu esquema en la BD.

3. Aplicar la migración (crear o actualizar la base de datos)
```bash
dotnet ef database update
```


Con eso, si tu AppDbContext apunta a SQLite, se genera automáticamente el archivo .db en la ruta que configuraste (ejemplo app.db).

4. Listar migraciones existentes
```bash
dotnet ef migrations list
```

5. Ver ayuda general
```bash
dotnet ef --help
```

Ejemplo práctico con SQLite
Modelo simple
namespace DemoEfSqlite.Models;

```csharp
public class Student
{
    public long StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
}

DbContext con SQLite
using Microsoft.EntityFrameworkCore;
using DemoEfSqlite.Models;

namespace DemoEfSqlite.Data;

public class AppDbContext : DbContext
{
    public DbSet<Student> Students => Set<Student>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Se genera un archivo SQLite llamado app.db en el directorio de salida
        optionsBuilder.UseSqlite("Data Source=app.db");
    }
}

Program.cs de prueba
using DemoEfSqlite.Data;
using DemoEfSqlite.Models;

using var context = new AppDbContext();

// Asegura que la base de datos esté creada
context.Database.EnsureCreated();

// Insertar datos de prueba
if (!context.Students.Any())
{
    context.Students.Add(new Student { Name = "Ana" });
    context.Students.Add(new Student { Name = "Luis" });
    context.SaveChanges();
    Console.WriteLine("Datos insertados.");
}

// Listar estudiantes
foreach (var s in context.Students)
{
    Console.WriteLine($"[{s.StudentId}] {s.Name}");
}

Flujo típico de trabajo

Definir tus modelos (Student, Contest, etc.).

Ejecutar:

dotnet ef migrations add InitialCreate


Crear/actualizar la BD:

dotnet ef database update


Verificar que app.db se haya creado (lo puedes abrir con DB Browser for SQLite).

```

