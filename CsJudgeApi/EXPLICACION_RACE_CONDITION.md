# Prevención de Race Conditions al Cambiar de Problema en el Juez Virtual

## ¿Por qué es necesario?
Cuando el usuario cambia rápidamente de problema en el select, pueden quedar varias peticiones asíncronas pendientes (fetch, carga de datasets, sincronización). Si no se controla, una respuesta atrasada puede sobrescribir la interfaz con datos de un problema anterior.

---

## Solución: Token de Cambio Global

Se implementa un **token global** (`problemChangeToken`) que se incrementa cada vez que el usuario selecciona un nuevo problema. Todas las funciones asíncronas que dependen del problema seleccionado reciben el valor del token al iniciar y lo verifican antes de actualizar la UI.

---

## Diagrama de Flujo

```
[Usuario elige Problema 1] ---> problemChangeToken = 1
    |---> fetch/loadInputs (usa myToken = 1)
[Usuario elige Problema 2] ---> problemChangeToken = 2
    |---> fetch/loadInputs (usa myToken = 2)

(respuesta de P1 llega después)
    |---> myToken (1) !== problemChangeToken (2) --> NO actualiza la UI

(respuesta de P2 llega)
    |---> myToken (2) === problemChangeToken (2) --> SÍ actualiza la UI
```

---

## ¿Dónde se verifica el token?
- Al cambiar de problema (evento `change` del select)
- Al cargar los datasets (`loadInputs`)
- Al seleccionar un archivo de dataset (evento `change` del select de datasets)
- Antes de actualizar cualquier parte de la UI relacionada con el problema

---

## Ejemplo de Código

```js
// Token global
let problemChangeToken = 0;

// Al cambiar de problema
select.addEventListener("change", async () => {
  const myToken = ++problemChangeToken;
  // ...
  await loadInputs(id, myToken);
  if (myToken !== problemChangeToken) return;
  // ...
});

// En loadInputs y eventos internos
async function loadInputs(problemId, token) {
  const myToken = typeof token === 'undefined' ? problemChangeToken : token;
  // ...
  select.addEventListener("change", async () => {
    if (myToken !== problemChangeToken) return;
    // ...
  });
}
```

---

## Beneficio
Solo la última selección del usuario puede actualizar la interfaz. Las respuestas atrasadas de selecciones anteriores se ignoran automáticamente.

---

**¡Así se previenen los race conditions en la UI del Juez Virtual!**
