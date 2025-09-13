<Query Kind="Program">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
</Query>

class Program
{
    static void Main()
    {
        // Crear un SortedSet y añadir elementos
        SortedSet<int> numbers = new SortedSet<int> { 5, 1, 10, 7, 3 };

        Console.WriteLine("Elementos en el SortedSet:");
        foreach (int number in numbers)
        {
            Console.WriteLine(number);
        }

        // Intentar añadir un elemento duplicado
        numbers.Add(5);
        Console.WriteLine("\nDespués de intentar añadir el número 5 otra vez:");
        foreach (int number in numbers)
        {
            Console.WriteLine(number);
        }

        // Eliminar un elemento
        numbers.Remove(7);
        Console.WriteLine("\nDespués de eliminar el número 7:");
        foreach (int number in numbers)
        {
            Console.WriteLine(number);
        }

        // Verificar si contiene un elemento
        bool contieneDiez = numbers.Contains(10);
        Console.WriteLine($"\n¿El SortedSet contiene el número 10? {contieneDiez}");
		
		Console.WriteLine($"Elemento mínimo:{numbers.Min}");
		Console.WriteLine($"Elemento máxiimo:{numbers.Max}");
    }
}