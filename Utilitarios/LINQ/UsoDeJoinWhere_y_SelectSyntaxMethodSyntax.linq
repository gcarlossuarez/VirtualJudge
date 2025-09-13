<Query Kind="Program" />

void Main()
{
    List<Empleado> empleados = new List<Empleado>
    {
        new Empleado { Id = 1, Nombre = "Ana", DepartamentoId = 2 },
        new Empleado { Id = 2, Nombre = "Luis", DepartamentoId = 1 },
        new Empleado { Id = 3, Nombre = "Carlos", DepartamentoId = 1 }
    };

    List<Departamento> departamentos = new List<Departamento>
    {
        new Departamento { Id = 1, Nombre = "IT" },
        new Departamento { Id = 2, Nombre = "Recursos Humanos" }
    };

    var consulta = empleados.Join(departamentos,
                                  e => e.DepartamentoId,
                                  d => d.Id,
                                  (e, d) => new { Empleado = e, Departamento = d })
                            .Where(x => x.Departamento.Nombre == "IT")
                            .Select(x => new { Empleado = x.Empleado.Nombre, Departamento = x.Departamento.Nombre });

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

