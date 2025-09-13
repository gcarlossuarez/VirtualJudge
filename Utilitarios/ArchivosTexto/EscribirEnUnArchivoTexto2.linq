<Query Kind="Program">
  <Namespace>System</Namespace>
  <Namespace>System.IO</Namespace>
</Query>

/*
using System;
using System.IO;

*/
class Program
{
    static void Main()
    {
        string path = @"D:\Universidad Católica Boliviana\Semestre2-2024\PrimerParcial\Utilitarios\ArchivosTexto\FileText2.txt";
        string[] lines = {"Hola, esto es una prueba", "Segunda línea", "Tercer línea"};
		File.WriteAllLines(path, lines);

		
        Console.WriteLine("Texto escrito en el archivo.");
        // Usar StreamReader para leer en el archivo
        using (StreamReader sw = new StreamReader(path))
        {
			string line;
			while((line = sw.ReadLine()) != null)
			{
				Console.WriteLine(line);
			}
        }
    }
}
