using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class Validator
{
    static void Main(string[] args)
    {
        try
        {
            Process(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Environment.Exit(1);
        }
    }

    static void Process(string[] args)
    {
        // 🔹 Validación de argumentos
        if (args.Length < 3)
        {
            Console.WriteLine("ERROR: argumentos insuficientes (uso: Validator <input> <expected> <actual>)");
            Environment.Exit(1);
        }

        string inputPath = args[0];
        string expectedPath = args[1];
        string actualPath = args[2];

        // 🔹 Leer archivos (limpiando líneas vacías y espacios)
        var inputLines = File.ReadAllLines(inputPath)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Trim())
            .ToList();

        var expected = File.ReadAllLines(expectedPath)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Trim())
            .ToList();

        var output = File.ReadAllLines(actualPath)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Trim())
            .ToList();

        // 🔹 Validaciones mínimas
        if (expected.Count == 0)
        {
            Console.WriteLine("ERROR: archivo esperado vacío");
            Environment.Exit(1);
        }
        if (output.Count == 0)
        {
            Console.WriteLine("ERROR: salida del estudiante vacía");
            Environment.Exit(1);
        }

        // 🔹 Validación base: primera línea
        //if (expected[0] != output[0])
        //{
        //    Console.WriteLine($"ERROR: la primera línea no coincide.\nEsperado: {expected[0]}\nObtenido: {output[0]}");
        //    Environment.Exit(1);
        //}

        // 🔹 Validación base: todas las líneas deben ser válidas respecto al input
        //var validLines = new HashSet<string>(inputLines.Skip(1)); // ignora encabezado si aplica
        //for (int i = 1; i < output.Count; i++)
        //{
        //    if (!validLines.Contains(output[i]))
        //    {
        //        Console.WriteLine($"ERROR: la línea '{output[i]}' no existe en el archivo de entrada.");
        //        Environment.Exit(1);
        //    }
        //}

        // ================================================
        // 🔹 Lógica específica del problema
        // Aquí, se deben agregar validaciones adicionales según el contest.
        //
        // Ejemplos:
        //  - Validar límites de capacidad (knapsack).
        //  - Validar formato de un tablero (N reinas).
        //  - Validar unicidad de soluciones.
        // ================================================

        // Si todo está bien
        Console.WriteLine("OK");
        Environment.Exit(0);
    }
}

