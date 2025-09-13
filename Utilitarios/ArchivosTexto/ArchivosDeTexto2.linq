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
			int character;
			
	       	// Leer carácter por carácter hasta el final del archivo
            while ((character = reader.Read()) != -1)  // Read devuelve -1 si llega al final del archivo
            {
                Console.Write((char)character);  // Convertir el entero a char para mostrar el carácter
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