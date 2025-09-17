# VirtualJudge – Installation Guide (University Server)

This document explains how to deploy **VirtualJudge** on a Linux server (Ubuntu recommended) using **Docker** and **Docker Compose**.

---

## 1. Prerequisites

Make sure the server has the following installed:

- **Docker**
  ```bash
  sudo apt update
  sudo apt install -y docker.io
  sudo systemctl enable docker
  sudo systemctl start docker
  ```

- **Docker Compose**
  ```bash
  sudo apt install -y docker-compose-plugin
  docker compose version
  ```

- **Git** (to clone the repository)
  ```bash
  sudo apt install -y git
  ```

---

## 2. Clone the Repository

```bash
cd /opt
sudo git clone https://github.com/<your-user>/VirtualJudge.git
cd VirtualJudge
```

*(replace `<your-user>` with your GitHub username or the repo URL you are using)*

---

## 3. Project Structure

Expected directory layout:

```
VirtualJudge/
│
├── CsJudgeApi/             # Backend (ASP.NET Core, SQLite)
│   ├── Program.cs
│   ├── CsJudgeApi.csproj
│   ├── submissions.db
│   └── Dockerfile
│
├── cs-single-runner/       # Runner (sandbox for C#/C++ submissions)
│   ├── Dockerfile
│   ├── run_single.sh
│   └── ...
│
├── problems/               # Problem repository (IN/OUT/Validator.cs)
├── Utilitarios/            # Extra utilities
├── docker-compose.yml      # Orchestration file
└── ARCHITECTURE.md
```

---

## 4. Configure Connection String (Backend)

In `CsJudgeApi/appsettings.json`, ensure SQLite points to `/app/data/submissions.db`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=/app/data/submissions.db"
}
```

---

## 5. Docker Compose File

`docker-compose.yml` (located in project root):

```yaml
version: "3.9"

services:
  backend:
    build: ./CsJudgeApi
    container_name: virtualjudge_backend
    ports:
      - "8080:80"
    environment:
      - DOTNET_RUNNING_IN_CONTAINER=true
      - DOTNET_USE_POLLING_FILE_WATCHER=true
    volumes:
      - ./problems:/app/problems
      - ./Utilitarios:/app/Utilitarios
      - ./CsJudgeApi/submissions.db:/app/data/submissions.db

  runner:
    build: ./cs-single-runner
    container_name: virtualjudge_runner
    stdin_open: true
    tty: true
    volumes:
      - ./problems:/problems
      - /var/run/docker.sock:/var/run/docker.sock
```

---

## 6. Build and Run

```bash
docker compose build
docker compose up -d
```

Check running containers:

```bash
docker ps
```

---

## 7. Verify the Installation

- Open a browser:  
  `http://<server-ip>:8080`

- Check logs if needed:
  ```bash
  docker compose logs -f backend
  docker compose logs -f runner
  ```

---

## 8. Updating the System

When new code is pushed to GitHub:

```bash
cd /opt/VirtualJudge
git pull
docker compose build
docker compose up -d
```

---

## 9. Stopping the System

```bash
docker compose down
```

---

## 10. Database

- SQLite file is stored in `CsJudgeApi/submissions.db`.
- This file is mounted inside the container at `/app/data/submissions.db`.
- To backup:
  ```bash
  cp CsJudgeApi/submissions.db submissions_backup.db
  ```

---

## 11. First Test

1. Add a sample problem in `problems/` with files:
   - `datos001.txt`
   - `Output_datos001.txt`

2. Submit a simple program via the API.

3. Verify that the backend uses the **runner** container to compile and check the submission.

---

✅ With this setup, VirtualJudge will be ready to run in the university server.
