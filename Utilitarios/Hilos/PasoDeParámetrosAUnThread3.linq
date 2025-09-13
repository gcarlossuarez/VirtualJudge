<Query Kind="Program">
  <Namespace>System</Namespace>
  <Namespace>System.Threading</Namespace>
</Query>


class Program {
    static void Main() {
        // Expresión lambda para crear una función anónima que encapsula la creación de una instancia de Data y la llamada al método Work con esta instancia como 
		// argumento. Esto es especialmente útil cuando deseas pasar múltiples datos o un objeto configurado de manera específica a un hilo, pero la firma del 
		// delegado del hilo no admite directamente parámetros adicionales, como es el caso con ThreadStart.
		// () => { ... } define una expresión lambda sin parámetros. Dentro de las llaves, se define el cuerpo de la función anónima que se ejecutará cuando el 
		// hilo inicie.
        Thread myThread = new Thread(() => 
		{
			Data data = new Data()
			{
				Id = 300,
				Name = "Marcela"
			};
			Work(data);
		}
		);

        // Inicia el hilo y envía un parámetro
        myThread.Start();
    }

    // Método modificado para aceptar un parámetro
    static void Work(Data data)
	{
        Console.WriteLine($"Id:{data.Id} Name:{data.Name}"); // Interpolación de cadenas. La interpolación de cadenas fue introducida en C# 6.0, que fue lanzada junto con Visual Studio 2015.
		Console.WriteLine("Id:{0} Name:{1}", data.Id, data.Name); // Utiliza String.Format implícitamente
    }
}


class Data
{
	public int Id {get; set;}
	public string Name {get; set;}
}

