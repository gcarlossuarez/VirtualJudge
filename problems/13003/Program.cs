// Resolución Jerárquica de Claves en la Caja Fuerte Inteligente

using System;
using System.Collections.Generic;

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
          DFS_Resolver(i);
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
    static void DFS_Resolver(int u)
    {
        if (visitado[u]) return;

        visitado[u] = true;
        enStack[u] = true;

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
                continue;
            }

            DFS_Resolver(h);
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
                pesosDisponibles.Add(claves[h].K);
            }
        }

        resoluble[u] = PuedeAlcanzarObjetivo(pesosDisponibles, claves[u].K);

        enStack[u] = false;
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


/********************************************************************
using System;
using System.Collections.Generic;

class Program
{
    class Clave
    {
        public string Nombre = string.Empty;
        public int K;
        public List<int> Pesos = new List<int>();
        public List<int> Hijos = new List<int>(); // relaciones A -> B se almacenan como: A.Hijos.Add(B)
    }

    static Clave[] claves;
    static Dictionary<string, int> indice = new Dictionary<string, int>();

    static bool[] visitado;
    static bool[] enPila;

    static bool HayCiclo(int u)
    {
        if (enPila[u]) return true;
        if (visitado[u]) return false;

        visitado[u] = true;
        enPila[u] = true;

        foreach (int h in claves[u].Hijos)
        {
            if (HayCiclo(h)) return true;
        }

        enPila[u] = false;
        return false;
    }

    // DFS para recolectar valores heredados válidos (solo si no hay ciclo en esa rama)
    static bool RecolectarValores(int u, HashSet<int> valores)
    {
        // Si esta clave puede resolver su propio K, se podrá heredar luego
        bool resoluble = false;

        // Primero obtenemos los valores heredados desde Hijos
        foreach (int h in claves[u].Hijos)
        {
            // Verificar si hay ciclo específico en la rama u -> h
            Array.Clear(visitado, 0, visitado.Length);
            Array.Clear(enPila, 0, enPila.Length);

            enPila[u] = true;
            if (HayCiclo(h))
            {
                // no heredamos de este hijo
                continue;
            }

            // Recolectar valores del hijo recursivamente
            HashSet<int> valoresHijo = new HashSet<int>();
            bool hijoResuelve = RecolectarValores(h, valoresHijo);

            if (hijoResuelve)
            {
                // Agrega el K del hijo
                valores.Add(claves[h].K);
            }

            foreach (var x in valoresHijo)
            {
                // Agrega los K heredados de los hijos del hijo. Por lo tanto, agrega los hijos de todo el
                // árbol genealógico, hacia abajo
                valores.Add(x);
            }
        }
        //
        // NOTA. - Si sale del for o nunca entra, significa que el hijo o no tiene descendientes o sus
        // descendientes ya fueron analizados en la bpusqueda en Profundidad y, ahora, le toca a él

        // Agregar los pesos propios de la clave
        foreach (var p in claves[u].Pesos)
        {
            valores.Add(p);
        }

        // Resolver subset sum para ver si esta clave puede lograr su K
        int objetivo = claves[u].K;
        bool[] dp = new bool[objetivo + 1];
        dp[0] = true;

        foreach (var v in valores)
        {
            for (int s = objetivo; s >= v; s--)
            { 
                if (dp[s - v])
                {
                    dp[s] = true;
                }
            }
        }

        resoluble = dp[objetivo];
        return resoluble;
    }

    static void Main()
    {
        // ----------------------------------------
        // Entrada:
        // N
        // Luego N líneas: Nombre K cantidadPesos p1 p2 ... pk
        // Luego R relaciones tipo: A -> B
        // ----------------------------------------

        int N = int.Parse(Console.ReadLine());
        claves = new Clave[N];

        for (int i = 0; i < N; i++)
        {
            string linea = Console.ReadLine().Trim();
            string[] parts = linea.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            //Console.WriteLine($"{string.Join(",", parts)}");
            Clave c = new Clave();
            c.Nombre = parts[0];
            c.K = int.Parse(parts[1]);

            int cantPesos = int.Parse(parts[2]);
            for (int j = 0; j < cantPesos; j++)
            {
                c.Pesos.Add(int.Parse(parts[3 + j]));
            }

            claves[i] = c;
            indice[c.Nombre] = i;
        }

        int R = int.Parse(Console.ReadLine());
        for (int i = 0; i < R; i++)
        {
            string linea = Console.ReadLine().Replace(" ", "");
            string[] parts = linea.Split(new string[] {"->"}, StringSplitOptions.None);

            string A = parts[0];
            string B = parts[1];

            int u = indice[A];
            int v = indice[B];

            claves[u].Hijos.Add(v); // mantener semántica visual tipo árbol
        }

        visitado = new bool[N];
        enPila = new bool[N];

        // Resolver cada clave
        bool[] resultado = new bool[N];
        for (int i = 0; i < N; i++)
        {
            HashSet<int> valores = new HashSet<int>();
            resultado[i] = RecolectarValores(i, valores);
        }

        // Imprimir en orden de nombre de clave
        List<int> orden = new List<int>();
        for (int i = 0; i < N; i++) orden.Add(i);
        orden.Sort((a, b) => string.Compare(claves[a].Nombre, claves[b].Nombre));

        foreach (int i in orden)
        {
            Console.WriteLine($"{claves[i].Nombre} {(resultado[i] ? "True" : "False")}");
        }
    }
}

********************************************************************/
