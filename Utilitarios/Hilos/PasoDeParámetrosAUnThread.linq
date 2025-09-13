<Query Kind="Program">
  <Namespace>System</Namespace>
  <Namespace>System.Threading</Namespace>
</Query>


class Program {
    static void Main() {
        // Creando el hilo con ParameterizedThreadStart
        Thread myThread = new Thread(new ParameterizedThreadStart(Work));

        // Inicia el hilo y envía un parámetro
        myThread.Start("Mensaje desde Main");
    }

    // Método modificado para aceptar un parámetro
    static void Work(object obj) {
        string message = (string)obj;  // Castear el objeto a su tipo original
        Console.WriteLine("Hilo iniciado, mensaje recibido: " + message);
    }
}

