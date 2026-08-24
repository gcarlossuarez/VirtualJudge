
using System;
using System.Linq;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
		string contenido = Console.ReadLine() ?? string.Empty;
		// Separar por comas y limpiar espacios en blanco si los hubiera
        List<string> numeros = contenido.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                            .Select(n => n.Trim())
                                            .Where(n => !string.IsNullOrEmpty(n))
                                            .ToList();

        // Validar la cantidad de elementos (entre 1 y 8)
        if (numeros.Count < 1 || numeros.Count > 8)
        {
            Console.WriteLine("Error: El conjunto debe tener entre 1 y 8 elementos.");
            return;
        }

        // 2. Generar las permutaciones usando una lista para guardar los resultados
        List<string> lineasResultado = new List<string>();
        GenerarPermutaciones(0, numeros.Count, numeros, lineasResultado);
        //lineasResultado.Sort();
		Console.WriteLine(string.Join('\n', lineasResultado));
	}


    /// <summary>
    /// Algoritmo de Heap para generar permutaciones de forma eficiente.
    /// </summary>
    static void GenerarPermutaciones(int c, int n, List<string> lista, List<string> resultado)
    {
        if (n == c)
        {
            // Formato estricto: valores separados por comas, sin espacios
            resultado.Add(string.Join(",", lista));
            return;
        }

        for (int i = c; i < n; i++)
        {
            Intercambiar(lista, c, i);
            GenerarPermutaciones(c + 1, n, lista, resultado);
            Intercambiar(lista, c, i);

        }
    }

    /// <summary>
    /// Método auxiliar para intercambiar dos elementos en la lista.
    /// </summary>
    static void Intercambiar(List<string> lista, int indiceA, int indiceB)
    {
        string temporal = lista[indiceA];
        lista[indiceA] = lista[indiceB];
        lista[indiceB] = temporal;
    }
}

