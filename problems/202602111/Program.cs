using System;

// Subcadena Común mas Larga
class Program
{
    static void Main()
    {
        // Lee las dos cadenas directamente desde el teclado
        string allText = Console.In.ReadToEnd();
        string[] allLines = allText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string s1 = allLines[0];
        string s2 = allLines[1];
        //Console.WriteLine($"s1 :{s1}");
        //Console.WriteLine($"s2 :{s2}");
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
        {
            return;
        }

        int m = s1.Length;
        int n = s2.Length;
        int[,] matriz = new int[m + 1, n + 1];

        int longitudMaxima = 0;
        int finPosicion = 0;

        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                if (s1[i - 1] == s2[j - 1])
                {
                    matriz[i, j] = matriz[i - 1, j - 1] + 1;

                    if (matriz[i, j] > longitudMaxima)
                    {
                        longitudMaxima = matriz[i, j];
                        finPosicion = i;
                    }
                }
            }
        }

        // Imprime exclusivamente el texto de la subcadena encontrada
        // string resultado = s1.Substring(finPosicion - longitudMaxima, longitudMaxima);
        //Console.WriteLine(resultado);
        Console.WriteLine(longitudMaxima);
    }
}
