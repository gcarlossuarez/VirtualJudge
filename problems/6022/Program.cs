using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        List<string> linesList = new List<string>();
	      // Leer datos de entrada
        string? val;
        //while (!string.IsNullOrEmpty(val = Console.ReadLine()))
        while ((val = Console.ReadLine()) != null)  // Permite leer lineas en blanco.
                                                  // Si se lee desde un archivo, sale con EOF
                                                  // Si se lee desde consola, sale del while con Control + C
        {
            if(!string.IsNullOrEmpty(val) && !string.IsNullOrWhiteSpace(val))
            {
              linesList.Add(val);
            }
        }

        // Procesar las entradas
        ProcessFile(linesList);

    }

    static void ProcessFile(List<string> linesList)
    {
        // Para evitar problemas con las líneas en blanco y facilitar el proceso de las líneas de texto, con
        // información
        var lines = linesList.Where(l=> !string.IsNullOrEmpty(l) && !string.IsNullOrWhiteSpace(l)).ToArray(); 

        int budget = int.Parse(lines[0].Split(':')[1].Trim());
        int countryCount = int.Parse(lines[1].Split(':')[1].Trim());
        //budget.Dump();
        //countryCount.Dump();
        //lines.Dump();

        var countries = new List<Country>();

        // Salta las 2 primeras líneas, que ya fueron leídas, por ejemplo
        //Presupuesto: 60
        //Países: 4

        // Se va a leer la información
        int index = 2;
        for (int i = 0; i < countryCount; i++)
        {
            string countryHeader = lines[index++];
            //countryHeader.Dump();

            // Leer ciudades y conexiones
            var cities = new List<Connection>();
            while (lines[index] != "Pedidos:") // Hasta que llegue a la línea con la cadena "Pedidos:"
            {
                var parts = lines[index++].Split(';');
                if(parts.Length == 1) 
                {
                  // Encabezado de ciudad
                  //parts.Dump();
                  continue;
                }

                cities.Add( new Connection(parts[0], parts[1], int.Parse(parts[2])));
                //cities.Add(new Connection(parts[1], parts[0], int.Parse(parts[2]))); // Según el contexto del problema, los grafos no son dirigidos
            }

            index++; // Saltar la cadena "Pedidos:", que acaba de ser leída
            var requests = new List<Request>();
            while (index < lines.Length && !lines[index].StartsWith("País")) // Hasta que llegue al siguiente país
            {
                var parts = lines[index++].Split(';');
                requests.Add(new Request(parts[0], parts[1])); // Se agrega el pedido
            }

            // Se adiciona el país con sus ciudades y sus pedidos
            countries.Add(new Country(countryHeader.Split(' ')[1].Replace(":", "").Trim(), cities, requests));
        }
        //countries.Dump("countries");
        //throw new Exception("Saliendo por depuración");

        // Calcular costos mínimos para cada país
        foreach (var country in countries)
        {
            country.CalculateMinimumCost();
        }

        //countries.Dump("countries");
        //budget.Dump("budget");
        // Resolver el problema de la mochila
        var selectedCountries = SolveKnapsack(countries, budget);

        // Generar la salida
        if (selectedCountries.Count == 0)
        {
            Console.WriteLine("Sin entregas posibles");
        }
        else
        {
            Console.WriteLine("Países seleccionados:");
            int totalCost = 0;
            int totalProfit = 0;

            for (int i = 0; i < selectedCountries.Count; i++)
            {
                var country = selectedCountries[i];
                Console.WriteLine($"{i + 1}. País {country.Id}:");
                // Las rutas utilizadas se muestra separadas por ",". Cada ruta es un conjunto de nodos
                // separados por " -> ".
                // Por ejemplo, esto, son 2 rutas: A -> C, B -> C -> D  
                Console.WriteLine($"   Rutas utilizadas: {string.Join(", ", country.SelectedRoutes)} (Costo total: {country.TotalCost})");
                Console.WriteLine($"   Ganancia: {country.Profit}");
                totalCost += country.TotalCost;
                totalProfit += country.Profit;
            }

            Console.WriteLine($"\nCosto total de las rutas: {totalCost}");
            Console.WriteLine($"Ganancia máxima obtenida: {totalProfit}");
        }
    }

    static List<Country> SolveKnapsack(List<Country> countries, int budget)
    {
        int n = countries.Count;
        var dp = new int[n + 1, budget + 1];

        for (int i = 1; i <= n; i++)
        {
            if (countries[i - 1].TotalCost > budget)
            {
// Si el país actual no puede ser incluido por su costo, los resultados de las subsoluciones permanecen idénticos a los de la fila anterior.
                for (int w = 0; w <= budget; w++)
                {
                    dp[i, w] = dp[i - 1, w];
                }
                continue;
            }

            for (int w = 1; w <= budget; w++)
            {
                if (countries[i - 1].TotalCost <= w)
                {
                    int remainingBudget = w - countries[i - 1].TotalCost;
                    dp[i, w] = Math.Max(dp[i - 1, w],
                                        dp[i - 1, remainingBudget] + countries[i - 1].Profit);
                }
                else
                {
                    dp[i, w] = dp[i - 1, w];
                }
            }
        }

        {
            var selectedCountries = new List<Country>();
            int remainingBudget = budget;
            for (int i = n; i > 0 && remainingBudget > 0; i--)
            {
                if (dp[i, remainingBudget] != dp[i - 1, remainingBudget])
                {
                    selectedCountries.Add(countries[i - 1]);
                    remainingBudget -= countries[i - 1].TotalCost;
                }
            }

            return selectedCountries;
        }
    }

}

class Country
{
    public string Id {get; private set;}
    public List<Connection> Cities { get; }
    public List<Request> Requests { get; }
    public List<string> SelectedRoutes { get; private set; }
    public int TotalCost { get; private set; }
    public int Profit => TotalCost * 2; // Ganancia

    public Country(string id, List<Connection> cities, List<Request> requests)
    {
        Id = id;
        Cities = cities;
        Requests = requests;
        SelectedRoutes = new List<string>();
        TotalCost = 0;
    }

    public void CalculateMinimumCost()
    {
        foreach (var request in Requests) // Por cada pedido
        {
            // Por cada pedido obtiene el costo mínimo y su ruta tomada se devuelve como una cadena
            // de nodos separados por " -> "
            var costAndRoute = FindShortestPathWithDijsktra(request.Origin, request.Destination); 
            if (costAndRoute.Item1 == int.MaxValue)
            {
                // Ruta no alcanzable, marcar el país como inválido
                TotalCost = int.MaxValue;
                SelectedRoutes.Clear();
                return;
            }
            TotalCost += costAndRoute.Item1;
            SelectedRoutes.Add(costAndRoute.Item2);
        }
    }


    private (int, string) FindShortestPathWithDijsktra(string origin, string destination)
    {
        var visited = new HashSet<string>();
        var distances = new Dictionary<string, int>();
        var previous = new Dictionary<string, string?>();
        var queue = new HashSet<string>(Cities.SelectMany(c => new[] { c.From, c.To }));
        var priorityQueue = new SortedSet<(int cost, string node)>(Comparer<(int , string )>.Create((a, b) =>
        {
            int comp = a.Item1.CompareTo(b.Item1);
            return comp == 0 ? a.Item2.CompareTo(b.Item2) : comp;
        }));

        foreach (var city in queue)
        {
            distances[city] = int.MaxValue;
            previous[city] = null;
        }
        distances[origin] = 0;
        priorityQueue.Add((0, origin));

        while (priorityQueue.Count > 0)
        {
            var (currentCost, currentNode) = priorityQueue.Min;
            priorityQueue.Remove(priorityQueue.Min);

            if (visited.Contains(currentNode))
            {
                continue;
            }

            visited.Add(currentNode);

            if (currentNode == destination)
            {
                break;
            }

            foreach (var neighbor in Cities.Where(c => c.From == currentNode))  // Se buscan los adyacentes, es
                                                                                // decir, todas las ciudades
                                                                                // destino de una ciudad origen
            {
                // NOTA. - Aunque no hay un riesgo directo de ciclos entre dos nodos (A -> B y B -> A no
                // ocurren automáticamente en un grafo dirigido), un grafo dirigido más complejo podría tener
                // otros tipos de ciclos o múltiples caminos hacia el mismo nodo.
                if (visited.Contains(neighbor.To))
                {
                    continue;
                }

                var next = neighbor.To;
                int newDist = distances[currentNode] + neighbor.Cost;

                if (newDist < distances[next])
                {
                    priorityQueue.Remove((distances[next], next));
                    distances[next] = newDist;
                    previous[next] = currentNode;
                    priorityQueue.Add((newDist, next));
                }
            }
        }

        // Si el destino sigue siendo int.MaxValue, no hay ruta
        if (distances[destination] == int.MaxValue)
        {
            return (int.MaxValue, "No hay ruta disponible");
        }

        // Reconstruir la ruta
        var path = new Stack<string>();
        string? step = destination;
        while (step != null)
        {
            path.Push(step);
            step = previous[step];
        }

        return (distances[destination], string.Join(" -> ", path)); // Cada ruta tomada por un pedido, se
                                                                    // devuelve como una cadena de nodos 
                                                                    // separados por " -> "
    }

}

class Connection
{
    public string From { get; }
    public string To { get; }
    public int Cost { get; }

    public Connection(string from, string to, int cost)
    {
        From = from;
        To = to;
        Cost = cost;
    }
}

class Request // Pedido
{
    public string Origin { get; }
    public string Destination { get; }

    public Request(string origin, string destination)
    {
        Origin = origin;
        Destination = destination;
    }
}

