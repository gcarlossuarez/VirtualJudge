using System;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        string[] lines = Console
            .In.ReadToEnd()
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        int idx = 0;
        int T = int.Parse(lines[idx++]); // Lee el total de secuencias de montaje de los componentes base

        for (int t = 0; t < T; t++)
        {
            string[] nm = lines[idx++].Split(' ');
            int N = int.Parse(nm[0]); // Longitud de la secuencia fuente
            int M = int.Parse(nm[1]); // Longitud de la secuencia destino;

            string[] source = lines[idx++].Split(' ', StringSplitOptions.RemoveEmptyEntries); // Lee todos los componestes de la secuencia fuente
            string[] target = lines[idx++].Split(' ', StringSplitOptions.RemoveEmptyEntries); // Lee todos los componestes de la secuencia destino

            // DP: dp[i][j] = min cost for source[0..i) to target[0..j)
            int[,] dp = new int[N + 1, M + 1];
            for (int i = 0; i <= N; i++)
                dp[i, 0] = i * 1; // deletes
            for (int j = 0; j <= M; j++)
                dp[0, j] = j * 3; // inserts
            /*
                (0)     (3)     (6)     (9)     (12)    (15)
                        M-M 	B-R 	T-A 	C-1 	S-N 	O-M (Si no hay nada en el modelo viejo se insertan todos los del nuevo
Si no hay   M-A (1)
nada en el  B-B	(2)
modelo      T-M (3)
nuevo, se   C-1 (4)
eliminan    O-E	(5)
todos los
del viejo,
por estar obsoleto
*/

            for (int i = 1; i <= N; i++)
            {
                for (int j = 1; j <= M; j++)
                {
                    // Substitute or match
                    int subCost = 0;
                    if (source[i - 1] == target[j - 1])
                    {
                        subCost = 0; // Iguales, no tiene costo
                    }
                    else if (source[i - 1][0] == target[j - 1][0])
                    {
                        subCost = 2; // Sustitución, costo es 2
                    }
                    else
                    {
                        subCost = 4; // incompatible: delete + insert: costo es 1 + 3
                    }

                    //if (
                    //    (subCost == 4)
                    //    && (dp[i - 1, j - 1] + subCost) < (dp[i - 1, j] + 1)
                    //    && (dp[i - 1, j - 1] + subCost) < (dp[i, j - 1] + 3)
                    //)
                    //{
                    //    throw new Exception("Existe un caso");
                    //}
                    dp[i, j] = Math.Min(
                        dp[i - 1, j - 1] + subCost, // update o; sino, delete + insert
                        Math.Min(
                            dp[i - 1, j] + 1, // delete puro
                            dp[i, j - 1] + 3 // insert puro
                        )
                    );
                    // Ejemplo Mini de Matriz DP: Origen ["X-A", "Z-1"] -> Destino ["Y-B", "Z-1"]
                    // (Demostrando por qué +4 en diagonal puede ser óptimo globalmente, gracias a coincidencia posterior)
                    // Costo mínimo: 4
                    // Filas: Origen (i=0 a 2), Columnas: Destino (j=0 a 2)
                    // Cálculo: min(diagonal + sub_cost, arriba +1 (delete), izquierda +3 (insert))
                    // sub_cost: 0 si iguales, 2 si prefijo igual, 4 si incompatibles ('X'!='Y', 'Z'=='Z')

                    //          j=0     j=1(Y-B)  j=2(Z-1)
                    // i=0      0        3         6
                    // i=1(X-A) 1        4         7
                    // i=2(Z-1) 2        5         4

                    // Ruta óptima (backtrack desde [2][2]):
                    // [2][2] <- [1][1] + sub Z-1->Z-1 (+0, match)  -> Total: 4 + 0 = 4
                    // [1][1] <- [0][0] + sub X-A->Y-B (+4, incompatible: prefijo 'X'!='Y')
                    //
                    // Operaciones: sub X-A->Y-B (4, equiv. delete+insert) + match Z-1 (0) = 4
                    //
                    // Nota: Sin el +4 en diagonal, si elige delete en [1][1] (1+3=4 anyway), pero al final,
                    // la coincidencia Z permite que el total sea bajo. Opciones alternativas en [2][2]:
                    // - Eliminar: [1][2] +1 =7+1=8 (peor, desalinea Z)
                    // - Insertar: [2][1] +3=5+3=8 (peor, ignora Z en origen)
                    // La diagonal con +4 "avanza" ambas secuencias, ahorrando en el match posterior.
                }
            }

            Console.WriteLine(dp[N, M]);
        }
    }
}
