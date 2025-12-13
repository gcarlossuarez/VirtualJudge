// File: Validator.cs 
// Uso: dotnet run -- <input> <expected> <actual>
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Validator
{
    static bool _debugMode = false;

    static void Main(string[] args)
    {
        if(_debugMode)
        {
            //args = new string[]{@"D:\Universidad Católica Boliviana\Semestre2-2025\Banco de preguntas para el Juez Virtual\6022\6022\IN\datos001.txt", @"D:\Universidad Católica Boliviana\Semestre2-2025\Banco de preguntas para el Juez Virtual\6022\6022\OUT\Output_datos001.txt", @"D:\Universidad Católica Boliviana\Semestre2-2025\Banco de preguntas para el Juez Virtual\6022\6022\TestOutput/Output_datos001.txt"};
            args = new string[]{@"D:\Universidad Católica Boliviana\Semestre2-2025\Banco de preguntas para el Juez Virtual\6022\6022\IN\datos002.txt", @"D:\Universidad Católica Boliviana\Semestre2-2025\Banco de preguntas para el Juez Virtual\6022\6022\OUT\Output_datos002.txt", @"D:\Universidad Católica Boliviana\Semestre2-2025\Banco de preguntas para el Juez Virtual\6022\6022\TestOutput/Output_datos002.txt"};
        }
        try
        {
            if (args.Length < 3)
            {
                Error("Argumentos insuficientes (uso: Validator <input> <expected> <actual>)");
            }

            string inputPath = args[0];
            string expectedPath = args[1];
            string actualPath = args[2];

            if (!File.Exists(inputPath)) Error("No existe el archivo input.");
            if (!File.Exists(expectedPath)) Error("No existe el archivo esperado.");
            if (!File.Exists(actualPath)) Error("No existe el archivo generado por el estudiante.");

            // Leer archivos
            string[] expectedLines = File.ReadAllLines(expectedPath)
                                         .Select(l => l.Trim())
                                         .Where(l => !string.IsNullOrEmpty(l) && !string.IsNullOrWhiteSpace(l))
                                         .ToArray();

            string[] actualLines = File.ReadAllLines(actualPath)
                                       .Select(l => l.Trim())
                                       //.Where(l => !string.IsNullOrEmpty(l) && !string.IsNullOrWhiteSpace(l))
                                       .ToArray();
            // Elimina líneas vacías o solo con espacios en blanco al final del array
            while (actualLines.Length > 0 && string.IsNullOrWhiteSpace(actualLines[actualLines.Length - 1]))
            {
                Array.Resize(ref actualLines, actualLines.Length - 1);
            }


            if (actualLines.Length == 0)
            {
                Error("Salida incompleta.");
            }

            // -----------------------------------------------
            // 1) Validar primera línea
            // -----------------------------------------------
            string expectedFirstLine = expectedLines[0];
            string actualFirstLine = actualLines[0];

            if (expectedFirstLine != actualFirstLine)
            {
                Error("Primera línea no coincide con la respuesta esperada");
            }

            if (expectedFirstLine == "Sin entregas posibles" && actualLines.Length > 1)
            {
                Error("Si la respuesta es \"Sin entregas posibles\", no deben existir más líneas.");
            }

            if (expectedFirstLine == "Sin entregas posibles" && actualFirstLine == expectedFirstLine)
            {
                Console.WriteLine("OK");
                return;
            }

            // -----------------------------------------------
            // 2) Validar Costo Total
            // -----------------------------------------------
            // Extraer Costo Total
            int totalCost = ExtractValue(actualLines, "Costo total de las rutas", 1);
            //Console.WriteLine($"totalCost:{totalCost}");
            string[] inputLines = File.ReadAllLines(inputPath).Where(l=> !string.IsNullOrEmpty(l) && !string.IsNullOrWhiteSpace(l)).ToArray();
            int budget = int.Parse(inputLines[0].Split(':')[1].Trim());
			      if(budget < totalCost)
            {
               Error($"Costo total calculado por el estudiante:{totalCost}, no puede ser mayor al presupuesto:{budget}.");
            }


            // -----------------------------------------------
            // 3) Validar Ganancia Máxima Obtenida
            // -----------------------------------------------
            // Extraer Costo Total
            int maxProfit = ExtractValue(actualLines, "Ganancia máxima obtenida", 1);
			      //maxProfit.Dump("maxProfit");

            // Validar ganancia máxima
            if (maxProfit != totalCost * 2)
            {
                Error($"[ERROR] Ganancia incorrecta. Esperado: {totalCost * 2}, Encontrado: {maxProfit}");
            }


            // -----------------------------------------------
            // 4) Validar sumatoria de los Costos Totales
            // -----------------------------------------------
            // Validar la sumatoria del costo total de las rutas seleccionadas
            var countrySections = actualLines.Where(l => l.Contains("Costo total:")).ToArray();
			      //countrySections.Dump();
            int computedCost = 0;
            foreach (var section in countrySections)
            {
                int routeCost = ExtractValue(section, "Costo total", 2);
				        //routeCost.Dump("Route Cost");
                computedCost += routeCost;
            }

            if (computedCost != totalCost)
            {
                Error($"[ERROR] Costo total incorrecto. Esperado: {totalCost}, Calculado: {computedCost}");
            }


            // -----------------------------------------------
            // 5) Validar sumatoria de las Ganancias
            // -----------------------------------------------
            // Validar la sumatoria del costo total de las rutas seleccionadas
            var profitSections = actualLines.Where(l => l.Contains("Ganancia:")).ToArray();
			      //countrySections.Dump();
            int computedProfit = 0;
            foreach (var section in profitSections)
            {
                int routeProfit = ExtractValue(section, "Ganancia", 1);
				        //routeCost.Dump("Route Profit");
                computedProfit += routeProfit;
            }

            if (computedProfit != maxProfit)
            {
                Error($"[ERROR] Ganancia total incorrecta. Esperada: {maxProfit}, Calculado: {computedProfit}");
            }
            
            
            // -----------------------------------------------
            // 6) Validar países
            // -----------------------------------------------
            const string PAIS = "País ";
            string[] countriesInActualLines = actualLines.Where(l=> l.Contains(PAIS) && l.Split(PAIS).Length > 0).Select(l=> l.Split(PAIS)[1].Replace(":", "").Trim()).ToArray();
            //foreach(var c in countriesInActualLines)
            //{
            //    Console.WriteLine($"País:{c}");
            //}

            string[] countriesInInputFile = inputLines.Where(l=> l.Contains(PAIS) && l.Split(PAIS).Length > 0).Select(l=> l.Split(PAIS)[1].Replace(":", "").Trim()).ToArray();
            //foreach(var c in countriesInInputFile)
            //{
            //    Console.WriteLine($"País:{c}");
            //}
            var countryNotFound = countriesInActualLines.FirstOrDefault(c=> !countriesInInputFile.Contains(c));
            if(countryNotFound != null)
            {
                Error($"País:{countryNotFound} no encontrado en el input correspondiente.");
            }


            // -----------------------------------------------
            // 7) Validar rutas y conexiones
            // -----------------------------------------------
            try
            {
                // Extraer las conexiones y rutas por país
                var inputConnectionsByCountry = ExtractConnectionsByCountry(inputLines);
                var outputRoutesByCountry = ExtractRoutesByCountry(actualLines);
			
			          //outputRoutesByCountry.Dump("outputRoutesByCountry");
			          //inputConnectionsByCountry.Dump("inputConnectionsByCountry");

                // Validar cada país
                foreach (var country in outputRoutesByCountry.Keys)
                {
                    if (!inputConnectionsByCountry.ContainsKey(country))
                    {
                        throw new Exception($"País '{country}' no encontrado en el archivo de entrada.");
                    }

                    ValidateRoutesForCountry(country, outputRoutesByCountry[country], inputConnectionsByCountry[country]);
                }

            }
            catch (Exception ex)
            {
                Error($"[ERROR] Fallo al validar la salida generada por el estudiante: {ex.Message}");
            }

            Console.WriteLine("OK");
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.Message);
            Environment.Exit(1);
        }
        finally
        {
           ShowConsoleMessageYesNo();
        }
    }
    static void ShowConsoleMessageYesNo()
    {
        if(_debugMode)
        {
            Console.Write("Pulse ENTER, para salir...");
            //System.Threading.Thread.Sleep(3000);
            Console.ReadLine();
        }
    }

    static void Error(string msg)
    {
        Console.WriteLine("Resultado incorrecto: " + msg);
        ShowConsoleMessageYesNo();
        Environment.Exit(1);
    }

    static int ExtractValue(string[] lines, string key, int indexValueNeeded)
    {
        var line = lines.FirstOrDefault(l => l.Contains(key));
        if (line == null)
            throw new Exception($"No se encontró la clave '{key}' en el archivo.");

        var parts = line.Split(':');
        return int.Parse(parts[indexValueNeeded].Trim());
    }

    static int ExtractValue(string line, string key, int indexValueNeeded)
    {
        var parts = line.Split(':');
        return int.Parse(parts[indexValueNeeded].Replace(")", "").Trim());
    }

    //////////////////////////////////////////////////////


    static Dictionary<int, Dictionary<(string, string), int>> ExtractConnectionsByCountry(string[] inputLines)
    {
        var connectionsByCountry = new Dictionary<int, Dictionary<(string, string), int>>();
        int currentCountry = 0;
        bool inConnections = false;

        foreach (var line in inputLines)
        {
            if (line.StartsWith("País"))
            {
                currentCountry = int.Parse(line.Split(' ')[1].Replace(":", "").Trim());
                connectionsByCountry[currentCountry] = new Dictionary<(string, string), int>();
                continue;
            }

            if (line.Contains("Pedidos:"))
            {
                inConnections = false;
                continue;
            }

            if (inConnections && line.Contains(";"))
            {
                var parts = line.Split(';');
                string from = parts[0].Trim();
                string to = parts[1].Trim();
                int cost = int.Parse(parts[2].Trim());
                connectionsByCountry[currentCountry][(from, to)] = cost;
                connectionsByCountry[currentCountry][(to, from)] = cost; // Para grafos no dirigidos
            }

            if (line.Contains("Ciudades:"))
            {
                inConnections = true;
            }
        }

		    //connectionsByCountry.Dump("connectionsByCountry");
        return connectionsByCountry;
    }

    static Dictionary<int, List<string>> ExtractRoutesByCountry(string[] outputLines)
    {
		    //outputLines.Dump("outputLines");
        var routesByCountry = new Dictionary<int, List<string>>();
        int currentCountry = 0;

        foreach (var line in outputLines)
        {
            if (line.Contains("País "))
            {
				        //line.Split(' ')[1].Replace(":", "").Trim().Dump("line.Split(' ')[2].Replace(...).Trim()");
				        //line.Split(' ')[2].Replace(":", "").Trim().Dump("line.Split(' ')[2].Replace(...).Trim()");
                currentCountry = int.Parse(line.Split(' ')[2].Replace(":", "").Trim());
                routesByCountry[currentCountry] = new List<string>();
                continue;
            }

            if (line.Contains("Rutas utilizadas:"))
            {
                routesByCountry[currentCountry].Add(line);
            }
        }

        return routesByCountry;
    }

    static void ValidateRoutesForCountry(int country, List<string> routes, Dictionary<(string, string), int> connections)
    {
        foreach (var route in routes)
        {
            ValidateRoute(route, connections);
        }
    }


    static Dictionary<(string, string), int> ExtractConnections(string[] inputLines)
    {
        var connections = new Dictionary<(string, string), int>();
        bool inConnections = false;

        foreach (var line in inputLines)
        {
            if (line.Contains("Pedidos:"))
                break;

            if (inConnections && line.Contains(";"))
            {
                var parts = line.Split(';');
                string from = parts[0].Trim();
                string to = parts[1].Trim();
                int cost = int.Parse(parts[2].Trim());
                connections[(from, to)] = cost;
                connections[(to, from)] = cost; // Para grafos no dirigidos
            }

            if (line.Contains("Ciudades:"))
                inConnections = true;
        }
		    //connections.Dump();
		    //throw new Exception("Saliendo para debugger");
        return connections;
    }

    static void ValidateRoute(string routeLine, Dictionary<(string, string), int> connections)
    {
		    //routeLine.Dump("RoutLine");
        // Extraer ruta y costo total
        var parts = routeLine.Split(':'); // Rutas utilizadas: X -> Y -> Z (Costo total: 15) // Se divide en "Rutas utilizadas", "X -> Y -> Z (Costo total" y "15)"
        string route = parts[1].Split('(')[0].Trim(); // Ruta como "X -> Y -> Z"
		    //route.Dump("route"); 
		    string strReportedCost = routeLine.Split('(')[1].Replace("Costo total:", "").Replace(")", "").Trim(); // Rutas utilizadas: X -> Y -> Z (Costo total: 15) // Se divide en "Rutas utilizadas: X -> Y -> Z (Costo total" y "Costo total: 15)"
		    //strReportedCost.Dump("strReportedCost");
        int reportedCost = int.Parse(strReportedCost );
		    //reportedCost.Dump("reportedCost"); return;

        // Calcular el costo real
        //var nodes = route.Split("->").Select(x => x.Trim()).ToArray();
		    route = route.Replace("->", ";");
		    //route.Dump();
		    var pedidos = route.Split(',').Select(x=> x.Trim());
		    //pedidos.Dump("Pedidos");
		    int calculatedCost = 0;
		    foreach(string pedido in pedidos)
		    {
			    var nodes = pedido.Replace("->", ";").Split(';').Select(x => x.Trim()).ToArray();
			    //nodes.Dump("nodes");
	        for (int i = 0; i < nodes.Length - 1; i++)
	        {
	            var key = (nodes[i], nodes[i + 1]);
				      //key.Dump("key");
	            if (!connections.ContainsKey(key))
	            {
					        //connections.Dump("connections");
	                throw new Exception($"Conexión no encontrada: {nodes[i]} -> {nodes[i + 1]}");
	            }
	            calculatedCost += connections[key];
	        }

		    }
		
        // Validar el costo
        if (calculatedCost != reportedCost)
        {
            throw new Exception($"Costo incorrecto en ruta '{route}'. Reportado: {reportedCost}, Calculado: {calculatedCost}");
        }
    }
}


