// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        string path = "personas.bin";
        bool running = true;

        //EscribirRegistro(path, new Persona(1, "Juan", "Calle 1"));
        //EscribirRegistro(path, new Persona(2, "Pedro", "Calle 2"));
        //EscribirRegistro(path, new Persona(3, "Maria", "Calle 3"));

        while (running)
        {
            Console.WriteLine("Elija una opción:");
            Console.WriteLine("1. Añadir Persona");
            Console.WriteLine("2. Leer Persona");
            Console.WriteLine("3. Modificar persona por código");
            Console.WriteLine("4. Salir");
            string? option = Console.ReadLine();
            if(string.IsNullOrEmpty(option))
            {
                continue;
            }

            switch (option)
            {
                case "1":
                    Console.Write("Ingrese código: ");
                    if(int.TryParse(Console.ReadLine(), out int codigo))
                    {
                        Console.Write("Ingrese nombre: ");
                        string? nombre = Console.ReadLine();
                        if(!string.IsNullOrEmpty(nombre))
                        {
                            Console.Write("Ingrese direccion: ");
                            string? direccion = Console.ReadLine();
                            if(!string.IsNullOrEmpty(direccion))
                            {
                                EscribirRegistro(path, new Persona(codigo, nombre, direccion));
                            }
                        }
                    }
                    break;
                case "2":
                    Console.Write("Ingrese el código de la persona: ");
                    if(int.TryParse(Console.ReadLine(), out codigo))
                    {
                        Persona persona = LeerRegistroVariable(path, codigo);
                        Console.WriteLine($"{persona}'");
                    }
                    break;
                case "3":
                    Console.Write("Ingrese el código de la persona a modificar: ");
                    if(int.TryParse(Console.ReadLine(), out int codigoBuscado))
                    {
                        Console.Write("Ingrese nuevo nombre: ");
                        string? nuevoNombre = Console.ReadLine();
                        if(!string.IsNullOrEmpty(nuevoNombre))
                        {
                            Console.Write("Ingrese nuevo direccion: ");
                            string? nuevaDireccion = Console.ReadLine();
                            if(!string.IsNullOrEmpty(nuevaDireccion))
                            {
                                ModificarRegistroPorCodigo(path, codigoBuscado, new Persona(codigoBuscado, nuevoNombre, nuevaDireccion));
                            }
                        }
                    }
                    
                    break;

                case "4":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Opción no válida.");
                    break;
            }
        }
    }

    public static void EscribirRegistro(string path, Persona persona)
    {
        using (var fs = new FileStream(path, FileMode.Append, FileAccess.Write))
        using (var writer = new BinaryWriter(fs))
        {
            writer.Write(persona.ToBytes());
        }
    }

    public static Persona LeerRegistroVariable(string path, int codigoBuscado)
    {
        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
        using (var reader = new BinaryReader(fs))
        {
            while (fs.Position < fs.Length)
            {
                int lengthRecord = reader.ReadInt32();
                byte[] recordBytes = reader.ReadBytes(lengthRecord);
                Persona persona = Persona.FromBytes(recordBytes);
                if (persona.Codigo == codigoBuscado && persona.Codigo != Persona.NULO) // Si es igual al código buscado y no es nulo (no está elkiminbado lógicamente)
                {
                    return persona;
                }
            }
            Console.WriteLine("Código no encontrado.");
        }
        return new Persona();
    }


    public static bool ModificarRegistroPorCodigo(string path, int codigoBuscado, Persona nuevaPersona)
    {
        bool founded = false;
        using (var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
        using (var reader = new BinaryReader(fs))
        using (var writer = new BinaryWriter(fs))
        {
            while (fs.Position < fs.Length)
            {
                int lengthRecord = reader.ReadInt32();
                byte[] recordBytes = reader.ReadBytes(lengthRecord);
                Persona persona = Persona.FromBytes(recordBytes);
                if (persona.Codigo == codigoBuscado && persona.Codigo != Persona.NULO) // Si es igual al código buscado y no es nulo (no está elkiminbado lógicamente)
                {
                    fs.Position -= (lengthRecord + sizeof(int)); // Retrocedemos para sobreescribir el registro. ==> Longitud del registro, mas el tamaño del entero que tiene toda la longitud del registro
                    persona.Codigo = Persona.NULO; // Eliminamos lógicamente el registro
                    writer.Write(persona.ToBytes());
                    founded = true;
                    break;
                }
            }
        }
        if(founded) // Se agrega al final, el modificado; ya que, si el nuevo registro tiene dimensión diferente al anterior, desfasa todo el archivo
        {
            EscribirRegistro(path, nuevaPersona);
        }

        return founded;
    }
}

public class Persona
{
    public const int NULO = -1;
    public int Codigo { get; set; }
    public string Nombre { get; set; }
    public string Direccion { get; set; } 

    public Persona()
    {
        Nombre = string.Empty;
        Direccion = string.Empty;
    }

    public Persona(int codigo, string nombre, string direccion)
    {
        Codigo = codigo;
        Nombre = nombre;
        Direccion = direccion;
    }

    public byte[] ToBytes()
    {
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            int lengthInt = sizeof(int);
            int lengthRecord = // La cantidad de bytes del registro, se guarda en el archivo; pero, no se toma en cuenta en el registro persona
                lengthInt // codigo
                + lengthInt // Longitud del nombre
                + Encoding.UTF8.GetByteCount(Nombre) // nombre
                + lengthInt // Longitud de la dirección
                + Encoding.UTF8.GetByteCount(Direccion); // dirección
            writer.Write(lengthRecord);

            writer.Write(Codigo);
            var nameBytes = Encoding.UTF8.GetBytes(Nombre); // Transforma el string a bytes
            writer.Write(nameBytes.Length); // Escribe el entero con la cantidad de bytes del nombre
            writer.Write(nameBytes); // Escribe el nombre, transformado a bytes

            var direccionBytes = Encoding.UTF8.GetBytes(Direccion); // Transforma el string a bytes
            writer.Write(direccionBytes.Length); // Escribe el entero con la cantidad de bytes de la dirección
            writer.Write(direccionBytes); // Escribe la dirección, transformada a bytes

            return ms.ToArray(); // Devuelve el array de bytes del MemoryStream
        }
    }

    public static Persona FromBytes(byte[] bytes)
    {
        using (var ms = new MemoryStream(bytes))
        using (var reader = new BinaryReader(ms))
        {
            var codigo = reader.ReadInt32();
            var nameLength = reader.ReadInt32(); // Lee la cantidad de bytes del nombre, que debe leer inmediatamente después
            var nameBytes = reader.ReadBytes(nameLength); // Lee en bytes, el nombre
            var nombre = Encoding.UTF8.GetString(nameBytes); // Transforma a string, los bytes leídos

            var direccionLength = reader.ReadInt32(); // Lee la cantida de bytes de la dirección, que debe leer inmediatmanete después
            var direccionBytes = reader.ReadBytes(direccionLength); // Lee en bytes, la dirección
            var direccion = Encoding.UTF8.GetString(direccionBytes); // Transforma a string, los bytes leídos

            return new Persona(codigo, nombre, direccion);
        }
    }

    public override string ToString()
    {
        return $"Código: {Codigo}, Nombre: {Nombre}, Dirección: {Direccion}";
    }
}



