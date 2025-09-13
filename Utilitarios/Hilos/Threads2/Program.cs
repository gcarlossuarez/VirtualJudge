// See https://aka.ms/new-console-template for more information
using System;
using System.Threading;

class Program {
    static void Main() {

        List<Thread> threads = new List<Thread>();
        for (int i = 0; i < 100; i++) 
        {
            Thread myThread = new Thread(new ThreadStart(Work));
            myThread.Start(); // Inicia el hilo
            threads.Add(myThread);
        }
        
        // Espera a que todos los hilos terminen
        foreach (Thread thread in threads) 
        {
            thread.Join();
        }
    }

    private static int _count = 0;
    private static object _lockObject = new object();
    static void Work() {
        lock (_lockObject)
        {
            _count++;
            Console.WriteLine($"Valor actual del contador: {_count}");
            System.Threading.Thread.Sleep(1000);
        }
    }
}


