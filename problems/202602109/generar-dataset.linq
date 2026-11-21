<Query Kind="Program" />

void Main()
{
    GenDragon();
}

void GenDragon()
{
    string currentPath = Path.GetDirectoryName(Util.CurrentQueryPath);
    string inputDirectory = Path.Combine(currentPath, "IN");
    Directory.CreateDirectory(inputDirectory);

    // ===== Caso 1: Ejemplo oficial con descripciones =====
    string text = @"4 10
Cofre del tesoro 1 6 12
Cofre 2 4 10
Cofre utilizado 3 5 8
Mi cofre utilizado 4 3 5

";
    File.WriteAllText(Path.Combine(inputDirectory, "datos0001.txt"), text);
    Console.WriteLine($"datos0001.txt (Ejemplo oficial)");

    // ===== Vocabulario para generar descripciones aleatorias =====
    string[] palabrasDescripcion = {
        "Cofre", "Tesoro", "Dragón", "Oro", "Plata", "Joyas",
        "Antiguo", "Maldito", "Real", "Sagrado", "Oculto",
        "del", "de", "la", "el", "los", "las",
        "Rey", "Reina", "Guerrero", "Mago", "Ancestral",
        "Perdido", "Brillante", "Oscuro", "Rojo", "Azul",
        "Grande", "Pequeño", "Dorado", "Plateado", "Rúnico"
    };

    Random rnd = new Random(111);

    for (int d = 2; d <= 10; d++)
    {
        int N, C;
        if (d <= 4) { N = rnd.Next(1, 20); C = rnd.Next(1, 100); }
        else if (d <= 7) { N = rnd.Next(200, 800); C = rnd.Next(5000, 50000); }
        else { N = rnd.Next(1500, 2001); C = rnd.Next(500000, 1000001); }

        var lines = new List<string> { $"{N} {C}" };

        for (int i = 0; i < N; i++)
        {
            // Cantidad aleatoria de palabras entre 1 y 5
            int numPalabras = rnd.Next(1, 6);
            var descripcion = new List<string>();
            for (int p = 0; p < numPalabras; p++)
                descripcion.Add(palabrasDescripcion[rnd.Next(palabrasDescripcion.Length)]);

            int peso = rnd.Next(1, 10001);
            int valor = rnd.Next(1, 10001);

            lines.Add($"{string.Join(" ", descripcion)} {peso} {valor}");
        }

        string inputFile = Path.Combine(inputDirectory, $"datos{d:0000}.txt");
        File.WriteAllLines(inputFile, lines);
        Console.WriteLine($"{inputFile} (N={N}, C={C})");
    }
}