<Query Kind="Program">
  <Namespace>System</Namespace>
  <Namespace>System.Text</Namespace>
  <Namespace>System.Threading</Namespace>
</Query>


class Program {
    static void Main() 
	{
        // Creando el hilo con ParameterizedThreadStart
        Thread myThread = new Thread(new ParameterizedThreadStart(Work));

        // Inicia el hilo y envía un parámetro
		Data data = new Data()
		{
			Id = 200,
			Name = "Carlos"
		};
        myThread.Start(data);
    }

    // Método modificado para aceptar un parámetro
    static void Work(object obj) {
		Data data = (Data)obj;
		
        string name = (string)((Data)obj).Name;  // Castear el objeto a su tipo original
        Console.WriteLine("Hilo iniciado, mensaje recibido: " + name);
		int id = data.Id;
		name = data.Name;  
		Console.WriteLine("Id:" + data.Id + " Name:" + data.Name); // Concatenación de cadens. Ineficiente si so muchos valores a concatenar y son muchas llamadas. Para esos casos, copnsidere utilizar StringBuilder
		
		StringBuilder sb = new StringBuilder(); // Necesita "using System.Text;", par apoder utilizar StringBuilder
		sb.Append("Id:");
		sb.Append(data.Id.ToString());
		sb.Append(" Name:");
		sb.Append(data.Name);
		Console.WriteLine(sb.ToString());
		
		sb.Clear(); // Limpia el StringBuilder
		sb.Append("Id:");
		sb.AppendLine(data.Id.ToString());
		sb.Append(" Name:");
		sb.Append(data.Name);
		Console.WriteLine(sb.ToString());

		Console.WriteLine($"Id:{data.Id} Name:{data.Name}"); // Interpolación de cadenas. La interpolación de cadenas fue introducida en C# 6.0, que fue lanzada junto con Visual Studio 2015.
		Console.WriteLine("Id:{0} Name:{1}", data.Id, data.Name); // Utiliza String.Format implícitamente. Esta es una forma más antigua de formatear cadenas en C#, donde cada {n} es un marcador de posición que se reemplaza con los argumentos proporcionados después de la cadena. En este caso, {0} se reemplaza por data.Id y {1} por data.Name. Esta sintaxis ha estado disponible desde las primeras versiones de C#.
		// NOTA.- Console.WritLine es polimórfica
		/*
		Ejemplo de Polimorfismo
		El polimorfismo en este contexto se refiere a la capacidad de Console.WriteLine para realizar "muchas formas" de operaciones de impresión, dependiendo del 
		tipo y número de argumentos pasados. Por ejemplo:

		Console.WriteLine("Hello, World!");  // Usa la sobrecarga de cadena
		Console.WriteLine(123);              // Usa la sobrecarga de entero
		Console.WriteLine(3.14);             // Usa la sobrecarga de double
		Console.WriteLine("Age: {0}", 25);   // Usa la sobrecarga con formato de cadena
		Cada llamada a Console.WriteLine utiliza una versión diferente del método, adecuada al tipo y cantidad de argumentos proporcionados, lo cual es un claro 
		ejemplo de polimorfismo a través de sobrecargas de método. Esto facilita la escritura de código flexible y fácil de leer, sin tener que preocuparse por 
		detalles de bajo nivel de la representación de los datos al imprimirlos.
		*/
    }
}

class Data
{
	public int Id {get; set;}
	public string Name {get; set;}
}

