BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
	"MigrationId"	TEXT NOT NULL,
	"ProductVersion"	TEXT NOT NULL,
	CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY("MigrationId")
);
CREATE TABLE IF NOT EXISTS "Contests" (
	"ContestId"	INTEGER NOT NULL,
	"Date"	TEXT NOT NULL,
	CONSTRAINT "PK_Contests" PRIMARY KEY("ContestId" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "Students" (
	"StudentId"	INTEGER NOT NULL,
	"Name"	TEXT NOT NULL,
	CONSTRAINT "PK_Students" PRIMARY KEY("StudentId" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "Submissions" (
	"SubmissionId"	INTEGER NOT NULL,
	"StudentId"	INTEGER NOT NULL,
	"ProblemId"	TEXT NOT NULL,
	"SourceCode"	TEXT NOT NULL,
	"OutputExpected"	TEXT NOT NULL,
	"OutputActual"	TEXT NOT NULL,
	"IsCorrect"	INTEGER NOT NULL,
	"BuildLog"	TEXT NOT NULL,
	"RunLog"	TEXT NOT NULL,
	"Time"	TEXT,
	"MemKb"	TEXT,
	"CreatedAt"	TEXT NOT NULL,
	"IP"	TEXT NOT NULL,
	CONSTRAINT "PK_Submissions" PRIMARY KEY("SubmissionId" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "Questions" (
	"QuestionId"	INTEGER NOT NULL,
	"Review"	TEXT NOT NULL,
	"Path"	TEXT NOT NULL,
	"ContestId"	INTEGER NOT NULL,
	CONSTRAINT "FK_Questions_Contests_ContestId" FOREIGN KEY("ContestId") REFERENCES "Contests"("ContestId") ON DELETE CASCADE,
	CONSTRAINT "PK_Questions" PRIMARY KEY("QuestionId" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "ContestStudents" (
	"ContestId"	INTEGER NOT NULL,
	"StudentId"	INTEGER NOT NULL,
	"DateParticipation"	TEXT NOT NULL,
	"IP"	TEXT NOT NULL,
	CONSTRAINT "FK_ContestStudents_Contests_ContestId" FOREIGN KEY("ContestId") REFERENCES "Contests"("ContestId") ON DELETE CASCADE,
	CONSTRAINT "PK_ContestStudents" PRIMARY KEY("ContestId","StudentId"),
	CONSTRAINT "FK_ContestStudents_Students_StudentId" FOREIGN KEY("StudentId") REFERENCES "Students"("StudentId") ON DELETE CASCADE
);
INSERT INTO "__EFMigrationsHistory" ("MigrationId","ProductVersion") VALUES ('20250822130511_InitialCreate','8.0.8');
INSERT INTO "Contests" ("ContestId","Date") VALUES (1,'2025-08-23');
INSERT INTO "Students" ("StudentId","Name") VALUES (123,'Estudiante de prueba'),
 (11572641,'NINA URQUIOLA YUVINCA MAYUMI'),
 (202360006963,'TERRAZAS LLANOS FERNANDO'),
 (202360008974,'LEWENSZTAIN ZELAYA DIEGO'),
 (202360009067,'RAMIREZ VALLEJOS ALEJANDRO'),
 (202360009248,'LOZADA LEON NICOLE'),
 (202360009314,'MALLEA ACEBEY ANDRES MATIAS'),
 (202360009415,'BALBONTIN UGARTECHE JOSUE GALO'),
 (202360009416,'MENDOZA UREÑA JOSE AGUSTIN'),
 (202360009454,'LOPEZ MELGAR KRISHNA ARIANY'),
 (202460009864,'OLY SANCHEZ NICOLAS EMANUEL'),
 (202460009969,'GUTIERREZ LARA ISRAEL'),
 (202460009995,'CATORCENO ORELLANA CAMILA ALISON'),
 (202460010231,'ZAMBRANA CRUZ JOSE ALFREDO'),
 (202460010282,'HEREDIA TICONA DIEGO ANDRES'),
 (202460011118,'MIRANDA ROMAN LEANDRO EMILIANO'),
 (202460011392,'CALLEJAS AGUIRRE MIJAEL ANDER'),
 (202460011419,'OJEDA JUSTINIANO ERWIN ALEJANDRO'),
 (202460011510,'ABUAWAD VELASCO OSCAR SANTIAGO'),
 (202460012372,'POL ARAMAYO DARIANA'),
 (202460012571,'MARTINEZ SANCHEZ JORGE');
INSERT INTO "Submissions" ("SubmissionId","StudentId","ProblemId","SourceCode","OutputExpected","OutputActual","IsCorrect","BuildLog","RunLog","Time","MemKb","CreatedAt","IP") VALUES (70,123,'1','using System;
class Program {
  static void Main() {
    Console.WriteLine("Hola CodeMirror!");
  }
}','','No se generó salida.',0,'','',NULL,NULL,'2025-08-24 17:16:46.001105','2800:320:41d7:d201:8979:5c9:48b7:908e'),
 (71,123,'1','using System;

// Ejemplo para ser probado en Monaco (con entrada desde teclado).
class Program
{
    static void Main()
    {
        string val;
        while (!string.IsNullOrEmpty(val = Console.ReadLine()))
        {
            Console.WriteLine($"Valor enviado={val}");
        }
        Console.WriteLine("Hola Monaco!");
    }
}
/*
// Ejemplo de programa minimo, para envio a juez virtual, que acepta el nombre del archivo como argumento desde la linea de comandos
using System;
using System.IO; // Para poder leer el archivo

class Program
{
    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            string fullFilePath = args[0];
            string[] lines = File.ReadAllLines(fullFilePath);
            foreach (string val in lines)
            {
                if (!string.IsNullOrEmpty(val))
                {
                    Console.WriteLine($"valor enviado={val}");
                }
            }
        }
    }
}
*/','','No se generó salida.',0,'','',NULL,NULL,'2025-08-24 18:02:26.9825012','2800:320:41d7:d201:8979:5c9:48b7:908e'),
 (72,202460011510,'1','using System;
// Ejemplo para ser probado en CodeMirror (con entrada desde teclado).
class Program
{
    static void Main()
    {
        string val;
        while (!string.IsNullOrEmpty(val = Console.ReadLine()))
        {
            Console.WriteLine($"valor enviado={val}");
        }
        Console.WriteLine("Hola CodeMirror!");
    }
}
/*
// Ejemplo de programa minimo, para envio a juez virtual, que acepta el nombre del archivo como argumento desde la linea de comandos
using System;
using System.IO; // Para poder leer el archivo

class Program
{
    static void Main(string[] args)
    {
		
		  if(args.Length > 0);
		  {
			  string fullFilePath = args[0];
			  string[] lines = File.ReadAllLines(fullFilePath);
			  // Resto de su codigo
			}
		}
}
*/
','','No se generó salida.',0,'','',NULL,NULL,'2025-08-25 00:15:07.9800075','2800:320:41d7:d201:8979:5c9:48b7:908e'),
 (73,202460011510,'2','
using System;
using System.Collections.Generic;
using System.IO;


class Program
{
	
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Debe proporcionar la ruta del archivo de entrada como argumento.");
            return;
        }

        string inputFile = args[0];
        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"El archivo {inputFile} no existe.");
            return;
        }

        // Leer los enteros del archivo
        string line = File.ReadAllText(inputFile).Trim();
        string[] parts = line.Split('','');
        int[] arr = Array.ConvertAll(parts, int.Parse);

        // Generar permutaciones
        List<int[]> permutations = new List<int[]>();
        permutations = GeneratePermutations(arr, 0, arr.Length - 1, permutations);

        // Imprimir las permutaciones
        foreach (int[] permutation in permutations)
        {
            Console.WriteLine(string.Join(",", permutation));
        }
    }
	static List<int[]> GeneratePermutations(int[] arr, int l, int r, List<int[]> permutationsList)
    {
        if (l == r)
        {
            int[] permutation = new int[arr.Length];
            Array.Copy(arr, 0, permutation, 0, arr.Length);
            permutationsList.Add(permutation);
            return permutationsList;
        }

        for (int i = l; i <= r; i++)
        {
            Swap(arr, l, i);
            permutationsList = GeneratePermutations(arr, l + 1, r, permutationsList);
            Swap(arr, l, i); // backtrack
        }

        return permutationsList;
    }

    static void Swap(int[] arr, int i, int j)
    {
        int temp = arr[i];
        arr[i] = arr[j];
        arr[j] = temp;
    }
}
','','No se generó salida.',1,'','',NULL,NULL,'2025-08-25 00:16:50.9721878','2800:320:41d7:d201:8979:5c9:48b7:908e');
INSERT INTO "Questions" ("QuestionId","Review","Path","ContestId") VALUES (1,'Promedio','/home/vboxuser/problems/1',1),
 (2,'Permutaciones','/home/vboxuser/problems/2',1);
INSERT INTO "ContestStudents" ("ContestId","StudentId","DateParticipation","IP") VALUES (1,123,'0001-01-01 00:00:00','2800:320:41d7:d201:8979:5c9:48b7:908e');
CREATE INDEX IF NOT EXISTS "IX_ContestStudents_StudentId" ON "ContestStudents" (
	"StudentId"
);
CREATE INDEX IF NOT EXISTS "IX_Questions_ContestId" ON "Questions" (
	"ContestId"
);
COMMIT;
