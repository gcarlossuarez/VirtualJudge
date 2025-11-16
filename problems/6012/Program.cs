using System;

class Program
{
    const int INF = 1000000;
    
    static void Main()
    {
        try
        {
            int n = int.Parse(Console.ReadLine() ?? "0");
            
            if (n < 2 || n > 100)
            {
                Console.WriteLine("Error: n debe estar entre 2 y 100");
                return;
            }
            
            int[,] costos = LeerMatriz(n, "INF");
            int[,] ganancias = LeerMatriz(n, "INF");
			
			/*
			Por qué no funciona este enfoque (complementar revisando el archivo ".\contraejemplo_floyd_ganancias_v2.docx"
			Problemas de ese enfoque:
			Se Mezclan dos cosas distintas (costo de transporte y ganancia de venta) en una sola matriz, cuando conceptualmente 
			son dos funciones diferentes.
			Pueden aparecer ciclos de “ganancia infinita”: si 
			𝐺𝑎𝑛𝑎𝑛𝑐𝑖𝑎 − 𝐶𝑜𝑠𝑡𝑜 > 0
			Ganancia−Costo>0 en un ciclo, entonces el “costo efectivo” es negativo y puedes dar vueltas y vueltas, disminuyendo 
			indefinidamente el “costo” → Floyd–Warshall está pensado para grafos sin ciclos negativos (o para detectarlos, pero 
			no para “maximizar ganancia infinita”).
			int[,] gananciasNetasAux = new int[n, n];
			for(int i = 0; i < n; ++i)
			{
				for(int j = 0; j < n; ++j)
				{
					if(i == j)
					{
						gananciasNetasAux[i, j] = 0;
					}
					else if(costos[i, j] == INF || ganancias[i, j] == INF)
					{
						gananciasNetasAux[i, j] = INF;
					}
					else
					{
						gananciasNetasAux[i, j] = -(ganancias[i, j] - costos[i, j]);
					}
				}
			}
			int[,] gananciasNetas = CalcularFloydWarshaLlMalAplicado(gananciasNetasAux, n);
			
			Podemos afirmar que:
			•  Ganancia[i][j] es la ganancia asociada al destino final, fijada en la matriz de Ganancias.
			Es un valor propio del par (i,j), no depende del camino tomado.
			•  CostoMínimo[i][j] es el costo de transporte más barato para ir desde i hasta j,
			calculado usando Floyd–Warshall (o cualquier cálculo de camino mínimo).

			*/
            
            int[,] costosMinimos = CalcularCostosMinimos(costos, n);
            int[,] gananciasNetas = CalcularGananciasNetas(ganancias, costosMinimos, n);
            
            ImprimirMatriz(gananciasNetas, n);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    
    static int[,] LeerMatriz(int n, string valorInfinito)
    {
        int[,] matriz = new int[n, n];
        
        for (int i = 0; i < n; i++)
        {
			string values = Console.ReadLine() ?? string.Empty;
            string[] valores = values.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (valores.Length != n)
                throw new Exception($"Se esperaban {n} valores en la fila {i + 1}");
                
            for (int j = 0; j < n; j++)
            {
                matriz[i, j] = (valores[j] == valorInfinito) ? INF : int.Parse(valores[j]);
            }
        }
        
        return matriz;
    }
    
	static int[,] CalcularFloydWarshaLlMalAplicado(int[,] costos, int n)
	{
		return CalcularCostosMinimos(costos,  n);
	}
	
    static int[,] CalcularCostosMinimos(int[,] costos, int n)
    {
        int[,] dist = new int[n, n];
        
        // Inicializar matriz de distancias
        for (int i = 0; i < n; i++)
		{
            for (int j = 0; j < n; j++)
			{
                dist[i, j] = costos[i, j];
			}
		}
        
        // Algoritmo de Floyd-Warshall
        for (int k = 0; k < n; k++)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (dist[i, k] != INF && dist[k, j] != INF)
                    {
                        int nuevaDistancia = dist[i, k] + dist[k, j];
                        if (nuevaDistancia < dist[i, j])
                        {
                            dist[i, j] = nuevaDistancia;
                        }
                    }
                }
            }
        }
		
		 // Verificar ciclos negativos
		for (int i = 0; i < n; i++)
		{
			if (dist[i, i] < 0)
			{
				throw new Exception("Hay ciclo negativo");
			}
		}
        
        return dist;
    }
    
    static int[,] CalcularGananciasNetas(int[,] ganancias, int[,] costosMinimos, int n)
    {
        int[,] resultado = new int[n, n];
        
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == j)
                {
                    resultado[i, j] = 0;
                }
                else if (costosMinimos[i, j] == INF || ganancias[i, j] == INF)
                {
                    resultado[i, j] = INF;
                }
                else
                {
                    resultado[i, j] = ganancias[i, j] - costosMinimos[i, j];
                }
            }
        }
        
        return resultado;
    }
    
    static void ImprimirMatriz(int[,] matriz, int n)
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (matriz[i, j] == INF) Console.Write("INF");
                else Console.Write(matriz[i, j]);
                
                if (j < n - 1) Console.Write(" ");
            }
            Console.WriteLine();
        }
    }
}