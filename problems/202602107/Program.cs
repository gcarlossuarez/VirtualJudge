using System;
using System.IO;
using System.Globalization;

// Knapsack Problem
public class Program
{
    // Uso: programa entrada001.txt
    // También acepta entrada estándar si no se indica un archivo.
    public static void Main(string[] args)
    {
        // Lee toda la entrada de la consola en una sola instrucción y lo carga en un solo
        // texto gigante
        string texto = args.Length > 0 ? File.ReadAllText(args[0]) : Console.In.ReadToEnd();
        // Carga toda la entrada, en un solo vector. Es resonsabilidad del programa, sincronizarse
        // al leer los datos
        string[] datos = texto.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        int k = 0;
        int n = int.Parse(datos[k++], CultureInfo.InvariantCulture);
        int presupuesto = int.Parse(datos[k++], CultureInfo.InvariantCulture);
        if (n < 1 || n > 60 || presupuesto < 100 || presupuesto > 3000000 || datos.Length != 2 + 3 * n)
            throw new ArgumentException("Entrada fuera de las restricciones.");
        int[] montos = new int[n];
        long[] ganancias = new long[n];
        for (int i = 0; i < n; i++)
        {
            _ = datos[k++]; // No interesa la descripción de la inversión
            montos[i] = int.Parse(datos[k++], CultureInfo.InvariantCulture);
            ganancias[i] = long.Parse(datos[k++], CultureInfo.InvariantCulture);
        }
        Console.WriteLine(Cartera.GananciaMaxima(presupuesto, montos, ganancias));
    }
}

public static class Cartera
{
    public static long GananciaMaxima(int presupuestoBs, int[] montosBs, long[] gananciasBs)
    {
        if (montosBs == null || gananciasBs == null || montosBs.Length != gananciasBs.Length)
            throw new System.ArgumentException("Arreglos inválidos.");
        if (presupuestoBs < 0 || presupuestoBs % 100 != 0)
            throw new System.ArgumentException("El presupuesto debe ser múltiplo de 100.");

        int n = montosBs.Length;
        int capacidad = presupuestoBs / 100;
        long[,] dp = new long[n + 1, capacidad + 1];

        // dp[i, c]: máxima ganancia en Bs usando las primeras i inversiones
        // y un presupuesto de hasta c dólares ficticios.
        for (int i = 1; i <= n; i++)
        {
            if (montosBs[i - 1] <= 0 || montosBs[i - 1] % 100 != 0 || gananciasBs[i - 1] < 0)
                throw new System.ArgumentException("Inversión inválida.");

            int costo = montosBs[i - 1] / 100;
            long ganancia = gananciasBs[i - 1]; // Se mantiene en bolivianos.
            for (int c = 0; c <= capacidad; c++)
            {
                long sinElegir = dp[i - 1, c];
                dp[i, c] = sinElegir;
                if (costo <= c)
                {
                    // La fila anterior evita elegir la misma inversión dos veces.
                    long eligiendo = ganancia + dp[i - 1, c - costo];
                    dp[i, c] = System.Math.Max(sinElegir, eligiendo);
                }
            }
        }
        return dp[n, capacidad];
    }
}
