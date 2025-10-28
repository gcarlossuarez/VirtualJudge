/* *
 * Arrays
 * Tamaño fijo - No puede cambiar después de la creación
 * Acceso rápido por índice
 *
 * Memoria contigua
 *
 * Listas
 * Tamaño dinámico - Crece automáticamente
 * Flexible para agregar/remover elementos
 * Usa arrays internamente
 *
 * Diccionarios
 * Clave-Valor - Acceso rápido por clave
 * Claves únicas - No se permiten duplicados
 * Busqueda muy rápida (O(1) en promedio)
 *
 * Queue (Cola)
 * FIFO - First In, First Out
 * Ejemplos: Cola de impresión, atención al cliente
 *
 * Stack (Pila)
 * LIFO - Last In, First Out
 * Ejemplos: Navegación web (back/forward), deshacer acciones
 * 
 * String.Split/Join
 * Split: Divide cadena en partes usando delimitador
 * Join: Une elementos en una cadena con separador
 * 
 * HashSet
 * Elementos únicos - No permite duplicados
 * Operaciones de conjuntos (unión, intersección, etc.)
 * */
using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== EJEMPLOS DE ESTRUCTURAS DE DATOS EN C# ===\n");
        
        EjemploArrays();
        EjemploListas();
        EjemploDiccionarios();
        EjemploQueue();
        EjemploStack();
        EjemploStringSplitJoin();
        EjemploHashSet();
        
        Console.WriteLine("\nTodos los ejemplos ejecutados correctamente!");
    }
    
    /// <summary>
    /// Ejmplo de uso de arrays.
    /// Salida esperada:
    /// 1. ARRAYS - Tamaño fijo
    /// Array de números: [10, 20, 30, 40, 50]
    /// Array de nombres: [Ana, Juan, María]
    /// Tamaño del array: 5
    /// Primer elemento: 10
    /// Último elemento: 50
    /// Array modificado: [10, 20, 35, 40, 50]
    /// </summary>
    static void EjemploArrays()
    {
        Console.WriteLine("1. ARRAYS - Tamaño fijo");
        
        // Declaración e inicialización
        int[] numeros = new int[5] { 10, 20, 30, 40, 50 };
        string[] nombres = { "Ana", "Juan", "María" };
        
        Console.WriteLine($"Array de números: [{string.Join(", ", numeros)}]");
        Console.WriteLine($"Array de nombres: [{string.Join(", ", nombres)}]");
        Console.WriteLine($"Tamaño del array: {numeros.Length}");
        Console.WriteLine($"Primer elemento: {numeros[0]}");
        Console.WriteLine($"Último elemento: {numeros[numeros.Length - 1]}");
        
        // Modificar elemento
        numeros[2] = 35;
        Console.WriteLine($"Array modificado: [{string.Join(", ", numeros)}]");
        Console.WriteLine();
    }
    
    /// <summary>
    /// Ejemplo de uso de listas.
    /// Salida esperada:
    /// 2. LISTAS - Tamaño dinámico
    /// Lista de frutas: [Manzana, Banana, Naranja, Uva]
    /// Cantidad de frutas: 4
    /// ¿Contiene Manzana? True
    /// Lista después de modificar: [Manzana, Pera, Naranja, Uva]
    /// Recorriendo lista: Manzana Pera Naranja Uva
    /// </summary>
    static void EjemploListas()
    {
        Console.WriteLine("2. LISTAS - Tamaño dinámico");
        
        // Declaración e inicialización
        List<string> frutas = new List<string>();
        List<int> edades = new List<int> { 25, 30, 35 };
        
        // Agregar elementos
        frutas.Add("Manzana");
        frutas.Add("Banana");
        frutas.Add("Naranja");
        frutas.Add("Uva");
        
        Console.WriteLine($"Lista de frutas: [{string.Join(", ", frutas)}]");
        Console.WriteLine($"Cantidad de frutas: {frutas.Count}");
        Console.WriteLine($"¿Contiene Manzana? {frutas.Contains("Manzana")}");
        
        // Insertar y remover
        frutas.Insert(1, "Pera");
        frutas.Remove("Banana");
        
        Console.WriteLine($"Lista después de modificar: [{string.Join(", ", frutas)}]");
        
        // Recorrer lista
        Console.Write("Recorriendo lista: ");
        foreach (string fruta in frutas)
        {
            Console.Write(fruta + " ");
        }
        Console.WriteLine("\n");
    }
    
    /// <summary>
    /// Ejemplo uso de Diccionarios.
    /// 3. DICCIONARIOS - Clave-Valor
    /// Diccionario de edades:
    ///   Juan: 25 años
    ///   María: 30 años
    ///   Pedro: 28 años
    /// 
    /// Edad de María: 30
    /// ¿Existe la clave 'Ana'? False
    /// Juan tiene 25 años
    /// </summary>
    static void EjemploDiccionarios()
    {
        Console.WriteLine("3. DICCIONARIOS - Clave-Valor");
        
        // Declaración e inicialización
        Dictionary<string, int> edades = new Dictionary<string, int>();
        Dictionary<int, string> estudiantes = new Dictionary<int, string>
        {
            { 101, "Carlos López" },
            { 102, "Ana García" },
            { 103, "Pedro Martínez" }
        };
        
        // Agregar elementos
        edades["Juan"] = 25;
        edades["María"] = 30;
        edades.Add("Pedro", 28);
        
        Console.WriteLine("Diccionario de edades:");
        foreach (KeyValuePair<string, int> entrada in edades)
        {
            Console.WriteLine($"  {entrada.Key}: {entrada.Value} años");
        }
        
        Console.WriteLine($"\nEdad de María: {edades["María"]}");
        Console.WriteLine($"¿Existe la clave 'Ana'? {edades.ContainsKey("Ana")}");
        
        // Intentar obtener valor
        if (edades.TryGetValue("Juan", out int edadJuan))
        {
            Console.WriteLine($"Juan tiene {edadJuan} años");
        }
        Console.WriteLine();
    }
    
    /// <summary>
    /// Ejemplo uso de Colas.
    /// Salida esperada:
    /// 4. QUEUE - FIFO (Primero en Entrar, Primero en Salir)
    /// Elementos en cola: 3
    /// Próximo en atender: Cliente 1
    /// Atendiendo clientes:
    ///   Atendiendo a: Cliente 1 - Quedan: 2
    ///   Atendiendo a: Cliente 2 - Quedan: 1
    ///   Atendiendo a: Cliente 3 - Quedan: 0
    /// </summary>
    static void EjemploQueue()
    {
        Console.WriteLine("4. QUEUE - FIFO (Primero en Entrar, Primero en Salir)");
        
        Queue<string> cola = new Queue<string>();
        
        // Encolar elementos
        cola.Enqueue("Cliente 1");
        cola.Enqueue("Cliente 2");
        cola.Enqueue("Cliente 3");
        
        Console.WriteLine($"Elementos en cola: {cola.Count}");
        Console.WriteLine($"Próximo en atender: {cola.Peek()}");
        
        // Desencolar
        Console.WriteLine("Atendiendo clientes:");
        while (cola.Count > 0)
        {
            string cliente = cola.Dequeue();
            Console.WriteLine($"  Atendiendo a: {cliente} - Quedan: {cola.Count}");
        }
        Console.WriteLine();
    }
    
    /// <summary>
    /// Ejemplo uso de Pilas.
    /// Salida esperada:
    /// 5. STACK - LIFO (Último en Entrar, Primero en Salir)
    /// Elementos en pila: 3
    /// Elemento en la cima: Libro 3
    /// Desapilando elementos:
    ///   Sacando: Libro 3 - Quedan: 2
    ///   Sacando: Libro 2 - Quedan: 1
    ///   Sacando: Libro 1 - Quedan: 0
    /// </summary>
    static void EjemploStack()
    {
        Console.WriteLine("5. STACK - LIFO (Último en Entrar, Primero en Salir)");
        
        Stack<string> pila = new Stack<string>();
        
        // Apilar elementos
        pila.Push("Libro 1");
        pila.Push("Libro 2");
        pila.Push("Libro 3");
        
        Console.WriteLine($"Elementos en pila: {pila.Count}");
        Console.WriteLine($"Elemento en la cima: {pila.Peek()}");
        
        // Desapilar
        Console.WriteLine("Desapilando elementos:");
        while (pila.Count > 0)
        {
            string libro = pila.Pop();
            Console.WriteLine($"  Sacando: {libro} - Quedan: {pila.Count}");
        }
        Console.WriteLine();
    }
    
    /// <summary>
    /// Ejemplo uso de string.Split y strng.Join.
    /// Salida esperada:
    /// 6. STRING.SPLIT y STRING.JOIN
    /// Texto original: manzana,banana,naranja,uva
    /// Después de Split: [manzana], [banana], [naranja], [uva]
    /// Array de colores: [Rojo, Verde, Azul, Amarillo]
    /// Después de Join: Rojo | Verde | Azul | Amarillo
    /// Oración original: Hola mundo desde C# programación
    /// Oración modificada: Hola_mundo_desde_C#_programación
    /// </summary>
    static void EjemploStringSplitJoin()
    {
        Console.WriteLine("6. STRING.SPLIT y STRING.JOIN");
        
        // String.Split - Dividir cadena en partes
        string texto = "manzana,banana,naranja,uva";
        string[] frutasArray = texto.Split(',');
        
        Console.WriteLine($"Texto original: {texto}");
        Console.WriteLine($"Después de Split: [{string.Join("], [", frutasArray)}]");
        
        // String.Join - Unir elementos en una cadena
        string[] colores = { "Rojo", "Verde", "Azul", "Amarillo" };
        string coloresUnidos = string.Join(" | ", colores);
        
        Console.WriteLine($"Array de colores: [{string.Join(", ", colores)}]");
        Console.WriteLine($"Después de Join: {coloresUnidos}");
        
        // Ejemplo más complejo
        string oracion = "Hola mundo desde C# programación";
        string[] palabras = oracion.Split(' ');
        string oracionModificada = string.Join("_", palabras);
        
        Console.WriteLine($"Oración original: {oracion}");
        Console.WriteLine($"Oración modificada: {oracionModificada}");
        Console.WriteLine();
    }
    
    /// <summary>
    /// Ejemplo uso de Hasset.
    /// Salida esperada:
    /// 7. HASHSET - Elementos únicos
    /// Emails únicos: usuario1@email.com, usuario2@email.com
    /// Cantidad de emails: 2
    /// Unión de conjuntos: 1, 2, 3, 4, 5, 6, 7, 8
    /// </summary>
    static void EjemploHashSet()
    {
        Console.WriteLine("7. HASHSET - Elementos únicos");
        
        HashSet<string> emails = new HashSet<string>();
        
        // Agregar elementos (no permite duplicados)
        emails.Add("usuario1@email.com");
        emails.Add("usuario2@email.com");
        emails.Add("usuario1@email.com"); // Este no se agregará
        
        Console.WriteLine($"Emails únicos: {string.Join(", ", emails)}");
        Console.WriteLine($"Cantidad de emails: {emails.Count}");
        
        HashSet<int> conjuntoA = new HashSet<int> { 1, 2, 3, 4, 5 };
        HashSet<int> conjuntoB = new HashSet<int> { 4, 5, 6, 7, 8 };
        
        // Operaciones de conjuntos
        conjuntoA.UnionWith(conjuntoB);
        Console.WriteLine($"Unión de conjuntos: {string.Join(", ", conjuntoA)}");
        Console.WriteLine();
    }
}
