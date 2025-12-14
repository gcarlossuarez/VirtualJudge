using System;
using System.Collections.Generic;

public class Program
{
    // 🚨 Firma OBLIGATORIA
    public static int[] Dijkstra(int n, int[][] edges, int source)
    {
        // El alumno implementa esto
        var dist = new int[n];
        for (int i = 0; i < n; i++) dist[i] = int.MaxValue;

        dist[source] = 0;

        var pq = new PriorityQueue<(int d, int node), int>();
        pq.Enqueue((0, source), 0);

        var adj = new List<(int to, int w)>[n];
        for (int i = 0; i < n; i++) adj[i] = new();

        foreach (var e in edges)
        {
            adj[e[0]].Add((e[1], e[2]));
        }

        while (pq.Count > 0)
        {
            var (d, u) = pq.Dequeue();
            if (d != dist[u]) continue;

            foreach (var (to, w) in adj[u])
            {
                if (dist[u] + w < dist[to])
                {
                    dist[to] = dist[u] + w;
                    pq.Enqueue((dist[to], to), dist[to]);
                }
            }
        }

        return dist;
    }
}
