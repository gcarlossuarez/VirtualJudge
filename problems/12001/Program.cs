// File: P2_MinItemsExact.cs
// Carga exacta con mínimo número de bultos
// Cambio de monedas
using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        var first = Console.ReadLine()!.Trim().Split();
        int N = int.Parse(first[0]);
        int C = int.Parse(first[1]);
        var w = Console.ReadLine()!.Trim().Split().Select(int.Parse).ToArray();

        var (ok, chosen) = Solve(w, C);
        if (!ok) { Console.WriteLine("IMPOSSIBLE"); return; }
        Console.WriteLine(chosen.Count);
        Console.WriteLine(string.Join(" ", chosen.Select(i => i + 1).OrderBy(x => x)));
    }

    static (bool ok, List<int> chosen) Solve(int[] a, int T)
    {
        const int INF = 1_000_000;
        int N = a.Length;
        var dp = Enumerable.Repeat(INF, T + 1).ToArray();
        var fromI = Enumerable.Repeat(-1, T + 1).ToArray();
        dp[0] = 0;

        for (int i = 0; i < N; i++)
        {
            for (int s = T; s >= a[i]; s--)
            {
                if (dp[s - a[i]] + 1 < dp[s])
                {
                    dp[s] = dp[s - a[i]] + 1;
                    fromI[s] = i;
                }
            }
        }

        if (dp[T] >= INF) return (false, new List<int>());

        var chosen = new List<int>();
        int cur = T;
        while (cur > 0)
        {
            int idx = fromI[cur];
            if (idx < 0) break; // robustez
            chosen.Add(idx);
            cur -= a[idx];
        }
        chosen.Sort();
        return (true, chosen);
    }
}
