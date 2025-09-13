<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.IO.dll</Reference>
</Query>

/*
using System;
using System.IO;
*/

void Main()
{
	string fullPathFile = @"D:\Universidad Católica Boliviana\AlgoritmosEjemplo\Archivos\ArchivosTexto\Ejemplo1.txt";

	try
	{
		using(StreamReader reader = new StreamReader(fullPathFile))
		{
			string line;
			// Leer línea por línea hasta el final del archivo
	        while ((line = reader.ReadLine()) != null)
	        {
	            Console.WriteLine(line);
	        }
		}
	}
	catch (Exception ex)
    {
        Console.WriteLine("Ocurrió un error al leer el archivo:");
        Console.WriteLine(ex.Message);
    }
}

// Define other methods and classes here
