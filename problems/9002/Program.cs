using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        string line;
        List<string> lines = new List<string>();
        // Leer hasta que no haya más líneas (hasta el final del stream)
        while ((line = Console.ReadLine()) != null)
        {
            lines.Add(line);
        }
        // Eliminar líneas en blanco, por las dudas
        lines = lines
            .Where(l => !string.IsNullOrEmpty(l) && !string.IsNullOrWhiteSpace(l))
            .ToList();

        int cont = 0; // Inicializa el contador de líneas leídas

        // Leer N
        int N = int.Parse(lines[cont++].Trim());
        List<string> norte = new List<string>();
        // Recorrer el primer grpo de boyardos
        for (int i = 0; i < N; i++)
        {
            line = lines[cont++].Trim();
            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                norte.Add(parts[1]); // Solo la etnia; porque es lo que realmente importa, en este problema
            }
        }

        // Leer M
        int M = int.Parse(lines[cont++].Trim());
        List<string> sur = new List<string>();
        // Recorrer el segundo grupo de boyardos
        for (int i = 0; i < M; i++)
        {
            line = lines[cont++].Trim();
            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                sur.Add(parts[1]); // Solo la etnia; porque es lo que realmente importa, en este problema
            }
        }

        // Computar LCS longitud con DP
        int[,] dp = new int[N + 1, M + 1];
        for (int i = 1; i <= N; i++)
        {
            for (int j = 1; j <= M; j++)
            {
                if (norte[i - 1] == sur[j - 1])
                {
                    dp[i, j] = dp[i - 1, j - 1] + 1;
                }
                else
                {
                    dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
                }
            }
        }

        // Output
        Console.WriteLine(dp[N, M]);
    }
}
