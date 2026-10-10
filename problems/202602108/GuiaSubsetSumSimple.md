# Guía docente

Esta versión reemplaza la anterior con presupuesto. Se elimina B de la entrada y cualquier restricción de capital; se conservan D, M y G. M es solamente informativo. La entrada empieza con N X y la salida es SI o NO. Cada inversión puede elegirse una sola vez.

La matriz booleana dp[i,s] indica si alguna combinación de las primeras i ganancias suma exactamente s unidades de 100 Bs. Se inicializa dp[i,0]=true; los demás estados empiezan en false. La recurrencia compara excluir con incluir mediante OR y siempre utiliza la fila anterior. No se calculan costos mínimos ni se reconstruyen combinaciones.

Solo G y X se dividen entre 100 para los índices. Tiempo O(N*X/100), espacio O(N*X/100). La matriz máxima contiene 61*30001 booleanos, aproximadamente 1,75 MiB para las celdas.

El generador se abre y se guarda en LINQPad antes de F5. Crea IN_SubsetSum y OUT_SubsetSum al lado del .linq para evitar mezclar esta versión con la anterior. Contiene 100 entradas y 100 respuestas, con 50 SI y 50 NO. Incluye casos de suma imposible, X=0, repetición prohibida y ganancias duplicadas. Cada resultado se compara con otra implementación 1D y con la respuesta esperada construida.

El entorno no tiene compilador C# ni LINQPad, por lo que no se ejecutaron aquí esos archivos. La recurrencia se comprobó por enumeración exhaustiva en casos pequeños y el Word se revisó visualmente.
