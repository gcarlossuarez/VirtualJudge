using System;
using System.IO;
using System.Globalization;

// SubsetSum
public class Program
{
    // Entrada: N X, luego N líneas: descripción monto ganancia.
    public static void Main(string[] args)
    {
        string texto = args.Length > 0 ? File.ReadAllText(args[0]) : Console.In.ReadToEnd();
        //string[] datos = texto.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        string[] datos = texto.Split(
                Array.Empty<char>(),
                StringSplitOptions.RemoveEmptyEntries);
        if (datos.Length < 2) throw new ArgumentException("Entrada incompleta.");
        int k = 0;
        int n = int.Parse(datos[k++], CultureInfo.InvariantCulture);
        int objetivo = int.Parse(datos[k++], CultureInfo.InvariantCulture);
        if (n < 1 || n > 60 || objetivo < 0 || objetivo > 3000000 || datos.Length != 2 + 3 * n)
            throw new ArgumentException("Entrada fuera de las restricciones.");
        int[] ganancias = new int[n];
        for (int i = 0; i < n; i++)
        {
            _ = datos[k++];
            int monto = int.Parse(datos[k++], CultureInfo.InvariantCulture);
            ganancias[i] = int.Parse(datos[k++], CultureInfo.InvariantCulture);
            if (monto < 100 || monto > 3600000 || monto % 100 != 0 ||
                ganancias[i] < 100 || ganancias[i] > 600000)
                throw new ArgumentException("Inversión fuera de las restricciones.");
            // El monto se lee, pero no limita la selección de inversiones.
        }
        Console.WriteLine(CarteraExacta.EsPosible(objetivo, ganancias) ? "SI" : "NO");
    }
}

public static class CarteraExacta
{
    public static bool EsPosible(int objetivoBs, int[] gananciasBs)
    {
        if (objetivoBs < 0 || objetivoBs % 100 != 0 || gananciasBs == null)
            throw new System.ArgumentException("Objetivo inválido.");
        int objetivo = objetivoBs / 100;
        int n = gananciasBs.Length;
        bool[,] dp = new bool[n + 1, objetivo + 1];
        // La suma cero siempre puede obtenerse eligiendo ninguna inversión.
        for (int i = 0; i <= n; i++) dp[i, 0] = true;
        for (int i = 1; i <= n; i++)
        {
            if (gananciasBs[i - 1] <= 0 || gananciasBs[i - 1] % 100 != 0)
                throw new System.ArgumentException("Ganancias positivas, múltiplos de 100.");
            int ganancia = gananciasBs[i - 1] / 100;
            for (int s = 1; s <= objetivo; s++)
            {
                dp[i, s] = dp[i - 1, s]; // Excluir la inversión.
                if (ganancia <= s)
                    dp[i, s] = dp[i, s] || dp[i - 1, s - ganancia]; // Incluirla una vez.
            }
        }
        return dp[n, objetivo];
    }
}
