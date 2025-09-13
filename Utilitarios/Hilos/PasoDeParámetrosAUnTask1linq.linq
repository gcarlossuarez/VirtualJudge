<Query Kind="Program">
  <Namespace>System</Namespace>
  <Namespace>System.Threading</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

// using System.Threading.Tasks; // Para poder utilizar Tasks

class Program {
    static void Main() {
        // Crear y ejecutar la tarea
        Task myTask = Task.Run(() =>
        {
            // Crear el objeto Data dentro de la tarea
            Data data = new Data()
            {
                Id = 300,
                Name = "Marcela"
            };
            Work(data);
        });

        // Esperar a que la tarea se complete (opcional, dependiendo del contexto)
        myTask.Wait();
    }

    // Método para usar los datos
    static void Work(Data data)
	{
        Console.WriteLine($"Id:{data.Id} Name:{data.Name}"); // Interpolación de cadenas
    }
}

class Data {
    public int Id { get; set; }
    public string Name { get; set; }
}
