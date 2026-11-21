using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

class Program
{
    struct Arista
    {
        public int Destino,
            Peso;

        public Arista(int destino, int peso)
        {
            Destino = destino;
            Peso = peso;
        }
    }

    

    static long Excentricidad(List<Arista>[] grafo, int origen, long limite)
    {
        int n = grafo.Length;
        long[] dist = new long[n];
        for (int i = 0; i < n; i++)
            dist[i] = long.MaxValue;
        dist[origen] = 0;
        var pendientes = new SortedSet<Tuple<long, int>>();
        pendientes.Add(Tuple.Create(0L, origen));
        int procesados = 0;
        long maximo = 0;
        while (pendientes.Count > 0)
        {
            var actual = pendientes.Min;
            pendientes.Remove(actual);
            long costo = actual.Item1;
            int u = actual.Item2;

            // Se ignoran las entradas desactualizadas porque un mismo vértice puede
            // aparecer varias veces en el SortedSet con distintas distancias.
            // Cuando se encuentra un camino más corto, se inserta la nueva distancia
            // sin eliminar la anterior. Por tanto, si la distancia extraída no coincide
            // con la mejor distancia conocida (dist[u]), significa que existe una
            // entrada más reciente y con menor costo.
            // Esta estrategia evita realizar operaciones Remove() adicionales.
            // Aunque SortedSet no admite duplicados, las tuplas (distancia, vértice)
            // son diferentes cuando sus distancias son distintas.
            if (costo != dist[u])
                continue;

            if (costo >= limite)
                return long.MaxValue; // No puede mejorar al ganador actual.
            procesados++;
            maximo = costo;
            foreach (var arista in grafo[u])
            {
                long nuevo = costo + arista.Peso;
                if (nuevo < dist[arista.Destino])
                {
                    dist[arista.Destino] = nuevo;
                    pendientes.Add(Tuple.Create(nuevo, arista.Destino));
                }
            }
        }
        return procesados == n ? maximo : long.MaxValue;
    }

    static void Main()
    {
        var lineas = Console.In.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
        int t = int.Parse(lineas[0]);
        int j = 1;
        var salida = new StringBuilder();
        for (int caso = 0; caso < t; caso++)
        {
          	string[] parts = lineas[j].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int n = int.Parse(parts[0]),
                m = int.Parse(parts[1]),
                k = int.Parse(parts[2]);
          	
            string linea = lineas[++j];
            parts = linea.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var candidatos = new int[k];
            for (int i = 0; i < k; i++)
            {
                candidatos[i] = int.Parse(parts[i]) - 1;
            }
            var grafo = new List<Arista>[n];
            for (int i = 0; i < n; i++)
            {
              	grafo[i] = new List<Arista>();
            }
            
            for (int i = 0; i < m; i++)
            {
                linea = lineas[++j];
                parts = linea.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int u = int.Parse(parts[0]) - 1,
                    v = int.Parse(parts[1]) - 1,
                    w = int.Parse(parts[2]);
                grafo[u].Add(new Arista(v, w));
            }
            long mejor = long.MaxValue;
            int ganador = -1;
            Array.Sort(candidatos);
            foreach (int candidato in candidatos)
            {
                // Se envía la mejor excentricidad encontrada hasta el momento para utilizarla
                // como límite de poda durante la ejecución de Dijkstra.
                // Como Dijkstra procesa los vértices en orden no decreciente de distancia,
                // si la distancia mínima de un vértice alcanza o supera ese límite,
                // el candidato actual ya no puede mejorar la mejor solución encontrada.
                // Por tanto, podemos detener anticipadamente su evaluación, evitando
                // procesar vértices innecesarios y reduciendo el tiempo de ejecución.
                // Esta optimización no modifica el resultado final, siempre que los
                // candidatos empatados se resuelvan según el criterio establecido.
                long valor = Excentricidad(grafo, candidato, mejor);
                if (valor < mejor)
                {
                    mejor = valor;
                    ganador = candidato + 1;
                }
            }
            salida.AppendLine(ganador == -1 ? "-1" : ganador + " " + mejor);
        }
        Console.Write(salida);
    }
}

/*****************************
using System;
using System.Collections.Generic;
using System.Text;

class Program
{
    struct Arista
    {
        public int Destino,
            Peso;

        public Arista(int destino, int peso)
        {
            Destino = destino;
            Peso = peso;
        }
    }

    class Lector
    {
        private readonly string texto;
        private int pos;

        public Lector(string texto)
        {
            this.texto = texto;
        }

        public int Entero()
        {
            while (pos < texto.Length && char.IsWhiteSpace(texto[pos]))
                pos++;
            int inicio = pos;
            while (pos < texto.Length && !char.IsWhiteSpace(texto[pos]))
                pos++;
            int valor;
            if (inicio == pos || !int.TryParse(texto.Substring(inicio, pos - inicio), out valor))
                throw new FormatException("Entero inválido en la entrada.");
            return valor;
        }
    }

    static long Excentricidad(List<Arista>[] grafo, int origen, long limite)
    {
        int n = grafo.Length;
        long[] dist = new long[n];
        for (int i = 0; i < n; i++)
            dist[i] = long.MaxValue;
        dist[origen] = 0;
        var pendientes = new SortedSet<Tuple<long, int>>();
        pendientes.Add(Tuple.Create(0L, origen));
        int procesados = 0;
        long maximo = 0;
        while (pendientes.Count > 0)
        {
            var actual = pendientes.Min;
            pendientes.Remove(actual);
            long costo = actual.Item1;
            int u = actual.Item2;

            // Se ignoran las entradas desactualizadas porque un mismo vértice puede
            // aparecer varias veces en el SortedSet con distintas distancias.
            // Cuando se encuentra un camino más corto, se inserta la nueva distancia
            // sin eliminar la anterior. Por tanto, si la distancia extraída no coincide
            // con la mejor distancia conocida (dist[u]), significa que existe una
            // entrada más reciente y con menor costo.
            // Esta estrategia evita realizar operaciones Remove() adicionales.
            // Aunque SortedSet no admite duplicados, las tuplas (distancia, vértice)
            // son diferentes cuando sus distancias son distintas.
            if (costo != dist[u])
                continue;

            if (costo >= limite)
                return long.MaxValue; // No puede mejorar al ganador actual.
            procesados++;
            maximo = costo;
            foreach (var arista in grafo[u])
            {
                long nuevo = costo + arista.Peso;
                if (nuevo < dist[arista.Destino])
                {
                    dist[arista.Destino] = nuevo;
                    pendientes.Add(Tuple.Create(nuevo, arista.Destino));
                }
            }
        }
        return procesados == n ? maximo : long.MaxValue;
    }

    static void Main()
    {
        var lector = new Lector(Console.In.ReadToEnd());
      
        int t = lector.Entero();
        var salida = new StringBuilder();
        for (int caso = 0; caso < t; caso++)
        {
            int n = lector.Entero(),
                m = lector.Entero(),
                k = lector.Entero();
            var candidatos = new int[k];
            for (int i = 0; i < k; i++)
                candidatos[i] = lector.Entero() - 1;
            var grafo = new List<Arista>[n];
            for (int i = 0; i < n; i++)
                grafo[i] = new List<Arista>();
            for (int i = 0; i < m; i++)
            {
                int u = lector.Entero() - 1,
                    v = lector.Entero() - 1,
                    w = lector.Entero();
                grafo[u].Add(new Arista(v, w));
            }
            long mejor = long.MaxValue;
            int ganador = -1;
            Array.Sort(candidatos);
            foreach (int candidato in candidatos)
            {
                // Se envía la mejor excentricidad encontrada hasta el momento para utilizarla
                // como límite de poda durante la ejecución de Dijkstra.
                // Como Dijkstra procesa los vértices en orden no decreciente de distancia,
                // si la distancia mínima de un vértice alcanza o supera ese límite,
                // el candidato actual ya no puede mejorar la mejor solución encontrada.
                // Por tanto, podemos detener anticipadamente su evaluación, evitando
                // procesar vértices innecesarios y reduciendo el tiempo de ejecución.
                // Esta optimización no modifica el resultado final, siempre que los
                // candidatos empatados se resuelvan según el criterio establecido.
                long valor = Excentricidad(grafo, candidato, mejor);
                if (valor < mejor)
                {
                    mejor = valor;
                    ganador = candidato + 1;
                }
            }
            salida.AppendLine(ganador == -1 ? "-1" : ganador + " " + mejor);
        }
        Console.Write(salida);
    }
}
**********/
