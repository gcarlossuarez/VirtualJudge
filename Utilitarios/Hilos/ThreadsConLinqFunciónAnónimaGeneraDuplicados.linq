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
        	Thread myThread = new Thread(
			()=>
			{
				Work(new Data(i, string.Format("Nombre {0}", i)));
			}
			);
			listThreads.Add(myThread);
		
        	myThread.Start();
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
	
	// Comportamiento No Deseado
	// Captura por Referencia: Debido a que i es capturada por referencia, los hilos no capturan el valor de i en el momento en que se crea el 
	// hilo, sino que acceden al valor actual de i en el momento en que realmente se ejecutan. Esto puede llevar a que varios hilos vean el mismo 
	// valor de i si i se incrementa en el bucle antes de que un hilo comience su ejecución.
	// Condiciones de Carrera en la Captura de Variable: Si un hilo se inicia tarde (debido a la planificación del sistema operativo u otros 
	// factores), puede recibir un valor de i que ya ha sido incrementado por el bucle principal, lo que lleva a que múltiples hilos puedan 
	// terminar usando el mismo valor de i.
    static void Work(Data obj)
	{
		string name = obj.Name;
		lock(_lock) // Para evitar condiuciones de carrera sobre la variable estática global utilizada por todos los hilos
		{
			listStrings.Add(name);
		}
		
		Console.WriteLine("Hilo iniciado, mensaje recibido: " + name);
		System.Threading.Thread.Sleep(1000);
    }
	
	static List<string> FindDuplicates(List<string> items)
	{
	    return items.GroupBy(x => x)
	                .Where(g => g.Count() > 1)
	                .Select(gg => gg.Key)
	                .ToList();
	}
}