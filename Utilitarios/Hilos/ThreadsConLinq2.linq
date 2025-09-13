<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.dll</Reference>
  <Namespace>System</Namespace>
  <Namespace>System.Text</Namespace>
  <Namespace>System.Threading</Namespace>
</Query>

//using System.Linq;

class Data
{
	public int Id {get; set;}
	public string Name {get; set; }
	
	public Data(int id, string name)
	{
		Id = id;
		Name = name;
	}
}

class Program 
{
	static List<string> listStrings = new List<string>();
    static void Main() 
	{
		const int n = 100;
		List<Thread> listThreads = new List<Thread>();
		for(int i = 0; i < n; ++i)
		{
			// Creando el hilo con ParameterizedThreadStart
        	Thread myThread = new Thread(new ParameterizedThreadStart(Work));
			listThreads.Add(myThread);
			

        	myThread.Start(new Data(i, string.Format("Nombre {0}", i)));
		}
		
		for(int j = 0; j < n; ++j)
		{
			listThreads[j].Join();
		}
				
		var duplicados = FindDuplicates(listStrings);
		Console.WriteLine($"Duplicados:");
		foreach(var d in duplicados)
		{
			Console.WriteLine($"{d}");
		}

    }

	private static object _lock = new object();
	
    // Método modificado para aceptar un parámetro
	// NOTA.- Cada hilo recibe una copia por referencia de un objeto Data distinto, porque para cada hilo se está creando un nuevo objeto Data.
	// Este objeto se construye con un Id único y un Name único antes de ser pasado al hilo. Dado que cada objeto Data es independiente de los 
	// demás, no hay problema directo de condiciones de carrera en cuanto a la manipulación de estos objetos Data en sí.
    static void Work(object obj)
	{
		string name = (string)((Data)obj).Name;
		lock(_lock) // Para evitar condiuciones de carrera sobre la variable estática global utilizada por todos los hilos
		{
			listStrings.Add(name);
		}
		
		Console.WriteLine("Hilo iniciado, mensaje recibido: " + name);
		//System.Threading.Thread.Sleep(500);
    }
	
	static List<string> FindDuplicates(List<string> items)
	{
	    return items.GroupBy(x => x)
	                .Where(g => g.Count() > 1)
	                .Select(gg => gg.Key)
	                .ToList();
	}
}