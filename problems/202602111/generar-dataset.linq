<Query Kind="Program" />

/*
Generador de DataSets (LINQPad)
Este generador produce archivos datos0001.txt … datos0100.txt en la carpeta IN, con casos de dificultad creciente. 
El caso 1 es siempre el ejemplo oficial y los casos 2..100 son aleatorios.

________________________________________
Notas sobre el generador
1.	Caso2 1 y 2 contienen los ejemplos oficiales }
		ABCBDAB / BDCABA → salida esperada 2  
		ABCDEFG / XYZABCD → salida esperada 4
2.	Casos 3–5: cadenas de 1–20 caracteres, alfabeto completo (26 letras). Sirven para verificar correctitud con casos donde la subcadena común suele ser corta.
3.	Casos 6–8: cadenas de 100–500 caracteres, alfabeto reducido a 10 letras. Aumenta la probabilidad de coincidencias y hace que la respuesta sea más interesante que 1.
4.	Casos 9–100: cadenas de 1500–2000 caracteres, alfabeto reducido a 4 letras. Pruebas de estrés: la matriz 2001 × 2001 de enteros ocupa ~16 MB, manejable pero exigente.
5.	Casos pares: inyectan un fragmento común de longitud ~min(|A|,|B|)/3 en posiciones aleatorias de ambas cadenas, garantizando que exista una subcadena común larga y conocida (útil para validar que la respuesta no es trivial).
6.	Nombres: datos0001.txt … datos0100.txt (formato de 4 dígitos).

*/
void Main()
{
    GenEnigma();
}

void GenEnigma()
{
    string currentPath = Path.GetDirectoryName(Util.CurrentQueryPath);
    string inputDirectory = Path.Combine(currentPath, "IN");
    Directory.CreateDirectory(inputDirectory);

    // ===== Caso 1: Ejemplo oficial =====
    string text = @"ABCBDAB
BDCABA
";
    File.WriteAllText(Path.Combine(inputDirectory, "datos0001.txt"), text);
    Console.WriteLine("datos0001.txt (Ejemplo oficial)");

text = @"
ABCDEFG
XYZABCD
";
	File.WriteAllText(Path.Combine(inputDirectory, "datos0002.txt"), text);
    Console.WriteLine("datos0002.txt (Ejemplo oficial)");

    Random rnd = new Random(333);
    string alfabeto = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    for (int d = 3; d <= 100; d++)
    {
        int lenA, lenB;

        if (d <= 5)
        {
            // Casos pequeños (correctitud)
            lenA = rnd.Next(1, 20);
            lenB = rnd.Next(1, 20);
        }
        else if (d <= 8)
        {
            // Casos medianos (rendimiento)
            lenA = rnd.Next(100, 500);
            lenB = rnd.Next(100, 500);
        }
        else
        {
            // Casos grandes (estrés)
            lenA = rnd.Next(1500, 2001);
            lenB = rnd.Next(1500, 2001);
        }

        // Reducir tamaño del alfabeto en casos grandes para garantizar
        // que existan subcadenas comunes largas (si no, casi siempre serían 1-2)
        string alfabetoUsado = alfabeto.Substring(0, d <= 4 ? 26 : (d <= 7 ? 10 : 4));

        string A = new string(Enumerable.Range(0, lenA)
            .Select(_ => alfabetoUsado[rnd.Next(alfabetoUsado.Length)]).ToArray());

        string B = new string(Enumerable.Range(0, lenB)
            .Select(_ => alfabetoUsado[rnd.Next(alfabetoUsado.Length)]).ToArray());

        // En algunos casos, inyectar una subcadena común larga conocida
        // para forzar que el resultado no sea trivialmente pequeño.
        if (d % 2 == 0 && lenA > 50 && lenB > 50)
        {
            int comun = Math.Min(lenA, lenB) / 3;
            string fragmento = new string(Enumerable.Range(0, comun)
                .Select(_ => alfabetoUsado[rnd.Next(alfabetoUsado.Length)]).ToArray());

            int posA = rnd.Next(0, lenA - comun + 1);
            int posB = rnd.Next(0, lenB - comun + 1);

            A = A.Substring(0, posA) + fragmento + A.Substring(posA + comun);
            B = B.Substring(0, posB) + fragmento + B.Substring(posB + comun);
        }

        var lines = new List<string> { A, B };
        string inputFile = Path.Combine(inputDirectory, $"datos{d:0000}.txt");
        File.WriteAllLines(inputFile, lines);
        Console.WriteLine($"{inputFile} (|A|={A.Length}, |B|={B.Length})");
    }
}

