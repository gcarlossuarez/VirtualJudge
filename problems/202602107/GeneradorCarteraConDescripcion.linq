<Query Kind="Program" />

// LINQPad: abrir este archivo y ejecutar (F5).
// Formato: N B, luego N líneas D M G (descripción sin espacios).
// Crea datos0001.txt...datos0100.txt y respuesta0001.txt...respuesta0100.txt.
// El caso 0001 coincide con el Word: respuesta 1600.
// Ejecutar otra vez sobrescribe los archivos del mismo nombre.
// No requiere paquetes NuGet. Cambiar inputDir si se desea.
void Main()
{
    string inputDir = System.IO.Path.Combine(
	    System.IO.Path.GetDirectoryName(Util.CurrentQueryPath),
	    "IN");
	System.IO.Directory.CreateDirectory(inputDir);
		
	string outputDir = System.IO.Path.Combine(
	    System.IO.Path.GetDirectoryName(Util.CurrentQueryPath),
	    "OUT");
	System.IO.Directory.CreateDirectory(outputDir);
		
    var random = new System.Random(20261003);
    var utf8 = new System.Text.UTF8Encoding(false);
    var resumen = new System.Text.StringBuilder("Caso;N;PresupuestoBs;GananciaMaximaBs;Tipo\n");

    for (int caso = 1; caso <= 100; caso++)
    {
        int presupuesto;
        int[] montos;
        long[] ganancias;
        string tipo;
        if (caso == 1)
        {
            presupuesto = 1200;
            montos = new[] { 600, 500, 500 };
            ganancias = new long[] { 900, 600, 700 };
            tipo = "Ejemplo del enunciado";
        }
        else if (caso == 2)
        {
            presupuesto = 1000;
            montos = new[] { 1100, 1500, 2000 };
            ganancias = new long[] { 100, 200, 300 };
            tipo = "Ninguna inversión cabe";
        }
        else if (caso == 3)
        {
            presupuesto = 1000;
            montos = new[] { 200, 300, 500 };
            ganancias = new long[] { 41, 72, 133 };
            tipo = "Todas caben exactamente";
        }
        else if (caso == 4)
        {
            presupuesto = 1000;
            montos = new[] { 400, 700 };
            ganancias = new long[] { 100, 150 };
            tipo = "Detecta mochila ilimitada; sobra presupuesto";
        }
        else if (caso == 5)
        {
            presupuesto = 1000;
            montos = new[] { 500, 500, 1000 };
            ganancias = new long[] { 70, 70, 140 };
            tipo = "Varias carteras óptimas";
        }
        else
        {
            int n = random.Next(20, 61);
            int capacidad = random.Next(10000, 30001);
            presupuesto = capacidad * 100;
            montos = new int[n];
            ganancias = new long[n];
            for (int i = 0; i < n; i++)
            {
                // Algunas alternativas pueden exceder el presupuesto.
                int costo = random.Next(100, capacidad + capacidad / 5 + 1);
                montos[i] = costo * 100;
                ganancias[i] = random.Next(1, 600001); // No necesitan ser múltiplos de 100.
            }
            tipo = "Aleatorio";
        }

        long respuesta = Cartera.GananciaMaxima(presupuesto, montos, ganancias);
        long control = Ganancia1D(presupuesto, montos, ganancias);
        if (respuesta != control) throw new System.Exception("Falló el control del caso " + caso);
        var entrada = new System.Text.StringBuilder();
        entrada.AppendLine(montos.Length + " " + presupuesto);
        for (int i = 0; i < montos.Length; i++)
            entrada.AppendLine("Inv" + (i + 1) + " " + montos[i] + " " + ganancias[i]);
        string numero = caso.ToString("D4");
        System.IO.File.WriteAllText(System.IO.Path.Combine(inputDir, "datos" + numero + ".txt"), entrada.ToString(), utf8);
        System.IO.File.WriteAllText(System.IO.Path.Combine(outputDir, "Output_datos" + numero + ".txt"), respuesta + System.Environment.NewLine, utf8);
        resumen.AppendLine(numero + ";" + montos.Length + ";" + presupuesto + ";" + respuesta + ";" + tipo);
    }
    System.IO.File.WriteAllText(System.IO.Path.Combine(inputDir, "resumen_docente.csv"), resumen.ToString(), utf8);
    inputDir.Dump("100 entradas y 100 respuestas generadas");
}

// Control con otra implementación: capacidad recorrida de mayor a menor.
long Ganancia1D(int presupuestoBs, int[] montosBs, long[] gananciasBs)
{
    int capacidad = presupuestoBs / 100;
    long[] dp = new long[capacidad + 1];
    for (int i = 0; i < montosBs.Length; i++)
        for (int c = capacidad; c >= montosBs[i] / 100; c--)
            dp[c] = System.Math.Max(dp[c], dp[c - montosBs[i] / 100] + gananciasBs[i]);
    return dp[capacidad];
}

public static class Cartera
{
    public static long GananciaMaxima(int presupuestoBs, int[] montosBs, long[] gananciasBs)
    {
        if (montosBs == null || gananciasBs == null || montosBs.Length != gananciasBs.Length)
            throw new System.ArgumentException("Arreglos inválidos.");
        if (presupuestoBs < 0 || presupuestoBs % 100 != 0)
            throw new System.ArgumentException("El presupuesto debe ser múltiplo de 100.");

        int n = montosBs.Length;
        int capacidad = presupuestoBs / 100;
        long[,] dp = new long[n + 1, capacidad + 1];

        // dp[i, c]: máxima ganancia en Bs usando las primeras i inversiones
        // y un presupuesto de hasta c dólares ficticios.
        for (int i = 1; i <= n; i++)
        {
            if (montosBs[i - 1] <= 0 || montosBs[i - 1] % 100 != 0 || gananciasBs[i - 1] < 0)
                throw new System.ArgumentException("Inversión inválida.");

            int costo = montosBs[i - 1] / 100;
            long ganancia = gananciasBs[i - 1]; // Se mantiene en bolivianos.
            for (int c = 0; c <= capacidad; c++)
            {
                long sinElegir = dp[i - 1, c];
                dp[i, c] = sinElegir;
                if (costo <= c)
                {
                    // La fila anterior evita elegir la misma inversión dos veces.
                    long eligiendo = ganancia + dp[i - 1, c - costo];
                    dp[i, c] = System.Math.Max(sinElegir, eligiendo);
                }
            }
        }
        return dp[n, capacidad];
    }
}
