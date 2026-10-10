# Guía del docente

El problema es mochila 0/1: costo = peso, ganancia = valor, presupuesto = capacidad.

Dividir exclusivamente presupuesto y costos entre 100. Mantener las ganancias en bolivianos: esto permite ganancias como 41 Bs sin pérdida de precisión. No dividir las ganancias ni sumar el capital recuperado a la respuesta.

`dp[i,c]` representa la mayor ganancia en bolivianos considerando las primeras `i` alternativas y una capacidad de hasta `c` dólares ficticios. La fila y columna iniciales valen cero. Si el costo cabe, comparar excluir con incluir usando siempre la fila anterior. Si no cabe, copiar la fila anterior. Devolver `dp[N,B/100]`.

Tiempo: O(N * B/100). Espacio de la matriz: O(N * B/100).

En el máximo, una matriz `long[61,3000001]` requeriría 1 464 000 488 bytes (aproximadamente 1396 MiB) solamente para las celdas. Tras escalar, `long[61,30001]` requiere 14 640 488 bytes (aproximadamente 13,96 MiB). Por eso se fija el límite de 64 MiB para la matriz y se solicita explícitamente la versión bidimensional. Una solución 1D sería válida algorítmicamente, pero no es la versión solicitada en este examen.

Como todos los costos y el presupuesto son múltiplos de 100, la conversión conserva exactamente las carteras factibles. También se puede reducir por el máximo común divisor; si un alumno lo hace correctamente, conserva la exactitud.

## Archivos

- `SolucionCartera.cs`: programa de consola completo. Recibe el archivo como primer argumento o lee la entrada estándar. Imprime solamente la ganancia máxima.
- `Generador100Cartera.linq`: abrir en LINQPad y pulsar F5. Es una consulta C# Program, con solución incluida; no requiere NuGet. Modificar `carpetaSalida` para cambiar el destino.
- El generador crea `entrada001.txt` a `entrada100.txt`, `salida001.txt` a `salida100.txt` y `resumen_docente.csv` en Documentos/ExamenCartera_100Datasets. Usa semilla fija para reproducibilidad dentro del mismo runtime de .NET. Sobrescribe estos archivos si se vuelve a ejecutar.
- Las primeras cinco entradas son casos especiales; las otras 95 son aleatorias grandes. Cada respuesta se compara con una implementación 1D antes de guardarse.
- Distribuir a los estudiantes solamente el enunciado y las entradas. Las salidas y el resumen son para el docente.

Respuestas de los primeros cinco casos: 1400, 0, 246, 150, 140.

## Validación y límites

Se comprobó la recurrencia con casos pequeños por enumeración exhaustiva y se revisaron los casos especiales. El entorno de creación no tiene compilador C# ni LINQPad; estos archivos no se ejecutaron aquí. El generador incorpora la comparación 2D/1D al ejecutarse.
