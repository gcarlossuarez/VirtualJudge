using System;

// Cambio de Monedas
class Program
{
    static void Main()
    {
        var allText = Console.In.ReadToEnd();
        var allLines = allText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int[] monedas = { 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000 };
        int T = int.Parse(allLines[0]);

        for (int t = 0; t < T; t++)
        {
            int X = int.Parse(allLines[t + 1]);
            int[] dp = new int[X + 1];
            for (int i = 0; i <= X; i++) dp[i] = int.MaxValue;
            dp[0] = 0;

            for (int i = 1; i <= X; i++)
                foreach (int m in monedas)
                    if (m <= i && dp[i - m] != int.MaxValue)
                        dp[i] = Math.Min(dp[i], dp[i - m] + 1);

            Console.WriteLine(dp[X] == int.MaxValue ? "-1" : dp[X].ToString());
        }
    }
}
