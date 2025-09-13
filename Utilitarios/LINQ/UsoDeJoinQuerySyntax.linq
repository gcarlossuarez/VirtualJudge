<Query Kind="Program" />

void Main()
{
    List<Empleado> empleados = new List<Empleado>
    {
        new Empleado { Id = 1, Nombre = "Ana", DepartamentoId = 2 },
        new Empleado { Id = 2, Nombre = "Luis", DepartamentoId = 1 }
    };
    
    List<Departamento> departamentos = new List<Departamento>
    {
        new Departamento { Id = 1, Nombre = "IT" },
        new Departamento { Id = 2, Nombre = "Recursos Humanos" }
    };
    
	// Sintaxis de consulta  (Query Syntax). Similar a SQL y más apta para las persons familiarizadas con SQL.
    var consulta = from e in empleados
					join d in departamentos
					on e.DepartamentoId equals d.Id
					select new { Empleado = e.Nombre, Departamento = d.Nombre };
    
    consulta.Dump();
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

