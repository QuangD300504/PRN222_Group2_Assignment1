# PRN222 – Group 2: ChatBot EDU

ASP.NET Core 8 MVC · EF Core Code-First · SQL Server

---

## Prerequisites

| Tool | Version |
|---|---|
| .NET SDK | 8.0+ |
| SQL Server | Any edition (Express / Developer / LocalDB) |
| `dotnet-ef` CLI | 8.x |

Install the EF CLI tool if you haven't:
```bash
dotnet tool install --global dotnet-ef --version 8.0.0
```

---

## First-time setup

### 1. Clone the repo
```bash
git clone <repo-url>
cd PRN222_Group2_Assignment1
```

### 2. Set your connection string

Create a local override file **next to** `appsettings.json` (this file is git-ignored):

**`PRN222_Group2_Assignment1/appsettings.Local.json`**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local);uid=sa;pwd=YOUR_PASSWORD;Database=ChatBotEduDb;TrustServerCertificate=True;"
  }
}
```

> Change `(local)`, `sa`, and `YOUR_PASSWORD` to match your local SQL Server.  
> If you use LocalDB: `Server=(localdb)\\mssqllocaldb;Database=ChatBotEduDb;Trusted_Connection=True;`

### 3. Apply migrations (creates the DB automatically)
```bash
cd PRN222_Group2_Assignment1
dotnet ef database update
```

### 4. Run the app
```bash
dotnet run
```

Open your browser at `https://localhost:{port}` — you'll land on the login page.

---

## Default seeded account

| Field | Value |
|---|---|
| Email | `leader@chatbot.edu.vn` |
| Password | `leader@123` |
| Role | `SubjectLeader` |

---

## Roles

| Role | Actor | Access |
|---|---|---|
| `SubjectLeader` | Trưởng môn | Upload & manage course documents (Flow 1) |
| `Student` | Sinh viên | Chatbot Q&A (Flow 2) |

---

## Project structure

```
PRN222_Group2_Assignment1/
├── Controllers/        ← MVC controllers
├── Data/               ← AppDbContext (EF Core)
│   └── AppDbContext.cs
├── Migrations/         ← Auto-generated, do NOT edit manually
├── Models/             ← Domain entities (DB-mapped)
├── Services/           ← Business logic
├── ViewModels/         ← View-specific data shapes
├── Views/              ← Razor views
└── wwwroot/            ← Static assets
```

---

## Adding a new migration (when a model changes)

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## Rolling back

```bash
dotnet ef database update <PreviousMigrationName>
dotnet ef migrations remove
```
