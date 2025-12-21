// Resolución Jerárquica de Claves en la Caja Fuerte Inteligente
// Ordenación topológica y Subset Sum.
using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    class Clave
    {
        public string Nombre;
        public int K;
        public List<int> Pesos = new List<int>();
        public List<int> Hijos = new List<int>(); // relaciones A -> B (A superior, B subordinada)
    }

    static Clave[] claves;
    static Dictionary<string, int> indice;

    static bool[] visitado;
    static bool[] enStack;
    static bool[] resoluble;

    static void Main()
    {
        // Formato de entrada:
        // N
        // N líneas:  Nombre  K  cantPesos  p1 p2 ... pk
        // R
        // R líneas:  A -> B   (con espacios libres)

        int N = int.Parse(Console.ReadLine()!.Trim());
        claves = new Clave[N];
        indice = new Dictionary<string, int>(N);

        for (int i = 0; i < N; i++)
        {
            string linea = Console.ReadLine()!.Trim();
            string[] parts = linea.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var c = new Clave
            {
                Nombre = parts[0],
                K = int.Parse(parts[1])
            };

            int cantPesos = int.Parse(parts[2]);
            for (int j = 0; j < cantPesos; j++)
            {
                c.Pesos.Add(int.Parse(parts[3 + j]));
            }

            claves[i] = c;
            indice[c.Nombre] = i;
        }

        int R = int.Parse(Console.ReadLine()!.Trim());
        for (int i = 0; i < R; i++)
        {
            string linea = Console.ReadLine()!;
            linea = linea.Replace(" ", "");
            string[] parts = linea.Split(new string[] { "->" }, StringSplitOptions.None);
            string A = parts[0];
            string B = parts[1];

            int u = indice[A];
            int v = indice[B];

            // A -> B : A es superior, B subordinada
            // Para herencia: A puede heredar K_B si B resuelve
            // Pero estructuralmente, A tiene como hijo a B
            claves[u].Hijos.Add(v);
        }

        visitado = new bool[N];
        enStack = new bool[N];
        resoluble = new bool[N];

        // Resolver todas las claves
        for (int i = 0; i < N; i++)
        {
            List<HashSet<int>> inCycles = new List<HashSet<int>>();
            HashSet<int> recursionStack = new HashSet<int>();
            DFS_Resolver(i, inCycles, recursionStack);
        }

        // Imprimir en orden por nombre de clave
        List<int> orden = new List<int>();
        for (int i = 0; i < N; i++) orden.Add(i);
        orden.Sort((a, b) => string.Compare(claves[a].Nombre, claves[b].Nombre, StringComparison.Ordinal));

        foreach (int i in orden)
        {
            Console.WriteLine($"{claves[i].Nombre} {(resoluble[i] ? "True" : "False")}");
        }
    }

    // Resuelve la clave u recursivamente (subordinadas primero)
    static void DFS_Resolver(int u, List<HashSet<int>> inCycles, HashSet<int> recursionStack)
    {
        if (visitado[u]) return;

        visitado[u] = true;
        enStack[u] = true;
        recursionStack.Add(u);

        // Primero resolver subordinadas
        List<int> candidates = new List<int>();
        foreach (int h in claves[u].Hijos)
        {
            if (enStack[h])
            {
                // Hay un ciclo u -> ... -> h -> ... -> u
                // Para este diseño sencillo: simplemente
                // NO usamos esa subordinada para herencia.
                // Pero igual dejamos que h se resuelva
                // cuando le toque por su propio DFS.
                HashSet<int> hashSetCycle = new HashSet<int>();
                foreach(var nodeInCycle in recursionStack)
                {
                    hashSetCycle.Add(nodeInCycle);
                }
                inCycles.Add(hashSetCycle);

                continue;
            }

            DFS_Resolver(h, inCycles, recursionStack);
        }

        // Construir lista de pesos disponibles para u:
        //  - pesos propios
        //  - K_h de cada subordinada h que resolvió su propio K_h
        List<int> pesosDisponibles = new List<int>();
        pesosDisponibles.AddRange(claves[u].Pesos);

        foreach (int h in claves[u].Hijos)
        {
            if (resoluble[h]) 	// Cuando llega aquí, ya analizó a todos sus descendientes; por ser una 
								// búsqueda DFS y, por lo tanto, ya se sabe si su descendiente es 
								// resoluble
            {
                bool includeK = true;
                foreach(var node in inCycles)
                {
                  if(node.Any(x=> x == u) && node.Any(x=> x == h))   // Si padre e hijo estan en uno
                                                                     // de los ciclos detectados
                  {
                      includeK = false;
                      //Console.WriteLine($"Ciclo detectado:{string.Join(", ", node)}");
                      break;
                  }
                }

                if(includeK) pesosDisponibles.Add(claves[h].K);
            }
        }

        resoluble[u] = PuedeAlcanzarObjetivo(pesosDisponibles, claves[u].K);

        enStack[u] = false;
        recursionStack.Remove(u);
    }


    // Subset sum clásico para ver si se puede alcanzar "objetivo"
    static bool PuedeAlcanzarObjetivo(List<int> valores, int objetivo)
    {
        if (objetivo == 0) return true;
        if (valores.Count == 0) return false;

        bool[] dp = new bool[objetivo + 1];
        dp[0] = true;

        foreach (int v in valores)
        {
            if (v > objetivo) continue;

            for (int s = objetivo; s >= v; s--)
            {
                if (dp[s - v])
                {
                    dp[s] = true;
                }
            }
        }

        return dp[objetivo];
    }
}



