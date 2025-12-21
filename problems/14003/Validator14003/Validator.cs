using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

class Validator
{
    private static bool _debugMode = false;
    //private static bool _debugMode = true;

    static void Main(string[] args)
    {
        try
        {

            if(_debugMode)
            {
                args = new string[]{
                  @"D:\Universidad Católica Boliviana\Semestre2-2025\Examen Final Segunda Opción\14003\14003\IN\datos0001.txt",
                  @"D:\Universidad Católica Boliviana\Semestre2-2025\Examen Final Segunda Opción\14003\14003\OUT\Output_datos0001.txt",
                  @"D:\Universidad Católica Boliviana\Semestre2-2025\Examen Final Segunda Opción\14003\14003\OUT\Output_datos0001.txt",
                  @"D:\Universidad Católica Boliviana\Semestre2-2025\Examen Final Segunda Opción\14003\14003\Program.cs"};
                string studentFile = args[0];
                if (args.Length == 0)
                {
                    Error("ERROR: Debe indicar el archivo .cs del alumno.");
                }
            }

            if (args.Length < 3)
            {
                Console.WriteLine(
                    "ERROR: argumentos insuficientes (uso: Validator <input> <expected> <actual>)");
                Environment.Exit(1);
            }

            string inputPath = args[0];
            string expectedPath = args[1];
            string actualPath = args[2];

            if (!File.Exists(inputPath))
            {
                Error("ERROR: no existe el archivo input");
            }

            if (!File.Exists(expectedPath))
            {
                Error("ERROR: no existe el archivo esperado");
            }

            if (!File.Exists(actualPath))
            {
                Error("ERROR: no existe el archivo generado por el estudiante");
            }

            string studentCodeFile = string.Empty;
            if (args.Length > 3)
            {
                studentCodeFile = args[3];

                if (!File.Exists(studentCodeFile))
                {
                    Error("ERROR: Archivo del alumno no encontrado.");
                }
            }

            // Leer todas las líneas de entrada
            var inputLines = File.ReadAllLines(inputPath);

            // .TrimEnd() es un método de la clase string en C# que elimina caracteres del final de una
            // cadena hasta que encuentra un carácter que no coincide con los que se quieren eliminar.
            // Comportamiento básico
            // Hay dos sobrecargas principales:
            // Sin parámetros: string.TrimEnd()
            // Elimina todos los espacios en blanco del final de la cadena.
            // Los espacios en blanco incluyen: espacio normal (' '), tabulación (\t), salto de línea
            // (\n, \r), etc.
            //
            // Ejemplo, cuando no toma el valor por defecto
            // string texto = "Hola mundo  !!!!!!  ";
            // string resultado = texto.TrimEnd('!', ' ');
            // Console.WriteLine(resultado + "___");
            // Salida por consola:
            // Hola mundo___
            var actualContent = File.ReadAllText(actualPath).TrimEnd();
            var expectedContent = File.ReadAllText(expectedPath).TrimEnd();
            if (expectedContent != actualContent)
            {
                Error("Respuesta incorrecta.");
            }

            var expectedLines = File.ReadAllLines(expectedPath);

            // ============================================================
            // 1) COMPILAR EL CÓDIGO DEL ALUMNO
            // ============================================================

            string dllPath = CompileStudentCode(studentCodeFile);


            // ============================================================
            // 2) CARGAR ENSAMBLADO POR REFLECTION
            // ============================================================

            Assembly asm = Assembly.LoadFrom(dllPath);

            Type? classProgramType = asm.GetTypes().FirstOrDefault(t => t.Name == "Program");

            if (classProgramType == null)
            {
                Error("❌ ERROR: Debe existir una clase pública llamada 'Program'.");
            }

            string expectedMethodName = "SolveLCS";
            MethodInfo? method = classProgramType?.GetMethod(expectedMethodName, BindingFlags.Public | BindingFlags.Static);

            if (method == null)
            {
                Error($"❌ ERROR: No existe el método público y estático '{expectedMethodName}'.");
            }


            // =====================================
            // 3) Verificar firma correcta
            // =====================================

            var pars = method?.GetParameters();

            bool firmaOk = pars == null ? false :
                pars.Length == 4 &&
                pars[0].ParameterType == typeof(int) &&
                pars[0].Name == "M" &&

                pars[1].ParameterType == typeof(int) &&
                pars[1].Name == "N" &&

                pars[2].ParameterType == typeof(List<string>) &&
                pars[2].Name == "norte" &&

                pars[3].ParameterType == typeof(List<string>) &&
                pars[3].Name == "sur" &&

                method.ReturnType == typeof(int[][]);

            if (!firmaOk)
            {
                Error("❌ FIRMA INCORRECTA.");
            }


            // ============================================================
            // 4) Leer input para pasarlo al código del alumno
            // ============================================================
            // Eliminar líneas en blanco, por las dudas
            List<string> lines =
              inputLines
                .Where(l => !string.IsNullOrEmpty(l) && !string.IsNullOrWhiteSpace(l))
                .ToList();

            int cont = 0; // Inicializa el contador de líneas leídas

            // Leer N
            int N = int.Parse(lines[cont++].Trim());
            List<string> norte = new List<string>();
            // Recorrer el primer grupo de boyardos
            for (int i = 0; i < N; i++)
            {
                string line = lines[cont++].Trim();
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    norte.Add(parts[1]); // Solo la etnia; porque es lo que realmente importa, en este problema
                }
            }

            // Leer M
            int M = int.Parse(lines[cont++].Trim());
            List<string> sur = new List<string>();
            // Recorrer el segundo grupo de boyardos
            for (int i = 0; i < M; i++)
            {
                string line = lines[cont++].Trim();
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    sur.Add(parts[1]); // Solo la etnia; porque es lo que realmente importa, en este problema
                }
            }

            // ============================================================
            // 5) INVOCAR EL MÉTODO DEL ALUMNO
            // ============================================================

            const int SIGN_ERROR_CODE = 2;
            int[][] result = null;

            try
            {
                result = (int[][])method.Invoke(null, new object[] { M, N, norte, sur });
            }
            catch (Exception ex)
            {
                Error("❌ ERROR al ejecutar el método del estudiante:" + 
                      ex.InnerException?.Message ?? ex.Message, SIGN_ERROR_CODE);
            }

            if(result == null)
            {
                Error("No se pudo obtener un resultado, de la compilación del código del estudiante",
                    SIGN_ERROR_CODE);
            }

            // ============================================================
            // 6) Validar resultado
            // ============================================================

            string resultStrLine = result[N][M].ToString().Trim();
            if (resultStrLine != expectedLines[0])
            {
                Error("❌ Resultado incorrecto. " + "Esperado: " + string.Join(",", expectedLines) +
                      "Obtenido: " + resultStrLine, SIGN_ERROR_CODE);
            }

            Console.WriteLine("OK");
        }
        catch(Exception ex)
        {
            Error($"Error:{ex.Message}");
        }
    }

    private static void Error(string message, int errorCode = 1)
    {
        Console.WriteLine(message);
        Environment.Exit(errorCode);
    }

    /// <sumary>
    /// Compila el cpdoigo del estudiante y devuelve la ruta de la DLL que se generó.
    /// En caso de error, muestra un mensaje en patnalla y aborta el programa.
    /// </summary>
    /// <param name="studentCodeFile">Ruta en el disco, del código fuente del estudiante
    /// </param>
    private static string CompileStudentCode(string studentCodeFile)
    {
        string sourceCode = File.ReadAllText(studentCodeFile);

        // Esta línea crea un árbol de sintaxis (Syntax Tree) a partir de una cadena de texto que contiene
        // código C# (sourceCode).
        // CSharpSyntaxTree.ParseText() es un método estático de la clase CSharpSyntaxTree (del namespace
        // Microsoft.CodeAnalysis.CSharp).
        // El resultado es un objeto SyntaxTree que representa el código fuente parseado, listo para ser
        // usado en una compilación.
        // Es el primer paso para analizar o compilar código dinámicamente.
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        // Esto crea una colección de referencias de metadatos (MetadataReference) que el compilador usará
        // para resolver tipos externos (como System.String, Console, etc.).
        // Desglosémoslo paso a paso:
        // AppDomain.CurrentDomain.GetAssemblies(): Obtiene todos los assemblies (ensamblados) que ya están
        // cargados en el dominio de aplicación actual (AppDomain). Esto incluye mscorlib/System.Runtime (el
        // núcleo de .NET), System.Core, y cualquier otro assembly que tu aplicación haya cargado.
        // .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location)): Filtra los assemblies:
        // !a.IsDynamic: Excluye assemblies dinámicos (generados en memoria, sin archivo físico).
        // !string.IsNullOrEmpty(a.Location): Excluye aquellos sin ruta de archivo (por ejemplo, algunos
        // facades o assemblies en memoria).
        // Esto asegura que solo se tomen assemblies con un archivo .dll real en disco.
        // .Select(a => MetadataReference.CreateFromFile(a.Location)): Para cada assembly filtrado, crea una
        // referencia de metadatos cargándola desde su archivo en disco (a.Location es la ruta completa del
        // .dll).
        // .Cast<MetadataReference>(): Convierte la secuencia resultante al tipo base MetadataReference
        // (necesario porque CreateFromFile devuelve PortableExecutableReference, que hereda de
        // MetadataReference).
        var refs = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>();

        // CSharpCompilation.Create(): Método estático que crea un objeto CSharpCompilation, que representa
        // una compilación completa de C#.
        // Parámetros:
        // 
        // Primer parámetro ("StudentSourceCodeAssembly"): El nombre del assembly que se generará si se emite
        // (compila) el código. Es solo un identificador interno.
        // Segundo parámetro (new[] { syntaxTree }): Una colección de árboles de sintaxis a compilar. Aquí
        // solo hay uno (el que se crea antes). Se pueden agregar más si se tienen múltiples archivos.
        // references: refs: El parámetro nombrado references pasa la colección de referencias que se preparó.
        // Esto permite al compilador resolver tipos externos (como using System;.
        // options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary): Configura opciones de
        // compilación.
        // OutputKind.DynamicallyLinkedLibrary: Indica que el output será una DLL (biblioteca dinámica). Otras
        // opciones comunes: ConsoleApplication (para exe con Main), WindowsApplication, etc.
        var compilation = CSharpCompilation.Create(
            "StudentSourceCodeAssembly",
            new[] { syntaxTree }, // Aquí está el árbol sintáctico que se generó, a partir del código del
                                  // estudiante
            references: refs,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        // Nombre de la dll que se va a generar, a partir del código del estudiante
        string dllPath = "StudentSourceCodeAssembly.dll";

        // Intenta compilar el código en memoria y escribir el resultado (el assembly DLL) en la ruta que
        // se le pase como dllPath.
        // ¿Qué hace exactamente compilation.Emit()?
        // 
        // Toma la CSharpCompilation que se creó antes.
        // Realiza la compilación completa (verifica errores de sintaxis, tipos, etc.).
        // Si no hay errores, genera el assembly en formato PE (Portable Executable), es decir, una DLL o EXE.
        // Intenta escribir ese assembly en el stream o archivo que se le indique.
        var emitResult = compilation.Emit(dllPath);

        // Verifica el resultado de la compilación
        if (!emitResult.Success)
        {
          List<string> errorList = new List<string>();
            foreach (var diag in emitResult.Diagnostics)
            {
                errorList.Add(diag.ToString());
            }
            Error("❌ ERROR: El código del alumno no compila." + string.Join(". ", errorList));
        }

        return dllPath;
    }


    public class DataSet
    {
        public int N { get; set; }
        public int[][] Edges { get; set; }
        public int Source { get; set; }
        public int[] Expected { get; set; }
    }
}

