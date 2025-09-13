<Query Kind="Program" />

public class Program
{
	public static void Main()
	{
		List<Person> listPersons = new List<Person>();
		listPersons.Add(new Person(10, "Juan"));
		listPersons.Add(new Person(12, "Pedro", new DateTime(2005, 9, 13)));
		listPersons.Add(new Person(9, "Juan", new DateTime(2007, 2, 20)));
		listPersons.Add(new Person(19, "El horroroso chico Pepe", new DateTime(1970, 4, 4)));
		
		// Declaración a nivel de ámbito. Vive y muere, dentro del ámbito
		{
			var result = listPersons.Where(x=> x.Name.Equals("Juan"));
			foreach(var p in result)
			{
				string x = p.ToString();
				Console.WriteLine($"Person==>{x}");
			}
		}
		
		// Declaración a nivel de ámbito. Vive y muere, dentro del ámbito
		{
			// Objeto anónimo: Los objetos anónimos en C# se crean utilizando la palabra clave new seguida de un inicializador de objeto. No tienen un tipo explícito definido en el código fuente, pero el compilador genera un tipo para ellos en tiempo de compilación
			// 
			var result = listPersons.Where(x=> x.Name.Equals("Juan")).Select(x=> new {x.Id, x.DateOfBirth});
			
			// En LINQ, cuando usas Select, debes proyectar a un solo objeto o tipo. Para proyectar más de un objeto, debes encapsular las propiedades en un objeto anónimo
			//var result = listPersons.Where(x=> x.Name.Equals("Juan")).Select(x=> x.Id, x.DateOfBirth);
			
			foreach(var p in result) // Crea un objeto an´nimo, para la repuesta. Por eso, lo recupera con new
			{
				string x = p.ToString(); 	// Como ToString() no fue sobreescrito para el objeto anónimo, muestra el ToString por defecto, heredado de object
											// Método ToString(): Cuando llamas a ToString() en un objeto anónimo, se utiliza la implementación predeterminada de ToString() de la clase object, que devuelve una cadena que representa el tipo del objeto. No puedes sobrescribir ToString() en un objeto anónimo porque no puedes definir métodos en ellos
				Console.WriteLine($"Person==>{x}");
			}
		}
		
		// https://learn.microsoft.com/es-es/dotnet/csharp/fundamentals/types/anonymous-types		
		var v = new { Amount = 108, Message = "Hello" };
		Console.WriteLine(v.Amount + ".. " + v.Message);
	}
}

class Person
{
	public int Id {get; set;}
	public String Name {get; set;}
	public DateTime DateOfBirth {get; set;}
	
	public Person(int id, string name, DateTime dateOfBirth = default(DateTime))
    {
        Id = id;
        Name = name;
        DateOfBirth = dateOfBirth == default(DateTime) ? new DateTime(2000, 1, 1) : dateOfBirth;
    }
	
	
	public override string ToString() // Sobreescritura del métido ToString() hereado de la clase object
	{
		return $"Id:{this.Id}, Name:{this.Name}";
	}
}