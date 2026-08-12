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

## Default Seeded Accounts

| Role | Email | Password | Access Rights |
|---|---|---|---|
| **Subject Leader** (Trưởng môn) | `leader@gmail.com` | `leader@123` | Upload PDF/DOCX/PPTX, Chapter CRUD, Delete Documents |
| **Student** (Sinh viên) | `student@gmail.com` | `student@123` | View & filter indexed documents, inspect extracted chunks modal |

---

## Key Features (Flow 1: Upload & Document Management)

1. **Document Uploading**: Supports `.pdf`, `.docx`, and `.pptx` slide presentations.
2. **Automated Extraction & Semantic Chunking**:
   - Parses text page-by-page / slide-by-slide.
   - Splits long text using sliding windows with ~300 character overlap to maintain context.
   - Computes token counts per chunk for LLM readiness.
3. **SHA256 Content Deduplication**: Automatically detects and prevents uploading duplicate files.
4. **Subject & Chapter Management**: Organize documents under **PRN222** course chapters with dynamic creation & filtering.

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
