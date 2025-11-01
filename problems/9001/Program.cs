using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

class Program
{
    static void Main()
    {
        string val;
        List<string> lstLines = new List<string>();
        while (!string.IsNullOrEmpty(val = Console.ReadLine()))
		{
            lstLines.Add(val);
        }

        string[] lines = lstLines.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray(); // Eliminar líneas vacías.

        var header = lines[0].Split(';');
        string unitClassifier = header[0].Trim();
        string unitName = header[1].Trim();
        int availableMembers = int.Parse(header[2]);

        var localities = new List<Locality>();
        LoadData(lines, localities);

        //Console.WriteLine($"{string.Join(", ", localities.Select(l => new {l.Name, l.EstimatedWagners, l.MaxNeutralized}))}");

        // Resolviendo el problema de la mochila
        var result = SolveKnapsack(localities, availableMembers, unitClassifier);

        // Crear y escribir en el archivo de salida
            
            
        Console.WriteLine($"{unitName};{availableMembers};{result.TotalNeutralized}");
        foreach (var loc in result.SelectedLocalities.OrderBy(l => l.Name)) // Ordenar alfabéticamente por nombre de localidad
        {
            Console.WriteLine($"{loc.Name};{loc.EstimatedWagners};{loc.MaxNeutralized}");
        }
    }
        
    private static void LoadData(string[] lines, List<Locality> localities)
    {
        // Para que sea 1-indizado, se carga la primera línea con información a no ser tomada en cuenta
        localities.Add(new Locality
        {
            Name = string.Empty,
            EstimatedWagners = 0,
            MaxNeutralized = 0
        });

        for (int i = 1; i <= lines.Length - 1; i++) // Ajustar el índice para que sea 1-indizado
        {
            var data = lines[i].Split(';');
            string localityName = data[0].Trim();
            int estimatedWagners = int.Parse(data[1]);
            int maxNeutralized = int.Parse(data[2]);

            localities.Add(new Locality
            {
                Name = localityName,
                EstimatedWagners = estimatedWagners,
                MaxNeutralized = maxNeutralized
            });
        }
    }

    private static KnapsackResult SolveKnapsack(List<Locality> localities, int membersAvailable, string unitClassifier)
    {
        int n = localities.Count;
        int[,] dp = new int[n + 1, membersAvailable + 1];
        //bool[,] keep = new bool[n + 1, membersAvailable + 1];
        int n1Indized = n - 1; // Para que sea 1-indizado

        int superiorityRatio = unitClassifier == "Rossgvardia" ? 8 : 30;

        for (int i = 1; i <= n1Indized; i++) // Por ser 1 indizado, empieza en "1" y va a hasta "n - 1"
        {
            for (int w = 1; w <= membersAvailable; w++)
            {
                if (localities[i].EstimatedWagners * superiorityRatio <= w)
                {
                    int includeValue = localities[i].MaxNeutralized + dp[i - 1, w - localities[i].EstimatedWagners * superiorityRatio];
                    int excludeValue = dp[i - 1, w];

                    if (includeValue > excludeValue)
                    {
                        dp[i, w] = includeValue;
                        //keep[i, w] = true;
                    }
                    else
                    {
                        dp[i, w] = excludeValue;
                    }
                }
                else
                {
                    dp[i, w] = dp[i - 1, w];
                }
            }
        }

        int remainingMembers = membersAvailable;
        var selectedLocalities = new List<Locality>();
        for (int i = n1Indized; i > 0; i--) // Por ser 1 indizado, empieza en "n - 1"
        {
            if (dp[i, remainingMembers] != dp[i - 1, remainingMembers])
            {
                selectedLocalities.Add(localities[i]);
                remainingMembers -= localities[i].EstimatedWagners * superiorityRatio;
            }
        }

        return new KnapsackResult
        {
            TotalNeutralized = dp[n1Indized, membersAvailable],
            SelectedLocalities = selectedLocalities
        };
    }
}

class Locality
{
    public string Name { get; set; } = string.Empty;
    public int EstimatedWagners { get; set; }
    public int MaxNeutralized { get; set; }
}

class KnapsackResult
{
    public int TotalNeutralized { get; set; }
    public List<Locality> SelectedLocalities { get; set; } = new List<Locality>();
}


