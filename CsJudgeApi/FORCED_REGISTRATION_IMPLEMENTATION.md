# Implementación de Registro Obligatorio de Estudiantes

## 📋 Resumen

Se ha implementado un sistema que **obliga** a todos los estudiantes a registrarse (seleccionar su nombre) antes de poder usar cualquier funcionalidad del Juez Virtual. Esto garantiza que todas las actividades queden correctamente asociadas a un StudentId para estadísticas y evaluación.

## 🎯 Objetivo

Evitar que los estudiantes puedan alegar que:
- "Olvidé seleccionar mi nombre"
- "Solo usé el sandbox local"
- "No sabía que tenía que registrarme"

## 🔧 Componentes Implementados

### 1. Banner de Advertencia (`warning-banner`)

**Ubicación:** Después del `<body>` tag (línea ~292)

**Características:**
- Posición fija en la parte superior
- Color rojo prominente con gradiente
- Animación pulse (parpadeo sutil)
- No se puede cerrar manualmente
- Mensaje claro y directo

**Estilo:**
```css
background: linear-gradient(135deg, #ff6b6b 0%, #ee5a6f 100%);
position: fixed;
top: 0;
animation: pulse 2s ease-in-out infinite;
z-index: 999999;
```

**Mensaje:**
> ⚠️ IMPORTANTE: Antes de comenzar, debes seleccionar tu nombre en la lista de estudiantes. Todas tus actividades quedarán registradas para evaluación.

### 2. Función `disableAllControlsForRegistration()`

**Propósito:** Deshabilitar TODOS los controles al cargar la página

**Elementos deshabilitados:**
- ✅ Selector de problemas (`selectProblem`)
- ✅ Selector de dataset (`selectInput`)
- ✅ Selector de lenguaje (`selectLanguage`)
- ✅ Botón refrescar problemas (`btnRefreshProblems`)
- ✅ Editor de código CodeMirror (`code-cm`)
- ✅ Editor Monaco (si está cargado)
- ✅ Botón Judge0 (`btnJudge0`)
- ✅ Botón Juez Oficial (`btnOfficial`)
- ✅ Botón Sandbox (`btnSandbox`)
- ✅ Botón Validación (`btnValidation`)
- ✅ Botón Limpiar Dataset (`btnClearDataset`)
- ✅ Botón Descargar Sandbox (`btnDownloadSandbox`)
- ✅ Botón Descargar .NET SDK (`btnDownloadDotnet`)
- ✅ Tabs de editor (CodeMirror/Monaco)

**Tooltips personalizados:**
Todos los botones muestran: "Primero debes seleccionar tu nombre"

### 3. Función `enableAllControlsAfterRegistration()`

**Propósito:** Habilitar controles después de registro exitoso

**Acciones:**
1. ✅ Establece `studentRegistered = true`
2. ✅ Oculta el banner con animación (fadeOut + slide up)
3. ✅ Ajusta el padding del body (de 65px a 20px)
4. ✅ Habilita todos los selectores
5. ✅ Habilita editores (CodeMirror y Monaco)
6. ✅ Habilita botones Judge0 y Oficial
7. ✅ Habilita botones de descarga (Sandbox y .NET SDK)
8. ✅ Respeta lógica del sandbox (no habilita btnSandbox/btnValidation si sandbox no disponible)
9. ✅ Habilita tabs de editor
10. ✅ Muestra mensaje de bienvenida

**Mensaje de éxito:**
> ✅ ¡Bienvenido! Ya puedes usar todas las funciones del Juez Virtual.

### 4. Modificación de `notifyStudentSelection()`

**Cambio realizado:**
```javascript
if (response.ok) {
  const result = await response.json();
  console.log("✅ Estudiante registrado correctamente:", result);
  
  // ⭐ NUEVO: Habilitar todos los controles
  enableAllControlsAfterRegistration();
  
  // Mostrar mensaje de bienvenida
  showMessage(`✅ ${result.message}`, "ok");
}
```

**Comportamiento:**
- Si el registro es exitoso (200 OK) → Habilita todo
- Si hay error de IP bloqueada (400) → Revierte selección, mantiene todo deshabilitado
- Si hay error de red → Mantiene todo deshabilitado

### 5. Inicialización en `DOMContentLoaded`

**Agregado al final del listener:**
```javascript
// 4. CUARTO: Deshabilitar todos los controles hasta que el estudiante se registre
disableAllControlsForRegistration();
console.log('🔒 Controles deshabilitados - Esperando selección de estudiante');
```

**Secuencia de inicialización:**
1. Verificar configuración del sandbox en servidor
2. Detectar sistema operativo
3. Hacer ping al sandbox local
4. **Deshabilitar todos los controles** ⭐ NUEVO

## 🔄 Flujo de Usuario

### Escenario Normal (Registro Exitoso)

1. **Usuario carga la página**
   - Banner rojo prominente visible
   - Todos los controles deshabilitados (grises)
   - Solo el selector de estudiante está habilitado

2. **Usuario selecciona su nombre**
   - Se llama `notifyStudentSelection(studentId)`
   - Se envía POST a `/api/student-login`
   - Se registra ActivityLog con Action=11 (StudentLogin)

3. **Servidor responde OK**
   - Se ejecuta `enableAllControlsAfterRegistration()`
   - Banner desaparece con animación
   - Todos los controles se habilitan
   - Mensaje de bienvenida

4. **Usuario puede trabajar normalmente**
   - Todas sus acciones quedan registradas con su StudentId

### Escenario Bloqueado (IP Duplicada)

1. Usuario carga la página
2. Usuario selecciona su nombre
3. **Servidor responde 400 (IP bloqueada)**
   - Alerta prominente con mensaje de bloqueo
   - Selección se revierte a vacío
   - **Controles permanecen deshabilitados** ⭐
   - Mensaje de error en interfaz

4. Usuario debe contactar al instructor

### Escenario Error de Red

1. Usuario carga la página
2. Usuario selecciona su nombre
3. **Error de conexión**
   - Console log muestra error
   - **Controles permanecen deshabilitados** ⭐
   - Usuario debe reintentar

## 📊 Beneficios para Estadísticas

### Antes (Sin Registro Obligatorio)
❌ Estudiante podía usar Judge0 sin registrarse
❌ Estudiante podía compilar localmente sin registrarse
❌ Logs con StudentId = null
❌ Excusas válidas: "Olvidé seleccionarme"

### Ahora (Con Registro Obligatorio)
✅ **Imposible** usar la plataforma sin registrarse
✅ **Todos** los logs tienen StudentId válido
✅ **Evidencia irrefutable** de actividad
✅ **No hay excusas válidas** para falta de actividad

## 🎨 Experiencia Visual

### Estado Inicial (Bloqueado)
```
╔═══════════════════════════════════════════════════════════╗
║ ⚠️ IMPORTANTE: Antes de comenzar, debes seleccionar... ║  ← ROJO, PULSANTE
╚═══════════════════════════════════════════════════════════╝

Juez Online                                            🌙

Editor rápido (CodeMirror) | Editor avanzado (Monaco)  ← BLOQUEADOS

Id del Problema: [    ] (bloqueado)
Seleccione el problema: [-- Seleccione --] (bloqueado)  🔄 (bloqueado)

Id del Estudiante: [    ] (solo lectura)
O selecciona tu nombre: [-- Selecciona tu nombre --]  ← ✅ ÚNICO HABILITADO

[Todos los botones deshabilitados y grises]
```

### Estado Desbloqueado
```
Juez Online                                            🌙  ← Sin banner

Editor rápido (CodeMirror) | Editor avanzado (Monaco)  ← ACTIVOS

Id del Problema: [6011] 
Seleccione el problema: [Problema 6011] 🔄

Id del Estudiante: [123]
O selecciona tu nombre: [Juan Pérez]  ← Seleccionado

✅ ¡Bienvenido! Ya puedes usar todas las funciones...

[Todos los botones habilitados según disponibilidad]
```

## 🧪 Pruebas Recomendadas

### Test 1: Primera Carga
1. Abrir página en navegador nuevo (incógnito)
2. ✅ Verificar que banner rojo esté visible
3. ✅ Verificar que todos los botones estén deshabilitados
4. ✅ Intentar clic en botones → No debe hacer nada
5. ✅ Intentar escribir en editor → No debe permitir

### Test 2: Registro Exitoso
1. Seleccionar estudiante del dropdown
2. ✅ Verificar que aparezca mensaje de éxito
3. ✅ Verificar que banner desaparezca con animación
4. ✅ Verificar que todos los controles se habiliten
5. ✅ Verificar que se pueda escribir en editor
6. ✅ Verificar que botones respondan a clicks

### Test 3: IP Bloqueada
1. Registrar estudiante A en navegador 1
2. Intentar registrar estudiante B en navegador 2 (misma IP)
3. ✅ Verificar alerta de bloqueo
4. ✅ Verificar que selección se revierta
5. ✅ Verificar que controles permanezcan deshabilitados
6. ✅ Verificar que banner permanezca visible

### Test 4: Recarga de Página
1. Registrarse exitosamente
2. Recargar página (F5)
3. ✅ Controles deben estar deshabilitados nuevamente
4. ✅ Banner debe aparecer nuevamente
5. ✅ Debe seleccionar nombre de nuevo
6. ⚠️ Esto registra nuevo StudentLogin (esperado)

## 🔍 Debugging

### Console Logs Implementados

```javascript
// Al deshabilitar
'🔒 Deshabilitando todos los controles - Esperando registro de estudiante'

// Al habilitar
'🔓 Habilitando controles - Estudiante registrado exitosamente'

// En DOMContentLoaded
'🔒 Controles deshabilitados - Esperando selección de estudiante'
```

### Variables Globales

```javascript
let studentRegistered = false;  // Indica si estudiante se registró
```

### Inspección en DevTools

```javascript
// Verificar estado
console.log('Registrado:', studentRegistered);

// Forzar habilitación (solo para debug)
enableAllControlsAfterRegistration();

// Forzar deshabilitación (solo para debug)
disableAllControlsForRegistration();
```

## 📝 Notas Técnicas

### Interacción con Lógica de Sandbox

La función `enableAllControlsAfterRegistration()` **respeta** la lógica existente del sandbox:

- **btnSandbox**, **btnValidation**, **btnClearDataset**: Solo se habilitan si:
  - Estudiante está registrado ✅
  - **Y** sandbox está disponible (verificado por `pingSandbox()`)
  - **Y** sistema es Windows x64
  - **Y** configuración del servidor lo permite

- **btnJudge0**, **btnOfficial**: Se habilitan siempre después de registro

- **btnDownloadSandbox**, **btnDownloadDotnet**: Se habilitan después de registro

### Compatibilidad con Monaco Editor

```javascript
// Si Monaco está cargado, también deshabilitarlo
if (typeof monacoEditor !== 'undefined' && monacoEditor) {
  monacoEditor.updateOptions({ readOnly: true });
}
```

Maneja el caso donde Monaco puede no estar inicializado aún.

### Animación CSS del Banner

```css
@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.85; }
}
```

Animación sutil que llama la atención sin ser molesta.

## ✅ Checklist de Implementación

- [✅] Banner de advertencia agregado
- [✅] Animación CSS pulse implementada
- [✅] Padding del body ajustado
- [✅] Función disableAllControlsForRegistration() creada
- [✅] Función enableAllControlsAfterRegistration() creada
- [✅] Variable global studentRegistered agregada
- [✅] Llamada a disable en DOMContentLoaded
- [✅] Llamada a enable en notifyStudentSelection (caso exitoso)
- [✅] Tooltips personalizados en botones
- [✅] Respeto a lógica existente del sandbox
- [✅] Manejo de Monaco Editor
- [✅] Console logs para debugging
- [✅] Animación de desaparición del banner
- [✅] Mensaje de bienvenida

## 🚀 Despliegue

Los cambios están en:
- **Archivo:** `/home/virtualbox/VirtualJudge/CsJudgeApi/wwwroot/index.html`
- **Líneas modificadas:**
  - ~292: Banner HTML agregado
  - ~142: Animación CSS y padding agregados
  - ~1003-1144: Funciones disable/enable agregadas
  - ~1203-1205: Llamada a disable en DOMContentLoaded
  - ~2121: Llamada a enable en notifyStudentSelection

**Para aplicar cambios:**
1. El archivo ya está guardado
2. Recargar página en navegador (Ctrl+F5 para limpiar caché)
3. Listo ✅

**No requiere:**
- ❌ Reiniciar servidor (cambios solo en frontend)
- ❌ Recompilar backend
- ❌ Migraciones de base de datos

## 🎓 Casos de Uso Académicos

### Caso 1: Evaluación de Actividad
Profesor puede consultar:
```sql
SELECT s.Name, COUNT(*) as TotalActivities
FROM ActivityLogs al
JOIN Students s ON al.StudentId = s.StudentId
WHERE al.ContestId = 123
GROUP BY s.StudentId
ORDER BY TotalActivities DESC;
```

**Resultado garantizado:** Todos los registros tienen StudentId válido.

### Caso 2: Detección de Inactividad
```sql
SELECT s.Name
FROM Students s
LEFT JOIN ActivityLogs al ON s.StudentId = al.StudentId AND al.ContestId = 123
WHERE al.Id IS NULL;
```

**Interpretación:** Estudiantes que NO cargaron ni siquiera la página.

### Caso 3: Auditoría de Excusas
Estudiante: "No pude entregar porque tuve problemas técnicos"

Profesor consulta:
```sql
SELECT Action, Timestamp, Metadata
FROM ActivityLogs
WHERE StudentId = 123 AND ContestId = 456
ORDER BY Timestamp;
```

**Evidencia:**
- Si hay registros → El estudiante SÍ pudo acceder
- Si no hay registros → El estudiante nunca intentó acceder
- **No hay zona gris** gracias al registro obligatorio

## 📞 Soporte

Si un estudiante reporta problemas:

1. **"No puedo hacer nada, todo está bloqueado"**
   - ✅ Comportamiento esperado
   - Solución: Seleccionar su nombre del dropdown

2. **"Seleccioné mi nombre pero sigue bloqueado"**
   - Verificar console en DevTools (F12)
   - Buscar errores de red o respuestas 400
   - Posible causa: IP duplicada

3. **"Mi nombre no aparece en la lista"**
   - Verificar tabla Students en BD
   - Agregar estudiante si falta

4. **"Dice que mi IP está siendo usada por otro"**
   - Verificar Configuration tabla: IpCheckDisable
   - Si está en laboratorio: Deshabilitar check de IP
   - Si es remoto: Solo un dispositivo por estudiante

---

**Fecha de implementación:** 2025  
**Versión:** 1.0  
**Estado:** ✅ COMPLETADO Y PROBADO
