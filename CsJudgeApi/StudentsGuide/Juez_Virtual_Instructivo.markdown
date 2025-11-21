# Instructivo de Uso del Juez Virtual

## 🎯 1. Objetivo

El **Juez Virtual** no es lo mismo que el **Judge de prueba**. Aquí te explicamos la diferencia:

- **Judge (prueba):** Sirve para probar tu código antes de enviarlo. Es un entorno de pruebas local o en CodeMirror.
- **Juez Virtual (oficial):** Es el encargado de corregir y calificar tu código de manera oficial.

> **⚠️ Importante:** Un programa que funciona en el Judge de prueba **puede fallar** en el Juez Virtual si la salida no coincide **exactamente** con los archivos de salida esperados.

---

## ⚙️ 2. Flujo de Trabajo

### ⚠️ PASO 0: Identificación Obligatoria

**Antes de poder usar cualquier función de la plataforma, DEBES identificarte:**

1. **Selecciona tu nombre** en el desplegable superior de estudiantes
2. **Aparecerá un mensaje de bienvenida** con tu nombre completo y tu ID
3. **Verifica que sea tu nombre correcto** - esto es importante porque todas tus actividades se registran con tu identidad
4. **Solo después de identificarte** se habilitarán todos los controles (editor, botones, selección de problemas, etc.)

> **📌 Importante:** Todos los controles de la plataforma permanecen deshabilitados hasta que te identifiques correctamente. Esto asegura que todas tus entregas y prácticas se registren bajo tu nombre.

---

### Flujo de Trabajo General:

1. **👤 Identifícate** seleccionando tu nombre (OBLIGATORIO - ver PASO 0 arriba).
2. **📚 Selecciona un problema** del concurso/práctica activa.
3. **✍️ Escribe tu programa** (por ejemplo, en C#).
4. **🧪 [OPCIONAL] Pruébalo en Sandbox Local** (recomendado - ver Sección 6).
5. **🧪 Pruébalo en Judge (local o CodeMirror):**
   - Aquí verificas que tu programa corre y responde correctamente a entradas simples.
6. **📤 Envía tu programa al Juez Virtual:**
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

## 🏗️ 6. Sandbox Local (Recomendado)

El **Sandbox Local** es una herramienta **opcional pero altamente recomendada** que te permite probar y validar tu código en tu propia máquina, sin depender del servidor remoto.

### ¿Qué es el Sandbox?
- Es un entorno de ejecución local que corre en tu computadora
- Permite ejecutar código C# y validar con los datasets del problema seleccionado
- Funciona sin conexión a internet una vez instalado y sincronizado
- **Solo disponible para Windows 64 bits**
- **Requiere .NET SDK 10** instalado en tu sistema

### ✅ Ventajas del Sandbox
- ✅ **Reduce la carga del servidor:** Especialmente útil cuando hay muchos estudiantes conectados simultáneamente
- ✅ **Funciona con red inestable:** No necesitas conexión constante al servidor una vez descargados los datasets
- ✅ **Validación instantánea:** Prueba tu código con todos los casos de prueba en segundos
- ✅ **Sin límite de intentos:** Puedes probar tantas veces como quieras sin afectar al servidor ni consumir recursos compartidos
- ✅ **Mayor autonomía:** Trabaja de forma independiente incluso si el servidor está caído

---

### 📦 Instalación Inicial del Sandbox (Solo Primera Vez)

#### Requisitos Previos:
1. **Sistema Operativo:** Windows 64 bits
2. **Estudiante Seleccionado:** Debes haber seleccionado tu nombre en el desplegable superior
3. **.NET SDK 10:** Instalado en tu sistema (puedes descargarlo desde [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download) o usar el botón en la interfaz)
4. **Problema Seleccionado:** Debes tener un problema cargado para poder descargar los datasets correspondientes

#### Pasos de Instalación:

1. **Descargar el Sandbox:**
   - Haz clic en el botón **"📥 Descargar Sandbox"** en la interfaz
   - Se descargará un archivo ZIP con el ejecutable del servidor local

2. **Extraer el Archivo:**
   - Extrae el contenido del ZIP en una carpeta de tu elección (ej: `C:\SandboxJuez\`)
   - **Importante:** Guarda bien la ubicación de esta carpeta para usos futuros

3. **Ejecutar el Sandbox:**
   - Navega a la carpeta extraída
   - Ejecuta el archivo `DotNetInteractiveServer.exe`
   - **Se abrirá una ventana de consola** que debe permanecer abierta mientras usas el sandbox
   - **NO cierres esta ventana** hasta terminar tu sesión de trabajo

4. **Permitir Comunicación Local (Cloudflare/Navegador):**
   - **MUY IMPORTANTE:** Al ejecutar el sandbox por primera vez, tu navegador (especialmente si usas Cloudflare WARP o similar) mostrará una alerta:
     > "Cloudflare quiere comunicarse con dispositivos de tu red local"
   - **DEBES PERMITIR** esta comunicación para que la interfaz web pueda conectarse con tu sandbox local
   - Sin este permiso, los botones del sandbox no funcionarán

5. **Verificación de Conexión:**
   - Una vez ejecutado el sandbox, los botones **"🧪 Probar en Sandbox"** y **"📂 Validar con DataSet local"** se habilitarán automáticamente en la interfaz
   - Si no se habilitan, verifica:
     - Que `DotNetInteractiveServer.exe` esté corriendo (ventana de consola abierta)
     - Que hayas permitido la comunicación local en el navegador
     - Que el puerto 1100 no esté ocupado por otra aplicación

---

### 🔄 Uso Diario del Sandbox

Una vez instalado, el uso diario es muy sencillo:

1. **Iniciar el Sandbox:**
   - Ejecuta `DotNetInteractiveServer.exe` (ubicado en la carpeta donde lo extrajiste)
   - Deja la ventana de consola abierta

2. **Abrir el Juez Virtual:**
   - Accede a la interfaz web del juez en tu navegador
   - **Selecciona tu nombre** en el desplegable de estudiantes (obligatorio)
   - Selecciona el problema que deseas resolver

3. **Descargar Datasets del Problema (Automático):**
   - Al seleccionar un problema, el sistema **descargará automáticamente** los archivos de entrada (`Input_datos1.txt`, `Input_datos2.txt`, etc.) y salida esperada (`Output_datos1.txt`, `Output_datos2.txt`, etc.) a tu sandbox local
   - **Esto ocurre en segundo plano**, no necesitas hacer nada
   - Los datasets se guardan en la misma carpeta donde está `DotNetInteractiveServer.exe`

4. **Escribir y Probar Código:**
   - Escribe tu código en el editor Mónaco
   - Usa **"🧪 Probar en Sandbox"** para ejecutar tu código con entrada personalizada (igual que el juez remoto, pero en tu máquina)
   - Usa **"📂 Validar con DataSet local"** para probar tu código contra **todos los casos de prueba oficiales** del problema

---

### 📊 Interpretación de Resultados en Sandbox

Cuando ejecutas **"📂 Validar con DataSet local"**, el sandbox compara la salida de tu código con los archivos `Output_datosX.txt` oficiales:

- **✅ "Salida Correcta: Coincide con Output_datosX.txt"**  
  → Tu código produjo la salida esperada para ese caso de prueba
  
- **❌ "Salida Incorrecta: NO coincide con Output_datosX.txt"**  
  → Tu código produjo una salida diferente. Revisa la lógica de tu algoritmo

- **⚠️ "Error de compilación"**  
  → Tu código tiene errores de sintaxis. Revisa el mensaje de error detallado

- **⏱️ "Timeout"**  
  → Tu código tardó demasiado en ejecutarse (más de 6 segundos por caso). Optimiza tu algoritmo o revisa bucles infinitos

- **💥 "Runtime Error"**  
  → Tu código lanzó una excepción durante la ejecución (división por cero, acceso a índice inválido, etc.)

**Tip:** Si pasas todos los casos en el sandbox pero fallas en el juez oficial, verifica:
- Espacios en blanco extra o saltos de línea adicionales
- Formato exacto de la salida (mayúsculas/minúsculas, separadores)
- Tipos de datos (int vs long, float vs double)

---

### 🔧 Solución de Problemas

#### Los botones del sandbox no se habilitan:
1. Verifica que `DotNetInteractiveServer.exe` esté corriendo (ventana de consola visible)
2. Asegúrate de haber **permitido la comunicación local** en el navegador cuando se solicitó
3. Cierra y vuelve a abrir el navegador, o prueba con otro navegador
4. Verifica que el puerto 1100 no esté ocupado (cierra otras aplicaciones que puedan usarlo)

#### Error "No se pudo conectar al sandbox":
- Reinicia `DotNetInteractiveServer.exe`
- Actualiza la página del juez virtual
- Verifica tu firewall/antivirus no esté bloqueando la comunicación local

#### Los datasets no se descargan automáticamente:
- Verifica que **hayas seleccionado tu nombre de estudiante primero** (obligatorio para cualquier operación)
- Asegúrate de tener conexión a internet al seleccionar el problema por primera vez
- Los datasets se descargan **una sola vez por problema**. Si cambias de problema, se descargan los nuevos automáticamente
- Si sospechas que los datasets están desactualizados, elimina los archivos `Input_datosX.txt` y `Output_datosX.txt` de la carpeta del sandbox y vuelve a seleccionar el problema

#### Resultados diferentes entre sandbox y juez oficial:
- El sandbox usa **los mismos datasets y validador** que el juez oficial
- Si hay diferencias, probablemente tu código tenga comportamiento no determinista (uso de `Random` sin semilla, variables no inicializadas, etc.)
- También puede ser un problema de formato de salida (espacios, saltos de línea)

---

### ⚠️ Importante sobre Calificaciones

- **Calificación Oficial:** Solo se considera válida la **entrega al Juez Oficial** (botón "Enviar en Juez Oficial")
- **Sandbox como respaldo:** En exámenes o evaluaciones calificadas, si hay **fuerte inestabilidad de internet**, el docente puede considerar las validaciones del sandbox local como calificación oficial, previo chequeo
- **Recomendación:** Usa el sandbox para practicar y reducir la carga del servidor, pero **siempre envía tu solución final al Juez Oficial** cuando sea posible

---

### 📌 Resumen Rápido Sandbox

| Acción | Herramienta | ¿Cuándo usarla? |
|--------|-------------|----------------|
| Ejecutar con entrada personalizada | 🧪 Probar en Sandbox | Probar casos específicos mientras desarrollas |
| Validar con todos los casos oficiales | 📂 Validar con DataSet local | Antes de enviar al juez oficial, para asegurarte que pasas todos los casos |
| Enviar para calificación | ✅ Enviar en Juez Oficial | Cuando estés seguro de tu solución y quieras la calificación oficial |

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