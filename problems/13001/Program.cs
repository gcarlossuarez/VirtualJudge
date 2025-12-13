using System;
using System.Collections.Generic;

// Dijkstra con firma obligatoria
public class Program
{
    // 🚨 Firma obligatoria
    // n      = cantidad de nodos (0..n-1)
    // edges  = lista de aristas [u, v, w]
    // source = nodo origen
    // Retorna: dist[i] = distancia mínima desde source hasta i
    //          o int.MaxValue si i es inalcanzable.
    public static int[] Resolver(int n, int[][] edges, int source)
    {
        const int INF = int.MaxValue;

        // 1) Inicializar distancias
        int[] dist = new int[n];
        for (int i = 0; i < n; i++)
            dist[i] = INF;
        dist[source] = 0;

        // 2) Construir lista de adyacencia
        var adj = new List<(int to, int w)>[n];
        for (int i = 0; i < n; i++)
            adj[i] = new List<(int to, int w)>();

        foreach (var e in edges)
        {
            int u = e[0];
            int v = e[1];
            int w = e[2];
            adj[u].Add((v, w));
        }

        // 3) Cola de prioridad (min-heap) con (distancia, nodo)
        var pq = new PriorityQueue<(int node, int d), int>();
        pq.Enqueue((source, 0), 0);

        // 4) Dijkstra
        while (pq.Count > 0)
        {
            var (u, d) = pq.Dequeue();

            // Si la distancia que sacamos ya no es la vigente, se descarta
            if (d != dist[u])
                continue;

            foreach (var (to, w) in adj[u])
            {
                if (dist[u] == INF) continue; // Por seguridad, aunque no debería ocurrir aquí

                long nuevaDist = (long)dist[u] + w;
                if (nuevaDist < dist[to])
                {
                    dist[to] = (int)nuevaDist;
                    pq.Enqueue((to, dist[to]), dist[to]);
                }
            }
        }

        return dist;
    }

    private static bool _debugMode = false;

    // Programa de ejemplo que respeta el enunciado del examen
    public static void Main()
    {
        // Leer n y m
        var linea = Console.ReadLine()!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int n = int.Parse(linea[0]);
        int m = int.Parse(linea[1]);

        int[][] edges = new int[m][];

        for (int i = 0; i < m; i++)
        {
            var p = Console.ReadLine()!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int u = int.Parse(p[0]);
            int v = int.Parse(p[1]);
            int w = int.Parse(p[2]);
            edges[i] = new int[] { u, v, w };
        }

        int source = int.Parse(Console.ReadLine()!);

        int[] result = Resolver(n, edges, source);

        // Imprimir las distancias en una sola línea
        Console.WriteLine(string.Join(" ", result.Select(x=> x != int.MaxValue ? x.ToString() : "INF")));
        if(_debugMode)
        {
            Console.WriteLine("Pulse una tecla, para continuar...");
            Console.ReadLine();
        }
    }
}

