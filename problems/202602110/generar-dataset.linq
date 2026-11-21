<Query Kind="Program" />

void Main()
{
    GenVuelto();
}

void GenVuelto()
{
    string currentPath = Path.GetDirectoryName(Util.CurrentQueryPath);
    string inputDirectory = Path.Combine(currentPath, "IN");
    Directory.CreateDirectory(inputDirectory);

    // ===== Caso 1: Ejemplo oficial con descripciones =====
    string text = @"3
11
30
99999

";
    File.WriteAllText(Path.Combine(inputDirectory, "datos0001.txt"), text);
    Console.WriteLine($"datos0001.txt (Ejemplo oficial)");

    text = @"3
11
30
100

";
    File.WriteAllText(Path.Combine(inputDirectory, "datos0002.txt"), text);
    Console.WriteLine($"datos0002.txt (Ejemplo oficial)");
    Random rnd = new Random(222);
    for (int d = 3; d <= 10; d++)
    {
        int T = d <= 5 ? rnd.Next(1, 4) : d <= 7 ? rnd.Next(4, 8) : rnd.Next(8, 11);
        var lines = new List<string> { T.ToString() };
        for (int t = 0; t < T; t++)
        {
            int X = d <= 3 ? rnd.Next(1, 100) : d <= 7 ? rnd.Next(100, 10000) : rnd.Next(10000, 100001);
            lines.Add(X.ToString());
        }
        string inputFile = Path.Combine(inputDirectory, $"datos{d:0000}.txt");
        File.WriteAllLines(inputFile, lines);
        Console.WriteLine($"{inputFile}");
    }
}