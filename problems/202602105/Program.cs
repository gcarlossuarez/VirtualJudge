using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Process();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Environment.Exit(1);
        }
    }

    static void Process()
    {
        List<string> inputLines = new List<string>();
        string? line = string.Empty;
        while ((line = Console.ReadLine()) != null)
        {
            if(string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line)) continue;

            inputLines.Add(line);
        }

        if (inputLines.Count == 0)
        {
            Console.WriteLine("No se proporcioanro atos de entrada");
            return;
        }
        if (!int.TryParse(inputLines[0], out int n))
        {
            Console.WriteLine("El valor de 'N' no es válido");
            return;
        }
        if (!Validar()) Console.WriteLine("Error");
        else Console.WriteLine("OK");
        bool Validar()
        {
            var lines = inputLines.Skip(1).ToArray();   // Se necesita hacer la asiganción, para tenere la referencia
                                                        // al objeto que se va a utilizar en el "foreach"
                                                        //Console.WriteLine($"Lines:{lines}");
            int[,] matrix = new int[n, n];
            int ci = 0;
            foreach (var line in lines)
            {
                string[] parts = line.Split('|', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < n)
                {
                    Console.WriteLine($"ERROR: formato inválido en línea de entrada: '{line}'");
                    return false;
                }

                //Console.WriteLine($"line:{line}")
                int cj = 0;
                foreach (var e in parts)
                {
                    //Console.Write(e.PadLeft(4, ' '));
                    if (false == new List<string> { "Q", "_" }.Contains(e))
                    {
                        return false; //ronment.Exit(1);
                    }
                    matrix[ci, cj] = e == "Q" ? 1 : 0;
                    //Console.Write(matrix[i,j]);
                    ++cj;
                }
                //Console.WriteLine()
                ++ci;
            }
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    if (matrix[i, j] == 1)
                    {
                        // Misma columna
                        for (int i1 = 0; i1 < i; ++i1)
                        {
                            if (matrix[i1, j] == 1)
                            {
                                return false;
                            }
                        }

                        // Misma fila
                        for (int j1 = 0; j1 < j; ++j1)
                        {
                            if (matrix[i, j1] == 1)
                            {
                                return false;
                            }
                        }

                        // Misma diagonal superior izquierda
                        for (int i1 = i - 1, j1 = j - 1; i1 > 0 && j1 > 0; --i1, --j1)
                        {
                            if (matrix[i1, j1] == 1)
                            {
                                return false;
                            }
                        }
                        // Misma diagonal superior derecha
                        for (int i1 = i - 1, j1 = j + 1; i1 > 0 && j1 < n; --i1, ++j1)
                        {
                            if (matrix[i1, j1] == 1)
                            {
                                return false;
                            }
                        }

                        // Misma diagonal inferior derecha
                        for (int i1 = i + 1, j1 = j + 1; i1 < n && j1 < n; ++i1, ++j1)
                        {
                            if (matrix[i1, j1] == 1)
                            {
                                return false;
                            }
                        }

                        // Misma diagonal inferior izquierda
                        for (int i1 = i + 1, j1 = j - 1; i1 < n && j1 > 0; ++i1, --j1)
                        {
                            if (matrix[i1, j1] == 1)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            int queensCounter = 0;
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    if (matrix[i, j] == 1) queensCounter++;
                }
            }
            if (n != queensCounter)
            {
                return false;
            }
            return true;
        }

    }
}
