(() => {
    const BootstrapModal = window.bootstrap?.Modal ?? null;

    const ready = () => {
        const form = document.querySelector('[data-followup-form]');
        if (form) {
            initFollowUpForm(form);
        }
    };

    function initFollowUpForm(form) {
        const callStatusSelect = form.querySelector('#FollowUpStatus');
        const nextSection = document.getElementById('nextFollowUpSection');
        const conversionSelect = form.querySelector('#IsConverted');
        const modalElement = document.getElementById('soldProductModal');
        const salesButton = form.querySelector('[data-action="open-sales-modal"]');
        const salesButtonText = salesButton?.querySelector('[data-sales-button-text]');
        const salesHint = form.querySelector('[data-sales-hint]');
        const salesSummary = document.getElementById('salesDetailsSummary');
        const summaryFields = {
            product: salesSummary?.querySelector('[data-sales-summary="product"]'),
            ticket: salesSummary?.querySelector('[data-sales-summary="ticket"]'),
            policyNumber: salesSummary?.querySelector('[data-sales-summary="policyNumber"]'),
            policyDate: salesSummary?.querySelector('[data-sales-summary="policyDate"]')
        };
        const modal = modalElement && BootstrapModal ? new BootstrapModal(modalElement) : null;
        const cancelButton = modalElement?.querySelector('[data-action="cancel-sales-modal"]');
        const saveButton = modalElement?.querySelector('[data-action="save-sales-modal"]');
        const requiredInputs = modalElement ? Array.from(modalElement.querySelectorAll('[data-sales-required]')) : [];
        let lastCommittedConversionValue = conversionSelect?.value ?? '';

        requiredInputs.forEach(input => {
            input.addEventListener('input', () => input.classList.remove('is-invalid'));
        });

        function toggleNextSection() {
            if (!nextSection || !callStatusSelect) {
                return;
            }

            const isInterested = (callStatusSelect.value || '').trim().toLowerCase() === 'interested';
            nextSection.classList.toggle('d-none', !isInterested);
        }

        function updateSalesButtonVisibility() {
            if (!salesButton || !conversionSelect) {
                return;
            }

            const isConverted = conversionSelect.value === 'true';
            salesButton.classList.toggle('d-none', !isConverted);
            salesHint?.classList.toggle('text-success', isConverted);
        }

        function clearSalesInputs() {
            requiredInputs.forEach(input => {
                input.value = '';
                input.classList.remove('is-invalid');
            });
        }

        function hideSalesSummary() {
            if (!salesSummary) {
                return;
            }

            salesSummary.classList.add('d-none');
            Object.values(summaryFields).forEach(field => {
                if (field) {
                    field.textContent = '-';
                }
            });

            if (salesButtonText) {
                salesButtonText.textContent = 'Add Sold Product Details';
            }
        }

        function openSalesModal() {
            modal?.show();
        }

        function closeSalesModal() {
            modal?.hide();
        }

        function validateSalesFields() {
            let isValid = true;
            requiredInputs.forEach(input => {
                const hasValue = !!(input.value && input.value.trim());
                input.classList.toggle('is-invalid', !hasValue);
                isValid = hasValue && isValid;
            });

            return isValid;
        }

        function getFieldValue(selector) {
            const element = form.querySelector(selector);
            return element?.value?.trim() ?? '';
        }

        function formatTicketSize(value) {
            if (!value) {
                return '-';
            }

            const numericValue = Number(value);
            if (Number.isNaN(numericValue)) {
                return value;
            }

            return numericValue.toLocaleString(undefined, {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            });
        }

        function hasSalesDetails() {
            return ['#SoldProductName', '#TicketSize', '#PolicyNumber', '#PolicyEnforceDate']
                .some(selector => !!getFieldValue(selector));
        }

        function updateSalesSummary() {
            if (!salesSummary) {
                return;
            }

            const product = getFieldValue('#SoldProductName');
            const ticket = getFieldValue('#TicketSize');
            const policyNumber = getFieldValue('#PolicyNumber');
            const policyDate = getFieldValue('#PolicyEnforceDate');

            const hasDetails = [product, ticket, policyNumber, policyDate].some(value => value);

            if (!hasDetails) {
                hideSalesSummary();
                return;
            }

            if (summaryFields.product) summaryFields.product.textContent = product || '-';
            if (summaryFields.ticket) summaryFields.ticket.textContent = ticket ? formatTicketSize(ticket) : '-';
            if (summaryFields.policyNumber) summaryFields.policyNumber.textContent = policyNumber || '-';
            if (summaryFields.policyDate) summaryFields.policyDate.textContent = policyDate || '-';

            salesSummary.classList.remove('d-none');

            if (salesButtonText) {
                salesButtonText.textContent = 'Edit Sold Product Details';
            }
        }

        function ensureModalIfNeeded() {
            if (!conversionSelect) {
                return;
            }

            const shouldOpen = conversionSelect.value === 'true' && !hasSalesDetails();
            if (shouldOpen) {
                openSalesModal();
            }
        }

        callStatusSelect?.addEventListener('change', toggleNextSection);

        conversionSelect?.addEventListener('change', () => {
            if (!conversionSelect) {
                return;
            }

            const currentValue = conversionSelect.value;
            const isConverted = currentValue === 'true';
            updateSalesButtonVisibility();

            if (isConverted) {
                openSalesModal();
            }
            else if (lastCommittedConversionValue === 'true') {
                clearSalesInputs();
                hideSalesSummary();
                lastCommittedConversionValue = currentValue;
            }
            else {
                lastCommittedConversionValue = currentValue;
            }
        });

        salesButton?.addEventListener('click', openSalesModal);

        cancelButton?.addEventListener('click', () => {
            if (lastCommittedConversionValue !== 'true' && conversionSelect) {
                conversionSelect.value = lastCommittedConversionValue;
                conversionSelect.dispatchEvent(new Event('change'));
            }

            closeSalesModal();
        });

        saveButton?.addEventListener('click', () => {
            if (!validateSalesFields()) {
                return;
            }

            lastCommittedConversionValue = 'true';
            updateSalesSummary();
            closeSalesModal();
        });

        modalElement?.addEventListener('hidden.bs.modal', () => {
            requiredInputs.forEach(input => input.classList.remove('is-invalid'));
        });

        toggleNextSection();
        updateSalesButtonVisibility();
        updateSalesSummary();
        ensureModalIfNeeded();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', ready);
    } else {
        ready();
    }
})();
