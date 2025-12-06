// El laberinto del político
// BFS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Pos
{
    public int R;
    public int C;

    public Pos(int r, int c)
    {
        R = r;
        C = c;
    }

   
}

class Program
{
    static void Main()
	{
		var lines = new List<string>();
		string? input;
		while (!string.IsNullOrEmpty(input = Console.ReadLine()))
		{
			lines.Add(input);
		}

		var path = BFS(lines.ToArray());

		if (path == null || path.Count() == 0)
			Console.WriteLine("No existe un camino desde I hasta F");
		else
		{
			Console.WriteLine($"Camino más corto encontrado con {path.Count() - 1} pasos.");
			//foreach (var p in path) Console.WriteLine(p);
		}
	}
	
	static List<Pos> BFS(string[] grid)
	{
		int n = grid.Length;
		int m = grid[0].Length;
		
		Pos start = null;
		Pos end = null;

		// Buscar I (start) y F (end)
		for (int i = 0; i < n; i++)
		{
			for (int j = 0; j < m; j++)
			{
				if (grid[i][j] == 'I')
					start = new Pos(i, j);
				else if (grid[i][j] == 'F')
					end = new Pos(i, j);
			}
		}

		// Si no encontró "start" o "end"
		if(start == null || end == null) return null;
		
		var queue = new Queue<Pos>();
		var visited = new bool[n, m];
		var parent = new Pos[n, m];

		queue.Enqueue(start);
		visited[start.R, start.C] = true;

		int[][] dirs = {
			new [] { 1, 0 }, new [] { -1, 0 },
			new [] { 0, 1 }, new [] { 0, -1 }
		};

		while (queue.Count > 0)
		{
			var current = queue.Dequeue();
			int r = current.R;
			int c = current.C;

			if (r == end.R && c == end.C)
			{
				return Reconstruir(parent, start, end);
			}

			foreach (var d in dirs)
			{
				int nr = r + d[0];
				int nc = c + d[1];

				if (nr < 0 || nr >= n || nc < 0 || nc >= m)
					continue;

				if (!visited[nr, nc] && grid[nr][nc] != '#')
				{
					visited[nr, nc] = true;
					parent[nr, nc] = current;
					queue.Enqueue(new Pos(nr, nc));
				}
			}
		}

		return null; // No existe camino
	}

	static List<Pos> Reconstruir(Pos[,] parent, Pos start, Pos end)
	{
		var path = new List<Pos>();
		Pos cur = end;

		while (!cur.Equals(start))
		{
			path.Add(cur);
			cur = parent[cur.R, cur.C];
		}

		path.Add(start);
		path.Reverse();
		return path;
	}

}

