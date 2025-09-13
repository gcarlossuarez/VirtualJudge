<Query Kind="Program">
  <Namespace>System</Namespace>
</Query>

// using System;

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
		
		
		MergeSort(vector, tamaño);
		Console.WriteLine("Números ordenados en el vector:");
        foreach (int numero in vector)
        {
            Console.Write($"{numero}\t");
        }
		Console.WriteLine();
    }
	
	private static void MergeSort(int[] vector, int n)
	{
		MergeSort(vector, 0, n - 1);
	}
	
	private static void MergeSort(int[] vector, int left, int right)
	{
		int middle = (left + right) / 2;
		if(left < right)
		{
			MergeSort(vector, left, middle);
			MergeSort(vector, middle + 1, right);
			Merge(vector, left, middle, right);
		}
	}
	
	private static void Merge(int[] vector, int left, int middle, int right)
	{
		int i = left, j = middle + 1, k = 0;
		int tamResult = right - left + 1;
		int[] result = new int[tamResult];
		while(i <= middle && j <= right)
		{
			if(vector[i] < vector[j])
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