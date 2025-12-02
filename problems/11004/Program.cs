using System;
using System.Collections.Generic;


// Fábrica de Conversión Óptima
class Program
{
    static void Main()
    {
        var data = Console.ReadLine()!.Split();
        int N = int.Parse(data[0]);
        int M = int.Parse(data[1]);

        // Grafo: de cada nodo salen procesos (destino, costo)
        var procesos = new List<(int to, int cost)>[N + 1];
        for (int i = 1; i <= N; i++)
		{
			procesos[i] = new List<(int, int)>();
		}

        for (int i = 0; i < M; i++)
        {
            var p = Console.ReadLine()!.Split();
            int A = int.Parse(p[0]);
            int B = int.Parse(p[1]);
            int C = int.Parse(p[2]);
            procesos[A].Add((B, C));
        }

        const long INF = long.MaxValue / 4;
        var costo = new long[N + 1];
        for (int i = 1; i <= N; i++)
		{
			costo[i] = INF;
		}

        costo[1] = 0;

        // Usamos SortedSet<(distancia, nodo)>
        var abiertos = new SortedSet<(long distancia, int nodo)>();
		/*
		Ejemplo de SortedSet, para probar en LinqPad, para ver cómo discrimina los repetidos y hace la doble compáración, por defecto
		SortedSet<(int distancia, int nodo)> a = new SortedSet<(int distancia, int nodo)> ();
		a.Add((10, 3));
		a.Add((11, 5));
		a.Add((10, 3));
		a.Add((11, 5));
		a.Add((11, 4));

		a.Dump();
		*/
        abiertos.Add((0, 1)); // (distancia, nodo)

        while (abiertos.Count > 0)
        {
            var (dist, nodo) = abiertos.Min;
            abiertos.Remove(abiertos.Min); // quitamos el de menor distancia

            // Si ya procesamos este nodo con una distancia mejor, ignoramos
            if (dist > costo[nodo]) continue;

            //if (nodo == N) break; // ya llegamos al final con la mejor distancia posible

            foreach (var (siguiente, c) in procesos[nodo])
            {
                long nuevoCosto = dist + c;
                if (nuevoCosto < costo[siguiente])
                {
                    // Importante: si ya estaba en el conjunto con peor distancia, lo eliminamos
                    // (aunque no siempre es necesario porque el comparador es por distancia+nodo)
                    // Pero para seguridad y claridad, lo quitamos si existe
					
					// No es obligatorio y en muchos casos hasta ralentiza un poco. 
					// Esa versión simple es la más usada y recomendada.
					// Ignora distancias no óptimas
					// if (dist > costo[nodo]) continue;
					
                    // Antes de insertar el nuevo:
					abiertos.Remove((costo[siguiente], siguiente)); // quita la versión vieja si existe
					costo[siguiente] = nuevoCosto;
					abiertos.Add((nuevoCosto, siguiente));
                    // Nota: no necesitamos eliminar la versión antigua porque tiene mayor distancia
                    // → nunca será seleccionada antes que la nueva
                }
            }
        }

        Console.WriteLine(costo[N] >= INF ? "IMPOSIBLE" : costo[N].ToString());
    }
}
