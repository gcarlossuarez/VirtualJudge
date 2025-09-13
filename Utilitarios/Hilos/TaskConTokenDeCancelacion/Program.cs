// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

class Program {
    static CancellationTokenSource _cts = new CancellationTokenSource();

    static async Task Main() {
        List<Task> tasks = new List<Task>();

        for (int i = 0; i < 100; i++)
        {
            int localI = i; // Crear una copia local de i para evitar condiciones de carrera.
            var task = Task.Run(() => {
                Work(localI);
                //Work(i); // Hasta que se dispara, lel parámetro "i" por referencia, se atropella entra cada llamaa de hilos
            }, _cts.Token);
            tasks.Add(task);
        }

        Console.Write("Presione 's' para detener las tareas:");
        string? detener = Console.ReadLine();
        if(detener?.Trim().ToLower() == "s")
        {
            _cts.Cancel();
            Console.WriteLine("Deteniendo tareas...");
        }

        try {
            // Espera a que todas las tareas terminen
            await Task.WhenAll(tasks);
        } catch (OperationCanceledException) {
            Console.WriteLine("Tareas canceladas.");
        }
    }

    private static object _lockObject = new object();
    static void Work(int i)
    {
        lock (_lockObject)
        {
            if (_cts.Token.IsCancellationRequested) {
                Console.WriteLine($"Tarea con contador: {i}, detenida.");
                return; // Salir temprano si se solicita la cancelación
            }
            Console.WriteLine($"Valor actual del contador i: {i}");
            System.Threading.Thread.Sleep(1000); // Simular un trabajo que lleva tiempo
        }
    }
}