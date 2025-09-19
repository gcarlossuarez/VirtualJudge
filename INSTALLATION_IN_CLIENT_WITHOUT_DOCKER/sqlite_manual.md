# 📘 Manual Personal de Uso de SQLite en el Juez Virtual

## 1. Instalación y uso básico de `sqlite3`

Primero instalé la herramienta de línea de comandos de SQLite, porque siempre es útil tenerla disponible en el servidor:

```bash
sudo apt-get install -y sqlite3
```

La uso para inspeccionar rápido la base de datos del juez (`submissions.db`):

```bash
sqlite3 ~/VirtualJudge/VirtualJudge/CsJudgeApi/submissions.db
```

Comandos útiles dentro del prompt de `sqlite3`:
```sql
.tables               -- ver tablas
.schema Problems      -- ver estructura de la tabla Problems
SELECT * FROM Problems LIMIT 5;   -- ver los primeros 5 registros
```

👉 Uso `sqlite3` porque es **ligero y rápido**. Me sirve para validar si las tablas existen o si hay datos cargados, sin necesidad de interfaz gráfica.

---

## 2. Instalación de DB Browser (`sqlitebrowser`)

También instalé **DB Browser for SQLite** en el servidor:

```bash
sudo apt-get install -y sqlitebrowser
```

Este programa es gráfico (usa Qt).  
El problema es que cuando lo intenté abrir por SSH me dio error:

```
qt.qpa.xcb: could not connect to display
```

👉 Eso pasa porque `sqlitebrowser` necesita un **servidor gráfico (X11)** y mi conexión SSH es solo de texto.  

Por eso tengo dos alternativas:
- Copiar el `.db` a mi PC con `scp` y abrirlo con DB Browser instalado en Windows.
- O usar `ssh -X` con **XLaunch** (VcXsrv) para redirigir ventanas gráficas a mi PC.  

Para el día a día, prefiero la primera opción:  
```powershell
scp docente@IP_DEL_SERVER:~/VirtualJudge/VirtualJudge/CsJudgeApi/submissions.db .
```
Así abro el archivo en Windows con DB Browser cómodamente.

---

## 3. Exploración vía Web con `sqlite-web`

Instalé **sqlite-web**, una aplicación en Python que monta una interfaz web ligera sobre la base de datos:

```bash
pip install --user sqlite-web
```

Como `pip` instaló los binarios en `~/.local/bin`, tuve que ejecutarlo con la ruta completa:

```bash
~/.local/bin/sqlite_web ~/VirtualJudge/VirtualJudge/CsJudgeApi/submissions.db --port 8090
```

Por defecto, corre en `http://127.0.0.1:8090`.

👉 Uso `sqlite-web` porque es una forma **simple, rápida y accesible desde el navegador** para explorar tablas y ejecutar consultas SQL, sin depender de GUI pesada.

---

## 4. Acceso desde mi máquina con **SSH Tunneling**

Como `sqlite-web` corre en `127.0.0.1:8090` dentro del servidor, no puedo abrirlo directamente desde mi navegador en Windows.  
Aquí es donde entra el **tunneling SSH**:

En mi PowerShell local hago:

```powershell
ssh -L 8090:127.0.0.1:8090 docente@IP_DEL_SERVER
```

Esto crea un túnel que:
- Escucha en mi máquina local en el puerto `8090`.
- Redirige el tráfico a `127.0.0.1:8090` dentro del servidor (donde corre sqlite-web).

De esta manera, abro en mi Windows:
👉 [http://localhost:8090](http://localhost:8090)  
y veo la interfaz de sqlite-web como si estuviera corriendo localmente.

---

## 5. Por qué uso cada herramienta

- **sqlite3 (CLI)**: validaciones rápidas, consultas simples sin entorno gráfico.  
- **sqlitebrowser (GUI nativa)**: ideal en Windows cuando quiero editar o explorar con interfaz más amigable.  
- **sqlite-web (interfaz web)**: me da la flexibilidad de acceder vía navegador, incluso con tunneling, sin depender de escritorio remoto.  
- **SSH Tunneling**: lo uso porque no quiero exponer directamente puertos en la red de la universidad. El túnel es **seguro, cifrado** y me permite acceder a servicios internos (como sqlite-web en 8090) desde mi máquina local.  

---

## 6. Buenas prácticas

- Mantengo `sqlite-web` en puertos distintos de `8080` (por ejemplo, `8090`) para evitar conflictos con Docker u otros servicios.  
- Uso `scp` para bajar o subir la base de datos cuando quiero trabajarla en Windows con DB Browser.  
- Si necesito abrir puertos hacia afuera, prefiero **ngrok** o **túneles SSH**, nunca dejar expuesto el servidor directamente.  

---

👉 Con esto tengo un **manual propio** que me recuerda por qué elegí cada herramienta y cómo las uso en conjunto para manejar `submissions.db` en mi Juez Virtual.  
