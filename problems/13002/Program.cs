// La dosis correcta de Nick
// Problema de la Mochila 0/1

using System;

class Program
{
    static void Main()
    {
        // Lectura de N (número de ítems) y L (capacidad de la mochila)
        var header = Console.ReadLine().Split();
        int N = int.Parse(header[0]);
        int L = int.Parse(header[1]);

        int[] cost = new int[N];
        int[] gain = new int[N];

        for (int i = 0; i < N; i++)
        {
            var p = Console.ReadLine().Split();
            cost[i] = int.Parse(p[0]);
            gain[i] = int.Parse(p[1]);
        }

        // dp[0, ...] y dp[1, ...] almacenarán los valores de la fila actual y anterior
        int[,] dp = new int[2, L + 1];

        int lastCurrent = 0;
        // El bucle externo itera sobre cada ítem (i de 0 a N-1)
        for (int i = 1; i <= N; i++)
        {
            // Determina qué fila es la actual y cuál es la anterior
            // Usamos (i % 2) para 'current' y ((i + 1) % 2) para 'previous'
            int current = i % 2 == 1 ? 1 : 0;
            int previous = i % 2 == 1 ? 0 : 1; // El índice opuesto

            lastCurrent = current;

            // El bucle interno itera sobre los pesos desde 0 hasta L
            for (int w = 0; w <= L; w++)
            {
                // Inicialmente, el valor máximo para el peso 'w' sin considerar el ítem actual
                // es simplemente el valor máximo que se podía obtener con los ítems anteriores
                dp[current, w] = dp[previous, w];

                // Si el peso actual 'w' es suficiente para llevar el ítem actual (cost[i])
                if (w >= cost[i - 1])
                {
                    // Comprueba si incluir el ítem actual (dp[previous, w - cost[i]] + gain[i])
                    // da un valor mayor que no incluirlo (dp[previous, w])
                    dp[current, w] = Math.Max(
                        dp[current, w], // Valor sin incluir el ítem (copiado de la fila anterior)
                        dp[previous, w - cost[i - 1]] + gain[i - 1] // Valor si se incluye el ítem
                    );
                }
            }
        }

        // El resultado final está en la última fila utilizada.
        // Después del bucle, la última fila usada fue (N-1) % 2.
        Console.WriteLine(dp[lastCurrent , L]);
    }
}


/*****************************************************************************
using System;

class Program
{
    static void Main()
    {
        // Lectura de N (número de ítems) y L (capacidad de la mochila)
        var header = Console.ReadLine().Split();
        int N = int.Parse(header[0]);
        int L = int.Parse(header[1]);

        int[] cost = new int[N];
        int[] gain = new int[N];

        for (int i = 0; i < N; i++)
        {
            var p = Console.ReadLine().Split();
            cost[i] = int.Parse(p[0]);
            gain[i] = int.Parse(p[1]);
        }

        // dp[0, ...] y dp[1, ...] almacenarán los valores de la fila actual y anterior
        int[,] dp = new int[2, L + 1];

        // El bucle externo itera sobre cada ítem (i de 0 a N-1)
        for (int i = 0; i < N; i++)
        {
            // Determina qué fila es la actual y cuál es la anterior
            // Usamos (i % 2) para 'current' y ((i + 1) % 2) para 'previous'
            int current = i % 2;
            int previous = (i + 1) % 2; // El índice opuesto

            // El bucle interno itera sobre los pesos desde 0 hasta L
            for (int w = 0; w <= L; w++)
            {
                // Inicialmente, el valor máximo para el peso 'w' sin considerar el ítem actual
                // es simplemente el valor máximo que se podía obtener con los ítems anteriores
                dp[current, w] = dp[previous, w];

                // Si el peso actual 'w' es suficiente para llevar el ítem actual (cost[i])
                if (w >= cost[i])
                {
                    // Comprueba si incluir el ítem actual (dp[previous, w - cost[i]] + gain[i])
                    // da un valor mayor que no incluirlo (dp[previous, w])
                    dp[current, w] = Math.Max(
                        dp[current, w], // Valor sin incluir el ítem (copiado de la fila anterior)
                        dp[previous, w - cost[i]] + gain[i] // Valor si se incluye el ítem
                    );
                }
            }
        }

        // El resultado final está en la última fila utilizada.
        // Después del bucle, la última fila usada fue (N-1) % 2.
        Console.WriteLine(dp[(N - 1) % 2, L]);
    }
}
*****************************************************************************/


/*****************************************************************************
using System;
using System.IO;

class Program
{
    static void Main()
    {
        var header = Console.ReadLine().Split();
        int N = int.Parse(header[0]);
        int L = int.Parse(header[1]);

        int[] cost = new int[N];
        int[] gain = new int[N];

        for (int i = 0; i < N; i++)
        {
            var p = Console.ReadLine().Split();
            cost[i] = int.Parse(p[0]);
            gain[i] = int.Parse(p[1]);
        }

        int[,] dp = new int[2, L + 1];
        int current = 0, previous = 1;

        for (int i = 0; i < N; i++)
        {
            current = i % 2;
            previous = 1 - current;

            // copiar fila anterior
            for (int w = 0; w <= L; w++)
                dp[current, w] = dp[previous, w];

            for (int w = L; w >= cost[i]; w--)
            {
                dp[current, w] = Math.Max(
                    dp[current, w],
                    dp[previous, w - cost[i]] + gain[i]
                );
            }
        }

        // La ultima fila es N - 1, al sacar el residuo de dividr entre 2,
        // se obtiene el verdadero indice del último
        Console.WriteLine(dp[(N - 1) % 2, L]);
    }
}
*****************************************************************************/


/*****************************************************************************
using System;
using System.IO;

class Program
{
    static void Main()
    {
        var header = Console.ReadLine().Split();
        int N = int.Parse(header[0]);
        int L = int.Parse(header[1]);

        int[] cost = new int[N];
        int[] gain = new int[N];

        for (int i = 0; i < N; i++)
        {
            var p = Console.ReadLine().Split();
            cost[i] = int.Parse(p[0]);
            gain[i] = int.Parse(p[1]);
        }

        int[] dp = new int[L + 1];

        for (int i = 0; i < N; i++)
        {
            int c = cost[i];
            int g = gain[i];

            for (int w = L; w >= c; w--)
            {
                dp[w] = Math.Max(dp[w], dp[w - c] + g);
            }
        }

        Console.WriteLine(dp[L]);
    }
}
*****************************************************************************/


/*****************************************************************************
// NOTA. - Esta es la versiíon clásica. Se come la memora; solamente, para pruebas
using System;
using System.IO;

class Program
{
    static void Main()
    {
        var header = Console.ReadLine().Split();
        int N = int.Parse(header[0]);
        int L = int.Parse(header[1]);

        int[] cost = new int[N];
        int[] gain = new int[N];

        for (int i = 0; i < N; i++)
        {
            var p = Console.ReadLine().Split();
            cost[i] = int.Parse(p[0]);
            gain[i] = int.Parse(p[1]);
        }

        // MATRIZ COMPLETA dp[N+1][L+1]
        int[,] dp = new int[N + 1, L + 1];

        for (int i = 1; i <= N; i++)
        {
            for (int w = 0; w <= L; w++)
            {
                // NO TOMAR LA AMPOLLA i
                dp[i, w] = dp[i - 1, w];

                // TOMAR LA AMPOLLA i (si entra)
                if (w >= cost[i - 1])
                {
                    dp[i, w] = Math.Max(
                        dp[i, w],
                        dp[i - 1, w - cost[i - 1]] + gain[i - 1]
                    );
                }
            }
        }

        Console.WriteLine(dp[N, L]);
    }
}

*****************************************************************************/
