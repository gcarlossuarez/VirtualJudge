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

## 📋 6. Resumen para Memorizar

1. Escribo el código.
2. Lo pruebo en Judge (simulador).
3. Lo envío al Juez Virtual.
4. El Juez Virtual compara mi salida exacta con la salida oficial.
5. Si coinciden → **ACCEPTED**. Si no → **WRONG ANSWER**.

✨ **Consejo final:** Asegúrate de que tu salida sea **idéntica** a la esperada, sin espacios, líneas o caracteres adicionales.