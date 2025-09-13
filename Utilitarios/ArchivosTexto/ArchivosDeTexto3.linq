<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.IO.dll</Reference>
</Query>

/*
using System;
using System.IO;
*/

void Main()
{
	//string fullPathFile = "D:\\Universidad Católica Boliviana\\AlgoritmosEjemplo\\Archivos\\ArchivosTexto\\Ejemplo1.txt";/
	string fullPathFile = @"D:\Universidad Católica Boliviana\AlgoritmosEjemplo\Archivos\ArchivosTexto\Ejemplo1.txt";

	try
	{
		// Leer todas las líneas del archivo
        //List<string> lines = new List<string>(File.ReadAllLines(fullPathFile));
		List<string> lines = File.ReadAllLines(fullPathFile).ToList(); // Equivalente a la instrucción de arriba

        // Mostrar cada línea de la lista
        foreach (string line in lines)
        {
            Console.WriteLine(line);
        }
	}
	catch (Exception ex)
    {
        Console.WriteLine("Ocurrió un error al leer el archivo:");
        Console.WriteLine(ex.Message);
    }
}