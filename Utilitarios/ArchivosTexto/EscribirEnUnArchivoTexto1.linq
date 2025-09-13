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
        string path = @"D:\Universidad Católica Boliviana\Semestre2-2024\PrimerParcial\Utilitarios\ArchivosTexto\FileText1.txt";
        string textoParaEscribir = "Hola, esto es una prueba";

        // Usar StreamWriter para escribir en el archivo
        using (StreamWriter sw = new StreamWriter(path))
        {
            sw.WriteLine(textoParaEscribir);
			sw.Write("Escribe algo. ");
			sw.Write("Escribe an la misma línea");
			sw.WriteLine();
        }
		
		string[] lines = File.ReadAllLines(path);
        Console.WriteLine("Texto escrito en el archivo.");
		Console.WriteLine(string.Join("\n", lines));
    }
}
