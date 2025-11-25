using System;

// Optimización de latencia en redes
class Program
{
	const int INF = int.MaxValue;
    static void Main()
    {
        int N = int.Parse(Console.ReadLine() ?? "0");
        int[,] dist = new int[N, N];

        for (int i = 0; i < N; i++)
        {
            // para casos, por ejemplo, así:
			// string input = "2    3 4";
			//string[] parts = input.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
			string input = Console.ReadLine() ?? string.Empty;
			string[] line = input.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            for (int j = 0; j < N; j++)
            {
                string val = line[j];
                dist[i, j] = val == "INF" ? INF : int.Parse(val);
            }
        }

        // Floyd-Warshall
        for (int k = 0; k < N; k++)
		{
            for (int i = 0; i < N; i++)
			{
                for (int j = 0; j < N; j++)
				{
                    if ( dist[i, k] != INF && dist[k, j] != INF && (dist[i, k] + dist[k, j] < dist[i, j]) )
					{
                        dist[i, j] = dist[i, k] + dist[k, j];
					}
				}
			}
		}

        // Detectar ciclos negativos
        bool hasNegativeCycle = false;
        for (int i = 0; i < N; i++)
		{
            if (dist[i, i] < 0)
			{
                hasNegativeCycle = true;
				break;
			}
		}
		if (hasNegativeCycle) 
		{
			Console.WriteLine("NEGATIVE CYCLE");
			return;
		}

        // Imprimir resultados
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                if (dist[i, j] == int.MaxValue)
				{
                    Console.Write("INF ");
				}
                else
				{
                    Console.Write(dist[i, j] + " ");
				}
            }
            Console.WriteLine();
        }
    }
}
