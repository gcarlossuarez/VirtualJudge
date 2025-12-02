using System;
using System.Collections.Generic;
using System.Linq;

// Festival Cultural de Sabores Bolivianoes
class Program
{
    class Plato
    {
        public string Nombre { get; set; }
        public int Costo { get; set; }
    }

    static void Main()
    {
        // ================================
        // 1) Leer número de platos
        // ================================
        int n = int.Parse(Console.ReadLine());

        var platos = new List<Plato>();

        // ================================
        // 2) Leer los platos "Nombre;Costo"
        // ================================
        for (int i = 0; i < n; i++)
        {
            string linea = Console.ReadLine().Trim();
            var partes = linea.Split(';');

            platos.Add(new Plato
            {
                Nombre = partes[0],
                Costo = int.Parse(partes[1])
            });
        }

        // ================================
        // 3) Leer presupuesto objetivo
        // ================================
        int presupuesto = int.Parse(Console.ReadLine());

        // ================================
        // 4) DP booleana de Subset Sum
        // ================================
        bool[,] dp = new bool[n + 1, presupuesto + 1];

        // Con 0 platos, siempre se puede llegar a 0
        for (int i = 0; i <= n; i++)
            dp[i, 0] = true;

        // Llenar la DP
        for (int i = 1; i <= n; i++)
        {
            int costo = platos[i - 1].Costo;

            for (int j = 0; j <= presupuesto; j++)
            {
                // No tomar el plato
                dp[i, j] = dp[i - 1, j];

                // Tomar el plato
                if (j >= costo && dp[i - 1, j - costo])
				{
					dp[i, j] = true;
                }
            }
        }

        // ================================
        // 5) Verificar si hay solución
        // ================================
        if (!dp[n, presupuesto])
        {
            Console.WriteLine("NO es posible usar exactamente el presupuesto.");
            return;
        }

        Console.WriteLine("Es posible usar exactamente el presupuesto.");
       
    }
}
