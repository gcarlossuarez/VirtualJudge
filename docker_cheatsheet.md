# 🐳 Chuleta de comandos Docker (VirtualJudge)

## 🔎 Ver contenedores
```bash
docker ps
```
Muestra los contenedores en ejecución (ID, imagen, puertos, estado).  
👉 Útil para confirmar que `virtualjudge_backend` y `virtualjudge_runner` están levantados.

```bash
docker ps -a
```
Muestra **todos los contenedores**, incluso los detenidos.  

---

## 📜 Ver logs
```bash
docker logs <nombre_contenedor>
```
Muestra los logs de un contenedor.  

```bash
docker logs -f <nombre_contenedor>
```
Sigue los logs en vivo (modo *follow*, como `tail -f`).  

👉 Ejemplo:
```bash
docker compose logs -f backend
```

---

## 🔍 Ejecutar comandos dentro del contenedor
```bash
docker exec -it <nombre_contenedor> bash
```
Abre una terminal *bash* dentro del contenedor.  

```bash
docker exec -it <nombre_contenedor> ls -l /app
```
Ejecuta un comando específico sin entrar en bash.  

---

## 📂 Volúmenes y archivos
```bash
docker inspect <nombre_contenedor>
```
Muestra información detallada del contenedor (incluyendo los **volúmenes montados**).

```bash
docker cp <nombre_contenedor>:<ruta_en_contenedor> <ruta_en_host>
```
Copia archivos **del contenedor al host**.  
Ejemplo:
```bash
docker cp virtualjudge_backend:/app/data/submissions.db ./submissions.db
```

```bash
docker cp <ruta_en_host> <nombre_contenedor>:<ruta_en_contenedor>
```
Copia archivos **del host al contenedor**.  

---

## 🔄 Reconstruir y reiniciar servicios
```bash
docker compose build
```
Reconstruye la imagen del servicio (por cambios en el código/Dockerfile).  

```bash
docker compose up -d
```
Levanta los contenedores en segundo plano (*detached*).  

```bash
docker compose down
```
Detiene y elimina contenedores, redes y volúmenes anónimos.  

👉 Secuencia típica cuando cambias algo en el código o `docker-compose.yml`:
```bash
docker compose down
docker compose build
docker compose up -d
```

---

## 📦 Imágenes
```bash
docker images
```
Lista las imágenes locales.  

```bash
docker rmi <id_imagen>
```
Elimina una imagen (debes detener los contenedores que la usan antes).  

---

## ⚡ Red y puertos
```bash
docker port <nombre_contenedor>
```
Muestra los puertos expuestos (ej: `8080 -> 5000`).  

```bash
docker network ls
```
Lista las redes creadas por Docker.  

---

## 🗑️ Limpieza
```bash
docker system prune -a
```
Borra **todo lo que no se está usando**: contenedores detenidos, imágenes huérfanas, volúmenes anónimos.  
⚠️ Ojo, libera espacio pero elimina cosas que luego quizás necesites reconstruir.  
