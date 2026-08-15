# 🧠 Flow 2: RAG-Vector Chatbot Architecture (NotebookLM-Style Grounding)

> **Project**: PRN222 — Educational AI RAG ChatBot (`Group 2`)  
> **Target Flow**: **Flow 2: Questions & Answers (Chatbot RAG)**  
> **Knowledge Base Reference**: Dev-Vault [`08-ai-data-science/rag-llm-agents/rag-pipeline-architecture.md`](file:///C:/Users/Admin/.gemini/config/skills/dev-vault/08-ai-data-science/rag-llm-agents/rag-pipeline-architecture.md)

---

## 📌 1. Core Requirements for Flow 2 (Assignment & Group Project)

According to the official course specification:
1. **Natural Conversational Context**: Multi-turn dialogue with memory preservation within an active session.
2. **Strict Document Grounding (Anti-Hallucination)**: The AI model must answer **STRICTLY** from the retrieved document chunks. It must NOT answer using external pre-trained knowledge if the concept is not in the course materials.
3. **Exact Source Citations**: Every claim or answer must cite the source document name, chapter title, section heading, and page number (`[Doc: PRN222_Architecture.pdf, Page 12, Sec 3.2]`).
4. **Session-Based Conversation History**: Users can create, switch between, and persist multiple chat sessions per subject.
5. **NotebookLM Source Selection**: Users can toggle specific documents on/off to define the exact query scope.

---

## 🏗️ 2. End-to-End RAG Architecture Pipeline (NotebookLM Pattern)

```
[ User Query + Selected Document IDs (e.g., [1, 3]) ]
      │
      ▼
[ Step 1: Query Vectorization ]
      │  Generate dense embedding vector (384-dim / 768-dim)
      ▼
[ Step 2: Filtered Vector Similarity Search (NotebookLM Scope) ]
      │  SQL Query: WHERE DocumentId IN (@SelectedDocIds)
      │  Compute Cosine Similarity against DocumentChunks
      │  Top-K Retrieval (K = 4 to 6 most relevant chunks)
      ▼
[ Step 3: Anti-Hallucination Guardrail Check ]
      │  If Top-K similarity score < threshold (0.65):
      │  ➔ Return: "Tài liệu môn học không chứa thông tin về câu hỏi này."
      ▼
[ Step 4: Prompt Assembly & Citation Indexing ]
      │  Inject numbered context blocks: [Nguồn 1 (Doc A, Trang 12)], [Nguồn 2 (Doc B, Trang 99)]
      │  System Prompt: "Answer strictly within context and attach citation markers ❶, ❷."
      ▼
[ Step 5: LLM Generation & Citation Extraction ]
      │  Stream response with inline citation pills
      │  Serialize Citation References JSON [{ index: 1, docTitle, page, heading, snippet }]
      ▼
[ Step 6: Session State Persistence ]
      │  Save to ChatSessions & ChatMessages tables in SQL Server
      ▼
[ Response rendered with Interactive Citation Popovers ("Xem nguồn") ]
```

---

## 📊 3. Comparison with Dev-Vault RAG Guidelines

| RAG Dimension | Dev-Vault Best Practice Recommendation | Our Current System (ASM 1 & 2) | Proposed Flow 2 Upgrade |
| :--- | :--- | :--- | :--- |
| **Chunking Size** | 256–512 tokens (focused semantic density) | `MaxChars = 3000` (~750 tokens), `Overlap = 300` (~75 tokens) | Keep ~500–750 tokens for textbook paragraphs, but split large code blocks cleanly. |
| **Chunk Boundaries** | Document-aware recursive splitting (`\n\n` ➔ `.` ➔ ` `) | ✅ Implemented with natural boundary fallback (`\n\n` ➔ `. ` ➔ `' '`) | Add parent-child header hierarchy metadata into prompt header. |
| **Deduplication** | SHA-256 content hashing to avoid duplicated retrieval | ✅ Implemented (`ComputeSha256` in `DocumentExtractionService`) | Retained as-is. |
| **Vector Storage** | Dense vector embeddings with Cosine Similarity | Flag `HasEmbedding = true`, metadata stored in `DocumentChunks` | Add `VectorEmbedding` column (JSON / `varbinary(max)` or SQL Vector) for fast vector dot-product ranking. |
| **Hybrid Search** | Combine Dense Vectors + Lexical Keyword Search (BM25/SQL LIKE) | Full SQL Chapter/Subject filtering | **Hybrid Search**: Dense Cosine Similarity + Exact Keyword Match for technical keywords (e.g. `DbContext`, `SignalR`). |
| **Grounding Policy** | Strict "No-Outside-Knowledge" System Prompt | N/A (Flow 2 implementation) | **Strict System Guardrail**: If similarity score < threshold ($0.65$), return *"Tài liệu môn học không chứa thông tin về câu hỏi này."* |

---

## 🗄️ 4. Proposed Database Schema for Flow 2

```csharp
// 1. Chat Session Table
public class ChatSession
{
    public int Id { get; set; }
    public string Title { get; set; } = "Cuộc trò chuyện mới";
    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string? SelectedDocumentIdsJson { get; set; } // e.g. "[1, 3, 5]" (NotebookLM scope)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

// 2. Chat Message Table
public class ChatMessage
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public ChatSession Session { get; set; } = null!;
    public string Role { get; set; } = "user"; // "user" | "assistant" | "system"
    public string Content { get; set; } = null!;
    public string? CitationsJson { get; set; } // JSON array: [{ index: 1, chunkId: 10, docTitle: "c-12-in-a-nutshell.pdf", page: 99, snippet: "..." }]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

---

## 🛡️ 5. Strict Anti-Hallucination System Prompt

```text
BẠN LÀ TRỢ LÝ HỌC TẬP AI DÀNH RIÊNG CHO MÔN HỌC [SUBJECT_CODE] - [SUBJECT_NAME].

QUY TẮC BẮT BUỘC:
1. BẠN CHỈ ĐƯỢC TRẢ LỜI DỰA TRÊN THÔNG TIN CÓ TRONG PHẦN [NGỮ CẢNH TÀI LIỆU] DƯỚI ĐÂY.
2. TUYỆT ĐỐI KHÔNG SỬ DỤNG KIẾN THỨC BÊN NGOÀI ĐỂ SUY DIỄN HOẶC BỔ SUNG.
3. NẾU NGỮ CẢNH TÀI LIỆU KHÔNG CHỨA THÔNG TIN ĐỂ TRẢ LỜI CÂU HỎI, HÃY TRẢ LỜI CHÍNH XÁC: 
   "Tài liệu môn học hiện tại không đề cập đến vấn đề này. Vui lòng tham khảo thêm tài liệu chính thức từ Giảng viên."
4. MỖI Ý TRẢ LỜI PHẢI ĐÍNH KÈM TRÍCH DẪN NGUỒN CỤ THỂ DƯỚI DẠNG ❶, ❷ TƯƠNG ỨNG VỚI NGUỒN ĐƯỢC ĐÁNH SỐ.

[NGỮ CẢNH TÀI LIỆU]:
---
[Nguồn ❶] Tài liệu: {DocTitle_1} | Trang {Page_1} | Mục: {Heading_1}
Nội dung: {Chunk_1_Content}
---
[Nguồn ❷] Tài liệu: {DocTitle_2} | Trang {Page_2} | Mục: {Heading_2}
Nội dung: {Chunk_2_Content}
---
```

---

## 📈 6. Evaluation & Ground Truth Benchmark (Test Set 50 Questions)

As required in Deliverables item B:
- Prepare a benchmark dataset of **50 golden QA pairs** from the course textbooks/slides (`c-12-in-a-nutshell.pdf`, `Chapter 01 - Networking Programming.pdf`, etc.).
- Evaluate the RAG pipeline on 3 metrics:
  1. **Faithfulness**: 100% answers grounded in chunks.
  2. **Citation Precision**: All cited page numbers match the real source file.
  3. **Rejection Accuracy**: Correctly rejecting out-of-scope questions (e.g. *"Lịch sử Ai Cập cổ đại"* ➔ Must reject).
