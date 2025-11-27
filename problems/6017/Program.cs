using System;
using System.Collections.Generic;
using System.Linq;

// WarehouseGroupingSolverDFS
class Program
{
    static void Main()
    {
        // Leer primera línea: N y M
        string? first = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(first))
        {
            return; // No hay datos
        }

        var parts = first.Split();
        int numWarehouses = int.Parse(parts[0]);
        int numConnections = int.Parse(parts[1]);

        // Lista de conexiones
        var connections = new List<(int u, int v)>();

        for (int i = 0; i < numConnections; i++)
        {
            string? line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                i--; 
                continue;
            }

            var p = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int u = int.Parse(p[0]);
            int v = int.Parse(p[1]);

            connections.Add((u, v));
        }

        // Resolver bipartición
        var result = SolveGroupingDFS(numWarehouses, connections);

        // Imprimir salida al estilo contest
        Console.WriteLine(result.Item1 ? "True" : "False");

        if (result.Item1)
        {
            Console.WriteLine(string.Join(" ", result.Item2));
            Console.WriteLine();
            Console.WriteLine(string.Join(" ", result.Item3));
        }
    }

    static (bool, List<int>, List<int>) SolveGroupingDFS(int numWarehouses, List<(int u, int v)> connections)
    {
        // Crear adyacencia
        var adjacencyList = new Dictionary<int, List<int>>();
        for (int i = 1; i <= numWarehouses; i++)
        {
            adjacencyList[i] = new List<int>();
        }

        foreach (var (u, v) in connections)
        {
            adjacencyList[u].Add(v);
            adjacencyList[v].Add(u);
        }

        // Colores
        int[] colors = Enumerable.Repeat(-1, numWarehouses + 1).ToArray();

        // DFS por componentes
        for (int i = 1; i <= numWarehouses; i++)
        {
            if (colors[i] == -1)
            {
                if (!DfsCheckBipartite(adjacencyList, colors, i, 0))
                {
                    return (false, new List<int>(), new List<int>());
                }
            }
        }

        // Grupos
        var group1 = new List<int>();
        var group2 = new List<int>();

        for (int i = 1; i <= numWarehouses; i++)
        {
            if (colors[i] == 0)
                group1.Add(i);
            else if (colors[i] == 1)
                group2.Add(i);
        }

        return (true, group1, group2);
    }

    static bool DfsCheckBipartite(Dictionary<int, List<int>> adjacencyList, int[] colors, int node, int currentColor)
    {
        colors[node] = currentColor;

        foreach (var neighbor in adjacencyList[node])
        {
            if (colors[neighbor] == -1)
            {
                if (!DfsCheckBipartite(adjacencyList, colors, neighbor, 1 - currentColor))
                {
                    return false;
                }
            }
            else if (colors[neighbor] == currentColor)
            {
                return false;
            }
        }

        return true;
    }
}
