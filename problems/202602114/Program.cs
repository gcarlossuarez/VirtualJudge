using System;
using System.Linq;

// Floyd-Warshall
class Program
{
    const long INF = long.MaxValue / 4;

    static void Main()
    {
        var lines = Console.In
            .ReadToEnd()
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();

        int t = int.Parse(lines[0]);
        int jj = 1;

        while (t-- > 0)
        {
            string[] parts = lines[jj++]
    			.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            int n = int.Parse(parts[0]);
            int m = int.Parse(parts[1]);

            long[,] d = new long[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    d[i, j] = (i == j) ? 0 : INF;
                }
            }

            for (int e = 0; e < m; e++)
            {
                parts = lines[jj++]
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                int a = int.Parse(parts[0]) - 1;
                int b = int.Parse(parts[1]) - 1;
                long w = long.Parse(parts[2]);

                if (w < d[a, b])
                {
                    d[a, b] = w;
                    d[b, a] = w;
                }
            }

            // Floyd-Warshall
            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    long dik = d[i, k];

                    if (dik == INF)
                        continue;

                    for (int j = 0; j < n; j++)
                    {
                        long dkj = d[k, j];

                        if (dkj != INF && dik + dkj < d[i, j])
                        {
                            d[i, j] = dik + dkj;
                        }
                    }
                }
            }

            long ans = 0;
            bool ok = true;

            for (int i = 0; i < n && ok; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (d[i, j] == INF)
                    {
                        ok = false;
                        break;
                    }

                    if (d[i, j] > ans)
                        ans = d[i, j];
                }
            }

            Console.WriteLine(ok ? ans.ToString() : "-1");
        }
    }
}
/*
using System;
using System.IO;
using System.Text;

class Program
{
    const long INF = long.MaxValue / 4;

    static void Main()
    {
        var f = new Fast(Console.OpenStandardInput());
        var output = new StringBuilder();
        int t = f.NextInt();
        while (t-- > 0)
        {
            int n = f.NextInt(),
                m = f.NextInt();
            long[,] d = new long[n, n];
            for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                d[i, j] = i == j ? 0 : INF;
            for (int e = 0; e < m; e++)
            {
                int a = f.NextInt() - 1,
                    b = f.NextInt() - 1;
                long w = f.NextInt();
                if (w < d[a, b])
                    d[a, b] = d[b, a] = w;
            }
            for (int k = 0; k < n; k++)
            for (int i = 0; i < n; i++)
            {
                long dik = d[i, k];
                if (dik == INF)
                    continue;
                for (int j = 0; j < n; j++)
                {
                    long dkj = d[k, j];
                    if (dkj != INF && dik + dkj < d[i, j])
                        d[i, j] = dik + dkj;
                }
            }
            long ans = 0;
            bool ok = true;
            for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                if (d[i, j] == INF)
                    ok = false;
                else if (d[i, j] > ans)
                    ans = d[i, j];
            }
            output.AppendLine(ok ? ans.ToString() : "-1");
        }
        Console.Write(output);
    }

    class Fast
    {
        readonly Stream s;
        readonly byte[] b = new byte[1 << 16];
        int p,
            l;

        public Fast(Stream s)
        {
            this.s = s;
        }

        int Read()
        {
            if (p >= l)
            {
                l = s.Read(b, 0, b.Length);
                p = 0;
                if (l == 0)
                    return -1;
            }
            return b[p++];
        }

        public int NextInt()
        {
            int c;
            do
            {
                c = Read();
            } while (c <= 32 && c >= 0);
            int x = 0;
            while (c > 32)
            {
                x = x * 10 + c - 48;
                c = Read();
            }
            return x;
        }
    }
}
*/
