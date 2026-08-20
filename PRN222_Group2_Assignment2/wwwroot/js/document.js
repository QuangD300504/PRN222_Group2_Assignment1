/**
 * AI Study Hub - Document Management Script
 * Handles Subject switching, Chunk inspection, Chapter management, Uploads & SignalR live sync
 */

let _activeSubjectId = window.DocConfig?.activeSubjectId || 0;
const _subjectDescriptions = window.DocConfig?.subjectDescriptions || {};

function escapeHtml(text) {
    if (!text) return '';
    return text.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#039;");
}

// ── 1. Subject Switching & Filtering ──────────────────────────────────────────────
function switchSubject(subjectId) {
    if (subjectId === _activeSubjectId) return;
    _activeSubjectId = subjectId;

    // Update pill active state
    document.querySelectorAll('[data-subject-id]').forEach(btn => {
        const isActive = parseInt(btn.dataset.subjectId) === subjectId;
        if (isActive) {
            btn.classList.remove('btn-slate-800', 'text-slate-300', 'border-slate-700', 'hover-slate-700');
            btn.classList.add('btn-primary', 'bg-gradient-primary', 'border-0', 'text-white', 'shadow-sm');
        } else {
            btn.classList.remove('btn-primary', 'bg-gradient-primary', 'border-0', 'text-white', 'shadow-sm');
            btn.classList.add('btn-slate-800', 'text-slate-300', 'border-slate-700', 'hover-slate-700');
        }
    });

    // Update subject info callout
    const callout = document.getElementById('subjectDescCallout');
    const info = _subjectDescriptions[subjectId];
    if (info && (info.name || info.desc)) {
        const badge = document.getElementById('subjectCodeBadge');
        const nameText = document.getElementById('subjectNameText');
        const descEl = document.getElementById('subjectDescText');
        if (badge) badge.textContent = info.code;
        if (nameText) nameText.textContent = info.name;
        if (descEl) {
            if (info.desc) {
                descEl.textContent = info.desc;
                descEl.classList.remove('d-none');
            } else {
                descEl.classList.add('d-none');
            }
        }
        if (callout) callout.classList.remove('d-none');
    } else {
        if (callout) callout.classList.add('d-none');
    }

    const region = document.getElementById('doc-partial-region');
    if (region) {
        region.style.opacity = '0.4';
        fetch(`?handler=DocumentsPartial&subjectId=${subjectId}`)
            .then(r => r.text())
            .then(html => {
                region.innerHTML = html;
                region.style.opacity = '1';
            })
            .catch(() => { region.style.opacity = '1'; });
    }
}

function applyFilters() {
    const subjectId = document.getElementById('filterSubjectId')?.value
        || (typeof _activeSubjectId !== 'undefined' ? _activeSubjectId : 0);
    const chapterId = document.getElementById('filterChapterId')?.value ?? '';
    const fileType  = document.getElementById('filterFileType')?.value ?? '';
    const search    = document.getElementById('filterSearch')?.value ?? '';

    const params = new URLSearchParams({ subjectId });
    if (chapterId) params.set('chapterId', chapterId);
    if (fileType)  params.set('fileType', fileType);
    if (search)    params.set('search', search);

    const region = document.getElementById('doc-partial-region');
    if (region) {
        region.style.opacity = '0.4';
        fetch(`?handler=DocumentsPartial&${params}`)
            .then(r => r.text())
            .then(html => { region.innerHTML = html; region.style.opacity = '1'; })
            .catch(() => { region.style.opacity = '1'; });
    }
}

function clearFilters() {
    const subjectId = document.getElementById('filterSubjectId')?.value
        || (typeof _activeSubjectId !== 'undefined' ? _activeSubjectId : 0);
    const region = document.getElementById('doc-partial-region');
    if (region) {
        region.style.opacity = '0.4';
        fetch(`?handler=DocumentsPartial&subjectId=${subjectId}`)
            .then(r => r.text())
            .then(html => { region.innerHTML = html; region.style.opacity = '1'; })
            .catch(() => { region.style.opacity = '1'; });
    }
}

// ── 2. Chunks Modal ──────────────────────────────────────────────────────────────
function viewDocumentChunks(docId) {
    const modal = new bootstrap.Modal(document.getElementById('chunksModal'));
    const modalBody = document.getElementById('chunksModalBody');
    const modalSubtitle = document.getElementById('modalDocSubtitle');

    modalBody.innerHTML = `
        <div class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <div class="text-slate-400 fs-8 mt-2">Đang tải thông tin chunks...</div>
        </div>`;
    modal.show();

    fetch(`?handler=Chunks&id=${docId}`)
        .then(res => {
            if (!res.ok) throw new Error('Document not found');
            return res.json();
        })
        .then(data => {
            modalSubtitle.innerHTML = `<span class="text-slate-200 fw-semibold">${escapeHtml(data.title)}</span> • ${escapeHtml(data.subjectName)} (${escapeHtml(data.chapterTitle)}) • ${data.chunkCount} Chunks`;

            if (!data.chunks || data.chunks.length === 0) {
                modalBody.innerHTML = `<div class="text-center py-4 text-slate-400">Chưa có chunk nào được trích xuất cho tài liệu này.</div>`;
                return;
            }

            let html = `<div class="mb-3 d-flex align-items-center justify-content-between bg-slate-850 p-3 rounded-3 border border-slate-800">
                <div>
                    <span class="badge bg-primary-subtle text-primary-accent me-2">${escapeHtml(data.fileExtension || '')}</span>
                    <span class="text-slate-300 fs-7">${escapeHtml(data.fileName || '')} (${data.formattedSize || ''})</span>
                </div>
                <span class="status-pill status-pill-ready"><i class="bi bi-check-circle-fill"></i> ${escapeHtml(data.status || 'Ready')}</span>
            </div>
            <div class="d-flex flex-column gap-3">`;

            data.chunks.forEach(c => {
                const chunkNum = (typeof c.chunkIndex === 'number') ? (c.chunkIndex + 1) : 1;
                html += `
                    <div class="chunk-card p-3 rounded-3 border border-slate-800 bg-slate-850">
                        <div class="d-flex align-items-center justify-content-between mb-2 pb-2 border-bottom border-slate-800">
                            <div class="d-flex align-items-center gap-2">
                                <span class="badge bg-slate-800 text-primary-accent border border-slate-700 fs-8">Chunk #${chunkNum}</span>
                                <span class="text-slate-400 fs-8"><i class="bi bi-journal-page"></i> Trang ${c.pageNumber || 1}</span>
                                ${c.heading ? `<span class="badge bg-slate-800 text-slate-300 fs-8">${escapeHtml(c.heading)}</span>` : ''}
                            </div>
                            <div class="d-flex align-items-center gap-2">
                                <span class="text-slate-400 fs-8">${c.tokenCount || 0} Tokens</span>
                                ${c.hasEmbedding ? `<span class="badge bg-emerald-500-subtle text-emerald-400 fs-8"><i class="bi bi-vector-pen"></i> Embedded</span>` : ''}
                            </div>
                        </div>
                        <div class="chunk-content text-slate-300 fs-7 whitespace-pre-wrap">${escapeHtml(c.content || '')}</div>
                    </div>
                `;
            });
            html += '</div>';
            modalBody.innerHTML = html;
        })
        .catch(err => {
            modalBody.innerHTML = `
                <div class="alert alert-danger rounded-3 fs-7 py-2 px-3">
                    <i class="bi bi-exclamation-triangle-fill me-1"></i> Không thể tải danh sách chunks: ${err.message}
                </div>`;
        });
}

function deleteDocument(docId, title) {
    if (!confirm(`Bạn có chắc chắn muốn xóa tài liệu "${title}"?\nHành động này không thể hoàn tác.`)) return;

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
    const formData = new FormData();
    formData.append('id', docId);
    formData.append('__RequestVerificationToken', token);

    fetch('?handler=Delete', {
        method: 'POST',
        body: formData
    })
    .then(r => r.json())
    .then(data => {
        if (data.success) {
            showSignalRToast(`✅ ${data.message}`, 'success');
            refreshCurrentSubjectTable(_activeSubjectId);
        } else {
            showSignalRToast(`❌ ${data.message}`, 'danger');
        }
    })
    .catch(() => {
        showSignalRToast('Lỗi kết nối khi xóa tài liệu.', 'danger');
    });
}

// ── 3. Upload Modal & SignalR Progress ────────────────────────────────────────────
function updateFilePreview(input) {
    if (input.files && input.files[0]) {
        const file = input.files[0];
        document.getElementById('dropzoneContent')?.classList.add('d-none');
        document.getElementById('selectedFileInfo')?.classList.remove('d-none');
        const nameEl = document.getElementById('selectedFileName');
        if (nameEl) nameEl.innerText = file.name;
        const sizeEl = document.getElementById('selectedFileSize');
        if (sizeEl) sizeEl.innerText = (file.size / (1024 * 1024)).toFixed(2) + ' MB';
        const docTitle = document.getElementById('docTitleInput');
        if (docTitle && !docTitle.value) docTitle.value = file.name.replace(/\.[^/.]+$/, '');
    }
}

function onChapterChange(select) {
    const newChapterForm = document.getElementById('newChapterForm');
    if (newChapterForm) newChapterForm.classList.toggle('d-none', select.value !== '__new__');
}

async function onUploadSubjectChange(subjectId) {
    const uploadSelect = document.getElementById('uploadSubjectSelect');
    const selectedOpt = uploadSelect?.options[uploadSelect.selectedIndex];
    const desc = selectedOpt?.getAttribute('data-desc') || '';
    const descContainer = document.getElementById('uploadSubjectDesc');
    if (descContainer) {
        descContainer.innerHTML = desc ? `<span><i class="bi bi-info-circle"></i> ${escapeHtml(desc)}</span>` : '';
    }

    const chapterSelect = document.getElementById('chapterSelect');
    if (!chapterSelect) return;
    chapterSelect.innerHTML = '<option value="">-- Đang tải danh sách chương... --</option>';
    try {
        const resp = await fetch(`?handler=Chapters&subjectId=${subjectId}`);
        const chapters = await resp.json();
        let html = '<option value="">-- Toàn bộ môn học (không thuộc chương cụ thể) --</option>';
        chapters.forEach(ch => {
            html += `<option value="${ch.id}">Ch.${ch.chapterNumber} – ${ch.title}</option>`;
        });
        html += '<option value="__new__">+ Tạo chương mới...</option>';
        chapterSelect.innerHTML = html;
        const newChapterForm = document.getElementById('newChapterForm');
        if (newChapterForm) newChapterForm.classList.add('d-none');
    } catch (err) {
        console.error('Failed to fetch chapters', err);
    }
}

async function submitUpload() {
    const fileInput = document.getElementById('fileInput');
    const alertEl = document.getElementById('uploadAlert');
    const btn = document.getElementById('uploadSubmitBtn');

    if (!fileInput.files || !fileInput.files[0]) {
        showUploadAlert('danger', 'Vui lòng chọn file PDF, DOCX hoặc PPTX.');
        return;
    }

    const chapterSelect = document.getElementById('chapterSelect');
    const isNewChapter = chapterSelect?.value === '__new__';
    const selectedSubjectId = document.getElementById('uploadSubjectSelect')?.value || _activeSubjectId;

    const formData = new FormData();
    formData.append('file', fileInput.files[0]);
    formData.append('title', document.getElementById('docTitleInput')?.value.trim() || '');
    formData.append('subjectId', selectedSubjectId);

    if (isNewChapter) {
        const num = document.getElementById('newChapterNumber')?.value;
        const title = document.getElementById('newChapterTitle')?.value.trim();
        if (!num || !title) {
            showUploadAlert('danger', 'Vui lòng nhập số chương và tên chương.');
            return;
        }
        formData.append('newChapterNumber', num);
        formData.append('newChapterTitle', title);
    } else if (chapterSelect && chapterSelect.value) {
        formData.append('chapterId', chapterSelect.value);
    }

    if (signalrConn && signalrConn.connectionId) {
        formData.append('connectionId', signalrConn.connectionId);
    }
    formData.append('__RequestVerificationToken', document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '');

    const progressContainer = document.getElementById('uploadProgressContainer');
    const progressBar = document.getElementById('uploadProgressBar');
    const progressPercent = document.getElementById('uploadProgressPercent');
    const progressMsg = document.getElementById('uploadProgressMsg');
    if (progressContainer) {
        progressContainer.classList.remove('d-none');
        if (progressBar) progressBar.style.width = '10%';
        if (progressPercent) progressPercent.innerText = '10%';
        if (progressMsg) progressMsg.innerText = 'Đang tải tệp lên máy chủ...';
    }

    if (btn) {
        btn.disabled = true;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Đang xử lý...';
    }

    try {
        const resp = await fetch('?handler=Upload', { method: 'POST', body: formData });
        const data = await resp.json();

        if (data.success) {
            showUploadAlert('success', `✅ ${data.message}`);
            if (data.subjectId && data.newDocCount !== undefined) {
                const countEl = document.getElementById(`subject-count-${data.subjectId}`);
                if (countEl) {
                    countEl.setAttribute('data-count', data.newDocCount);
                    countEl.innerText = `(${data.newDocCount})`;
                }
            }
            setTimeout(() => {
                const modalEl = document.getElementById('uploadModal');
                const modal = bootstrap.Modal.getInstance(modalEl);
                if (modal) modal.hide();

                // Reset form state
                if (fileInput) fileInput.value = '';
                const docTitleInput = document.getElementById('docTitleInput');
                if (docTitleInput) docTitleInput.value = '';
                if (chapterSelect) chapterSelect.value = '';
                document.getElementById('dropzoneContent')?.classList.remove('d-none');
                document.getElementById('selectedFileInfo')?.classList.add('d-none');
                if (alertEl) alertEl.classList.add('d-none');
                if (progressContainer) progressContainer.classList.add('d-none');
                if (btn) {
                    btn.disabled = false;
                    btn.innerHTML = '<i class="bi bi-cpu-fill"></i> Xử lý &amp; Index Document';
                }

                refreshCurrentSubjectTable(_activeSubjectId);
            }, 1200);
        } else {
            showUploadAlert('danger', `❌ ${data.message}`);
            if (progressContainer) progressContainer.classList.add('d-none');
            if (btn) {
                btn.disabled = false;
                btn.innerHTML = '<i class="bi bi-cpu-fill"></i> Xử lý &amp; Index Document';
            }
        }
    } catch (err) {
        showUploadAlert('danger', 'Lỗi kết nối. Vui lòng thử lại.');
        if (progressContainer) progressContainer.classList.add('d-none');
        if (btn) {
            btn.disabled = false;
            btn.innerHTML = '<i class="bi bi-cpu-fill"></i> Xử lý &amp; Index Document';
        }
    }
}

function showUploadAlert(type, msg) {
    const el = document.getElementById('uploadAlert');
    if (el) {
        el.className = `alert alert-${type} rounded-3 fs-7`;
        el.innerHTML = msg;
        el.classList.remove('d-none');
    }
}

// ── 4. Manage Subject & Chapters Modal ───────────────────────────────────────────
function openManageSubjectModal() {
    fetch(`?handler=ManageSubjectModalPartial&subjectId=${_activeSubjectId}`)
        .then(r => r.text())
        .then(html => {
            const region = document.getElementById('manageSubjectModalRegion');
            if (region) region.innerHTML = html;
            const modalEl = document.getElementById('manageSubjectModal');
            if (modalEl) {
                const modal = new bootstrap.Modal(modalEl);
                modal.show();

                if (signalrConn && signalrConn.state === signalR.HubConnectionState.Connected) {
                    const code = window.DocConfig?.subjectDescriptions?.[_activeSubjectId]?.code || '';
                    const user = window.DocConfig?.currentUserName || 'Subject Leader';
                    signalrConn.invoke("NotifyEditingSubject", _activeSubjectId, code, user).catch(() => {});
                }

                modalEl.addEventListener('hidden.bs.modal', function () {
                    if (signalrConn && signalrConn.state === signalR.HubConnectionState.Connected) {
                        const user = window.DocConfig?.currentUserName || 'Subject Leader';
                        signalrConn.invoke("NotifyFinishedEditingSubject", _activeSubjectId, user).catch(() => {});
                    }
                }, { once: true });
            }
        });
}

async function saveSubjectInfo() {
    const subjectId = document.getElementById('manageSubjectId')?.value || _activeSubjectId;
    const code = (document.getElementById('subjectCodeInput')?.value || document.getElementById('editSubjectCode')?.value || '').trim().toUpperCase();
    const name = (document.getElementById('subjectNameInput')?.value || document.getElementById('editSubjectName')?.value || '').trim();
    const desc = (document.getElementById('subjectDescInput')?.value || document.getElementById('editSubjectDesc')?.value || '').trim();

    if (!code || !name) {
        showSubjectAlert('danger', 'Vui lòng nhập đầy đủ Mã và Tên môn học.');
        return;
    }

    const formData = new FormData();
    formData.append('subjectId', subjectId);
    formData.append('code', code);
    formData.append('name', name);
    formData.append('description', desc);
    formData.append('__RequestVerificationToken', document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '');

    try {
        const resp = await fetch('?handler=UpdateSubject', { method: 'POST', body: formData });
        const data = await resp.json();
        if (data.success) {
            showSubjectAlert('success', '✅ ' + data.message);
            setTimeout(() => window.location.reload(), 1000);
        } else {
            showSubjectAlert('danger', '❌ ' + data.message);
        }
    } catch (err) {
        showSubjectAlert('danger', 'Lỗi hệ thống. Vui lòng thử lại.');
    }
}
const saveSubjectMeta = saveSubjectInfo;

async function deleteSubjectItem(id, code) {
    const subjectId = id || document.getElementById('manageSubjectId')?.value || _activeSubjectId;
    if (!subjectId) return;
    if (!confirm(`⚠️ CẢNH BÁO: Bạn có chắc chắn muốn XÓA môn học "${code || ''}"?\nTất cả chương và tài liệu thuộc môn học này sẽ bị xóa khỏi hệ thống!`)) return;

    const formData = new FormData();
    formData.append('id', subjectId);
    formData.append('__RequestVerificationToken', document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '');

    try {
        const resp = await fetch('?handler=DeleteSubject', { method: 'POST', body: formData });
        const data = await resp.json();
        if (data.success) {
            showSubjectAlert('success', '✅ ' + data.message);
            setTimeout(() => { window.location.href = '/Document/Index'; }, 1000);
        } else {
            showSubjectAlert('danger', '❌ ' + data.message);
        }
    } catch (err) {
        showSubjectAlert('danger', 'Lỗi hệ thống. Vui lòng thử lại.');
    }
}

function editChapter(id, num, title, summary) {
    const idInput = document.getElementById('editChapterId');
    const numInput = document.getElementById('editChapterNum');
    const titleInput = document.getElementById('editChapterTitleInput');
    const sumInput = document.getElementById('editChapterSummaryInput');
    const heading = document.getElementById('chapterEditTitle');

    if (idInput) idInput.value = id;
    if (numInput) numInput.value = num;
    if (titleInput) titleInput.value = title;
    if (sumInput) sumInput.value = summary || '';
    if (heading) heading.innerHTML = `<i class="bi bi-pencil-square text-primary-accent"></i> Chỉnh sửa chương ${num}`;
}
const startEditChapter = editChapter;

function resetChapterForm() {
    const idInput = document.getElementById('editChapterId');
    const numInput = document.getElementById('editChapterNum');
    const titleInput = document.getElementById('editChapterTitleInput');
    const sumInput = document.getElementById('editChapterSummaryInput');
    const heading = document.getElementById('chapterEditTitle');

    if (idInput) idInput.value = '';
    if (numInput) numInput.value = '';
    if (titleInput) titleInput.value = '';
    if (sumInput) sumInput.value = '';
    if (heading) heading.innerHTML = '<i class="bi bi-plus-circle text-primary-accent"></i> Thêm chương mới';
}

async function saveChapterItem() {
    const subjectId = document.getElementById('manageSubjectId')?.value || _activeSubjectId;
    const chapterId = document.getElementById('editChapterId')?.value || null;
    const num = document.getElementById('editChapterNum')?.value;
    const title = document.getElementById('editChapterTitleInput')?.value?.trim();
    const summary = document.getElementById('editChapterSummaryInput')?.value?.trim();

    if (!num || !title) {
        showSubjectAlert('danger', 'Vui lòng nhập đầy đủ Số chương và Tiêu đề chương.');
        return;
    }

    const formData = new FormData();
    formData.append('subjectId', subjectId);
    if (chapterId) formData.append('id', chapterId);
    formData.append('chapterNumber', num);
    formData.append('title', title);
    if (summary) formData.append('summary', summary);
    formData.append('__RequestVerificationToken', document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '');

    try {
        const resp = await fetch('?handler=SaveChapter', { method: 'POST', body: formData });
        const data = await resp.json();
        if (data.success) {
            showSubjectAlert('success', '✅ ' + data.message);
            setTimeout(() => window.location.reload(), 1000);
        } else {
            showSubjectAlert('danger', '❌ ' + data.message);
        }
    } catch (err) {
        showSubjectAlert('danger', 'Lỗi hệ thống. Vui lòng thử lại.');
    }
}

async function deleteChapterItem(chapterId) {
    if (!confirm('Bạn có chắc chắn muốn xóa chương này?\nCác tài liệu thuộc chương này sẽ được chuyển về "Toàn bộ môn học".')) return;

    const formData = new FormData();
    formData.append('id', chapterId);
    formData.append('__RequestVerificationToken', document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '');

    try {
        const resp = await fetch('?handler=DeleteChapter', { method: 'POST', body: formData });
        const data = await resp.json();
        if (data.success) {
            showSubjectAlert('success', '✅ ' + data.message);
            setTimeout(() => window.location.reload(), 1000);
        } else {
            showSubjectAlert('danger', '❌ ' + data.message);
        }
    } catch (err) {
        showSubjectAlert('danger', 'Lỗi hệ thống. Vui lòng thử lại.');
    }
}

function showSubjectAlert(type, msg) {
    const el = document.getElementById('subjectAlert');
    if (el) {
        el.className = `alert alert-${type} alert-dismissible fade show py-2 px-3 fs-7 rounded-3`;
        el.innerHTML = msg;
        el.classList.remove('d-none');
    }
}

// ── 5. Create Subject Modal ───────────────────────────────────────────────────────
async function createSubjectSubmit() {
    const code = document.getElementById('newSubjectCode').value.trim();
    const name = document.getElementById('newSubjectName').value.trim();
    const desc = document.getElementById('newSubjectDesc').value.trim();
    const alertEl = document.getElementById('createSubjectAlert');

    if (!code || !name) {
        alertEl.className = 'alert alert-danger rounded-3 fs-7 py-2 px-3';
        alertEl.innerHTML = 'Vui lòng nhập Mã môn học và Tên môn học.';
        alertEl.classList.remove('d-none');
        return;
    }

    const formData = new FormData();
    formData.append('code', code);
    formData.append('name', name);
    formData.append('description', desc);
    formData.append('__RequestVerificationToken', document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '');

    try {
        const resp = await fetch('?handler=CreateSubject', { method: 'POST', body: formData });
        const data = await resp.json();
        if (data.success) {
            alertEl.className = 'alert alert-success rounded-3 fs-7 py-2 px-3';
            alertEl.innerHTML = '✅ ' + data.message;
            alertEl.classList.remove('d-none');
            setTimeout(() => {
                window.location.href = '/Document/Index?selectedSubjectId=' + data.subjectId;
            }, 1000);
        } else {
            alertEl.className = 'alert alert-danger rounded-3 fs-7 py-2 px-3';
            alertEl.innerHTML = '❌ ' + data.message;
            alertEl.classList.remove('d-none');
        }
    } catch (err) {
        alertEl.className = 'alert alert-danger rounded-3 fs-7 py-2 px-3';
        alertEl.innerHTML = 'Lỗi hệ thống. Vui lòng thử lại.';
        alertEl.classList.remove('d-none');
    }
}

// ── 6. SignalR Real-Time Client Integration ────────────────────────────────────────
const signalrConn = new signalR.HubConnectionBuilder()
    .withUrl("/documentHub")
    .withAutomaticReconnect()
    .build();

signalrConn.on("DocumentUploaded", function (subjectId, title, newDocCount) {
    showSignalRToast(`📢 Tài liệu mới "${title}" vừa được tải lên!`, 'success');
    if (subjectId && newDocCount !== undefined) {
        const countEl = document.getElementById(`subject-count-${subjectId}`);
        if (countEl) {
            countEl.setAttribute('data-count', newDocCount);
            countEl.innerText = `(${newDocCount})`;
        }
    }
    refreshCurrentSubjectTable(subjectId);
});

signalrConn.on("DocumentDeleted", function (subjectId, docId, newDocCount) {
    showSignalRToast(`📢 Một tài liệu vừa bị xóa khỏi hệ thống!`, 'warning');
    if (subjectId && newDocCount !== undefined) {
        const countEl = document.getElementById(`subject-count-${subjectId}`);
        if (countEl) {
            countEl.setAttribute('data-count', newDocCount);
            countEl.innerText = `(${newDocCount})`;
        }
    }
    refreshCurrentSubjectTable(subjectId);
});

signalrConn.on("SubjectUpdated", function (subjectId, actionType) {
    showSignalRToast(`📢 Dữ liệu môn học / chương vừa được cập nhật!`, 'info');
    setTimeout(() => window.location.reload(), 1500);
});

signalrConn.on("UploadProgress", function (percent, msg) {
    const progressContainer = document.getElementById('uploadProgressContainer');
    const progressBar = document.getElementById('uploadProgressBar');
    const progressPercent = document.getElementById('uploadProgressPercent');
    const progressMsg = document.getElementById('uploadProgressMsg');
    if (progressContainer) {
        progressContainer.classList.remove('d-none');
        if (progressBar) progressBar.style.width = percent + '%';
        if (progressPercent) progressPercent.innerText = percent + '%';
        if (progressMsg) progressMsg.innerText = msg;
    }
});

signalrConn.on("UserEditingSubject", function (userName, subjectId, subjectCode) {
    const banner = document.getElementById('concurrentEditBanner');
    const msg = document.getElementById('concurrentEditMessage');
    if (banner && msg) {
        msg.innerHTML = `<strong>${escapeHtml(userName || 'Subject Leader')}</strong> đang thực hiện chỉnh sửa cấu trúc môn học <strong>${escapeHtml(subjectCode || '')}</strong>...`;
        banner.classList.remove('d-none');
    }
});

signalrConn.on("UserFinishedEditingSubject", function (userName, subjectId) {
    const banner = document.getElementById('concurrentEditBanner');
    if (banner) banner.classList.add('d-none');
});

signalrConn.start().then(() => {
    console.log("✅ SignalR connected to /documentHub.");
}).catch(err => console.error("SignalR connection error:", err));

function refreshCurrentSubjectTable(evtSubjectId) {
    const currentSubId = (typeof _activeSubjectId !== 'undefined') ? _activeSubjectId : 0;
    if (!evtSubjectId || evtSubjectId == currentSubId) {
        if (typeof applyFilters === 'function') {
            applyFilters();
        } else {
            const region = document.getElementById('doc-partial-region');
            if (region) {
                region.style.opacity = '0.4';
                fetch(`?handler=DocumentsPartial&subjectId=${currentSubId}`)
                    .then(r => r.text())
                    .then(html => { region.innerHTML = html; region.style.opacity = '1'; })
                    .catch(() => { region.style.opacity = '1'; });
            }
        }
    }
}

function showSignalRToast(msg, type) {
    const toast = document.createElement('div');
    toast.className = `alert alert-${type} alert-dismissible fade show position-fixed bottom-0 end-0 m-4 shadow-lg border-0 rounded-3 z-3`;
    toast.style.minWidth = '320px';
    toast.innerHTML = `<div>${msg}</div><button type="button" class="btn-close" data-bs-dismiss="alert"></button>`;
    document.body.appendChild(toast);
    setTimeout(() => { if (toast.parentNode) toast.remove(); }, 5000);
}
