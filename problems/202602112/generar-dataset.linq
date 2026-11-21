<Query Kind="Program" />

void Main()
{
	GenSudoku();
}

void GenSudoku()
{
	string currentPath = Path.GetDirectoryName(Util.CurrentQueryPath);
    string inputDirectory = Path.Combine(currentPath, "IN");
    Directory.CreateDirectory(inputDirectory);

    // ===== Caso 1: Ejemplo oficial =====
	string text = @"3
3 3
A B C
A B C
3 3
A B C
A X C
4 3
A B C D
A B C
";
    File.WriteAllText(Path.Combine(inputDirectory, "datos0001.txt"), text);
    Console.WriteLine("datos0001.txt (Ejemplo oficial)");
	
	// ===== Caso 1: Ejemplo oficial =====
	text = @"3

3 3

A B C
A B C

3 3
A B C


A X C
4 3
A B C D
A B C



";
	File.WriteAllText(Path.Combine(inputDirectory, "datos0002.txt"), text);
    Console.WriteLine("datos0002.txt (Ejemplo oficial)");
	
    Random rnd = new Random(444);
    string[] simbolos = { "A", "B", "C", "D", "E", "F", "G", "H", "X", "Y", "Z" };
    for (int d = 3; d <= 12; d++)
    {
        int T = rnd.Next(1, 11);
        var lines = new List<string> { T.ToString() };
        for (int t = 0; t < T; t++)
        {
            int N, M;
            if (d <= 5) { N = rnd.Next(1, 10); M = rnd.Next(1, 10); }
            else if (d <= 9) { N = rnd.Next(100, 500); M = rnd.Next(100, 500); }
            else { N = rnd.Next(1500, 2001); M = rnd.Next(1500, 2001); }

            var A = Enumerable.Range(0, N).Select(_ => simbolos[rnd.Next(simbolos.Length)]).ToArray();
            var B = Enumerable.Range(0, M).Select(_ => simbolos[rnd.Next(simbolos.Length)]).ToArray();

            lines.Add($"{N} {M}");
            lines.Add(string.Join(" ", A));
            lines.Add(string.Join(" ", B));
        }
        string inputFile = Path.Combine(inputDirectory, $"datos{d:0000}.txt");
        File.WriteAllLines(inputFile, lines);
        Console.WriteLine($"{inputFile} (T={T})");
    }
}
