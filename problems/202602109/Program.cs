using System;
using System.Linq;

// Knapsack Problem  (Mochila 0/1)
class Program
{
    static void Main()
    {
        var allText = Console.In.ReadToEnd().Trim();
        var allLines = allText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var firstLine = allLines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int N = int.Parse(firstLine[0]);
        int C = int.Parse(firstLine[1]);

        int[] peso = new int[N];
        int[] valor = new int[N];

        for (int i = 0; i < N; i++)
        {
            var line = allLines[i + 1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int lenghtArray = line.Length;
            peso[i] = int.Parse(line[lenghtArray - 2]);
            valor[i] = int.Parse(line[lenghtArray - 1]);
        }

        int[] dp = new int[C + 1];
        for (int i = 0; i < N; i++)
		{
            for (int j = C; j >= peso[i]; j--)
			{
                dp[j] = Math.Max(dp[j], dp[j - peso[i]] + valor[i]);
			}
		}

        int maxValor = 0;
        for (int j = 0; j <= C; j++)
		{
			maxValor = Math.Max(maxValor, dp[j]);
		}

        Console.WriteLine(maxValor);
    }
}

/*******************************************
using System;
using System.Linq;

class Program
{
    static void Main()
    {
        var allText = Console.In.ReadToEnd().Trim();
        var allLines = allText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var firstLine = allLines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int N = int.Parse(firstLine[0]);
        int C = int.Parse(firstLine[1]);

        int[] peso = new int[N];
        int[] valor = new int[N];

        for (int i = 0; i < N; i++)
        {
            var line = allLines[i + 1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int len = line.Length;
            peso[i] = int.Parse(line[len - 2]);
            valor[i] = int.Parse(line[len - 1]);
        }

        // dp[i][j] = mejor valor usando los primeros i ítems con capacidad j
        int[,] dp = new int[N + 1, C + 1];

        for (int i = 1; i <= N; i++)
        {
            for (int j = 0; j <= C; j++)
            {
                // No tomar el ítem i-1
                dp[i, j] = dp[i - 1, j];

                // Tomarlo si cabe
                if (j >= peso[i - 1])
                {
                    dp[i, j] = Math.Max(dp[i, j],
                                        dp[i - 1, j - peso[i - 1]] + valor[i - 1]);
                }
            }
        }

        // Imprimir matriz completa
        //PrintMatrix(dp, peso, valor, C);

        Console.WriteLine(dp[N, C]);
    }

    static void PrintMatrix(int[,] dp, int[] peso, int[] valor, int C)
    {
        int rows = dp.GetLength(0);
        int cols = dp.GetLength(1);

        Console.WriteLine("\nMatriz DP (filas = ítems, columnas = capacidad):\n");

        // Encabezado
        Console.Write("     ");
        for (int j = 0; j <= C; j++)
            Console.Write($"{j,4}");
        Console.WriteLine();

        Console.WriteLine(new string('-', 5 + 4 * (C + 1)));

        for (int i = 0; i < rows; i++)
        {
            string label = (i == 0) ? "0" : $"{peso[i - 1]}/{valor[i - 1]}";
            Console.Write($"{label,4}|");

            for (int j = 0; j < cols; j++)
            {
                Console.Write($"{dp[i, j],4}");
            }
            Console.WriteLine();
        }

        Console.WriteLine();
    }
}

*******************************************/
