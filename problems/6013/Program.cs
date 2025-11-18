
using System;
using System.Collections.Generic;

// ConectarAmigos
class Program
{
    static void Main()
    {
		string? val;
		List<string> lines = new List<string>();
        while (!string.IsNullOrEmpty(val = Console.ReadLine()))
        {
			if(!string.IsNullOrEmpty(val))
			{
				lines.Add(val);
			}
        }
        
		// Representación de la red social
		var socialNetwork = new Dictionary<string, List<string>>();
	
		if (lines.Count() == 0)
		{
			Console.WriteLine($"Datos de entrada vacíos.");
			return;
		}

		// Leer el origen y destino de la primera línea del archivo de entrada
		string[] targetFriends = lines[0].Split(';');
		
		string source = targetFriends[0].Trim();
		string destiny = targetFriends[1].Trim();
		//lines.Skip(1).Dump();
		foreach(var line in lines.Skip(1))
		{
			string[] fields = line.Split(';');
			List<string> friends = new List<string>();
			for(int i = 1; i < fields.Length; ++i)
			{
				friends.Add(fields[i].Trim());
			}
			socialNetwork.Add(fields[0], friends);
		}

		// Encontrar el camino más corto
		List<string> path = FindShortestPath(socialNetwork, source, destiny);

		// Mostrar el resultado en el archivo de salida		
		if (path.Count() > 0)
		{
			//Console.WriteLine($"El camino más corto de {source} a {destiny} es: {string.Join(" -> ", path)}");
			Console.WriteLine($"Número de conexiones: {path.Count - 1}");
		}
		else
		{
			Console.WriteLine($"No hay conexión entre {source} y {destiny}.");
		}
    }

    static List<string> FindShortestPath(Dictionary<string, List<string>> graph, string inicio, string objetivo)
    {
        var visited = new HashSet<string>();
        var queue = new Queue<List<string>>();

        // Iniciar la cola con el nodo de inicio
        queue.Enqueue(new List<string> { inicio });
        visited.Add(inicio);

        while (queue.Count > 0)
        {
            // Obtener el primer camino de la cola
            var pathAct = queue.Dequeue();
            string nodeAct = pathAct[pathAct.Count() - 1];

            // Verificar si hemos llegado al nodo objetivo
            if (nodeAct == objetivo)
            {
                return pathAct;
            }

            // Agregar a la cola los amigos no visitados del nodo actual
            if (graph.ContainsKey(nodeAct))
            {
                foreach (var friend in graph[nodeAct])
                {
                    if (!visited.Contains(friend))
                    {
                        visited.Add(friend);
                        var newPath = new List<string>(pathAct) { friend };
                        queue.Enqueue(newPath);
                    }
                }
            }
        }

        // Retorna null si no hay conexión
        return new List<string>();
    }
}