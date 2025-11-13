# Instructivo de Uso del Juez Virtual

## 🎯 1. Objetivo

El **Juez Virtual** no es lo mismo que el **Judge de prueba**. Aquí te explicamos la diferencia:

- **Judge (prueba):** Sirve para probar tu código antes de enviarlo. Es un entorno de pruebas local o en CodeMirror.
- **Juez Virtual (oficial):** Es el encargado de corregir y calificar tu código de manera oficial.

> **⚠️ Importante:** Un programa que funciona en el Judge de prueba **puede fallar** en el Juez Virtual si la salida no coincide **exactamente** con los archivos de salida esperados.

---

## ⚙️ 2. Flujo de Trabajo

1. **✍️ Escribe tu programa** (por ejemplo, en C#).
2. **🧪 Pruébalo en Judge (local o CodeMirror):**
   - Aquí verificas que tu programa corre y responde correctamente a entradas simples.
3. **📤 Envía tu programa al Juez Virtual:**
   - El Juez Virtual toma todos los archivos de entrada del dataset (ejemplo: `datos0001.txt`, `datos0002.txt`, …).
   - Tu programa genera salidas por consola.
   - El Juez compara cada salida con el archivo oficial de respuesta (`Output_datos0001.txt`, `Output_datos0002.txt`, …).
   - **Resultado:**
     - Si son **idénticas** → **ACCEPTED** ✅.
     - Si hay **cualquier diferencia** (un espacio, una coma, etc.) → **WRONG ANSWER** ❌.

---

## 📌 3. Ejemplo Visual

### Caso 1: Salida Correcta
**Entrada del juez (`datos0001.txt`):**
```
3
5
```

**Salida de tu programa (por consola):**
```
8
```

**Respuesta oficial (`Output_datos0001.txt`):**
```
8
```

✅ **Coinciden → ACCEPTED**

---

### Caso 2: Error por Formato
**Entrada del juez (`datos0001.txt`):**
```
3
5
```

**Salida de tu programa (por consola):**
```
El resultado es 8
```

**Respuesta oficial (`Output_datos0001.txt`):**
```
8
```

❌ **No coinciden → WRONG ANSWER**

---

## 🔑 4. Puntos Importantes para Recordar

- **✅ Judge de prueba = simulador local.**
- **✅ Juez Virtual = evaluador oficial.**
- **🚫 Que tu programa corra en Judge no garantiza que sea aceptado en el Juez Virtual.**
- **✨ Regla de oro:** La salida de tu programa por consola debe coincidir **exactamente** con el archivo `Output_datos<nro>.txt`.

---

## 🧩 5. Ejemplo con el Esqueleto Dado

**Código (prueba en Judge / CodeMirror):**

```csharp
using System;
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
```

👉 **Problema:** Este programa imprime texto extra (`valor enviado=...`, `Hola CodeMirror!`).

👉 **Resultado:** Aunque funcione en Judge, **no será aceptado** por el Juez Virtual, ya que las salidas no coinciden con el archivo `Output_datos<nro>.txt`.

---

## �️ 6. Sandbox Local (Recomendado)

El **Sandbox Local** es una herramienta opcional que te permite probar y validar tu código en tu propia máquina, sin depender del servidor remoto.

### ¿Qué es el Sandbox?
- Es un entorno de ejecución local que corre en tu computadora
- Permite ejecutar código C# y validar con los datasets del problema
- Funciona sin conexión a internet una vez instalado
- **Solo disponible para Windows 64 bits**

### Ventajas del Sandbox
- ✅ **Reduce la carga del servidor:** Especialmente útil cuando hay muchos estudiantes conectados
- ✅ **Funciona con red inestable:** No necesitas conexión constante al servidor
- ✅ **Validación instantánea:** Prueba tu código con todos los casos de prueba en segundos
- ✅ **Sin límite de intentos:** Puedes probar tantas veces como quieras sin afectar al servidor

### ¿Cómo usar el Sandbox?

**Requisitos:**
1. Windows 64 bits
2. .NET SDK 10 instalado (puedes descargarlo desde el botón en la interfaz)
3. Descargar y ejecutar el Sandbox (DotNetInteractiveServer.exe)

**Pasos:**
1. Haz clic en "📥 Descargar Sandbox" (solo la primera vez)
2. Extrae el archivo ZIP
3. Ejecuta `DotNetInteractiveServer.exe`
4. Los botones del sandbox se habilitarán automáticamente
5. Usa "🧪 Probar en Sandbox" para ejecutar tu código
6. Usa "📂 Validar con DataSet local" para probar todos los casos

### ⚠️ Importante sobre Calificaciones

- **Calificación Oficial:** Solo se considera válida la **entrega al Juez Oficial** (botón "Enviar en Juez Oficial")
- **Sandbox como respaldo:** En exámenes o evaluaciones calificadas, si hay **fuerte inestabilidad de internet**, el docente puede considerar las validaciones del sandbox local como calificación oficial, previo chequeo
- **Recomendación:** Usa el sandbox para practicar y reducir la carga del servidor, pero siempre envía tu solución final al Juez Oficial cuando sea posible

---

## �📋 7. Resumen para Memorizar

1. Escribo el código.
2. **[RECOMENDADO] Lo pruebo en Sandbox Local** (si está disponible).
3. Lo pruebo en Judge de Prueba (simulador).
4. Lo envío al Juez Virtual (Oficial).
5. El Juez Virtual compara mi salida exacta con la salida oficial.
6. Si coinciden → **ACCEPTED**. Si no → **WRONG ANSWER**.

✨ **Consejo final:** Asegúrate de que tu salida sea **idéntica** a la esperada, sin espacios, líneas o caracteres adicionales.

💡 **Consejo extra:** Usa el Sandbox Local siempre que puedas para reducir la carga del servidor y trabajar más rápido, especialmente en condiciones de red inestable.