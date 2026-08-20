# PRN222 – Group 2: AI Study Hub (RAG Educational Chatbot Platform)

> **Enterprise-grade Academic RAG Chatbot** built with **ASP.NET Core 8 Razor Pages / MVC**, **Entity Framework Core Code-First**, **SQL Server**, **SignalR Real-Time Streaming**, and **Local AI Models (Ollama)**.

---

## 🌟 Key Capabilities

### 1. Document Ingestion & Vector Indexing (Flow 1)
* **Multi-Format Document Parsing**: Automated text and structure extraction for `.pdf`, `.docx`, and `.pptx` slides.
* **Semantic Chunking Engine**: Intelligent page-by-page & slide-by-slide boundary splitting with overlapping context windows (~300 chars) to prevent context loss.
* **True Vector Embeddings**: Integrated with Ollama's `nomic-embed-text` model to compute 768-dimensional dense vectors stored directly in SQL Server.
* **Content Deduplication**: SHA-256 hash checking prevents duplicate document uploads.
* **Chapter & Subject Scoping**: Multi-subject structure with chapter hierarchy and per-document filtering.

### 2. Intelligent RAG Chatbot & Q&A Workspace (Flow 2)
* **Dense Vector Search & Cosine Similarity**: High-precision semantic retrieval mapping cross-lingual queries (Vietnamese $\leftrightarrow$ English) to relevant lecture slides.
* **Real-Time Token Streaming**: Powered by **SignalR** (`DocumentHub`), providing instant word-by-word typing responses with sub-second perceived latency.
* **NotebookLM-Style Interactive Citations**: Direct inline citation badges (`1`, `2`, `3`) that renumber in sequential order of appearance. Clicking any citation opens a slide/page inspector modal with exact text snippets and similarity scores.
* **Strict Anti-Hallucination Guardrails**: Prompts engineered with strict grounding constraints to refuse answering out-of-scope questions instead of fabricating course information.
* **Session Management**: Dynamic chat naming, persistent conversation history, and multi-session switching in the sidebar.

---

## 🛠️ Prerequisites

| Dependency | Version / Requirements | Notes |
|---|---|---|
| **.NET SDK** | `8.0+` | C# 12 / ASP.NET Core |
| **SQL Server** | Express / Developer / LocalDB | EF Core 8.x Code-First |
| **Ollama** | Latest (`v0.3+`) | Local LLM & Embedding Host |
| **`dotnet-ef` CLI** | `8.x` | `dotnet tool install --global dotnet-ef` |

---

## 🤖 Local AI Model Setup (Ollama)

Ensure Ollama is installed and running on your system, then pull the required models:

```powershell
# 1. Pull the 768-dim Embedding Model (274 MB)
ollama pull nomic-embed-text

# 2. Pull the 7B Instruction-tuned LLM (~4.7 GB)
ollama pull qwen2.5:7b
```

Verify that models are ready:
```powershell
ollama list
```

---

## 🚀 Quickstart & Setup Guide

### 1. Clone the Repository
```bash
git clone <repo-url>
cd PRN222_Group2_Assignment1
```

### 2. Configure Database Connection String
Create a local override configuration file **`PRN222_Group2_Assignment1/appsettings.Local.json`** (this file is git-ignored):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local);uid=sa;pwd=YOUR_PASSWORD;Database=ChatBotEduDb;TrustServerCertificate=True;"
  },
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "Model": "qwen2.5:7b",
    "EmbeddingModel": "nomic-embed-text"
  }
}
```

> **Note for LocalDB users**:  
> `"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ChatBotEduDb;Trusted_Connection=True;"`

### 3. Apply Database Migrations (Automatic DB Creation)
Run EF Core migrations from the project root:
```bash
cd PRN222_Group2_Assignment1
dotnet ef database update
cd ..
```

### 4. Run the Web Application
Launch the Razor Pages web application:
```bash
dotnet run --project PRN222_Group2_Assignment2
```

Navigate to `https://localhost:7xxx` or `http://localhost:5000` in your web browser.

---

## 👥 Default Seeded Accounts

| Role | Email | Password | Permissions |
|---|---|---|---|
| **Subject Leader** (Trưởng bộ môn) | `leader@gmail.com` | `leader@123` | Full Access: Document upload/delete, chapter CRUD, RAG chat, vector index inspection |
| **Student** (Sinh viên) | `student@gmail.com` | `student@123` | Read & Chat: View indexed documents, source filtering, interactive RAG Q&A |

---

## 📁 Solution Architecture

```
PRN222_Group2_Assignment1/
├── PRN222_Group2_Assignment1/        ← Core Business Layer & Data Access (Class Library)
│   ├── Data/                         ← AppDbContext & EF Core Model Configurations
│   ├── Migrations/                   ← EF Core Migration Snapshots
│   ├── Models/                       ← Domain Entities (Document, Chunk, ChatSession, Message, etc.)
│   ├── Services/                     ← DocumentService, RagChatService, ChunkingService, AuthService
│   └── ViewModels/                   ← Shared DTOs (ChatStreamPacket, CitationDto, SendChatRequest)
│
├── PRN222_Group2_Assignment2/        ← Web Presentation Layer (ASP.NET Core 8 Web App)
│   ├── Hubs/                         ← SignalR Hubs (DocumentHub - live streaming & upload events)
│   ├── Pages/                        ← Razor Pages
│   │   ├── Auth/                     ← Login / Logout Handlers
│   │   ├── Chat/                     ← RAG Chat UI, Sidebar, Canvas, Modals
│   │   └── Document/                 ← Document Management & Indexing UI
│   └── wwwroot/                      ← CSS Design Tokens, Vanilla JS (chat.js, document.js), Libs
│
└── TEST_SET_50_QUESTIONS_GROUND_TRUTH.md ← 50-Question Human Ground-Truth Evaluation Benchmark
```

---

## 🧪 Evaluation Benchmark (50 Questions + Ground Truth)
For model evaluation and academic grading, refer to:
* [`TEST_SET_50_QUESTIONS_GROUND_TRUTH.md`](./TEST_SET_50_QUESTIONS_GROUND_TRUTH.md) — Comprehensive 50-question test set categorized into Single-Doc, Bilingual, Cross-Doc, Deep Technical, and Guardrail Refusal tests.

---

## 📝 Database Migration Commands

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> --project PRN222_Group2_Assignment1 --startup-project PRN222_Group2_Assignment2

# Update database schema
dotnet ef database update --project PRN222_Group2_Assignment1 --startup-project PRN222_Group2_Assignment2
```
