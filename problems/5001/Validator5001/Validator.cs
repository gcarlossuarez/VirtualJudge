using System;
using System.IO;

class Validator
{
    static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("ERROR: argumentos insuficientes (uso: Validator <input> <expected> <actual>)");
            Environment.Exit(1);
        }

        string inputPath = args[0];
        string expectedPath = args[1];
        string actualPath = args[2];

        int n = int.Parse(File.ReadAllText(inputPath).Trim());
        var expected = File.ReadAllLines(expectedPath);
        var output = File.ReadAllLines(actualPath);

        if (expected.Length == 0)
        {
            Console.WriteLine("ERROR: archivo esperado vacío");
            Environment.Exit(1);
        }
        if (output.Length == 0)
        {
            Console.WriteLine("ERROR: salida del estudiante vacía");
            Environment.Exit(1);
        }

        string expectedFirst = expected[0].Trim();
        string actualFirst = output[0].Trim();

        if (expectedFirst != "S" && expectedFirst != "N")
        {
            Console.WriteLine("ERROR: salida esperada inválida");
            Environment.Exit(1);
        }
        if (actualFirst != "S" && actualFirst != "N")
        {
            Console.WriteLine("ERROR: salida del estudiante inválida");
            Environment.Exit(1);
        }

        // 🔹 Comparación directa del primer valor
        if (expectedFirst != actualFirst)
        {
            Console.WriteLine($"ERROR: salida incorrecta, esperado '{expectedFirst}', recibido '{actualFirst}'");
            Environment.Exit(1);
        }

        // 🔹 Caso N: ya está validado
        if (expectedFirst == "N")
        {
            if (output.Length > 1)
            {
                Console.WriteLine("ERROR: salida con N no debe incluir tablero");
                Environment.Exit(1);
            }
            Console.WriteLine("OK");
            return;
        }

        // 🔹 Caso S: validar tablero
        if (output.Length != n + 1)
        {
            Console.WriteLine($"ERROR: tablero debe tener {n} filas, recibido {output.Length - 1}");
            Environment.Exit(1);
        }

        int[,] board = new int[n, n];
        int queens = 0;

        for (int i = 0; i < n; i++)
        {
            string row = output[i + 1].Trim();
            var cells = row.Split('|', StringSplitOptions.RemoveEmptyEntries);

            if (cells.Length != n)
            {
                Console.WriteLine($"ERROR: fila {i} debe tener {n} celdas, recibido {cells.Length}");
                Environment.Exit(1);
            }

            for (int j = 0; j < n; j++)
            {
                if (cells[j] == "Q") board[i, j] = 1;
                else if (cells[j] == "_") board[i, j] = 0;
                else
                {
                    Console.WriteLine($"ERROR: celda inválida en fila {i}, col {j}: {cells[j]}");
                    Environment.Exit(1);
                }
                if (board[i, j] == 1) queens++;
            }
        }

        if (queens != n)
        {
            Console.WriteLine($"ERROR: deben existir {n} reinas, encontrado {queens}");
            Environment.Exit(1);
        }

        // Validar conflictos
        for (int r = 0; r < n; r++)
        {
            for (int c = 0; c < n; c++)
            {
                if (board[r, c] == 1)
                {
                    // fila
                    for (int k = 0; k < n; k++)
                        if (k != c && board[r, k] == 1)
                        {
                            Console.WriteLine($"ERROR: conflicto en fila {r}");
                            Environment.Exit(1);
                        }
                    // columna
                    for (int k = 0; k < n; k++)
                        if (k != r && board[k, c] == 1)
                        {
                            Console.WriteLine($"ERROR: conflicto en columna {c}");
                            Environment.Exit(1);
                        }
                    // diagonales
                    for (int dr = 1; r + dr < n && c + dr < n; dr++)
                        if (board[r + dr, c + dr] == 1)
                        {
                            Console.WriteLine($"ERROR: conflicto diagonal ↘ en ({r},{c})");
                            Environment.Exit(1);
                        }
                    for (int dr = 1; r + dr < n && c - dr >= 0; dr++)
                        if (board[r + dr, c - dr] == 1)
                        {
                            Console.WriteLine($"ERROR: conflicto diagonal ↙ en ({r},{c})");
                            Environment.Exit(1);
                        }
                }
            }
        }

        Console.WriteLine("OK");
    }
}

