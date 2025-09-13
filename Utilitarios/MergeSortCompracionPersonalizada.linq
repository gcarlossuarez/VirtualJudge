<Query Kind="Program">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
</Query>

// using System;
// using System.Collections.Generic;

public class DescendingComparer : IComparer<int>
{
    public int Compare(int x, int y)
    {
        // Comparar en orden descendente
        return y.CompareTo(x);
    }
}

public class AscendingComparer : IComparer<int>
{
    public int Compare(int x, int y)
    {
        // Comparar en orden descendente
        return x.CompareTo(y);
    }
}

class Program
{
    static void Main()
    {
        // Tamaño del vector
        int tamaño = 10;
        
        // Crear el vector
        int[] vector = new int[tamaño];
        
        // Crear una instancia de Random
        Random random = new Random();
        
        // Llenar el vector con números aleatorios
        for (int i = 0; i < tamaño; i++)
        {
            vector[i] = random.Next(1, 101); // Genera números entre 1 y 100
        }
        
        // Imprimir el vector
        Console.WriteLine("Números aleatorios en el vector:");
        foreach (int numero in vector)
        {
            Console.Write($"{numero}\t");
        }
		Console.WriteLine();
		
		
		MergeSort(vector, tamaño, new DescendingComparer());
		Console.WriteLine("Números ordenados descendentemente en el vector:");
        Console.WriteLine(string.Join("\t", vector));
        
		MergeSort(vector, tamaño, new AscendingComparer());
		Console.WriteLine("Números ordenados ascenddentemente en el vector:");
        Console.WriteLine(string.Join("\t", vector));
    }
	
	private static void MergeSort<T>(T[] vector, int n, IComparer<T> comparer)
	{
		MergeSort(vector, 0, n - 1, comparer);
	}
	
	private static void MergeSort<T>(T[] vector, int left, int right, IComparer<T> comparer)
	{
		int middle = (left + right) / 2;
		if(left < right)
		{
			MergeSort(vector, left, middle, comparer);
			MergeSort(vector, middle + 1, right, comparer);
			Merge(vector, left, middle, right, comparer);
		}
	}
	
	private static void Merge<T>(T[] vector, int left, int middle, int right, IComparer<T> comparer)
	{
		int i = left, j = middle + 1, k = 0;
		int tamResult = right - left + 1;
		T[] result = new T[tamResult];
		while(i <= middle && j <= right)
		{
			if(comparer.Compare(vector[i], vector[j]) <= 0)
			{
				result[k++] = vector[i++];
			}
			else
			{
				result[k++] = vector[j++];
			}
		}
		while(i <= middle)
		{
			result[k++] = vector[i++];
		}
		
		while(j <= right)
		{
			result[k++] = vector[j++];
		}
		
		int ii = left;
		for(k = 0; k < tamResult; ++k)
		{
			vector[ii++] = result[k];
		}
	}
}