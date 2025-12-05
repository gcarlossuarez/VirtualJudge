// File: Validator.cs 
// Uso: dotnet run -- <input> <expected> <actual>
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Validator
{
    static void Main(string[] args)
    {
        try
        {
            if (args.Length < 3)
                Error("Argumentos insuficientes (uso: Validator <input> <expected> <actual>)");

            string inputPath = args[0];
            string expectedPath = args[1];
            string actualPath = args[2];

            if (!File.Exists(inputPath)) Error("No existe el archivo input.");
            if (!File.Exists(expectedPath)) Error("No existe el archivo esperado.");
            if (!File.Exists(actualPath)) Error("No existe el archivo generado por el estudiante.");

            // Leer archivos
            string[] expectedLines = File.ReadAllLines(expectedPath)
                                         .Select(l => l.Trim())
                                         .Where(l => l != "")
                                         .ToArray();

            string[] actualLines = File.ReadAllLines(actualPath)
                                       .Select(l => l.Trim())
                                       .Where(l => l != "")
                                       .ToArray();

            if (actualLines.Length == 0)
                Error("Salida incompleta.");

            // -----------------------------------------------
            // 1) Validar primera línea True/False
            // -----------------------------------------------
            string expectedTF = expectedLines[0];
            string actualTF = actualLines[0];

            if (expectedTF != "True" && expectedTF != "False")
                Error("El archivo esperado tiene formato incorrecto (True/False).");

            if (actualTF != "True" && actualTF != "False")
                Error("La salida del estudiante debe comenzar con True o False.");

            if (actualTF != expectedTF)
                Error("Primera línea (True/False) incorrecta.");

            // -----------------------------------------------
            // Si la solución es FALSE no deben existir más líneas
            // -----------------------------------------------
            if (expectedTF == "False")
            {
                if (actualLines.Length > 1)
                    Error("Si la respuesta es False, no deben existir más líneas.");

                Console.WriteLine("OK");
                return;
            }

            // -----------------------------------------------
            // Leer input
            // -----------------------------------------------
            var inputLines = File.ReadAllLines(inputPath)
                                 .Select(l => l.Trim())
                                 .Where(l => l != "")
                                 .ToArray();

            int target = int.Parse(inputLines[0]);
            int nrTransactions = int.Parse(inputLines[1]);

            // -----------------------------------------------
            // 2) Validar cantidad de transacciones
            // -----------------------------------------------
            int studentCount = actualLines.Length - 1;

            if (studentCount != nrTransactions)
                Error($"Número de transacciones incorrecto. Se esperaban {nrTransactions}, pero se entregaron {studentCount}.");

            // -----------------------------------------------
            // 3) Validar suma
            // -----------------------------------------------
            int sumaAlumno;

            try
            {
                sumaAlumno = actualLines.Skip(1).Select(int.Parse).Sum();
            }
            catch
            {
                Error("El estudiante imprimió una línea que no es número válido.");
                return;
            }

            if (sumaAlumno != target)
                Error($"La suma ({sumaAlumno}) no coincide con la suma objetivo ({target}).");

            // -----------------------------------------------
            // 4) Validar que las transacciones provienen del input
            // -----------------------------------------------
            Dictionary<int, int> dictionary = new Dictionary<int, int>();

            foreach (var l in inputLines.Skip(2))
            {
                int v = int.Parse(l);
                if (!dictionary.ContainsKey(v)) dictionary[v] = 1;
                else dictionary[v]++;
            }

            foreach (var l in actualLines.Skip(1))
            {
                int num = int.Parse(l);

                if (!dictionary.ContainsKey(num) || dictionary[num] == 0)
                    Error($"Elemento {num} no existe en la entrada o fue usado más veces de las disponibles.");

                dictionary[num]--;
            }

            Console.WriteLine("OK");
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.Message);
            Environment.Exit(1);
        }
    }

    static void Error(string msg)
    {
        Console.WriteLine("Resultado incorrecto: " + msg);
        Environment.Exit(1);
    }
}

