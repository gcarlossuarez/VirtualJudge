using System;
using System.Collections.Generic;
using System.IO;


// Red de transporte urbano
// FloydWarshall
class Program
{
    static void Main()
    {
		
		
      List<string> linesList = new List<string>();
	    // Leer datos de entrada
      string val;
      //while (!string.IsNullOrEmpty(val = Console.ReadLine()))
      while ((val = Console.ReadLine()) != null)  // Permite leer lineas en blanco.
                                                  // Si se lee desde un archivo, sale con EOF
                                                  // Si se lee desde consola, sale del while con Control + C
      {
          if(!string.IsNullOrEmpty(val) && !string.IsNullOrWhiteSpace(val))
          {
            linesList.Add(val);
          }
      }
	    var lines = linesList.ToArray();
	    int N = int.Parse(lines[0]);
	    int[,] dist = new int[N, N];

	    // Inicializar la matriz de distancias
	    for (int i = 0; i < N; i++)
	    {
	        for (int j = 0; j < N; j++)
	        {
	            dist[i, j] = (i == j) ? 0 : int.MaxValue;
	        }
	    }

	    // Leer rutas y llenar la matriz
	    for (int i = 1; i < lines.Length; i++)
	    {
	        var parts = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
	        int origen = int.Parse(parts[0]) - 1;
	        int destino = int.Parse(parts[1]) - 1;
	        int costo = int.Parse(parts[2]);
				//dist[origen, destino] = costo; // CUIDADO CON LOS REPETIDOS
	        dist[origen, destino] = Math.Min(dist[origen, destino], costo); // Si existen varias rutas directas, se escoge la minima; ya que, si para llegar 
																				// de manera directa de una parada a otra hay más de una alternativa, se escoge 
																				// la más pequeña, porque viene siendo lo mismo
	    }

      //int h = 0;
      //for(int i = 0; i < N; ++i)
      //{
      //  for(int j = 0; j < N; ++j)
      //    Console.Write(dist[i, j] + " ");
      //  Console.WriteLine();
      //}


	    // Algoritmo de Floyd-Warshall
	    for (int k = 0; k < N; k++)
	    {
	        for (int i = 0; i < N; i++)
	        {
	            for (int j = 0; j < N; j++)
	            {
	                if (dist[i, k] != int.MaxValue && dist[k, j] != int.MaxValue)
	                {
	                    dist[i, j] = Math.Min(dist[i, j], dist[i, k] + dist[k, j]);
	                }
	            }
	        }
	    }

	    // Detectar ciclos negativos
	    bool cicloNegativo = false;
	    for (int i = 0; i < N; i++)
	    {
	        if (dist[i, i] < 0)
	        {
	            cicloNegativo = true;
	            Console.WriteLine("Existe un ciclo negativo.");
	            break;
	        }
	    }

	    if (!cicloNegativo)
	    {
          
	        for (int i = 0; i < N; i++)
	        {
	            for (int j = 0; j < N; j++)
	            {
	                Console.Write((dist[i, j] == int.MaxValue ? "-1" : dist[i, j].ToString()) + " ");
	            }
	            Console.WriteLine();
	        }
	    }
		
    }
}
