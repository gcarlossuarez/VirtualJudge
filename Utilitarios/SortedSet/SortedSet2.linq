<Query Kind="Program">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
</Query>

class Program
{
    static void Main()
    {
        // Crear dos SortedSets con algunos elementos
        SortedSet<int> setA = new SortedSet<int> { 1, 2, 3, 4, 5 };
        SortedSet<int> setB = new SortedSet<int> { 4, 5, 6, 7, 8 };

        Console.WriteLine("Conjunto A:");
        foreach (int elemento in setA)
        {
            Console.WriteLine(elemento);
        }

        Console.WriteLine("\nConjunto B:");
        foreach (int element in setB)
        {
            Console.WriteLine(element);
        }

        // Realizar la unión de A y B
        SortedSet<int> union = new SortedSet<int>(setA);
        union.UnionWith(setB);
        Console.WriteLine("\nUnión de A y B:");
        foreach (int element in union)
        {
            Console.WriteLine(element);
        }

        // Realizar la intersección de A y B
        SortedSet<int> interseccion = new SortedSet<int>(setA);
        interseccion.IntersectWith(setB);
        Console.WriteLine("\nIntersección de A y B:");
        foreach (int element in interseccion)
        {
            Console.WriteLine(element);
        }
    }
}