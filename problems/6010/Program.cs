using System;
using System.Collections.Generic;
using System.Linq;

namespace VenganzaFastHands
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== LA VENGANZA DE FAST HANDS ===");
            //Console.WriteLine("Pega el input línea por línea (origen;destino;días). Línea vacía para terminar:");
            
            var graph = new Dictionary<string, List<(string destino, int dias)>>();
            var nodos = new HashSet<string>();

            // Leer input
            while (true)
            {
                string? linea = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(linea)) break;

                string[] partes = linea.Split(';');
                if (partes.Length != 3) 
                {
                    Console.WriteLine($"Error en línea: {linea}");
                    continue;
                }

                string origen = partes[0].Trim();
                string destino = partes[1].Trim();
                if (!int.TryParse(partes[2].Trim(), out int dias))
                {
                    Console.WriteLine($"Peso inválido: {partes[2]}");
                    continue;
                }

                nodos.Add(origen);
                nodos.Add(destino);

                if (!graph.ContainsKey(origen))
                    graph[origen] = new List<(string, int)>();

                graph[origen].Add((destino, dias));
            }

            // Ejecutar Dijkstra
            var (diasMinimos, camino) = Dijkstra(graph, "Accidente", "Alta Médica Total");

            // Salida
            Console.WriteLine(new string('=', 50));
            if (camino != null)
            {
                //Console.WriteLine($"Secuencia óptima:");
                //Console.WriteLine(string.Join(" → ", camino));
                Console.WriteLine($"Mínimo de días sin poder 'tallar': {diasMinimos}");
                //Console.WriteLine($"Pepe vuelve a tallar en el día {diasMinimos} (lunes + {diasMinimos} días).");
            }
            else
            {
                Console.WriteLine("No hay forma de llegar al alta médica.");
            }
            Console.WriteLine(new string('=', 50));

            //Console.WriteLine("\nPresiona cualquier tecla para salir...");
            //Console.ReadKey();
        }

        static (int distancia, List<string>? camino) Dijkstra(
            Dictionary<string, List<(string destino, int dias)>> graph,
            string inicio, string fin)
        {
            var distancias = new Dictionary<string, int>();
            var previos = new Dictionary<string, string>();
            var pq = new SortedSet<(int distancia, string nodo)>(Comparer<(int distancia, string nodo)>.Create((a, b) =>
                a.distancia != b.distancia ? a.distancia.CompareTo(b.distancia) : a.nodo.CompareTo(b.nodo)));

            foreach (var nodo in graph.Keys)
            {
                distancias[nodo] = int.MaxValue;
            }
			
			// Explicación de graph.SelectMany(g => g.Value.Select(v => v.destino) => 
			// graph["A"] = { ("B", 10) }  // "B" es destino, pero no tiene salidas aún
			// → "B" no está como clave en graph, pero sí debe tener una distancia.
			// Esta instrucción está asegurando que todos los nodos destino estén 
			// inicializados en el diccionario de distancias, incluso si no aparecen como 
			// nodos origen en el grafo.
            foreach (var nodo in graph.SelectMany(g => g.Value.Select(v => v.destino)))
            {
                if (!distancias.ContainsKey(nodo))
                    distancias[nodo] = int.MaxValue;
            }
			/*
			NOTA. - Esta línea solo itera sobre nodos destino de las conexiones existentes. 
			Si un nodo origen no tiene ninguna arista saliente (no tiene destinos), entonces:
			Está como clave en graph (porque se añadió cuando se leyó el input)
			Pero graph[origen].Value es una lista vacía
			Por lo tanto, no aparecerá en el SelectMany (no produce ningún destino)
			// Grafo:
			// A → B
			// C → D  
			// E (nodo origen sin destinos)

			var graph = new Dictionary<string, List<(string, int)>>();
			graph["A"] = new List<(string, int)> { ("B", 10) };
			graph["C"] = new List<(string, int)> { ("D", 5) };
			graph["E"] = new List<(string, int)>(); // LISTA VACÍA

			// El SelectMany producirá: ["B", "D"]
			// El nodo "E" NO estará en esta lista
			
			Equivalente 1 =>
			// 1. Añadir todos los nodos que son claves (origen)
			foreach (var nodo in graph.Keys)
			{
				if (!distancias.ContainsKey(nodo))
					distancias[nodo] = int.MaxValue;
			}

			// 2. Añadir todos los nodos que son destinos (pero no claves)
			foreach (var vecinos in graph.Values)
			{
				foreach (var (destino, _) in vecinos)
				{
					if (!distancias.ContainsKey(destino))
						distancias[destino] = int.MaxValue;
				}
			}
			
			Equivalente 2
			var todosLosNodos = graph.Keys
				.Concat(graph.Values.SelectMany(v => v.Select(x => x.destino)))
				.Distinct();

			foreach (var nodo in todosLosNodos)
			{
				distancias[nodo] = int.MaxValue;
			}
			*/

            distancias[inicio] = 0;
            pq.Add((0, inicio));

            while (pq.Count > 0)
            {
                var (dist, actual) = pq.Min;
                pq.Remove(pq.Min);

                if (dist > distancias[actual]) continue;

                if (!graph.ContainsKey(actual)) continue;

                foreach (var (vecino, peso) in graph[actual])
                {
                    int nuevaDist = distancias[actual] + peso;
                    if (nuevaDist < distancias[vecino])
                    {
                        if (distancias[vecino] != int.MaxValue) // Los nodos con ifnitio, no son cargados
                            pq.Remove((distancias[vecino], vecino));

                        distancias[vecino] = nuevaDist;
                        previos[vecino] = actual;
                        pq.Add((nuevaDist, vecino));
                    }
                }
            }

            if (distancias[fin] == int.MaxValue)
                return (0, null);

            // Reconstruir camino
            var camino = new List<string>();
            string actualNodo = fin;
            while (actualNodo != null)
            {
                camino.Add(actualNodo);
                if (!previos.ContainsKey(actualNodo)) break;
                actualNodo = previos[actualNodo];
            }
            camino.Reverse();

            if (camino[0] != inicio) return (0, null);

            return (distancias[fin], camino);
        }
    }
}