// See https://aka.ms/new-console-template for more information
using System;
using System.Threading;

class Program {
    static void Main() {
        Thread myThread = new Thread(new ThreadStart(Work));
        myThread.Start(); // Inicia el hilo
    }

    static void Work() {
        Console.WriteLine("Hilo iniciado");
    }
}

