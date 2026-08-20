# BÁO CÁO ĐÓNG GÓP THÀNH VIÊN - NHÓM 2
**Môn học:** PRN222 - C# and .NET Core Development  
**Đề tài:** Hệ thống Quản lý Tài liệu Học tập & Trợ lý RAG AI Chatbot  
**Nhóm:** Group 2  

---

## 👥 Danh Sách Thành Viên & Thông Tin Sinh Viên

| STT | Họ và Tên | MSSV | Email FPT | Git Account |
| :-: | :--- | :-: | :--- | :--- |
| 1 | **Nguyễn Quang Duy Quang** *(Leader)* | **SE183155** | `quangnqdse183155@fpt.edu.vn` | `QuangD300504` (`duyq099@gmail.com`) |
| 2 | **Phan Lê Huy** | **SE171769** | `huyplse171769@fpt.edu.vn` | `MG Jerry` (`kietthemouse@gmail.com`) |
| 3 | **Trần Tiến Đạt** | **SE182072** | `datttse182072@fpt.edu.vn` | `trandatse2309` (`trandatse2309@gmail.com`) |
| 4 | **Nguyễn Chí Thanh** | **SE160622** | `thanhncse160622@fpt.edu.vn` | `Nero` (`thanhnguyen2409@pm.me`) |
| 5 | **Hoàng Minh Nhật** | **SE170357** | `nhathhmse170357@fpt.edu.vn` | `ZebraHecker` (`nhathm2406@gmail.com`) |

---

## 📄 1. Bảng Đóng Góp Assignment 2 (Document Management & SignalR)

| Họ và Tên | MSSV | Vai trò & Phân hệ đảm nhiệm | Đóng góp (%) | Chi tiết công việc hoàn thành |
| :--- | :-: | :--- | :---: | :--- |
| **Nguyễn Quang Duy Quang** *(Leader)* | **SE183155** | **Backend & Real-Time SignalR Architect** | **26%** | • Xây dựng SignalR `DocumentHub.cs` và `IHubContext` backend broadcasts.<br>• Telemetry đo tiến trình tải lên 4 giai đoạn (`UploadProgress`: 20% $\rightarrow$ 50% $\rightarrow$ 85% $\rightarrow$ 100%).<br>• Banner cảnh báo chỉnh sửa đồng thời (`NotifyEditingSubject` / `Clients.Others`).<br>• Tích hợp Windows Native PDF renderer & OCR fallback engine. |
| **Nguyễn Chí Thanh** | **SE160622** | **Frontend Logic & Client Script Engineer** | **19%** | • Modularize client scripts vào `wwwroot/js/document.js`.<br>• Xây dựng bộ lọc AJAX (Tìm kiếm, Chương, Loại file) không load lại trang.<br>• Kết nối SignalR listeners cập nhật real-time bảng tài liệu & badge số lượng.<br>• Xử lý chuyển đổi động mô tả môn học qua `window.DocConfig`. |
| **Hoàng Minh Nhật** | **SE170357** | **Subject/Chapter Management & Auth** | **19%** | • Xây dựng Modal Quản lý môn & chương (`_ManageSubjectModal.cshtml`).<br>• Phân quyền `SubjectLeader` cho các tính năng nhạy cảm (Xóa tài liệu, sửa môn).<br>• Xây dựng trang Đăng nhập / Đăng xuất Razor Pages và Session state.<br>• Validation dữ liệu form và component thông báo alert. |
| **Phan Lê Huy** | **SE171769** | **UI/UX & Razor Layout Specialist** | **18%** | • Thiết kế giao diện Dark Glassmorphism và Typography system.<br>• Xây dựng Partial View bảng tài liệu `_DocumentTable.cshtml` và phân trang.<br>• Thiết kế thanh pills điều hướng chọn môn học và badge trạng thái.<br>• Tích hợp markup banner cảnh báo realtime. |
| **Trần Tiến Đạt** | **SE182072** | **Upload & Chunking Engine Engineer** | **18%** | • Xây dựng Modal Upload kéo thả `_UploadModal.cshtml`.<br>• Kiểm tra tính hợp lệ của tệp tải lên (PDF, DOCX, PPTX max 25MB).<br>• Xây dựng Modal Chunks Inspector (`_ChunksModal.cshtml`) hiển thị phân mảnh token.<br>• Kết nối pipeline xử lý file với `IDocumentService`. |
| **TỔNG CỘNG** | | | **100%** | |

---

## 🤖 2. Bảng Đóng Góp Final Project (Grounded RAG AI Chatbot)

| Họ và Tên | MSSV | Vai trò & Phân hệ đảm nhiệm | Đóng góp (%) | Chi tiết công việc hoàn thành |
| :--- | :-: | :--- | :---: | :--- |
| **Nguyễn Quang Duy Quang** *(Leader)* | **SE183155** | **AI Core & RAG Vector Engine** | **28%** | • Xây dựng `RagChatService.cs` với thuật toán Cosine Similarity vector search.<br>• Thiết kế prompt grounding và kết nối mô hình local Ollama (`Qwen2.5:7b`).<br>• Thiết kế Database schema & EF Core migrations cho `ChatSession` và `ChatMessage`.<br>• Xử lý trích xuất citation và thuật toán đánh giá độ tương đồng. |
| **Phan Lê Huy** | **SE171769** | **NotebookLM UI/UX Specialist** | **19%** | • Xây dựng bố cục 3 cột phong cách NotebookLM với collapsible slim rails.<br>• Modularize trang Chat thành 5 Partial Views (`_LeftSourcesSidebar`, `_ChatMainCanvas`, `_RightHistorySidebar`, v.v.).<br>• Tinh chỉnh thanh cuộn custom, giao diện kính mờ và dropdown môn học (`chat.css`). |
| **Trần Tiến Đạt** | **SE182072** | **Source Management & Upload Modal** | **19%** | • Xây dựng Modal Upload tài liệu trực tiếp trong Chat (`_UploadSourceModal.cshtml`).<br>• Kết nối thanh tiến trình SignalR real-time trong modal chat.<br>• Xây dựng bộ điều khiển danh sách tài liệu nguồn (Chọn tất cả / từng file).<br>• Đồng bộ số lượng tài liệu nguồn thời gian thực. |
| **Hoàng Minh Nhật** | **SE170357** | **Interaction & Prompt UI Designer** | **17%** | • Xây dựng bộ sinh 3 câu hỏi gợi ý mở đầu ngữ cảnh động.<br>• Sinh các chip câu hỏi tiếp nối (Follow-up suggestions) sau phản hồi AI.<br>• Xây dựng Modal hiển thị trích dẫn nguồn `_CitationModal.cshtml` (Score, Page, Heading).<br>• Xử lý Markdown rendering, code block highlight và nút copy code. |
| **Nguyễn Chí Thanh** | **SE160622** | **SPA State & Session Engine** | **17%** | • Xây dựng kiến trúc SPA chuyển đổi mượt mà trong `wwwroot/js/chat.js`.<br>• Reset phiên chat mới & chuyển đổi lịch sử chat không cần tải lại trang.<br>• Xử lý đổi tên, xóa phiên chat và đồng bộ URL `history.pushState`.<br>• Xử lý gõ tiếng Việt (IME) và hiệu ứng typing indicator. |
| **TỔNG CỘNG** | | | **100%** | |

---

## 📌 3. Tham Chiếu Git Commit

| Commit Hash | Tác giả | Thời gian | Phân hệ thực hiện |
| :--- | :--- | :--- | :--- |
| `6faacce` | **Nero (Nguyễn Chí Thanh)** | Sun Aug 16 21:15 +0700 | `feat(client-spa): extract and optimize modular client script for document management and real-time SignalR table synchronization` |
| `84a7915` | **ZebraHecker (Hoàng Minh Nhật)** | Sun Aug 16 17:50 +0700 | `feat(chat-interaction): implement citation inspector modal, dynamic context-aware starter prompts, and chat interaction client` |
| `71bed1f` | **trandatse2309 (Trần Tiến Đạt)** | Sun Aug 16 14:22 +0700 | `feat(chat-sources): implement in-chat direct source uploader with chapter selection and dynamic checklist controls` |
| `b550650` | **MG Jerry (Phan Lê Huy)** | Sun Aug 16 09:35 +0700 | `feat(chat-ui): build NotebookLM 3-pane responsive layout with collapsible slim rails and modular partial views` |
| `beda335` | **QuangD300504 (Nguyễn Quang Duy Quang)** | Sat Aug 15 23:18 +0700 | `feat(rag-core): implement vector similarity chunk retrieval, context grounding prompt, and local Ollama Qwen2.5 integration` |
| `de377c5` | **QuangD300504 (Nguyễn Quang Duy Quang)** | Sat Aug 15 19:42 +0700 | `feat(asm2-signalr): complete real-time SignalR suite with live table sync, progress telemetry, and concurrent edit presence` |
