(function () {
    const previewModalEl = document.getElementById('documentPreviewModal');
    if (!previewModalEl) {
        return;
    }

    const previewFrame = document.getElementById('documentPreviewFrame');
    const previewTitle = document.getElementById('documentPreviewTitle');
    const modal = new bootstrap.Modal(previewModalEl);

    document.addEventListener('click', function (event) {
        const trigger = event.target.closest('[data-document-preview]');
        if (!trigger) {
            return;
        }

        event.preventDefault();
        const url = trigger.getAttribute('data-document-preview');
        const title = trigger.getAttribute('data-document-name') ?? 'Document preview';

        if (previewTitle) {
            previewTitle.textContent = title;
        }

        if (previewFrame) {
            previewFrame.src = url;
        }

        modal.show();
    });

    previewModalEl.addEventListener('hidden.bs.modal', function () {
        if (previewFrame) {
            previewFrame.src = '';
        }
    });
})();
