// See https://aka.ms/new-console-template for more information
using System;
using System.Threading;

class Program {
    static void Main() {

        List<Thread> threads = new List<Thread>();
        // NOTA.- Condición de carrera.
        // La repetición de valores entre distintos hilos se debe a las condiciones de carrera. Esto ocurre porque varios 
        // hilos intentan acceder y modificar la misma variable (i en este caso) simultáneamente sin sincronización adecuada.
        // El problema específico en este código es que la variable i se captura por referencia en la expresión lambda que 
        // se define para crear el hilo. Debido a esto, todos los hilos creados en el bucle for pueden terminar usando el mismo
        // valor de i, el cual cambia con cada iteración del bucle. 
        // Para corregir este problema, se podría crear una copia local de i dentro del bucle y pasar esa copia al hilo, 
        // asegurándose, así, que cada hilo tenga su propia instancia de la variable, y evitar la condición de carrera.
        // for (int i = 0; i < 100; i++) 
        //  {
        //      Thread myThread = new Thread(()=>{
        //          Work(i);
        //      });
        //      myThread.Start(); // Inicia el hilo. condiución de carrera ocurre aquí; porque, mientras el hilo se está creando, el valor de i puede cambiar, dentro de la siguiente iteración del ciclo for. Mientras el hilo se dispara, el ciclo for continúa trabajando y altera el valor de i.
        //      threads.Add(myThread);
        // }
        for (int i = 0; i < 100; i++) 
        {
            int localI = i; // Crear una copia local de i. Cuando el hilo se dispare, cada hilo tendrá su propia copia de i.
            Thread myThread = new Thread(()=>{
                Work(localI);
            });
            myThread.Start(); // Inicia el hilo
            threads.Add(myThread);
        }
        
        // Espera a que todos los hilos terminen
        foreach (Thread thread in threads) 
        {
            thread.Join();
        }
    }

    
    private static object _lockObject = new object();
    static void Work(int i) {
        lock (_lockObject)
        {
            Console.WriteLine($"Valor actual del contador i: {i}");
            System.Threading.Thread.Sleep(1000);
        }
    }
}
