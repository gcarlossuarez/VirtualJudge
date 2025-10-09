using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        // Leer toda la entrada
        List<string> allLines = new List<string>();
        string line;
        while ((line = Console.ReadLine()) != null)
        {
            allLines.Add(line);
        }

        // Separar en base al delimitador ====
        int sepIndex = allLines.IndexOf("====");
        if (sepIndex == -1)
        {
            Console.WriteLine("ERROR: no se encontró el separador '===='");
            return;
        }

        var original = allLines.Take(sepIndex).ToList();
        var modified = allLines.Skip(sepIndex + 1).ToList();

        // Convertir a diccionarios indexados por línea
        Dictionary<int, string> hashDictionary1 = new Dictionary<int, string>();
        Dictionary<int, string> hashDictionary2 = new Dictionary<int, string>();

        for (int i = 0; i < original.Count; i++)
            hashDictionary1[i] = original[i];

        for (int i = 0; i < modified.Count; i++)
            hashDictionary2[i] = modified[i];

        // Calcular distancia de Levenshtein con detalle
        (int distance, List<string> operations) = LevenshteinDistance(
            hashDictionary1,
            hashDictionary2
        );

        // Mostrar salida en consola
        Console.WriteLine($"Cantidad de operaciones:{distance}");
        Console.WriteLine("OPERACIONES");
        foreach (var op in operations)
        {
            Console.WriteLine(op);
        }
    }

    static (int, List<string>) LevenshteinDistance(
        Dictionary<int, string> s1,
        Dictionary<int, string> s2
    )
    {
        int len1 = s1.Count;
        int len2 = s2.Count;
        int[,] dp = new int[len1 + 1, len2 + 1];

        for (int j = 0; j <= len2; ++j)
            dp[0, j] = j;
        for (int i = 0; i <= len1; ++i)
            dp[i, 0] = i;

        foreach (var val1 in s1)
        {
            int i = val1.Key + 1;
            foreach (var val2 in s2)
            {
                int j = val2.Key + 1;
                if (val1.Value == val2.Value)
                {
                    dp[i, j] = dp[i - 1, j - 1];
                }
                else
                {
                    dp[i, j] = 1 + Math.Min(dp[i - 1, j], Math.Min(dp[i, j - 1], dp[i - 1, j - 1]));
                }
            }
        }

        // Recuperar operaciones
        Stack<string> operations = new Stack<string>();
        int xIndex = len1,
            yIndex = len2;

        while (xIndex > 0 || yIndex > 0)
        {
            if (xIndex > 0 && yIndex > 0 && s1[xIndex - 1] == s2[yIndex - 1])
            {
                xIndex--;
                yIndex--;
            }
            else if (
                xIndex > 0
                && (yIndex == 0 || dp[xIndex, yIndex] == dp[xIndex - 1, yIndex] + 1)
            )
            {
                operations.Push($"+ Línea:{xIndex}, valor:'{s1[xIndex - 1]}'");
                xIndex--;
            }
            else if (
                yIndex > 0
                && (xIndex == 0 || dp[xIndex, yIndex] == dp[xIndex, yIndex - 1] + 1)
            )
            {
                operations.Push($"- Línea:{yIndex}, valor:'{s2[yIndex - 1]}'");
                yIndex--;
            }
            else
            {
                operations.Push(
                    $"Cambiado Línea:{yIndex}, valor '{s2[yIndex - 1]}' por => Línea:{xIndex}, valor:'{s1[xIndex - 1]}'"
                );
                xIndex--;
                yIndex--;
            }
        }

        return (dp[len1, len2], operations.ToList());
    }
}
