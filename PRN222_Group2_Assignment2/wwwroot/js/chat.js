/**
 * AI Study Hub - Chat Workspace Script
 * Clean SPA Interaction, SignalR Live Sync, Dynamic Markdown & Grounded Prompts
 */

let _activeSubjectId = window.ChatConfig?.activeSubjectId || 1;
let _activeSessionId = window.ChatConfig?.activeSessionId || 0;
const _currentUserName = window.ChatConfig?.currentUserName || "Bạn";
const _currentSubjectCode = window.ChatConfig?.currentSubjectCode || "PRN222";
let _signalrConnection = null;

// ── 1. SignalR Real-Time Setup ──────────────────────────────────────────────────
async function initSignalR() {
    try {
        _signalrConnection = new signalR.HubConnectionBuilder()
            .withUrl("/documentHub")
            .withAutomaticReconnect()
            .build();

        _signalrConnection.on("UploadProgress", function (percent, text) {
            const bar = document.getElementById('sourceUploadProgressBar');
            const percentText = document.getElementById('sourceUploadPercentText');
            const statusText = document.getElementById('sourceUploadStatusText');
            if (bar) bar.style.width = percent + '%';
            if (percentText) percentText.innerText = percent + '%';
            if (statusText) statusText.innerText = text;
        });

        _signalrConnection.on("DocumentUploaded", function (subjectId, docTitle, newDocCount) {
            if (subjectId === _activeSubjectId) {
                showChatToast(`Tài liệu mới "${docTitle}" vừa được lập chỉ mục!`);
            }
        });

        _signalrConnection.on("DocumentDeleted", function (subjectId, docId, newDocCount) {
            if (subjectId === _activeSubjectId) {
                const item = document.getElementById(`sourceDocItem-${docId}`);
                if (item) {
                    item.remove();
                    updateSelectedSourcesCount();
                }
                showChatToast(`Một tài liệu vừa bị xóa khỏi hệ thống.`);
            }
        });

        await _signalrConnection.start();
    } catch (err) {
        console.warn("SignalR connection failed:", err);
    }
}
initSignalR();

function showChatToast(msg) {
    const toastEl = document.getElementById('chatSignalRToast');
    const textEl = document.getElementById('chatSignalRToastText');
    if (toastEl && textEl) {
        textEl.innerText = msg;
        const toast = new bootstrap.Toast(toastEl, { delay: 4000 });
        toast.show();
    }
}

// ── 2. Direct Source Upload Modal Handling ──────────────────────────────────────
async function openUploadSourceModal() {
    try {
        const res = await fetch(`?handler=Chapters&subjectId=${_activeSubjectId}`);
        const chapters = await res.json();
        const select = document.getElementById('sourceChapterSelect');
        select.innerHTML = '<option value="">-- Toàn bộ môn học (Chung) --</option>';
        if (chapters && chapters.length > 0) {
            chapters.forEach(c => {
                select.innerHTML += `<option value="${c.id}">Chương ${c.chapterNumber}: ${escapeHtml(c.title)}</option>`;
            });
        }
    } catch (e) {
        console.error("Failed to load chapters:", e);
    }

    document.getElementById('uploadSourceForm').reset();
    document.getElementById('sourceFileNameDisplay').innerText = "Nhấp để chọn hoặc kéo thả tệp tài liệu vào đây";
    document.getElementById('sourceUploadProgressWrapper').classList.add('d-none');
    document.getElementById('btnSubmitSourceUpload').disabled = false;
    
    const modal = new bootstrap.Modal(document.getElementById('uploadSourceModal'));
    modal.show();
}

function handleSourceFileSelect(input) {
    if (input.files && input.files[0]) {
        const file = input.files[0];
        document.getElementById('sourceFileNameDisplay').innerText = `${file.name} (${(file.size / 1024 / 1024).toFixed(2)} MB)`;
        const titleInput = document.getElementById('sourceTitleInput');
        if (!titleInput.value.trim()) {
            const nameWithoutExt = file.name.substring(0, file.name.lastIndexOf('.')) || file.name;
            titleInput.value = nameWithoutExt;
        }
    }
}

async function submitSourceUpload(event) {
    event.preventDefault();
    const fileInput = document.getElementById('sourceFileInput');
    if (!fileInput.files || !fileInput.files[0]) {
        alert('Vui lòng chọn một tệp tài liệu.');
        return;
    }

    const formData = new FormData(document.getElementById('uploadSourceForm'));
    if (_signalrConnection && _signalrConnection.connectionId) {
        formData.append('connectionId', _signalrConnection.connectionId);
    }

    const progressWrapper = document.getElementById('sourceUploadProgressWrapper');
    progressWrapper.classList.remove('d-none');
    document.getElementById('sourceUploadProgressBar').style.width = '10%';
    document.getElementById('sourceUploadPercentText').innerText = '10%';
    document.getElementById('sourceUploadStatusText').innerText = 'Đang tải tệp lên máy chủ...';
    document.getElementById('btnSubmitSourceUpload').disabled = true;

    try {
        const res = await fetch('?handler=UploadSource', {
            method: 'POST',
            body: formData
        });
        const data = await res.json();

        if (data.success && data.document) {
            appendNewSourceDocument(data.document);
            bootstrap.Modal.getInstance(document.getElementById('uploadSourceModal'))?.hide();
            showChatToast(`Đã thêm tài liệu "${data.document.title}" thành công!`);
        } else {
            alert(data.message || 'Lỗi khi tải tài liệu lên.');
            document.getElementById('btnSubmitSourceUpload').disabled = false;
            progressWrapper.classList.add('d-none');
        }
    } catch (err) {
        alert('Lỗi kết nối khi tải tài liệu lên.');
        document.getElementById('btnSubmitSourceUpload').disabled = false;
        progressWrapper.classList.add('d-none');
    }
}

function appendNewSourceDocument(doc) {
    const noNotice = document.getElementById('noSourcesNotice');
    if (noNotice) noNotice.remove();

    const sourcesList = document.getElementById('sourcesList');
    if (!sourcesList) return;

    const badgeClass = doc.fileExtension === '.pdf' ? 'bg-danger-subtle text-danger border border-danger-subtle' : 
                       doc.fileExtension === '.docx' ? 'bg-primary-subtle text-primary border border-primary-subtle' : 'bg-warning-subtle text-warning border border-warning-subtle';
    const extName = (doc.fileExtension || '').toUpperCase().replace('.', '');

    const label = document.createElement('label');
    label.id = `sourceDocItem-${doc.id}`;
    label.className = 'source-item d-flex align-items-center gap-2 px-2-5 py-2 rounded-3 border border-slate-800 bg-slate-900/50 hover-bg-slate-850 cursor-pointer transition';
    label.innerHTML = `
        <input type="checkbox" class="source-checkbox form-check-input flex-shrink-0 m-0" value="${doc.id}" checked data-title="${escapeHtml(doc.title)}" />
        <div class="d-flex align-items-center gap-1-5 overflow-hidden flex-grow-1" style="min-width: 0;">
            <span class="badge ${badgeClass} fs-9 flex-shrink-0">${extName}</span>
            <span class="fs-8 text-slate-200 fw-medium text-truncate" title="${escapeHtml(doc.title)}">${escapeHtml(doc.title)}</span>
        </div>
    `;

    label.querySelector('.source-checkbox').addEventListener('change', () => {
        updateSelectedSourcesCount();
        renderDynamicStarterPrompts();
    });
    sourcesList.prepend(label);

    const slimList = document.getElementById('slimSourcesList');
    if (slimList) {
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'btn btn-icon btn-sm text-primary p-2 rounded-2 hover-bg-slate-800';
        btn.title = `${doc.title} (${doc.chunkCount} chunks)`;
        btn.innerHTML = '<i class="bi bi-file-earmark-text fs-5"></i>';
        slimList.prepend(btn);
    }

    updateSelectedSourcesCount();
    renderDynamicStarterPrompts();
}

// ── 3. Sidebar Toggle Controls (NotebookLM Slim Rail Style) ─────────────────────
function toggleLeftSidebar() {
    const left = document.getElementById('leftSidebar');
    if (!left) return;
    left.classList.toggle('collapsed');
}

function toggleRightSidebar() {
    const right = document.getElementById('rightSidebar');
    if (!right) return;
    right.classList.toggle('collapsed');
}

function getSelectedDocIds() {
    const checkedBoxes = document.querySelectorAll('.source-checkbox:checked');
    return Array.from(checkedBoxes).map(cb => parseInt(cb.value));
}

function getSelectedDocTitles() {
    const checkedBoxes = document.querySelectorAll('.source-checkbox:checked');
    return Array.from(checkedBoxes).map(cb => {
        return cb.dataset.title || cb.closest('.source-item')?.querySelector('.text-slate-200')?.innerText?.trim() || 'Tài liệu';
    });
}

function updateSelectedSourcesCount() {
    const selected = getSelectedDocIds().length;
    const total = document.querySelectorAll('.source-checkbox').length;
    const countDisplay = document.getElementById('selectedCountDisplay');
    const totalDisplay = document.getElementById('totalSourcesDisplay');
    const badge = document.getElementById('inputScopeBadge');
    if (countDisplay) countDisplay.innerText = selected;
    if (totalDisplay) totalDisplay.innerText = total;
    if (badge) badge.innerText = `${selected} nguồn`;
}

document.querySelectorAll('.source-checkbox').forEach(cb => {
    cb.addEventListener('change', () => {
        updateSelectedSourcesCount();
        renderDynamicStarterPrompts();
    });
});

document.getElementById('selectAllSources')?.addEventListener('change', function(e) {
    document.querySelectorAll('.source-checkbox').forEach(cb => cb.checked = e.target.checked);
    updateSelectedSourcesCount();
    renderDynamicStarterPrompts();
});

function autoResize(textarea) {
    textarea.style.height = 'auto';
    textarea.style.height = Math.min(textarea.scrollHeight, 140) + 'px';
}

// Vietnamese IME Composition handling (Unikey/Telex)
function handleKeyDown(event) {
    if (event.isComposing || event.keyCode === 229) return;
    if (event.key === 'Enter' && !event.shiftKey) {
        event.preventDefault();
        submitChatQuestion();
    }
}

function fillPrompt(text) {
    const input = document.getElementById('chatInput');
    if (!input) return;
    input.value = text;
    autoResize(input);
    input.focus();
}

function submitFollowUpPrompt(btn) {
    const prompt = btn.dataset.prompt || btn.innerText.trim();
    fillPrompt(prompt);
    submitChatQuestion();
}

// Dynamic Source-Aware Starter Prompts Generator (NotebookLM Style)
function renderDynamicStarterPrompts() {
    const container = document.getElementById('starterChipsContainer');
    if (!container) return;

    const titles = getSelectedDocTitles();
    let prompts = [];

    if (titles.length === 0) {
        prompts = [
            "Tạo đề cương ôn tập tổng quát cho môn học",
            "Giải thích các khái niệm nền tảng quan trọng",
            "Tạo 5 câu hỏi trắc nghiệm ôn tập"
        ];
    } else if (titles.length === 1) {
        const t = titles[0];
        prompts = [
            `Tóm tắt các nội dung cốt lõi trong ${t}`,
            `Phân tích các khái niệm chính được trình bày trong ${t}`,
            `Tạo câu hỏi trắc nghiệm ôn tập theo ${t}`
        ];
    } else {
        const t1 = titles[0];
        const t2 = titles[1];
        prompts = [
            `Tóm tắt nội dung trọng tâm của ${t1}`,
            `Phân tích các điểm chính trong ${t2}`,
            `So sánh và tổng hợp kiến thức từ các tài liệu đã chọn`
        ];
    }

    container.innerHTML = prompts.map(p => `
        <button class="starter-chip btn btn-sm btn-dark-glass text-slate-300 rounded-pill px-3 py-1-5 border border-slate-700 fs-8 text-truncate"
                style="max-width: 420px;"
                onclick="fillPrompt('${escapeHtml(p)}')">
            ${escapeHtml(p)}
        </button>
    `).join('');
}

// Safe Citation Chip Click Handler via Dataset
function handleCitationChipClick(btn) {
    const index = btn.dataset.citIndex || '1';
    const title = btn.dataset.citTitle || 'Tài liệu trích dẫn';
    const page = btn.dataset.citPage || '1';
    const heading = btn.dataset.citHeading || 'Tổng quan';
    const snippet = btn.dataset.citSnippet || '';
    const score = btn.dataset.citScore || '0.90';

    populateAndShowCitationModal(index, title, page, heading, snippet, score);
}

// NotebookLM-Style Interactive Inline Citation Click Handler
function openInlineCitation(btn, index) {
    const bubble = btn.closest('.assistant-bubble');
    if (!bubble) return;
    
    try {
        const citationsJson = bubble.getAttribute('data-citations');
        if (citationsJson) {
            const citations = JSON.parse(citationsJson);
            const targetIdx = parseInt(index, 10);
            const found = citations.find(c => (c.index !== undefined ? c.index : c.Index) === targetIdx);
            if (found) {
                populateAndShowCitationModal(
                    found.index ?? found.Index ?? targetIdx, 
                    found.documentTitle ?? found.DocumentTitle ?? 'Tài liệu trích dẫn', 
                    found.pageNumber ?? found.PageNumber ?? 1, 
                    found.heading ?? found.Heading ?? 'Chung', 
                    found.snippet ?? found.Snippet ?? ''
                );
                return;
            }
        }
    } catch (e) {
        console.warn('Error reading inline citation data:', e);
    }
}

function populateAndShowCitationModal(index, title, page, heading, snippet) {
    document.getElementById('modalCitIndexBadge').innerText = index;
    document.getElementById('modalCitDocTitle').innerText = title;
    document.getElementById('modalCitPage').innerText = page;
    document.getElementById('modalCitHeading').innerText = heading || 'Chung';
    document.getElementById('modalCitSnippet').innerText = snippet;
    
    const modal = new bootstrap.Modal(document.getElementById('citationModal'));
    modal.show();
}

// Lightweight Client-Side Markdown Formatter with NotebookLM Inline Citations
function formatMarkdown(text) {
    if (!text) return '';
    let html = escapeHtml(text);

    // 1. Code blocks (```lang ... ```)
    html = html.replace(/```([a-zA-Z0-9_-]*)\n([\s\S]*?)```/g, function(match, lang, code) {
        return `<pre><button type="button" class="code-copy-btn" onclick="copyCodeBlock(this)">Sao chép</button><code>${code.trim()}</code></pre>`;
    });

    // 2. Inline code (`code`)
    html = html.replace(/`([^`]+)`/g, '<code>$1</code>');

    // 3. Bold (**text**) & Italic (*text*)
    html = html.replace(/\*\*([^*]+)\*\*/g, '<strong class="text-white">$1</strong>');
    html = html.replace(/\*([^*]+)\*/g, '<em>$1</em>');

    // 4. Headings (### Heading)
    html = html.replace(/^### (.*$)/gim, '<h6 class="fw-bold text-white mt-2 mb-1">$1</h6>');
    html = html.replace(/^## (.*$)/gim, '<h5 class="fw-bold text-white mt-2 mb-1">$1</h5>');

    // 5. NotebookLM Interactive Inline Citations [1], [2], [3]
    html = html.replace(/\[(\d+)\]/g, '<button type="button" class="inline-cit-pill" onclick="openInlineCitation(this, $1)" title="Xem nguồn trích dẫn $1">$1</button>');

    // 6. Linebreaks
    html = html.replace(/\n/g, '<br/>');

    return html;
}

function copyCodeBlock(btn) {
    const pre = btn.closest('pre');
    const code = pre.querySelector('code')?.innerText || '';
    navigator.clipboard.writeText(code).then(() => {
        btn.innerText = 'Đã chép!';
        setTimeout(() => btn.innerText = 'Sao chép', 2000);
    });
}

// Smooth Scroll to Bottom with Frame Tick
function smoothScrollToBottom() {
    const container = document.getElementById('chatMessages');
    if (!container) return;
    requestAnimationFrame(() => {
        container.scrollTop = container.scrollHeight;
    });
}

// Format all pre-rendered markdown messages and starter prompts on load
document.querySelectorAll('.markdown-body[data-raw-content]').forEach(el => {
    const raw = el.getAttribute('data-raw-content');
    if (raw) el.innerHTML = formatMarkdown(raw);
});
renderDynamicStarterPrompts();

// ── 4. Seamless SPA Functions (No Page Reloads) ─────────────────────────────────

function startNewChatSeamless() {
    _activeSessionId = 0;
    document.getElementById('chatHeaderTitle').innerText = "Cuộc trò chuyện mới";

    document.querySelectorAll('.session-item').forEach(item => {
        item.classList.remove('active-session');
        item.classList.add('hover-session');
        const icon = item.querySelector('.session-icon');
        const text = item.querySelector('.session-title-text');
        if (icon) icon.className = 'bi bi-chat-left text-slate-500 fs-8 session-icon';
        if (text) {
            text.classList.remove('text-white', 'fw-semibold');
            text.classList.add('text-slate-300');
        }
    });

    updateSlimRailActiveSession(0);
    renderHeroState();
    window.history.pushState({}, '', `?subjectId=${_activeSubjectId}`);
    const input = document.getElementById('chatInput');
    if (input) input.focus();
}

function renderHeroState() {
    const container = document.getElementById('chatMessages');
    container.innerHTML = `
        <div class="chat-hero-state d-flex flex-column align-items-center justify-content-center text-center p-4 my-auto">
            <div class="hero-sparkle-icon mb-3">
                <i class="bi bi-stars fs-1 text-primary-accent animate-pulse"></i>
            </div>
            <h3 class="fw-bold text-white mb-2">Đã đến lượt bạn hỏi, ${_currentUserName}!</h3>
            <p class="text-slate-400 fs-7 max-w-lg mb-4">
                Đặt câu hỏi liên quan đến tài liệu môn học <strong>${_currentSubjectCode}</strong>. 
                AI sẽ chỉ trả lời dựa trên các tài liệu bạn đã chọn kèm trích dẫn chính xác.
            </p>
            <div class="starter-chips d-flex flex-wrap gap-2 justify-content-center max-w-lg" id="starterChipsContainer"></div>
        </div>
    `;
    renderDynamicStarterPrompts();
}

function updateSlimRailActiveSession(sessionId) {
    document.querySelectorAll('#slimSessionsList button').forEach(btn => {
        btn.classList.remove('text-primary', 'active-slim-item');
        btn.classList.add('text-slate-400');
        const icon = btn.querySelector('i');
        if (icon) icon.className = 'bi bi-chat-left-text fs-5';
    });

    if (sessionId > 0) {
        const slimBtn = document.getElementById(`slimSessionItem-${sessionId}`);
        if (slimBtn) {
            slimBtn.classList.add('text-primary', 'active-slim-item');
            slimBtn.classList.remove('text-slate-400');
            const icon = slimBtn.querySelector('i');
            if (icon) icon.className = 'bi bi-chat-dots-fill fs-5';
        }
    }
}

async function loadSessionSeamless(sessionId, title, btn) {
    if (_activeSessionId === sessionId) return;
    _activeSessionId = sessionId;

    document.querySelectorAll('.session-item').forEach(item => {
        item.classList.remove('active-session');
        item.classList.add('hover-session');
        const icon = item.querySelector('.session-icon');
        const text = item.querySelector('.session-title-text');
        if (icon) icon.className = 'bi bi-chat-left text-slate-500 fs-8 session-icon';
        if (text) {
            text.classList.remove('text-white', 'fw-semibold');
            text.classList.add('text-slate-300');
        }
    });

    const parentItem = btn ? btn.closest('.session-item') : document.getElementById(`sessionItem-${sessionId}`);
    if (parentItem) {
        parentItem.classList.add('active-session');
        parentItem.classList.remove('hover-session');
        const icon = parentItem.querySelector('.session-icon');
        const text = parentItem.querySelector('.session-title-text');
        if (icon) icon.className = 'bi bi-chat-dots-fill text-primary fs-8 session-icon';
        if (text) {
            text.classList.add('text-white', 'fw-semibold');
            text.classList.remove('text-slate-300');
        }
    }

    updateSlimRailActiveSession(sessionId);
    document.getElementById('chatHeaderTitle').innerText = title || "Cuộc trò chuyện";
    window.history.pushState({}, '', `?subjectId=${_activeSubjectId}&sessionId=${sessionId}`);

    const container = document.getElementById('chatMessages');
    container.innerHTML = `
        <div class="d-flex align-items-center justify-content-center h-100 text-slate-400 gap-2">
            <div class="spinner-border spinner-border-sm text-primary"></div>
            <span class="fs-7">Đang tải cuộc trò chuyện...</span>
        </div>
    `;

    try {
        const res = await fetch(`?handler=SessionMessages&sessionId=${sessionId}`);
        const data = await res.json();
        if (data.success) {
            renderSessionMessages(data.messages);
        } else {
            container.innerHTML = `<div class="p-4 text-center text-danger fs-7">${data.message || 'Lỗi khi tải cuộc trò chuyện.'}</div>`;
        }
    } catch (err) {
        container.innerHTML = `<div class="p-4 text-center text-danger fs-7">Lỗi kết nối máy chủ.</div>`;
    }
}

function renderSessionMessages(messages) {
    const container = document.getElementById('chatMessages');
    container.innerHTML = '';

    if (!messages || messages.length === 0) {
        renderHeroState();
        return;
    }

    messages.forEach(msg => {
        if (msg.role === 'user') {
            const userRow = document.createElement('div');
            userRow.className = 'message-row user-row d-flex justify-content-end mb-3';
            userRow.innerHTML = `
                <div class="message-bubble user-bubble rounded-4 p-3 max-w-xl">
                    <div class="text-white fs-7 whitespace-pre-wrap">${escapeHtml(msg.content)}</div>
                </div>
            `;
            container.appendChild(userRow);
        } else {
            const aiRow = document.createElement('div');
            aiRow.className = 'message-row assistant-row d-flex justify-content-start mb-4';

            let followupsHtml = '';
            if (msg.suggestedFollowUps && msg.suggestedFollowUps.length > 0) {
                followupsHtml = `
                    <div class="followups-bar mt-3 pt-2 border-top border-slate-800/70 d-flex flex-column gap-1-5">
                        <span class="fs-8 text-slate-400 fw-semibold"><i class="bi bi-lightbulb text-warning me-1"></i> Câu hỏi gợi ý tiếp theo:</span>
                        <div class="d-flex flex-wrap gap-1-5">
                            ${msg.suggestedFollowUps.map(q => `
                                <button type="button" class="btn-followup-chip" onclick="submitFollowUpPrompt(this)" data-prompt="${escapeHtml(q)}">
                                    <span>${escapeHtml(q)}</span>
                                </button>
                            `).join('')}
                        </div>
                    </div>
                `;
            }

            const citationsJson = msg.citations ? JSON.stringify(msg.citations) : '[]';

            aiRow.innerHTML = `
                <div class="assistant-avatar rounded-circle d-flex align-items-center justify-content-center me-2 flex-shrink-0">
                    <i class="bi bi-robot text-primary-accent fs-6"></i>
                </div>
                <div class="message-bubble assistant-bubble rounded-4 p-3-5 max-w-2xl border border-slate-800 bg-slate-900/80 shadow-sm"
                     data-citations='${escapeHtml(citationsJson)}'>
                    <div class="markdown-body fs-7 text-slate-200">
                        ${formatMarkdown(msg.content)}
                    </div>
                    ${followupsHtml}
                </div>
            `;
            container.appendChild(aiRow);
        }
    });

    smoothScrollToBottom();
}

async function deleteSessionSeamless(sessionId, event, btn) {
    event.stopPropagation();
    if (!confirm('Bạn có chắc muốn xóa cuộc trò chuyện này?')) return;

    const sessionEl = document.getElementById(`sessionItem-${sessionId}`);
    const slimBtn = document.getElementById(`slimSessionItem-${sessionId}`);
    if (sessionEl) {
        sessionEl.style.opacity = '0.3';
        sessionEl.style.pointerEvents = 'none';
    }

    try {
        const res = await fetch(`?handler=DeleteSessionAjax&sessionId=${sessionId}`, {
            method: 'POST'
        });
        const data = await res.json();
        if (data.success) {
            if (sessionEl) sessionEl.remove();
            if (slimBtn) slimBtn.remove();
            if (_activeSessionId === sessionId) {
                startNewChatSeamless();
            }
            const sessionsList = document.getElementById('sessionsList');
            if (sessionsList && sessionsList.querySelectorAll('.session-item').length === 0) {
                sessionsList.innerHTML = '<div class="text-slate-500 fs-8 text-center py-4" id="noSessionsNotice"><i class="bi bi-chat-square-text fs-4 d-block mb-2 text-slate-600"></i>Chưa có cuộc trò chuyện nào.</div>';
            }
        } else {
            if (sessionEl) {
                sessionEl.style.opacity = '1';
                sessionEl.style.pointerEvents = 'auto';
            }
            alert('Lỗi khi xóa cuộc trò chuyện.');
        }
    } catch (err) {
        if (sessionEl) {
            sessionEl.style.opacity = '1';
            sessionEl.style.pointerEvents = 'auto';
        }
        alert('Lỗi kết nối khi xóa cuộc trò chuyện.');
    }
}

function prependSessionToSidebar(sessionId, title) {
    const noNotice = document.getElementById('noSessionsNotice');
    if (noNotice) noNotice.remove();

    const sessionsList = document.getElementById('sessionsList');
    if (sessionsList) {
        const div = document.createElement('div');
        div.id = `sessionItem-${sessionId}`;
        div.className = 'session-item d-flex align-items-center justify-content-between p-2 rounded-2 active-session transition';
        div.innerHTML = `
            <button type="button" class="btn btn-link text-decoration-none d-flex align-items-center gap-2 flex-grow-1 overflow-hidden p-0 text-start border-0 bg-transparent"
                    onclick="loadSessionSeamless(${sessionId}, '${escapeHtml(title)}', this)">
                <i class="bi bi-chat-dots-fill text-primary fs-8 session-icon"></i>
                <span class="fs-8 text-white fw-semibold text-truncate session-title-text" title="${escapeHtml(title)}">
                    ${escapeHtml(title)}
                </span>
            </button>
            <button type="button" class="btn btn-icon btn-sm text-slate-500 hover-text-danger p-0 border-0 bg-transparent ms-1 flex-shrink-0"
                    onclick="deleteSessionSeamless(${sessionId}, event, this)" title="Xóa cuộc trò chuyện">
                <i class="bi bi-trash fs-8"></i>
            </button>
        `;
        sessionsList.prepend(div);
    }

    const slimSessions = document.getElementById('slimSessionsList');
    if (slimSessions) {
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.id = `slimSessionItem-${sessionId}`;
        btn.className = 'btn btn-icon btn-sm text-primary active-slim-item p-2 rounded-2 hover-bg-slate-800';
        btn.title = title;
        btn.onclick = () => loadSessionSeamless(sessionId, title, null);
        btn.innerHTML = '<i class="bi bi-chat-dots-fill fs-5"></i>';
        slimSessions.prepend(btn);
    }
}

async function submitChatQuestion() {
    const input = document.getElementById('chatInput');
    const query = input.value.trim();
    if (!query) return;

    const selectedDocIds = getSelectedDocIds();
    if (selectedDocIds.length === 0) {
        showChatToast('Vui lòng chọn ít nhất 1 tài liệu nguồn ở cột bên trái!');
        return;
    }

    const isNewSession = _activeSessionId === 0;

    const messagesContainer = document.getElementById('chatMessages');
    const heroState = document.querySelector('.chat-hero-state');
    if (heroState) heroState.remove();

    const userRow = document.createElement('div');
    userRow.className = 'message-row user-row d-flex justify-content-end mb-3';
    userRow.innerHTML = `
        <div class="message-bubble user-bubble rounded-4 p-3 max-w-xl">
            <div class="text-white fs-7 whitespace-pre-wrap">${escapeHtml(query)}</div>
        </div>
    `;
    messagesContainer.appendChild(userRow);

    const aiRow = document.createElement('div');
    aiRow.className = 'message-row assistant-row d-flex justify-content-start mb-4';
    aiRow.innerHTML = `
        <div class="assistant-avatar rounded-circle d-flex align-items-center justify-content-center me-2 flex-shrink-0">
            <i class="bi bi-robot text-primary-accent fs-6"></i>
        </div>
        <div class="message-bubble assistant-bubble rounded-4 p-3-5 max-w-2xl border border-slate-800 bg-slate-900/80 shadow-sm" id="activeStreamingBubble">
            <div class="d-flex align-items-center gap-2 text-slate-400 fs-7" id="streamingSpinnerIndicator">
                <div class="spinner-border spinner-border-sm text-primary" role="status"></div>
                <span>Đang trích xuất tri thức & tổng hợp câu trả lời...</span>
            </div>
            <div class="markdown-body fs-7 text-slate-200 d-none" id="streamingTextContent"></div>
        </div>
    `;
    messagesContainer.appendChild(aiRow);
    smoothScrollToBottom();

    input.value = '';
    autoResize(input);
    document.getElementById('btnSendChat').disabled = true;

    const payload = {
        sessionId: _activeSessionId > 0 ? _activeSessionId : null,
        subjectId: _activeSubjectId,
        message: query,
        selectedDocumentIds: selectedDocIds
    };

    let accumulatedRawText = "";
    const bubble = aiRow.querySelector('#activeStreamingBubble');
    const spinner = aiRow.querySelector('#streamingSpinnerIndicator');
    const textContainer = aiRow.querySelector('#streamingTextContent');

    // Attempt real-time SignalR token streaming (Option B)
    if (_signalrConnection && _signalrConnection.state === signalR.HubConnectionState.Connected) {
        try {
            _signalrConnection.stream("StreamChatMessage", payload).subscribe({
                next: (packet) => {
                    if (packet.type === 'init') {
                        const prevSessionId = _activeSessionId;
                        _activeSessionId = packet.sessionId;
                        document.getElementById('chatHeaderTitle').innerText = packet.sessionTitle || "Cuộc trò chuyện";
                        window.history.pushState({}, '', `?subjectId=${_activeSubjectId}&sessionId=${packet.sessionId}`);

                        if (isNewSession || prevSessionId === 0) {
                            prependSessionToSidebar(packet.sessionId, packet.sessionTitle);
                        }
                    } else if (packet.type === 'token') {
                        if (spinner) spinner.classList.add('d-none');
                        if (textContainer) {
                            textContainer.classList.remove('d-none');
                            accumulatedRawText += packet.token;
                            textContainer.innerHTML = formatMarkdown(accumulatedRawText) + '<span class="streaming-cursor animate-pulse">▋</span>';
                            smoothScrollToBottom();
                        }
                    } else if (packet.type === 'done') {
                        if (bubble && packet.assistantMessage) {
                            const citationsJson = packet.assistantMessage.citations ? JSON.stringify(packet.assistantMessage.citations) : '[]';
                            bubble.setAttribute('data-citations', citationsJson);
                            if (textContainer) {
                                textContainer.innerHTML = formatMarkdown(packet.assistantMessage.content || accumulatedRawText);
                            }
                        }
                    }
                },
                complete: () => {
                    document.getElementById('btnSendChat').disabled = false;
                    smoothScrollToBottom();
                },
                error: (err) => {
                    console.warn("SignalR stream error, falling back to standard AJAX:", err);
                    fallbackAjaxChat(payload, aiRow, query, isNewSession);
                }
            });
            return;
        } catch (e) {
            console.warn("Could not initiate SignalR stream:", e);
        }
    }

    // Fallback: Standard AJAX Request
    await fallbackAjaxChat(payload, aiRow, query, isNewSession);
}

async function fallbackAjaxChat(payload, aiRow, query, isNewSession) {
    const bubble = aiRow.querySelector('.assistant-bubble');
    try {
        const res = await fetch('?handler=SendMessage', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        const data = await res.json();
        document.getElementById('btnSendChat').disabled = false;

        if (data.success && data.assistantMessage) {
            const prevSessionId = _activeSessionId;
            _activeSessionId = data.sessionId;
            document.getElementById('chatHeaderTitle').innerText = data.sessionTitle || "Cuộc trò chuyện";
            window.history.pushState({}, '', `?subjectId=${_activeSubjectId}&sessionId=${data.sessionId}`);

            if (isNewSession || prevSessionId === 0) {
                prependSessionToSidebar(data.sessionId, data.sessionTitle);
            }

            const citationsJson = data.assistantMessage.citations ? JSON.stringify(data.assistantMessage.citations) : '[]';
            bubble.setAttribute('data-citations', citationsJson);
            bubble.innerHTML = `
                <div class="markdown-body fs-7 text-slate-200">
                    ${formatMarkdown(data.assistantMessage.content)}
                </div>
            `;
            smoothScrollToBottom();
        } else {
            showChatToast(data.message || 'Lỗi khi xử lý câu hỏi.');
            aiRow.remove();
        }
    } catch (err) {
        document.getElementById('btnSendChat').disabled = false;
        showChatToast('Lỗi kết nối máy chủ khi gửi tin nhắn.');
        aiRow.remove();
    }
}

function escapeHtml(text) {
    if (!text) return '';
    return text.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#039;");
}
