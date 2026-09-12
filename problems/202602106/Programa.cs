// Fast Hands - The Origin
// Subsetsum
using System;
using System.Collections.Generic;
using System.Linq;

public class App
{
    public string Name { get; set; }
    public int Size { get; set; }
    public int Inactivity { get; set; }

    //public int Priority => Math.Max(0, 100 - Inactivity); // Valor de mantenerla
    public int Priority => Inactivity; // Valor de mantenerla
}

public class Program
{
    public static void Main()
    {
        // Leer memoria total
        int X = int.Parse(Console.ReadLine().Trim());

        // Leer espacio reservado para videos
        int Y = int.Parse(Console.ReadLine().Trim());

        // Leer cantidad de apps
        int N = int.Parse(Console.ReadLine().Trim());

        // Leer valor minimo de días de inactividad
        int T = int.Parse(Console.ReadLine().Trim());

        List<App> apps = new List<App>();

        // Leer apps
        string line;
        for (int i = 0; i < N && (line = Console.ReadLine()) != null; i++)
        {
            var parts = line.Split(';');
            if (parts.Length == 3 && int.TryParse(parts[2].Trim(), out int inactivity) && inactivity >= T)
            {
                apps.Add(
                    new App
                    {
                        Name = parts[0].Trim(),
                        Size = int.Parse(parts[1].Trim()),
                        Inactivity = inactivity,
                    }
                );
            }
        }

        int capacity = Y; // Espacio que se quiere liberar
        int n = apps.Count;

        // Programación dinámica
        bool[,] dp = new bool[n + 1, capacity + 1];

        for(int i = 0; i <= n; ++i)
        {
            dp[i, 0] = true;
        }

        for (int i = 1; i <= n; i++)
        {
            int w = apps[i - 1].Size;

            for (int cap = 1; cap <= capacity; cap++)
            {
                dp[i, cap] = dp[i - 1, cap]; // No tomar
                if (cap >= w && !dp[i, cap]) // Si se puede tomar y no vino heredado
                {
                    bool newVal = dp[i - 1, cap - w];
                    if (newVal)
                    {
                        dp[i, cap] = newVal;
                    }
                }
            }
        }

        // Reconstruir apps seleccionadas

        List<App> deletedApps = RebuildPath(dp, apps, capacity);

        // Salida
        Console.WriteLine(dp[n, capacity]);
        if (deletedApps.Count == 0)
        {
            Console.WriteLine("Ninguna");
        }
        else
        {
            Console.WriteLine(string.Join(", ", deletedApps.Select(a => a.Name)));
        }
    }

    private static List<App> RebuildPath(bool[,] dp, List<App> apps, int target)
	{
		int n = apps.Count;
		if(!dp[n, target]) return new List<App>();
		
		List<App> path = new List<App>();
		int row = n, col = target;
		while(row > 0 && col > 0)
		{
			if(dp[row, col] && !dp[row - 1, col])
			{
				col = col - apps[row - 1].Size;
				path.Add(apps[row - 1]);
			}
			--row;
		}
		path.Reverse();
		return path;
	}
}
