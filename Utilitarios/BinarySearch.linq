<Query Kind="Program">
  <Namespace>System</Namespace>
</Query>

// using System;

class Program
{
	private const int NULL = -1;
	
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
        /*
		foreach (int numero in vector)
        {
            Console.Write($"{numero}\t");
        }
		Console.WriteLine();
		*/
		Console.WriteLine(String.Join("\t", vector));
		
		
		QuickSort(vector, tamaño);
		Console.WriteLine("Números ordenados en el vector:");
		Console.WriteLine(String.Join("\t", vector));
		Console.WriteLine($"{BinarySearch(vector, tamaño, vector[random.Next(0, tamaño - 1)])}");
		Console.WriteLine($"{BinarySearch(vector, tamaño, -vector[random.Next(0, tamaño - 1)])}");
		Console.WriteLine($"{BinarySearch(vector, tamaño, 2000)}");
    }
	
	static int BinarySearch(int[] vector, int n, int searched)
	{
		int pos = NULL;
		int left = 0, right = n - 1;
		
		while(left <= right && pos == NULL)
		{
			int middle = (left + right) / 2;
			if(vector[middle] < searched)
			{
				left = middle + 1;
			}
			else if(vector[middle] > searched)
			{
				right = middle - 1;
			}
			else
			{
				pos = middle;
			}
		}
		
		return pos;
	}
	
	private static void QuickSort(int[] vector, int n)
	{
		QuickSort(vector, 0, n - 1);
	}
	
	private static void QuickSort(int[] vector, int left, int right)
	{
		int middle = (left + right) / 2;
		int pivot = vector[middle];
		int i = left;
		int j = right;
		while(i <= j)
		{
			while(vector[i] < pivot)
			{
				++i;
			}
			while(vector[j] > pivot)
			{
				--j;
			}
			
			if(i <= j)
			{
				int aux = vector[i];
				vector[i] = vector[j];
				vector[j] = aux;
				++i;
				--j;
			}
		}
		
		if(left < j)
		{
			QuickSort(vector, left, j);
		}
		
		if(i < right)
		{
			QuickSort(vector, i, right);
		}
	}
}
