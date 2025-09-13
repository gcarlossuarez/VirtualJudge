<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.dll</Reference>
  <Namespace>System</Namespace>
  <Namespace>System.Text</Namespace>
  <Namespace>System.Threading</Namespace>
</Query>

//using System.Linq;

class Program 
{
	static List<int> listIntegers = new List<int>();
    static void Main() 
	{
		const int n = 100;
		List<Thread> listThreads = new List<Thread>();
		for(int i = 0; i < n; ++i)
		{
			// Creando el hilo con ParameterizedThreadStart
        	Thread myThread = new Thread(new ParameterizedThreadStart(Work));
			listThreads.Add(myThread);

        	myThread.Start(i);
		}
		
		for(int j = 0; j < n; ++j)
		{
			listThreads[j].Join();
		}
		// Para provocar repetidos y analizar Group by con Linq
		listIntegers.Add(10);
		listIntegers.Add(4);
		
		var duplicados = FindDuplicates(listIntegers);
		Console.WriteLine($"Duplicados:");
		foreach(var d in duplicados)
		{
			Console.WriteLine($"{d}");
		}

    }

	private static object _lock = new object();
    // Método modificado para aceptar un parámetro
    static void Work(object obj)
	{
		int i = (int)obj;
		lock(_lock) // Para evitar condiuciones de carrera sobre la variable estática global utilizada por todos los hilos
		{
			listIntegers.Add(i);
		}
		
        Console.WriteLine("Hilo iniciado, mensaje recibido: " + i);
    }
	
	static List<int> FindDuplicates(List<int> items)
	{
	    return items.GroupBy(x => x)
	                .Where(group => group.Count() > 1)
	                .Select(group => group.Key)
	                .ToList();
	}
}