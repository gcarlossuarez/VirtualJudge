using System;
using System.Collections.Generic;

// Conectar amigos
class Program
{
    static void Main()
    {
        var lines = new List<string>();
        string? input;
        while (!string.IsNullOrEmpty(input = Console.ReadLine()))
        {
            lines.Add(input);
        }

        if (lines.Count == 0)
        {
            Console.WriteLine("Datos de entrada vacíos.");
            return;
        }

        // Primera línea: origen;destino
        string[] targets = lines[0].Split(';', StringSplitOptions.RemoveEmptyEntries);
        if (targets.Length < 2)
        {
            Console.WriteLine("Formato inválido en la primera línea.");
            return;
        }
        string source = targets[0].Trim();
        string destiny = targets[1].Trim();

        // Construir grafo no dirigido (amigos mutuos)
        var graph = new Dictionary<string, HashSet<string>>();

        for (int i = 1; i < lines.Count; i++)
        {
            string[] parts = lines[i].Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 1) continue;

            string person = parts[0].Trim();
            if (!graph.ContainsKey(person))
                graph[person] = new HashSet<string>();

            for (int j = 1; j < parts.Length; j++)
            {
                string friend = parts[j].Trim();
                graph[person].Add(friend);

                // Añadir conexión en ambos sentidos (grafo no dirigido)
                if (!graph.ContainsKey(friend))
                    graph[friend] = new HashSet<string>();
                graph[friend].Add(person);
            }
        }

        // BFS optimizado en memoria
        int connections = FindShortestDistance(graph, source, destiny);

        if (connections >= 0)
            Console.WriteLine($"Número de conexiones: {connections}");
        else
            Console.WriteLine($"No hay conexión entre {source} y {destiny}.");
    }

    // Devuelve el número de conexiones (aristas) o -1 si no hay camino
    static int FindShortestDistance(Dictionary<string, HashSet<string>> graph, string start, string end)
    {
        if (start == end) return 0;

        var queue = new Queue<string>();
        var parent = new Dictionary<string, string>(); // Para reconstruir (opcional)
        var distance = new Dictionary<string, int>(); // Distancia desde start

        queue.Enqueue(start);
        distance[start] = 0;

        while (queue.Count > 0)
        {
            string current = queue.Dequeue();

            if (!graph.TryGetValue(current, out var friends))
                continue;

            foreach (string neighbor in friends)
            {
                if (!distance.ContainsKey(neighbor)) // Equivalente a verificar si está visitado
                {
                    distance[neighbor] = distance[current] + 1;
                    parent[neighbor] = current; // Opcional: para reconstruir camino completo
                    queue.Enqueue(neighbor);

                    if (neighbor == end)
					{
						return distance[neighbor]; // ¡Encontrado! Devolvemos la distancia
					}
                }
            }
        }

        return -1; // No hay camino
    }
}
