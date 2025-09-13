<Query Kind="Program" />

void Main()
{
    List<Empleado> empleados = new List<Empleado>
    {
        new Empleado { Id = 1, Nombre = "Ana", Apellido = "Gonzales", DepartamentoId = 2 },
        new Empleado { Id = 2, Nombre = "Luis", Apellido = "Perez", DepartamentoId = 1 },
        new Empleado { Id = 3, Nombre = "Carlos", Apellido = "Perez", DepartamentoId = 1 },
		new Empleado { Id = 4, Nombre = "Luis", Apellido = "Perez", DepartamentoId = 1 },
		new Empleado { Id = 4, Nombre = "Lucero", Apellido = "Espinoza", DepartamentoId = 1 }
    };
    
    List<Departamento> departamentos = new List<Departamento>
    {
        new Departamento { Id = 1, Nombre = "IT" },
        new Departamento { Id = 2, Nombre = "Recursos Humanos" }
    };
    
	"Con Repetidos".Dump();
	var result = (from e in empleados join d in departamentos on e.DepartamentoId equals d.Id
				where d.Nombre == "IT" && e.Nombre.StartsWith("Lu")
				select new {e.Nombre, e.Apellido}).OrderBy(x=> x.Nombre).ThenByDescending(x=> x.Apellido);
    
    result.Dump();
	
	
	"Sin Repetidos".Dump();
	var resultSinRepetidos = (from e in empleados join d in departamentos on e.DepartamentoId equals d.Id 
							where d.Nombre == "IT" && e.Nombre.StartsWith("Lu")
							select new {e.Nombre, e.Apellido}).Distinct().OrderBy(x=> x.Nombre).ThenByDescending(x=> x.Apellido);
	
    resultSinRepetidos.Dump();
	
	var resultSinRepetidosOrderQuerySintax = (from e in empleados
                  join d in departamentos on e.DepartamentoId equals d.Id
                  select new {name = e.Nombre}).Distinct();

    var orderedResult = from item in resultSinRepetidosOrderQuerySintax orderby item.name select item.name;
	orderedResult.Dump();
	
	"Sin Repetidos, de manera más directa".Dump();
	var orderedResult2 = (from e in empleados join d in departamentos on e.DepartamentoId equals d.Id
					orderby e.Nombre, d.Nombre descending
					select new {e.Nombre, NombreDpto = d.Nombre}).Distinct();
	Console.WriteLine(orderedResult2);
					
	Console.WriteLine("Iterando con un foreach");
	foreach(var e in orderedResult2)
	{
		Console.WriteLine(e.Nombre + ".." + e.NombreDpto);
	}
}

public class Empleado
{
    public int Id { get; set; }
    public string Nombre { get; set; }
	public string Apellido {get; set; }
    public int DepartamentoId { get; set; }
	
}

public class Departamento
{
    public int Id { get; set; }
    public string Nombre { get; set; }
}