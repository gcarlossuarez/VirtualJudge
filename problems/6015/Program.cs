using System;
using System.Collections.Generic;

// Campo minado
class Program
{
    
    static void Main()
    {
		string? l = Console.ReadLine();
		if(string.IsNullOrEmpty(l)) throw new Exception("Entrada inválida");
        string[] input = l.Split();
		
        Solver.n = int.Parse(input[0]);
        Solver.m = int.Parse(input[1]);
        Solver.k = int.Parse(input[2]);

        Solver.grid = new string[Solver.n][];
        Solver.visited = new bool[Solver.n][];
        for (int i = 0; i < Solver.n; i++)
        {
			string line = Console.ReadLine() ?? string.Empty;
			if(string.IsNullOrEmpty(line)) continue;
			
			// StringSplitOptions.RemoveEmptyEntries para remover las entradas vacías
			// Ejemplo:
			// string texto = "Hola   mundo  con muchos espacios";
			// Usa StringSplitOptions.RemoveEmptyEntries para eliminar las entradas vacías
			// string[] palabras = texto.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			// El arreglo 'palabras' contendrá: ["Hola", "mundo", "con", "muchos", "espacios"]
			
            var aux = line.Trim().Split( ' ' , StringSplitOptions.RemoveEmptyEntries);
			Solver.grid[i] = new string[Solver.m];
			int j = 0;
			foreach(var p in aux)
			{
				Solver.grid[i][j] = aux[j];
				++j;
			}
			
            Solver.visited[i] = new bool[Solver.m];
        }

        Console.WriteLine(Solver.Solve());
    }
}


class Solver
{
    public static int n, m, k;
    public static string[][] grid;
    public static bool[][] visited;

    static bool IsPathClear(int r1, int c1, int r2, int c2)
	{
		if (r1 == r2) // horizontal
		{
			int step = c2 > c1 ? 1 : -1;
			for (int col = c1 + step; col != c2; col += step)
			{
				if (grid[r1][col] == "X")
				{
					return false;
				}
			}
		}
		else // vertical
		{
			int step = r2 > r1 ? 1 : -1;
			for (int row = r1 + step; row != r2; row += step)
			{
				if (grid[row][c1] == "X")
				{
					return false;
				}
			}
		}

		return true;
	}


    public static int Solve()
    {
        var queue = new Queue<(int r, int c, int moves)>();
        queue.Enqueue((0, 0, 0));
        visited[0][0] = true;

        while (queue.Count > 0)
        {
            var (r, c, moves) = queue.Dequeue();

            if (r == n - 1 && c == m - 1)
                return moves;

            (int dr, int dc)[] directions = { (k, 0), (-k, 0), (0, k), (0, -k) };
            foreach (var (dr, dc) in directions)
            {
                int nr = r + dr;
                int nc = c + dc;

                if (nr >= 0 && nr < n && nc >= 0 && nc < m && !visited[nr][nc])
                {
                    if (IsPathClear(r, c, nr, nc) && grid[nr][nc] != "X")
                    {
                        visited[nr][nc] = true;
                        queue.Enqueue((nr, nc, moves + 1));
                    }
                }
            }
        }
        return -1;
    }
}