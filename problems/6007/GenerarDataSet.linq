<Query Kind="Program" />

/*

*/

void Main()
{
    var etnias = new[] { "VAL", "HUN", "SAJ", "MOL" };
    var random = new Random(42); // Seed fijo para reproducibilidad, cámbialo si quieres variedad extra
    
    var dir = Path.Combine(@"D:\Universidad Católica Boliviana\Semestre2-2025\Banco de ejercicios para práctica en el Juez Virtual\Problem6007\6007", "IN");
    Directory.CreateDirectory(dir); // Crea la carpeta si no existe
    
    for (int num = 4; num <= 50; num++)
    {
        var filename = Path.Combine(dir, $"datos{num:0000}.txt");
        
        // Genera N y M aleatorios (10-200 para variedad)
        int N = random.Next(10, 201);
        int M = random.Next(10, 201);
        
        // Genera lista norte: IDs únicos, etnias random
        var norteIds = new HashSet<int>();
        var norteBuilder = new System.Text.StringBuilder();
        norteBuilder.AppendLine(N.ToString());
        for (int i = 0; i < N; i++)
        {
            int id;
            do { id = random.Next(1, 1000); } while (norteIds.Contains(id)); // Único por lista
            norteIds.Add(id);
            string etnia = etnias[random.Next(etnias.Length)];
            norteBuilder.AppendLine($"{id} {etnia}");
        }
        
        // Genera lista sur: similar
        var surIds = new HashSet<int>();
        var surBuilder = new System.Text.StringBuilder();
        surBuilder.AppendLine(M.ToString());
        for (int i = 0; i < M; i++)
        {
            int id;
            do { id = random.Next(1, 1000); } while (surIds.Contains(id));
            surIds.Add(id);
            string etnia = etnias[random.Next(etnias.Length)];
            surBuilder.AppendLine($"{id} {etnia}");
        }
        
        // Escribe el archivo completo
        var fullContent = norteBuilder.ToString() + surBuilder.ToString();
        File.WriteAllText(filename, fullContent);
        
        // Opcional: Imprime progreso
        Console.WriteLine($"Generado {filename} (N={N}, M={M})");
    }
    
    Console.WriteLine("¡Listo! 47 casos generados en .\\IN\\");
}

// Para probar LCS en uno (opcional, si quieres verificar outputs aquí mismo)
// Función helper para LCS (pega esto si quieres auto-verificar un archivo)
static int LCSLength(List<string> seq1, List<string> seq2)
{
    int n = seq1.Count, m = seq2.Count;
    var dp = new int[n+1, m+1];
    for (int i=1; i<=n; i++)
        for (int j=1; j<=m; j++)
        {
            if (seq1[i-1] == seq2[j-1]) dp[i,j] = dp[i-1,j-1] + 1;
            else dp[i,j] = Math.Max(dp[i-1,j], dp[i,j-1]);
        }
    return dp[n,m];
}