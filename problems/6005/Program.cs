using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

class MyChar
{
    private int _n;
    private char[] _arrayChar;

    public int Length => _n;

    public MyChar(int n)
    {
        this._n = n;
        this._arrayChar = new char[n];
    }

    public MyChar(int n, char[] arrayChar)
    {
        this._n = n;
        this._arrayChar = new char[n];
        for (int i = 0; i < n; ++i)
        {
            _arrayChar[i] = arrayChar[i];
        }
    }

    public char this[int index]
    {
        get
        {
            if (index <= 0 || index > _arrayChar.Length)
            {
                throw new IndexOutOfRangeException("Índice fuera de los límites del arreglo.");
            }
            return _arrayChar[index - 1];
        }
        set
        {
            if (index <= 0 || index > _arrayChar.Length)
            {
                throw new IndexOutOfRangeException("Índice fuera de los límites del arreglo.");
            }
            _arrayChar[index - 1] = value;
        }
    }
}

class Program
{
    static void Main()
    {
        // Leer los dos archivos de texto
        List<string> lines = new List<string>();

        string line;

        // Leer hasta que no haya más líneas (hasta el final del stream)
        while ((line = Console.ReadLine()) != null)
        {
            lines.Add(line);
        }
        lines = lines
            .Where(l => !string.IsNullOrEmpty(l) && !string.IsNullOrWhiteSpace(l))
            .ToList();
        int cont = 0;
        foreach (var l in lines)
        {
            if (l == "File2")
            {
                break;
            }
            ++cont;
        }
        //Console.WriteLine("Todos los files");
        //Console.WriteLine(string.Join("\n", lines));
        //Console.WriteLine();

        //Console.WriteLine("Solo el File1");
        var file1 = string.Join("\n", lines.Skip(1).Take(cont - 1).ToList());
        //Console.WriteLine(file1);
        var file2 = string.Join("\n", lines.Skip(cont + 1));
        //Console.WriteLine("Solo el File2");
        //Console.WriteLine(file2);


        string nombreEstudiante1 = file1.Split('\n')[0];
        string text1 = string.Join("\n", file1.Split('\n').Skip(1));
    
        string nombreEstudiante2 = file2.Split('\n')[0];

        string text2 = string.Join("\n", file2.Split('\n').Skip(1));
    
        // Convertir los textos en arreglos de caracteres
        MyChar arr1 = new MyChar(text1.Count(), text1.ToCharArray());
        MyChar arr2 = new MyChar(text2.Count(), text2.ToCharArray());

        // Calcular la LCS
        string lcs = LongestCommonSubsequence(arr1, arr2);
        //Console.WriteLine(lcs.Count());

        // Calcular el porcentaje de similitud
        double similarity = CalculateSimilarity(lcs, text1, text2);

        // Mostrar el resultado
        Console.WriteLine("Texto común:");
        Console.WriteLine(lcs);
        Console.WriteLine($"Similitud: {similarity.ToString("P2", CultureInfo.InvariantCulture).Replace(" %", "%")}"); // Remover espacio si existe;

        // Determinar si hay plagio
        const double plagiarismThreshold = 0.7;  // Umbral del 70%
        if (similarity >= plagiarismThreshold)
        {
            Console.WriteLine($"¡Posible plagio detectado! entre {nombreEstudiante1} y {nombreEstudiante2}.");
        }
        else
        {
            Console.WriteLine("No se detectó plagio.");
        }
    }


    static string LongestCommonSubsequence(MyChar X, MyChar Y)
    {
        int m = X.Length;
        int n = Y.Length;

        // Matriz para almacenar las longitudes de las subsecuencias comunes más largas
        int[,] dp = new int[m + 1, n + 1];

        for (int i = 0; i <= m; ++i)
        {
            dp[i, 0] = 0;
        }

        for (int j = 0; j <= n; j++)
        {
            dp[0, j] = 0;
        }

        // Construcción de la matriz LCS en forma de tabla
        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                if (X[i] == Y[j])
                {
                    dp[i, j] = dp[i - 1, j - 1] + 1;
                }
                else
                {
                    dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
                }
            }
        }

        // Recuperar la subsecuencia común más larga desde la tabla
        int index = dp[m, n];
        char[] lcs = new char[index];

        int k = m,
            l = n;
        while (k > 0 && l > 0)
        {
            if (X[k] == Y[l])
            {
                lcs[--index] = X[k];
                k--;
                l--;
            }
            else if (dp[k - 1, l] >= dp[k, l - 1])
            {
                k--;
            }
            else
            {
                l--;
            }
        }

        return new string(lcs);
    }

    static double CalculateSimilarity(string lcs, string text1, string text2)
    {
        // Longitud del LCS
        int lcsLength = lcs.Length;

        // Longitudes de los textos originales
        int length1 = text1.Length;
        int length2 = text2.Length;

        // Calcular el porcentaje de similitud como el promedio del porcentaje de texto compartido
        double similarity1 = (double)lcsLength / length1;
        double similarity2 = (double)lcsLength / length2;

        // Devolver el promedio de ambas similitudes
        return (similarity1 + similarity2) / 2.0;
    }
}
