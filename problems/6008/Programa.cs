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

        List<App> apps = new List<App>();

        // Leer apps
        string line;
        for (int i = 0; i < N && (line = Console.ReadLine()) != null; i++)
        {
            var parts = line.Split(';');
            if (parts.Length == 3)
            {
                apps.Add(
                    new App
                    {
                        Name = parts[0].Trim(),
                        Size = int.Parse(parts[1].Trim()),
                        Inactivity = int.Parse(parts[2].Trim()),
                    }
                );
            }
        }

        int capacity = Y; // Espacio que se quiere liberar
        int n = apps.Count;

        // Programación dinámica
        int[,] dp = new int[n + 1, capacity + 1];
        bool[,] taken = new bool[n + 1, capacity + 1];

        for (int i = 1; i <= n; i++)
        {
            int w = apps[i - 1].Size;
            int v = apps[i - 1].Priority;

            for (int cap = 1; cap <= capacity; cap++)
            {
                dp[i, cap] = dp[i - 1, cap]; // No tomar
                if (cap >= w)
                {
                    int newVal = dp[i - 1, cap - w] + v;
                    if (newVal > dp[i, cap])
                    {
                        dp[i, cap] = newVal;
                        taken[i, cap] = true;
                    }
                }
            }
        }

        // Reconstruir apps mantenidas
        List<App> deletedApps = new List<App>();
        int currCap = capacity;
        for (int i = n; i >= 1; i--)
        {
            if (taken[i, currCap])
            {
                deletedApps.Add(apps[i - 1]);
                currCap -= apps[i - 1].Size;
            }
        }
        deletedApps.Reverse();

        // Calcular eliminadas
        int freedSpace = deletedApps.Sum(a => a.Size);
        var keepApps = apps.Where(x => !deletedApps.Exists(y => y.Name == x.Name));

        // Salida
        Console.WriteLine(freedSpace);
        if (deletedApps.Count == 0)
        {
            Console.WriteLine("Ninguna");
        }
        else
        {
            Console.WriteLine(string.Join(", ", deletedApps.Select(a => a.Name)));
        }
    }
}
