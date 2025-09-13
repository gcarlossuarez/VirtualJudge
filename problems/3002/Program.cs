using System;
using System.Collections.Generic;
using System.Collections;

class SubsetSumMemo
{

    static void Main()
    {
        string[] line1 = Console.ReadLine().Split();
        int n = int.Parse(line1[0]);
        int target = int.Parse(line1[1]);

        int[] fragments = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);

        var memo = new Dictionary<(int, int), bool>();
        //bool result = SubsetExists(fragments, 0, 0, target, memo);
        bool result = SubsetBitsetExists(fragments, target);

        Console.WriteLine(result ? "YES" : "NO");
    }

    // Muy lento para valores n = 2000 y target n exponente 16
    static bool SubsetExists(int[] arr, int i, int suma, int target, Dictionary<(int, int), bool> memo)
    {
        if (suma == target) return true;
        if (i >= arr.Length || suma > target) return false;

        if (memo.ContainsKey((i, suma)))
            return memo[(i, suma)];

        // Opción 1: incluir el elemento
        bool tomar = SubsetExists(arr, i + 1, suma + arr[i], target, memo);
        // Opción 2: no incluirlo
        bool noTomar = SubsetExists(arr, i + 1, suma, target, memo);

        return memo[(i, suma)] = (tomar || noTomar);
    }

    /*
    static bool SubsetBitsetExists(int[] arr, int target)
    {
        BitArray dp = new BitArray(target + 1);
        dp[0] = true; // la suma 0 siempre es posible (con conjunto vacío)

        foreach (int x in arr)
        {
            // recorrer de atrás hacia adelante
            for (int s = target; s >= x; s--)
            {
                if (dp[s - x]) dp[s] = true;
            }
        }

        return dp[target];
    }
    */
    static bool SubsetBitsetExists(int[] arr, int target)
    {
        // Número de bloques de 64 bits necesarios
        int blocks = (target >> 6) + 1; // target/64 + 1
        ulong[] dp = new ulong[blocks];
        dp[0] = 1UL; // solo suma=0 posible al inicio

        foreach (int x in arr)
        {
            int shiftBlocks = x >> 6;   // desplazamiento de bloques completos
            int shiftBits = x & 63;     // desplazamiento dentro del bloque

            ulong[] next = new ulong[blocks];

            for (int i = 0; i < blocks; i++)
            {
                ulong val = dp[i];

                // copiar el valor original
                next[i] |= val;

                // desplazar a la izquierda por x
                int j = i + shiftBlocks;
                if (j < blocks)
                {
                    next[j] |= val << shiftBits;
                    if (shiftBits > 0 && j + 1 < blocks)
                        next[j + 1] |= val >> (64 - shiftBits);
                }
            }

            dp = next;
        }

        int blockIndex = target >> 6;
        int bitIndex = target & 63;
        return (dp[blockIndex] & (1UL << bitIndex)) != 0;
    }
}


