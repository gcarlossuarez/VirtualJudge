using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// CostoDeTransferenciaTransaccionesEnElBanco => SubsetSum
class Program
{
    //const int MULTIPLOS = 1; // Caso extremo para generar prueba de stress
    const int MULTIPLOS = 100;

    static void Main()
    {

        List<string> lines = new List<string>();
        string? line;
        // Leer hasta que no haya más líneas (hasta el final del stream)
        while ((line = Console.ReadLine()) != null)
        {
            if(!string.IsNullOrEmpty(line) && !string.IsNullOrWhiteSpace(line))
            {
              lines.Add(line);
            }
        }

        int targetSum = int.Parse(lines[0].Trim()) / MULTIPLOS; // Suma objetivo
        int nroTransactionsRequired = int.Parse(lines[1].Trim()); // Número máximo de transacciones requeridas
        var values = new List<int>();

        for (int i = 2; i < lines.Count(); i++)
        {
            values.Add(int.Parse(lines[i].Trim()) / MULTIPLOS); // Valores de transacciones disponibles
        }

        // Resolver el problema
        //var result = SolveSubsetSum(targetSum, nroTransactionsRequired, values);
        //Console.WriteLine(result.Item1 ? "True" : "False");
        //if (result.Item1)
        //{
        //   foreach (var value in result.Item2)
        //   {
        //      Console.WriteLine(value);
        //   }
        //}
        (bool found, List<int> elements) = FindSubsetWithK(values.ToArray(), targetSum, nroTransactionsRequired);

        // Generar la salida
        Console.WriteLine(found ? "True" : "False");
        if (found)
        {
           foreach (var value in elements)
           {
              Console.WriteLine(value * MULTIPLOS);
           }
        }
    }

    // Método que también retorna los elementos que forman la suma
    public static (bool found, List<int> elements) FindSubsetWithK(int[] nums, int target, int k)
    {
        if (nums == null || nums.Length < k || k <= 0)
            return (false, new List<int>());
        
        int n = nums.Length;
        bool[,,] dp = new bool[n + 1, target + 1, k + 1];
        
        for (int i = 0; i <= n; i++)
            dp[i, 0, 0] = true;
        
        for (int i = 1; i <= n; i++)
        {
            for (int j = 0; j <= target; j++)
            {
                for (int count = 0; count <= k; count++)
                {
                    dp[i, j, count] = dp[i - 1, j, count];
                    
                    if (count > 0 && j >= nums[i - 1])
                    {
                        dp[i, j, count] = dp[i, j, count] || 
                                         dp[i - 1, j - nums[i - 1], count - 1];
                    }
                }
            }
        }
        
        if (!dp[n, target, k])
            return (false, new List<int>());
        
        // Reconstruir la solución
        List<int> result = new List<int>();
        int currentSum = target;
        int currentCount = k;
        
        for (int i = n; i > 0 && currentCount > 0; i--)
        {
            // Si este elemento fue incluido
            if (currentSum >= nums[i - 1] && 
                dp[i - 1, currentSum - nums[i - 1], currentCount - 1])
            {
                result.Add(nums[i - 1]);
                currentSum -= nums[i - 1];
                currentCount--;
            }
        }
        
        return (true, result);
    }


    // Función para resolver el problema del Subset Sum
    static (bool, List<int>) SolveSubsetSum(int targetSum, int nroTransactionsRequired, List<int> values)
    {
      int[] numbers = new int[values.Count() + 1];
      numbers[0] = 0;
      for(int i = 1; i < numbers.Length; ++i)
      {
        numbers[i] = values[i - 1];
      }

      // Matriz dp
      var dp = new bool[targetSum + 1, values.Count() + 1];
      var dpTransactions = new int[targetSum + 1, values.Count() + 1];
      for(int i = 0; i < numbers.Length; ++i)
      {
        dp[0, i] = true;
        dpTransactions[0, i] = 0;
      }


      for(int i = 1; i <= targetSum; ++i)
      {
        dp[i, 0] = false;
        dpTransactions[i, 0] = 0;
      }

      for(int i = 1; i <= targetSum; ++i)
      {
        for(int j = 1; j < numbers.Length; ++j)
        {
          dp[i, j] = dp[i, j - 1];
          dpTransactions[i, j] = dpTransactions[i, j - 1];
          if(i >= numbers[j])
          {
            //$"i:{i}, j:{j}, numbers[j]:{numbers[j]}".Dump();
            if(dp[i - numbers[j], j - 1] && dpTransactions[i - numbers[j], j - 1] < nroTransactionsRequired)
            {
              dp[i, j] = true;
              dpTransactions[i, j] = dpTransactions[i - numbers[j], j - 1] + 1;
            }
          }
        }
      }

      int ii = targetSum, jj = numbers.Length - 1;
      if(dp[ii, jj] && dpTransactions[ii, jj] == nroTransactionsRequired)
      {
        Stack<int> selectedNumbers = new Stack<int>();
        while(ii > 0 && jj > 0)
        {
          if(dp[ii, jj] && !dp[ii, jj - 1])
          {
            selectedNumbers.Push(numbers[jj] * MULTIPLOS);
            ii -= numbers[jj];
          }
          --jj;
        }

        return (dp[ii, jj], selectedNumbers.Reverse().ToList());
      }
      else
      {
        return (false, new List<int>());
      }

    }
}
