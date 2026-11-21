using System;
using System.Linq;

// Distancia de Edición o Distancia de Levenshtein
class Program
{
    static void Main()
    {
        // Tolerar líneas en blanco y \r\n / \n indistintamente
        var allLines = Console.In.ReadToEnd()
            .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToArray();

        if (allLines.Length == 0) return;

        int T = int.Parse(allLines[0]);
        int idx = 1; // índice del cursor en allLines

        for (int caso = 0; caso < T; caso++)
        {
            var nm = allLines[idx].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int N = int.Parse(nm[0]);
            int M = int.Parse(nm[1]);

            string[] A = allLines[idx + 1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string[] B = allLines[idx + 2].Split(' ', StringSplitOptions.RemoveEmptyEntries);

            idx += 3;

            int[,] dp = new int[N + 1, M + 1];
            for (int i = 0; i <= N; i++) dp[i, 0] = i;
            for (int j = 0; j <= M; j++) dp[0, j] = j;

            for (int i = 1; i <= N; i++)
                for (int j = 1; j <= M; j++)
                {
                    if (A[i - 1] == B[j - 1])
                        dp[i, j] = dp[i - 1, j - 1];
                    else
                        dp[i, j] = Math.Min(Math.Min(
                            dp[i - 1, j - 1] + 1,
                            dp[i - 1, j] + 1),
                            dp[i, j - 1] + 1);
                }

            Console.WriteLine(dp[N, M]);
        }
    }
}