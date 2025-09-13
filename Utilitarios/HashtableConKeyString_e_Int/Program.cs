// See https://aka.ms/new-console-template for more information
using System;
using System.Collections;

class Program
{
    static void Main()
    {
        // Crear un Hashtable
        Hashtable ht = new Hashtable();

        // Agregar elementos al Hashtable
        ht.Add("ID001", "Juan Pérez");
        ht.Add("ID002", "María López");
        ht.Add("ID003", "Luis Morales");
        ht.Add(1, 11);

        // Mostrar un elemento específico basado en la clave
        Console.WriteLine("ID001: " + ht["ID001"]);
        // Acceder a un elemento específico mediante su clave
        string? nombre = ht["001"] as string; // Use null-conditional operator to safely access the value
        Console.WriteLine("El valor asociado a la clave '001' es: " + nombre);


        // Recorrer todos los elementos del Hashtable
        foreach (DictionaryEntry item in ht)
        {
            Console.WriteLine($"{item.Key}: {item.Value}");
        }

        // Verificar si una clave existe en el Hashtable
        if (ht.ContainsKey("ID002"))
        {
            Console.WriteLine("ID002 está presente en el Hashtable.");
        }
    }
}

