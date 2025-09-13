<Query Kind="Program">
  <Namespace>System</Namespace>
</Query>

/*
System.IO
*/

// Lista la información de los archivos texto, que contiene los datos de las tablas de una base de datos; cuyos campos están separadas por
// el caracter ";" (similar al concepto de archivo con extensión ".csv")
void Main()
{
	var listFiles = Directory.GetFiles(@"D:\Universidad Católica Boliviana\AlgoritmosEjemplo\Archivos\ArchivosTexto\", "Departamento.txt"); // Busca todos los archivos con extensión .txt
	foreach(var f in listFiles)
	{
		FileInfo fileInfo = new FileInfo(f);
		Console.WriteLine($"Datos del archivo:{fileInfo.Name}");
		using(StreamReader reader = new StreamReader(f))
		{
			string line;
			while((line = reader.ReadLine()) != null)
			{
				string[]campos = line.Split(';'); // Crea un vector de campos, separados por el caracter ';'. cada campo, es un elemento en el vector
				int codDpto = int.Parse(campos[0]);
				string nombreDpto = campos[1];
				Console.WriteLine($"{codDpto}\t{nombreDpto}");
			}
		}
	}
}