using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EntregaDeRecursos
{
    class Program
    {
        static void Main()
        {
			// Para forzar UTF-8 en la salida, si no se hace la conversión, de manera directa desde la consola, al ejecutarlo, 
			// o desde donde se lo ejecute
			Console.OutputEncoding = System.Text.Encoding.UTF8;

            // --- Lectura rápida desde STDIN ---
            var lines = new List<string>();
            string? line;
            while ((line = Console.In.ReadLine()) != null)
            {
                string cleaned = line.Trim();
                if (cleaned.Length > 0)
                    lines.Add(cleaned);
				//if (cleaned[0] == '\uFEFF') cleaned = cleaned.Substring(1);
            }

            if (lines.Count == 0) return;

            var conexiones = new List<(int u, int v, int w)>();

			string firstLine = string.Empty;
			int firstSource = int.MinValue, lastDestiny = int.MinValue;
            // Leer todas las conexiones
            foreach (var lineAct in lines)
            {
                if(int.TryParse(lineAct.Substring(0, 9), out int source) &&     // Primeros 9 caracteres
					int.TryParse(lineAct.Substring(9, 9), out int destiny) &&     // Siguientes 9 caracteres
					int.TryParse(lineAct.Substring(18, 9), out int weight ))       // Últimos 9 caracteres
				{
					//Console.WriteLine($"Origen: {source}, Destino: {destiny}, Peso: {weight}");
					if(string.IsNullOrEmpty(firstLine))
					{
						//Console.WriteLine(source);
						firstLine = lineAct;
						firstSource = source;
					}
					lastDestiny = destiny;
					conexiones.Add((source, destiny, weight));
				}
				//else{
				//	Console.WriteLine(lineAct.Substring(0, 9));
				//	Console.WriteLine(lineAct.Substring(9, 9));
				//	Console.WriteLine(lineAct.Substring(18, 9));
				//	Console.WriteLine("Problemas con la linea " + (++i) + " " + lineAct);
				//}
			}
            
			if(conexiones.Count > 0)
			{
				AlmacenLogistico.RutaMenorResistencia(conexiones.Count, conexiones, firstSource, lastDestiny);
			}
        }
    }

    public class AlmacenLogistico
    {
        public static void RutaMenorResistencia(int n, List<(int u, int v, int w)> conexiones, int inicio, int destino)
        {
            var resistencia = new int[n];
            for (int i = 0; i < n; i++) resistencia[i] = int.MaxValue;
            resistencia[inicio] = 0;

            // Usamos SortedSet: ordena por (distancia, nodo)
            var queue = new SortedSet<(int distancia, int nodo)>();
            queue.Add((0, inicio));

            // Grafo: lista de adyacencia indexada por nodo
            var grafo = new List<(int nodo, int peso)>[n];
            for (int i = 0; i < n; i++) grafo[i] = new List<(int, int)>();

            // Construir grafo (no dirigido)
            foreach (var (u, v, w) in conexiones)
            {
                grafo[u].Add((v, w));
                grafo[v].Add((u, w));
            }

            // Dijkstra con SortedSet
            while (queue.Count > 0)
            {
                var (distActual, u) = queue.Min;
                queue.Remove(queue.Min);

                // Si ya tenemos mejor camino, saltar
                if (distActual > resistencia[u]) continue;

                foreach (var (v, peso) in grafo[u])
                {
                    if (resistencia[u] + peso < resistencia[v])
                    {
                        // Remover la versión anterior si existe
                        queue.Remove((resistencia[v], v));
						
                        resistencia[v] = resistencia[u] + peso;
						
						// Cargar la nueva versión
                        queue.Add((resistencia[v], v));
                    }
                }
            }

            // --- SALIDA ---
            string resultado = resistencia[destino] == int.MaxValue
                ? "INF"
                : resistencia[destino].ToString();

            
            // SALIDA POR STDOUT
            Console.WriteLine($"Resistencia mínima desde {inicio} a {destino}: {resultado}");
        }
    }
}