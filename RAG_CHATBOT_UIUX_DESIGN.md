# 🎨 Modern AI ChatBot UI/UX Design: NotebookLM + ChatGPT Hybrid (Flow 2)

> **Inspiration**: Google NotebookLM (Document Grounding & Sources), ChatGPT & Claude (Conversational Polish).  
> **Aesthetic Theme**: Dark Glassmorphism, Inter Typography, Iris Accent (`#6366f1`), Emerald Citations (`#10b981`).  
> **Compatibility**: Seamlessly integrated into PRN222 Assignment 2 & Group Project Layout.

---

## 🖼️ 1. NotebookLM-Inspired 3-Pane Workspace Layout

```
+-------------------------------------------------------------------------------------------------------------------------+
|  [AI Study Hub Logo]   PRN222: Enterprise Web App Development      [Quang (SubjectLeader)]  [🔔 SignalR]  [🚪 Đăng xuất] |
+-------------------------------+---------------------------------------------------------+-------------------------------+
| 📚 NGUỒN TÀI LIỆU (SOURCES)   | 💬 KHÔNG GIAN HỘI THOẠI (CHAT WORKSPACE)                | 📑 TRÍCH DẪN GỐC (POPOVER)    |
|                               |                                                         |                               |
| [+ Thêm tài liệu mới]         |   [AI Assistant Message]                                | ┌───────────────────────────┐ |
|                               |   "Theo tài liệu bài giảng, khi triển khai              | │ 📖 Slide Chương 1 (Trang 12)│ |
| ☑ Chọn tất cả (4 nguồn)       |   SignalR trong ASP.NET Core:                           | ├───────────────────────────┤ |
|                               |   • Hub là trung tâm điều phối kết nối ❶                | │ "Hub là lớp trừu tượng hoá│ |
| ☑ 📄 c-12-in-a-nutshell.pdf   |   • IHubContext dùng khi gửi broadcast từ bên ngoài ❷ ❸ | │ các kết nối WebSocket song│ |
|    (99 chunks • Ch.1)         |                                                         | │ công, quản lý ClientId..."│ |
|                               |   Tóm lại: SignalR hỗ trợ tự động reconnect ❹           | ├───────────────────────────┤ |
| ☑ 📄 Slide_Ch01_Network.pptx  |                                                         | │ 🔗 Xem chi tiết chunk...  │ |
|    (14 chunks • Ch.1)         |   [ Lưu ghi chú ] [ 📋 Sao chép ] [ 👍 ] [ 👎 ]          | └───────────────────────────┘ |
|                               |                                                         |                               |
| ☑ 📄 Lab02_SignalR_Guide.docx |---------------------------------------------------------|                               |
|    (8 chunks • Ch.2)          | [ ⌨️ Đặt câu hỏi hoặc yêu cầu tóm tắt...       [3 nguồn] [➤] ]                           |
+-------------------------------+-----------------------------------------------------------------------------------------+
```

---

## 🌟 2. Key UI/UX Innovations Inspired by NotebookLM

### 1️⃣ Left Column: Active Sources Management (Nguồn tài liệu)
- **`+ Thêm tài liệu` Button**: Quick trigger for the upload modal.
- **Source Selection Checkboxes**:
  - Each uploaded document has a toggle checkbox.
  - **Selective Query Scope**: Students can uncheck irrelevant documents (e.g. only select *Chapter 1* documents to study for Quiz 1).
  - The chat prompt automatically restricts vector search to `WHERE DocumentId IN (@SelectedIds)`.
- **Source Metadata Pills**: Shows file extension badge (`PDF`, `DOCX`, `PPTX`), total chunks count, and chapter tag.

---

### 2️⃣ Center Workspace: Conversational Stream & Grounding Chips
- **Interactive Citation Badges (❶, ❷, ❸)**:
  - Inside the AI response text, citations appear as clickable numbered circular pills (similar to NotebookLM).
  - Clicking any citation badge opens a floating **Citation Popover Card**.
- **Response Action Toolbar**:
  - **Lưu vào ghi chú (Save Note)**: Pin important answers.
  - **Sao chép (Copy Markdown)**: Copies formatted text and code blocks.
  - **Đánh giá phản hồi (Good / Bad response)**.

---

### 3️⃣ Floating Input Island with Dynamic Source Counter
- **Pill-Shaped Input Box**: Floats at the bottom center with `backdrop-filter: blur(16px)`.
- **Active Source Counter**: Displays `[3 nguồn]` pill inside the input bar, reassuring the student that the AI is grounded exclusively in the 3 checked documents.
- **Keyboard Convenience**: `Enter` to submit, `Shift+Enter` for multi-line code/prompts.

---

### 4️⃣ Right-Side Popover / Drawer: "Xem nguồn" (Grounding Inspector)
- When clicking any citation badge (e.g. `❶`):
  - Displays the original document title (`c-12-in-a-nutshell.pdf`).
  - Displays the exact page number (`Trang 85`) and chapter heading.
  - Displays the exact extracted text chunk that the LLM used to synthesize that specific sentence.
  - Includes a direct button: **"👁 Xem toàn bộ chunk trong Inspector"**.

---

## 🎨 3. CSS Component Styling Guide

```css
/* NotebookLM-style Circular Citation Badge */
.citation-badge {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 18px;
    height: 18px;
    font-size: 11px;
    font-weight: 700;
    color: #10b981;
    background: rgba(16, 185, 129, 0.15);
    border: 1px solid rgba(16, 185, 129, 0.35);
    border-radius: 50%;
    margin: 0 2px;
    cursor: pointer;
    transition: all 0.2s ease;
    vertical-align: super;
}

.citation-badge:hover {
    background: #10b981;
    color: #0f172a;
    transform: scale(1.15);
    box-shadow: 0 0 10px rgba(16, 185, 129, 0.4);
}

/* NotebookLM-style Citation Popover Card */
.citation-popover {
    background: #1e293b;
    border: 1px solid rgba(255, 255, 255, 0.12);
    border-radius: 12px;
    padding: 16px;
    box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.5), 0 8px 10px -6px rgba(0, 0, 0, 0.5);
    max-width: 380px;
    color: #e2e8f0;
}
```
