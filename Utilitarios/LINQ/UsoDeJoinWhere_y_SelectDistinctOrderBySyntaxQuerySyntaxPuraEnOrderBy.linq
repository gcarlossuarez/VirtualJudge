<Query Kind="Program" />

void Main()
{
    List<Empleado> empleados = new List<Empleado>
    {
        new Empleado { Id = 1, Nombre = "Ana", DepartamentoId = 2 },
        new Empleado { Id = 2, Nombre = "Luis", DepartamentoId = 1 },
        new Empleado { Id = 3, Nombre = "Carlos", DepartamentoId = 1 },
        new Empleado { Id = 4, Nombre = "Luis", DepartamentoId = 1 }
    };

    List<Departamento> departamentos = new List<Departamento>
    {
        new Departamento { Id = 1, Nombre = "IT" },
        new Departamento { Id = 2, Nombre = "Recursos Humanos" }
    };

	{
	    var result = (from e in empleados
	                  join d in departamentos on e.DepartamentoId equals d.Id
	                  select e.Nombre).Distinct();

	    var orderedResult = from name in result orderby name select name;

	    orderedResult.Dump();
	}
	
	{
		var result = (from e in empleados
	                  join d in departamentos on e.DepartamentoId equals d.Id
	                  select new { e.Nombre, e.Id }).Distinct();

	    var orderedResult = from item in result
	                        orderby item.Nombre ascending, item.Id descending
	                        select item;

	    orderedResult.Dump();
	}
}


public class Empleado
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public int DepartamentoId { get; set; }
}

public class Departamento
{
    public int Id { get; set; }
    public string Nombre { get; set; }
}
