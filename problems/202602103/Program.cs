using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // 1. Leer las coordenadas de inicio y fin
        string primeraLinea = Console.ReadLine() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(primeraLinea)) return;

        string[] partesCoordenadas = primeraLinea.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int startX = int.Parse(partesCoordenadas[0]);
        int startY = int.Parse(partesCoordenadas[1]);
        int endX = int.Parse(partesCoordenadas[2]);
        int endY = int.Parse(partesCoordenadas[3]);

        // 2. Leer las filas del laberinto hasta encontrar una línea en blanco
        List<List<int>> listaLaberinto = new List<List<int>>();
        string linea;

        while (!string.IsNullOrWhiteSpace(linea = Console.ReadLine() ?? string.Empty))
        {
            string[] partesFila = linea.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            List<int> fila = new List<int>();
            foreach (var celda in partesFila)
            {
                fila.Add(int.Parse(celda));
            }
            listaLaberinto.Add(fila);
        }

        // Convertir la lista dinámica a una matriz bidimensional tradicional
        int filas = listaLaberinto.Count;
        if (filas == 0) return;
        int columnas = listaLaberinto[0].Count;
        
        int[,] laberinto = new int[filas, columnas];
        for (int i = 0; i < filas; i++)
        {
            for (int j = 0; j < columnas; j++)
            {
                laberinto[i, j] = listaLaberinto[i][j];
            }
        }

        // Matriz para recordar qué celdas están en el camino actual y evitar bucles
        bool[,] visitado = new bool[filas, columnas];

        // 3. Ejecutar el algoritmo de Backtracking
        if (ResolverLaberintoBacktracking(laberinto, startX, startY, endX, endY, visitado))
        {
            Console.WriteLine("S");
        }
        else
        {
            Console.WriteLine("N");
        }
    }

    static bool ResolverLaberintoBacktracking(int[,] laberinto, int x, int y, int endX, int endY, bool[,] visitado)
    {
        int filas = laberinto.GetLength(0);
        int columnas = laberinto.GetLength(1);

        // Caso Base 1: Si llegamos a la celda de destino con éxito
        if (x == endX && y == endY)
        {
            // Validar que el destino final no sea una pared
            return laberinto[x, y] == 0;
        }

        // Caso Base 2: Validar límites del laberinto, si es pared (1) o si ya fue visitado
        if (x < 0 || x >= filas || y < 0 || y >= columnas || laberinto[x, y] == 1 || visitado[x, y])
        {
            return false;
        }

        // --- PASO DE BACKTRACKING ---
        
        // 1. Marcar la celda actual como visitada (la agregamos a la ruta actual)
        visitado[x, y] = true;

        // 2. Intentar moverse en las 4 direcciones cardinales recursivamente
        // Intentar ir hacia ABAJO
        if (ResolverLaberintoBacktracking(laberinto, x + 1, y, endX, endY, visitado)) return true;
        
        // Intentar ir hacia la DERECHA
        if (ResolverLaberintoBacktracking(laberinto, x, y + 1, endX, endY, visitado)) return true;
        
        // Intentar ir hacia ARRIBA
        if (ResolverLaberintoBacktracking(laberinto, x - 1, y, endX, endY, visitado)) return true;
        
        // Intentar ir hacia la IZQUIERDA
        if (ResolverLaberintoBacktracking(laberinto, x, y - 1, endX, endY, visitado)) return true;

        // 3. Si ninguna dirección funcionó, desmarcamos la celda (RETROCESO / BACKTRACK)
        // Esto permite que esta misma celda pueda ser usada por otro camino alternativo
        visitado[x, y] = false;

        return false;
    }
}
