<Query Kind="Program" />

// Abrir en LINQPad, guardar la consulta y pulsar F5.
// Cada nueva ejecución sobrescribe los mismos archivos.
void Main()
{
    if (string.IsNullOrEmpty(Util.CurrentQueryPath))
        throw new System.Exception("Primero debe guardarse la consulta .linq.");
    string raiz = System.IO.Path.GetDirectoryName(Util.CurrentQueryPath);
    string carpetaIn = System.IO.Path.Combine(raiz, "IN");
    string carpetaOut = System.IO.Path.Combine(raiz, "OUT");
    System.IO.Directory.CreateDirectory(carpetaIn);
    System.IO.Directory.CreateDirectory(carpetaOut);
    var rng = new System.Random(20261003);
    var utf8 = new System.Text.UTF8Encoding(false);
    var resumen = new System.Text.StringBuilder("Caso;N;ObjetivoBs;Respuesta;Tipo\n");
    for (int caso = 1; caso <= 100; caso++)
    {
        int x;
        int[] m, g;
        bool esperado;
        string tipo;
        switch (caso)
        {
            case 1:
                x = 1600;
                m = new[] {600, 500, 500}; g = new[] {900, 600, 700};
                esperado = true; tipo = "Ejemplo del Word"; break;
            case 2:
                x = 1100;
                m = new[] {600, 500, 500}; g = new[] {900, 600, 700};
                esperado = false; tipo = "Suma exacta imposible"; break;
            case 3:
                x = 1200;
                m = new[] {600, 500, 500}; g = new[] {900, 600, 700};
                esperado = false; tipo = "No existe la suma exacta"; break;
            case 4:
                x = 0;
                m = new[] {200, 300}; g = new[] {100, 200};
                esperado = true; tipo = "Subconjunto vacío"; break;
            case 5:
                x = 600;
                m = new[] {100}; g = new[] {300};
                esperado = false; tipo = "No se puede repetir una inversión"; break;
            case 6:
                x = 500;
                m = new[] {600, 500}; g = new[] {500, 500};
                esperado = true; tipo = "Dos inversiones con la misma ganancia"; break;
            case 7:
                x = 400;
                m = new[] {600}; g = new[] {500};
                esperado = false; tipo = "Única ganancia diferente de X"; break;
            case 8:
                x = 600;
                m = new[] {200, 300, 500}; g = new[] {100, 200, 300};
                esperado = true; tipo = "Se necesitan todas las inversiones"; break;
            default:
                int n = rng.Next(20, 61);
                m = new int[n]; g = new int[n];
                esperado = caso % 2 == 0;
                for (int i = 0; i < n; i++)
                {
                    m[i] = rng.Next(1, 36001) * 100;
                    g[i] = esperado ? rng.Next(1, 6001) * 100 : rng.Next(1, 3001) * 200;
                }
                if (esperado)
                {
                    // Testigo garantizado: se suman las ganancias de las primeras tres inversiones.
                    x = g[0] + g[1] + g[2];
                    tipo = "SI con subconjunto testigo";
                }
                else
                {
                    // Toda suma es múltiplo de 200; X es múltiplo de 100, pero no de 200.
                    x = (2 * rng.Next(5000, 15000) + 1) * 100;
                    tipo = "NO por suma inalcanzable";
                }
                break;
        }
        bool respuesta = CarteraExacta.EsPosible(x, g);
        bool control = Control1D(x, g);
        if (respuesta != control || respuesta != esperado)
            throw new System.Exception("Falló la validación del caso " + caso);
        var entrada = new System.Text.StringBuilder();
        entrada.AppendLine(m.Length + " " + x);
        for (int i = 0; i < m.Length; i++)
            entrada.AppendLine("Inv" + (i + 1) + " " + m[i] + " " + g[i]);
        string id = caso.ToString("D4");
        string salida = respuesta ? "SI" : "NO";
        System.IO.File.WriteAllText(System.IO.Path.Combine(carpetaIn, "datos" + id + ".txt"), entrada.ToString(), utf8);
        System.IO.File.WriteAllText(System.IO.Path.Combine(carpetaOut, "Output_datos" + id + ".txt"), salida + System.Environment.NewLine, utf8);
        resumen.AppendLine(id + ";" + m.Length + ";" + x + ";" + salida + ";" + tipo);
    }
    System.IO.File.WriteAllText(System.IO.Path.Combine(raiz, "ResumenSubsetSumSimpleDocente.csv"), resumen.ToString(), utf8);
    carpetaIn.Dump("100 datasets");
    carpetaOut.Dump("100 respuestas: 50 SI y 50 NO");
}

bool Control1D(int x, int[] g)
{
    int objetivo = x / 100;
    bool[] dp = new bool[objetivo + 1];
    dp[0] = true;
    for (int i = 0; i < g.Length; i++)
        for (int s = objetivo; s >= g[i] / 100; s--)
            dp[s] = dp[s] || dp[s - g[i] / 100];
    return dp[objetivo];
}

public static class CarteraExacta
{
    public static bool EsPosible(int objetivoBs, int[] gananciasBs)
    {
        if (objetivoBs < 0 || objetivoBs % 100 != 0 || gananciasBs == null)
            throw new System.ArgumentException("Objetivo inválido.");
        int objetivo = objetivoBs / 100;
        int n = gananciasBs.Length;
        bool[,] dp = new bool[n + 1, objetivo + 1];
        // La suma cero siempre puede obtenerse eligiendo ninguna inversión.
        for (int i = 0; i <= n; i++) dp[i, 0] = true;
        for (int i = 1; i <= n; i++)
        {
            if (gananciasBs[i - 1] <= 0 || gananciasBs[i - 1] % 100 != 0)
                throw new System.ArgumentException("Ganancias positivas, múltiplos de 100.");
            int ganancia = gananciasBs[i - 1] / 100;
            for (int s = 1; s <= objetivo; s++)
            {
                dp[i, s] = dp[i - 1, s]; // Excluir la inversión.
                if (ganancia <= s)
                    dp[i, s] = dp[i, s] || dp[i - 1, s - ganancia]; // Incluirla una vez.
            }
        }
        return dp[n, objetivo];
    }
}
