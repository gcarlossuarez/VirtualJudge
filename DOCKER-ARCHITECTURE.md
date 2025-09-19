# VirtualJudge – Docker Architecture

This document explains what happens when you run `docker compose build` and `docker compose up -d` for the **VirtualJudge** project.

---

## 1. Project Layout (on host machine)

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

## 2. What `docker compose build` does

- Reads the **`docker-compose.yml`** at the root of `VirtualJudge/`.
- Looks for the services:
  ```yaml
  backend:
    build: ./CsJudgeApi
  runner:
    build: ./cs-single-runner
  ```
- Executes the `Dockerfile` inside each subfolder.
- Produces **two local Docker images**:
  - `virtualjudge_backend:latest`
  - `virtualjudge_runner:latest`

---

## 3. What `docker compose up -d` does

- Creates and starts **two containers**:
  - **virtualjudge_backend**  
    - Runs the ASP.NET Core API.  
    - Mounts `submissions.db` at `/app/data/submissions.db`.  
    - Mounts `problems/` at `/app/problems`.
  - **virtualjudge_runner**  
    - Sandbox that compiles and runs student submissions.  
    - Mounts `problems/` at `/problems`.

- Docker also creates:
  - **One internal network** so backend and runner can communicate.  
  - **Two mounted volumes** from the host:
    - `./CsJudgeApi/submissions.db` → `/app/data/submissions.db`  
    - `./problems/` → shared with both containers

---

## 4. Execution Flow

1. A **student** sends code to the backend (`http://localhost:8080`).  
2. The **backend** receives the submission, spawns a temporary runner container, and executes `run_single.sh`.  
3. The **runner** compiles the student’s code, runs it against the datasets (`.in` / `.out`), and validates with `Validator.cs` if present.  
4. The **runner** returns results to the backend.  
5. The **backend** stores the verdict in `submissions.db` and responds to the student.

---

## 5. Diagram (PlantUML)

```plantuml
@startuml
actor "Administrator" as Admin
actor "Student" as Student

rectangle "Host Machine\n(Your PC or University Server)" {
  
  folder "VirtualJudge/" {
    [CsJudgeApi/] 
    [cs-single-runner/]
    [problems/]
    [Utilitarios/]
    [docker-compose.yml]
    [submissions.db]
  }

  rectangle "Docker" {
    [virtualjudge_backend\n(container)] as backend
    [virtualjudge_runner\n(container)] as runner
    database "submissions.db\n(mounted volume)" as db
    folder "problems\n(mounted volume)" as problems
  }

  backend --> db : SQLite mounted at /app/data
  backend --> problems : Reads problems
  runner --> problems : Executes IN/OUT tests
}

Student --> backend : Submit code (HTTP)
backend --> runner : Runs execution
Admin --> backend : Manage problems, view logs
@enduml
```

---

✅ With this setup, your system is fully dockerized:  
- **2 images** → backend + runner  
- **2 containers** → running services  
- **1 network** → internal communication  
- **2 volumes** → persistent DB + problems  
