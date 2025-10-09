<Query Kind="Program">
  <Namespace>System</Namespace>
  <Namespace>System.IO</Namespace>
  <Namespace>System.Linq</Namespace>
</Query>

/*
using System;
using System.IO;
using System.Linq;
*/

class GeneradorEntradas
{
    static void Main()
    {
        string basePath = @"D:\Universidad Católica Boliviana\Semestre2-2025\Banco de ejercicios para práctica en el Juez Virtual\Problem6006\6006\IN\";

        // Obtiene todos los subdirectorios tipo "VersionesNNN"
        var versionDirs = Directory.GetDirectories(basePath, "Versiones*")
                                   .OrderBy(d => d)
                                   .ToList();

        // Carpeta donde se guardarán los .in
        string outPath = Path.Combine(basePath, "DataSet_Juez");
        if (!Directory.Exists(outPath))
            Directory.CreateDirectory(outPath);

		int counter = 0;
        foreach (var dir in versionDirs)
        {
            string originalFile = Path.Combine(dir, "ArchivoOriginal.txt");
            string modificadoFile = Path.Combine(dir, "ArchivoModificado.txt");

            if (!File.Exists(originalFile) || !File.Exists(modificadoFile))
            {
                Console.WriteLine($"⚠️ Archivos faltantes en {dir}");
                continue;
            }

            string nombre = $"datos{(++counter).ToString("0000")}.txt"; // new DirectoryInfo(dir).Name; // ej: Versiones000
            string outFile = Path.Combine(outPath, nombre);

            // Leer y concatenar con separador
            var original = File.ReadAllLines(originalFile);
            var modificado = File.ReadAllLines(modificadoFile);

            var contenido = original.Concat(new string[] { "====" }).Concat(modificado);

            File.WriteAllLines(outFile, contenido);

            Console.WriteLine($"✔ Generado: {outFile}");
        }
    }
}
