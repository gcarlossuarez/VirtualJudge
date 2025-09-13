using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace CsvToEfImporter
{
    public class Student
    {
        public long StudentId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AppDbContext : DbContext
    {
        public DbSet<Student> Students { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 📌 Usa SQLite en un archivo local
            optionsBuilder.UseSqlite("Data Source=/home/vboxuser/CsJudgeApi/submissions.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>().HasKey(s => s.StudentId);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string filePath = args.Length > 0 ? args[0] : "REP_Inscritos_T.csv";

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"❌ No se encontró el archivo {filePath}");
                return;
            }

            var students = new List<Student>();
            int rowIndex = 0;

            foreach (var line in File.ReadLines(filePath))
            {
                rowIndex++;

                // ⏭ Salta primera fila (cabeceras y datos no útiles)
                if (rowIndex <= 1) continue;

                var parts = line.Split(';');

                //if (parts.Length < 12) continue;

                if (long.TryParse(parts[3], out long nroRegistro))
                {
                    string nombre = parts[13].Trim();

                    if (!string.IsNullOrWhiteSpace(nombre))
                    {
                        students.Add(new Student
                        {
                            StudentId = nroRegistro,
                            Name = nombre
                        });
                    }
                }
            }

            Console.WriteLine($"Leídos {students.Count} estudiantes del CSV.");

            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated(); // ✅ crea el archivo ContestDB.db si no existe
                db.Students.AddRange(students);
                db.SaveChanges();
            }

            Console.WriteLine("✅ Importación completada en SQLite (ContestDB.db).");
        }
    }
}

