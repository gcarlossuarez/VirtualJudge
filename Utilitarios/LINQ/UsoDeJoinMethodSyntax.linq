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
    
	// Sintaxis de Método (Methos Syntax)
    var consulta = empleados.Join(departamentos, 
                                  e => e.DepartamentoId, 
                                  d => d.Id, 
                                  (e, d) => new { Empleado = e.Nombre, Departamento = d.Nombre });
    
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

