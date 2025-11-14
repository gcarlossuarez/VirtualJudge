using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

class Program
{
    static void Main()
    {
        List<(int, int, int)> caminos = new List<(int, int, int)>();
        
        // Leer desde stdin en lugar de archivos
        string input = ReadFromStdin();
        var lines = input.Split('\n')
                        .Where(l => !string.IsNullOrEmpty(l) && !string.IsNullOrWhiteSpace(l))
                        .ToList();

        // Procesar cada línea de entrada
		int maxNode = int.MinValue;
        foreach (string line in lines)
        {
            string[] fields = line.Split(';');
            
            if (fields.Length == 3 && 
                int.TryParse(fields[0], out int source) && 
                int.TryParse(fields[1], out int destiny) && 
                int.TryParse(fields[2], out int weight))
            {
                caminos.Add((source, destiny, weight));
				if(source > maxNode)
				{
					maxNode = source;
				}
				if(destiny > maxNode)
				{
					maxNode = destiny;
				}
            }
        }

        if (caminos.Count > 0)
        {
            // Tomar el primer vértice como inicio
            (int sourceVertex, int _, int _) = caminos[0];
            
            // Ejecutar Dijkstra
            RedEmergencia.Dijkstra(maxNode + 1, caminos, sourceVertex);
        }
        else
        {
            Console.WriteLine("No se encontraron datos válidos en la entrada.");
        }
    }
    
    static string ReadFromStdin()
    {
        StringBuilder sb = new StringBuilder();
        string? line;
        
        // Leer todas las líneas de la entrada estándar
        while ((line = Console.ReadLine()) != null)
        {
            sb.AppendLine(line);
        }
        
        return sb.ToString();
    }
}

public class RedEmergencia
{
    public static void Dijkstra(int n, List<(int, int, int)> caminos, int inicio)
    {
        var distancia = new int[n];
        for (int i = 0; i < n; i++) 
        {
            distancia[i] = int.MaxValue;
        }
        distancia[inicio] = 0;

        var queue = new SortedSet<(int, int)>();
        queue.Add((0, inicio));

        var grafo = new List<(int, int)>[n];
        for (int i = 0; i < n; i++) 
        {
            grafo[i] = new List<(int, int)>();
        }

        foreach (var (u, v, peso) in caminos)
        {
            grafo[u].Add((v, peso));
            grafo[v].Add((u, peso));
        }

        while (queue.Count > 0)
        {
            var (distActual, u) = queue.Min;
            queue.Remove(queue.Min);

            foreach (var (v, peso) in grafo[u])
            {
                if (distancia[u] + peso < distancia[v])
                {
                    queue.Remove((distancia[v], v));
                    distancia[v] = distancia[u] + peso;
                    queue.Add((distancia[v], v));
                }
            }
        }

        // Mostrar resultados en consola
        for (int i = 0; i < n; i++)
        {
            Console.WriteLine($"Distancia desde {inicio} a {i}: {(distancia[i] == int.MaxValue ? "INF" : distancia[i].ToString())}");
        }
    }
}