/*
Matrices (Arrays Multidimensionales)
int[,] - Matriz rectangular 2D

Tamaño fijo en todas las dimensiones

Acceso rápido con múltiples índices [i,j]

Memoria contigua

Vectores de Listas
List<int>[] - Array donde cada elemento es una List

Flexibilidad - Cada lista puede tener diferente tamaño

Dinámico - Se pueden agregar/remover elementos de cada lista

Matriz Irregular (Jagged Array)
int[][] - Array de arrays

Cada fila puede tener diferente longitud

Más flexible que matriz rectangular

Lista de Listas
List<List<int>> - Estructura completamente dinámica

Puede crecer en ambas dimensiones

Máxima flexibilidad

Diferencias Clave:
Tipo	Sintaxis	Tamaño	Flexibilidad
Matriz 2D	int[,]	Fijo	Baja
Vector de Listas	List<int>[]	Semi-flexible	Media
Lista de Listas	List<List<int>>	Dinámico	Alta
Casos de Uso:
Matrices 2D: Tableros de juego, imágenes, planillas

Vector de Listas: Categorías con items, cursos con estudiantes

Lista de Listas: Estructuras que cambian frecuentemente en ambas dimensiones
*/
using System;
using System.Collections.Generic;
using System.Linq;

class Estudiante
{
    public string Nombre { get; set; }
    public int Edad { get; set; }
    
    public Estudiante(string nombre, int edad)
    {
        Nombre = nombre;
        Edad = edad;
    }
    
	// Sobreescribe el método ToString(), para agregar funcionalidad extra
    public override string ToString()
    {
        return $"{Nombre} ({Edad} años)";
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("=== MATRICES Y VECTORES DE LISTAS EN C# ===\n");
        
        EjemploMatrizEnteros();
        EjemploMatrizCadenas();
        EjemploMatrizObjetos();
        EjemploVectorListasEnteros();
        EjemploVectorListasCadenas();
        EjemploVectorListasObjetos();
        EjemploListaDeListas();
        
        Console.WriteLine("\nTodos los ejemplos ejecutados correctamente!");
    }
    
	/// <sumary>
	/// 1. MATRIZ DE ENTEROS - 2D
	/// Salida esperada:
	/// 1. MATRIZ DE ENTEROS - 2D
	/// Matriz 3x3:
	/// 1 2 3
	/// 4 5 6
	/// 7 8 9
	/// 
	/// Elemento [1,2]: 6
	/// Dimensión: 2D
	/// Tamaño total: 9
	/// Filas: 3, Columnas: 3
	/// Elemento [0,0] modificado: 100
	/// </sumary>
    static void EjemploMatrizEnteros()
    {
        Console.WriteLine("1. MATRIZ DE ENTEROS - 2D");
        
        // Matriz 3x3 de enteros
        int[,] matriz = new int[3, 3] 
        {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 9 }
        };
        
        Console.WriteLine("Matriz 3x3:");
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Console.Write(matriz[i, j] + " ");
            }
            Console.WriteLine();
        }
        
        Console.WriteLine($"\nElemento [1,2]: {matriz[1, 2]}");
        Console.WriteLine($"Dimensión: {matriz.Rank}D");
        Console.WriteLine($"Tamaño total: {matriz.Length}");
        Console.WriteLine($"Filas: {matriz.GetLength(0)}, Columnas: {matriz.GetLength(1)}");
        
        // Modificar elemento
        matriz[0, 0] = 100;
        Console.WriteLine($"Elemento [0,0] modificado: {matriz[0, 0]}");
        Console.WriteLine();
    }
    
	/// <sumary>
	/// Salida esperada:
	/// 2. MATRIZ DE CADENAS - 2D
	/// Tablero 3x3 (Tres en raya):
	/// X O X
	/// O X O
	/// X O X
	/// 
	/// Matriz irregular (jagged array):
	/// Fila 0: [A, B]
	/// Fila 1: [C, D, E]
	/// Fila 2: [F]
	/// </sumary>
    static void EjemploMatrizCadenas()
    {
        Console.WriteLine("2. MATRIZ DE CADENAS - 2D");
        
        // Matriz 2x2 de cadenas (tablero de juego)
        string[,] tablero = new string[3, 3] 
        {
            { "X", "O", "X" },
            { "O", "X", "O" },
            { "X", "O", "X" }
        };
        
        Console.WriteLine("Tablero 3x3 (Tres en raya):");
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Console.Write(tablero[i, j] + " ");
            }
            Console.WriteLine();
        }
        
        // Matriz irregular (jagged array)
        string[][] matrizIrregular = new string[3][];
        matrizIrregular[0] = new string[] { "A", "B" };
        matrizIrregular[1] = new string[] { "C", "D", "E" };
        matrizIrregular[2] = new string[] { "F" };
        
        Console.WriteLine("\nMatriz irregular (jagged array):");
        for (int i = 0; i < matrizIrregular.Length; i++)
        {
            Console.WriteLine($"Fila {i}: [{string.Join(", ", matrizIrregular[i])}]");
        }
        Console.WriteLine();
    }
    
	/// <sumary>
	/// Salida esperada:
	/// 3. MATRIZ DE OBJETOS - 2D
	/// Matriz de estudiantes 2x2:
	/// Posición [0,0]: Ana (20 años)
	/// Posición [0,1]: Juan (22 años)
	/// Posición [1,0]: María (21 años)
	/// Posición [1,1]: Pedro (23 años)
	///
	/// Estudiante en [0,1]: Juan (22 años)
	/// Estudiante modificado en [1,0]: Carlos (24 años)
	/// </sumary>
    static void EjemploMatrizObjetos()
    {
        Console.WriteLine("3. MATRIZ DE OBJETOS - 2D");
        
        // Matriz 2x2 de objetos Estudiante
        Estudiante[,] estudiantes = new Estudiante[2, 2] 
        {
			// Primera fila
            { 
                new Estudiante("Ana", 20), 
                new Estudiante("Juan", 22) 
            },
			// Segunda fila
            { 
                new Estudiante("María", 21), 
                new Estudiante("Pedro", 23) 
            }
        };
        
        Console.WriteLine("Matriz de estudiantes 2x2:");
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
				// NOTA.. - Ver definición de la clase "Estudiante", en donde se sobreescribe el método ToString(), para agregar funcionalidad extra
                Console.WriteLine($"  Posición [{i},{j}]: {estudiantes[i, j]}");
            }
        }
        
        // Acceder y modificar
        Console.WriteLine($"\nEstudiante en [0,1]: {estudiantes[0, 1]}");
        estudiantes[1, 0] = new Estudiante("Carlos", 24);
        Console.WriteLine($"Estudiante modificado en [1,0]: {estudiantes[1, 0]}");
        Console.WriteLine();
    }
    
	/// <sumary>
	/// Salida esperada:
	/// 4. VECTOR DE LISTAS DE ENTEROS
	/// Vector de listas de enteros:
	///   Lista 0: [1, 2, 3]
    ///		 Cantidad: 3, Suma: 6
	///   Lista 1: [4, 5, 6, 7]
    ///      Cantidad: 4, Suma: 22
	///   Lista 2: [8, 9]
	///      Cantidad: 2, Suma: 17
	///
	/// Después de modificar:
	///   Lista 0: [1, 2, 3, 10]
	///   Lista 1: [4, 6, 7]
	///   Lista 2: [8, 9]
	/// </sumary>
    static void EjemploVectorListasEnteros()
    {
        Console.WriteLine("4. VECTOR DE LISTAS DE ENTEROS");
        
        // Array de listas de enteros
        List<int>[] vectorListas = new List<int>[3];
        
        // Inicializar cada lista
        vectorListas[0] = new List<int> { 1, 2, 3 };
        vectorListas[1] = new List<int> { 4, 5, 6, 7 };
        vectorListas[2] = new List<int> { 8, 9 };
        
        Console.WriteLine("Vector de listas de enteros:");
        for (int i = 0; i < vectorListas.Length; i++)
        {
            Console.WriteLine($"  Lista {i}: [{string.Join(", ", vectorListas[i])}]");
            Console.WriteLine($"    Cantidad: {vectorListas[i].Count}, Suma: {vectorListas[i].Sum()}");
        }
        
        // Operaciones en listas específicas
        vectorListas[0].Add(10);
        vectorListas[1].Remove(5);
        
        Console.WriteLine("\nDespués de modificar:");
        for (int i = 0; i < vectorListas.Length; i++)
        {
            Console.WriteLine($"  Lista {i}: [{string.Join(", ", vectorListas[i])}]");
        }
        Console.WriteLine();
    }
    
	/// <sumary>
	/// 5. VECTOR DE LISTAS DE CADENAS
	/// Inventario por categorías:
	///   Frutas: [Manzana, Banana, Naranja]
	///   Lácteos: [Leche, Queso, Yogur]
	///   Panadería: [Pan, Galletas]
	///   Carnes: [Carne, Pollo, Pescado]
	///
	/// Buscando 'Queso':
	///   Encontrado en: Lácteos
	/// </sumary>
    static void EjemploVectorListasCadenas()
    {
        Console.WriteLine("5. VECTOR DE LISTAS DE CADENAS");
        
        // Array de listas de cadenas (ejemplo: categorías de productos)
        List<string>[] categorias = new List<string>[4];
        
        categorias[0] = new List<string> { "Manzana", "Banana", "Naranja" };
        categorias[1] = new List<string> { "Leche", "Queso", "Yogur" };
        categorias[2] = new List<string> { "Pan", "Galletas" };
        categorias[3] = new List<string> { "Carne", "Pollo", "Pescado" };
        
        string[] nombresCategorias = { "Frutas", "Lácteos", "Panadería", "Carnes" };
        
        Console.WriteLine("Inventario por categorías:");
        for (int i = 0; i < categorias.Length; i++)
        {
            Console.WriteLine($"  {nombresCategorias[i]}: [{string.Join(", ", categorias[i])}]");
        }
        
        // Buscar en todas las listas
        string productoBuscado = "Queso";
        Console.WriteLine($"\nBuscando '{productoBuscado}':");
        for (int i = 0; i < categorias.Length; i++)
        {
            if (categorias[i].Contains(productoBuscado))
            {
                Console.WriteLine($"  Encontrado en: {nombresCategorias[i]}");
            }
        }
        Console.WriteLine();
    }
    
	/// <sumary>
	/// Salida esperada:
	/// 6. VECTOR DE LISTAS DE OBJETOS
	/// Estudiantes por curso:
	///   Matemáticas (2 estudiantes):
	///      - Ana (20 años)
	///      - Juan (22 años)
	///   Programación (3 estudiantes):
    ///		 - María (21 años)
    ///      - Pedro (23 años)
    ///      - Carlos (24 años)
	///   Historia (1 estudiantes):
    ///      - Laura (19 años)
	///
	/// Estadísticas por curso:
	///   Matemáticas: 2 estudiantes, Edad promedio: 21,0
	///   Programación: 3 estudiantes, Edad promedio: 22,7
	///   Historia: 1 estudiantes, Edad promedio: 19,0
	/// </sumary>
    static void EjemploVectorListasObjetos()
    {
        Console.WriteLine("6. VECTOR DE LISTAS DE OBJETOS");
        
        // Array de listas de estudiantes por curso
        List<Estudiante>[] estudiantesPorCurso = new List<Estudiante>[3];
        
        estudiantesPorCurso[0] = new List<Estudiante> 
        {
            new Estudiante("Ana", 20),
            new Estudiante("Juan", 22)
        };
        
        estudiantesPorCurso[1] = new List<Estudiante> 
        {
            new Estudiante("María", 21),
            new Estudiante("Pedro", 23),
            new Estudiante("Carlos", 24)
        };
        
        estudiantesPorCurso[2] = new List<Estudiante> 
        {
            new Estudiante("Laura", 19)
        };
        
        string[] nombresCursos = { "Matemáticas", "Programación", "Historia" };
        
        Console.WriteLine("Estudiantes por curso:");
        for (int i = 0; i < estudiantesPorCurso.Length; i++)
        {
            Console.WriteLine($"  {nombresCursos[i]} ({estudiantesPorCurso[i].Count} estudiantes):");
            foreach (var estudiante in estudiantesPorCurso[i])
            {
				// NOTA.. - Ver definición de la clase "Estudiante", en donde se sobreescribe el método ToString(), para agregar funcionalidad extra
                Console.WriteLine($"    - {estudiante}");
            }
        }
        
        // Estadísticas
        Console.WriteLine("\nEstadísticas por curso:");
        for (int i = 0; i < estudiantesPorCurso.Length; i++)
        {
            double promedioEdad = estudiantesPorCurso[i].Average(e => e.Edad);
            Console.WriteLine($"  {nombresCursos[i]}: {estudiantesPorCurso[i].Count} estudiantes, Edad promedio: {promedioEdad:F1}");
        }
        Console.WriteLine();
    }
    
	/// <sumary>
	/// Salida esperada:
	/// 7. LISTA DE LISTAS - Estructura más dinámica
	/// Lista de listas (matriz dinámica):
	///   Fila 0: [1, 2, 3]
	///   Fila 1: [4, 5, 6, 7]
	///   Fila 2: [8, 9]
	/// 
	/// Después de agregar elementos:
	///   Fila 0: [1, 2, 3]
	///   Fila 1: [4, 5, 6, 7]
	///   Fila 2: [8, 9, 99]
	///   Fila 3: [10, 11, 12, 13, 14]
	///
	/// Total de filas: 4
	/// Total de elementos: 15
	/// </sumary>
    static void EjemploListaDeListas()
    {
        Console.WriteLine("7. LISTA DE LISTAS - Estructura más dinámica");
        
        // Lista de listas de enteros (matriz dinámica)
        List<List<int>> matrizDinamica = new List<List<int>>();
        
        // Agregar filas
        matrizDinamica.Add(new List<int> { 1, 2, 3 });
        matrizDinamica.Add(new List<int> { 4, 5, 6, 7 }); // Fila de diferente tamaño
        matrizDinamica.Add(new List<int> { 8, 9 });
        
        Console.WriteLine("Lista de listas (matriz dinámica):");
        for (int i = 0; i < matrizDinamica.Count; i++)
        {
            Console.WriteLine($"  Fila {i}: [{string.Join(", ", matrizDinamica[i])}]");
        }
        
        // Agregar nueva fila dinámicamente
        matrizDinamica.Add(new List<int> { 10, 11, 12, 13, 14 });
        
        // Agregar elemento a fila específica
        matrizDinamica[2].Add(99);
        
        Console.WriteLine("\nDespués de agregar elementos:");
        for (int i = 0; i < matrizDinamica.Count; i++)
        {
            Console.WriteLine($"  Fila {i}: [{string.Join(", ", matrizDinamica[i])}]");
        }
        
        Console.WriteLine($"\nTotal de filas: {matrizDinamica.Count}");
        Console.WriteLine($"Total de elementos: {matrizDinamica.Sum(fila => fila.Count)}");
    }
}