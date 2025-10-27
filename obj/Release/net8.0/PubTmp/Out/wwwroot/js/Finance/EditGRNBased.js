(function () {
    window.initializeEditGRNBasedPaymentRequest = function (initialData) {
        console.log('Initializing Edit GRN Based Payment Request', initialData);

        // Ensure initialData has a default structure to prevent errors
        initialData = initialData || {};
        initialData.details = initialData.details || [];
        initialData.departments = initialData.departments || [];
        initialData.branches = initialData.branches || [];

        // --- DOM Elements (safely retrieved with error handling) ---
        const form = document.getElementById('editGRNBasedForm');
        if (!form) {
            console.error('Form editGRNBasedForm not found');
            return;
        }

        const selfApplicantSwitch = document.getElementById('selfApplicantSwitch');
        const supplierField = document.getElementById('supplier-field');
        const payeeNameInput = document.getElementById('PayeeName');
        const supplierSelect = supplierField ? supplierField.querySelector('select[name="SupplierId"]') : null;
        const detailsContainer = document.getElementById('details-container');
        const addDetailBtn = document.getElementById('addDetailBtn');
        const totalAmountExTaxDisplay = document.getElementById('totalAmountExTaxDisplay');
        const totalOtherTaxDisplay = document.getElementById('totalOtherTaxDisplay');
        const grandTotalDisplay = document.getElementById('grandTotalDisplay');
        const grnTotalAmount = initialData.grnTotalAmount || 0;
        const submitBtn = document.getElementById('submitBtn');

        // Check for required elements
        if (!detailsContainer) {
            console.error('Details container not found');
            return;
        }

        // --- Pop-up message element ---
        const errorPopup = document.getElementById('error-popup-message');

        // --- Helper functions for calculations and UI updates ---
        function showErrorMessage(message) {
            if (errorPopup) {
                errorPopup.textContent = message;
                errorPopup.style.display = 'block';
            } else {
                alert(message);
            }
        }

        function hideErrorMessage() {
            if (errorPopup) {
                errorPopup.style.display = 'none';
                errorPopup.textContent = '';
            }
        }

        function formatCurrency(value) {
            const num = parseFloat(value);
            return (isNaN(num) || num === null) ? '0.00' : num.toFixed(2);
        }

        function calculateLineTotal(row) {
            const amountField = row.querySelector('.amount-field');
            const taxField = row.querySelector('.tax-field');
            const otherTaxField = row.querySelector('.other-tax-field');
            const lineTotalDisplay = row.querySelector('.line-total-display');
            const lineTotalHidden = row.querySelector('.line-total-hidden');

            if (!amountField || !lineTotalDisplay || !lineTotalHidden) return 0;

            const amount = parseFloat(amountField.value) || 0;
            const taxRate = parseFloat(taxField ? taxField.value : 0) || 0;
            const otherTax = parseFloat(otherTaxField ? otherTaxField.value : 0) || 0;
            const total = amount + (amount * taxRate / 100) + otherTax;

            lineTotalDisplay.value = formatCurrency(total);
            lineTotalHidden.value = total.toFixed(2); // Set the value for form submission
            return total;
        }

        function updateOverallTotals() {
            let totalAmountExTax = 0;
            let totalOtherTax = 0;
            let grandTotal = 0;

            document.querySelectorAll('.detail-row').forEach(row => {
                totalAmountExTax += parseFloat(row.querySelector('.amount-field')?.value) || 0;
                totalOtherTax += parseFloat(row.querySelector('.other-tax-field')?.value) || 0;
                grandTotal += parseFloat(row.querySelector('.line-total-hidden')?.value) || 0;
            });

            if (totalAmountExTaxDisplay) totalAmountExTaxDisplay.value = formatCurrency(totalAmountExTax);
            if (totalOtherTaxDisplay) totalOtherTaxDisplay.value = formatCurrency(totalOtherTax);
            if (grandTotalDisplay) grandTotalDisplay.value = formatCurrency(grandTotal);

            // GRN Total Amount validation
            if (grnTotalAmount > 0 && totalAmountExTax > grnTotalAmount) {
                const message = `The total Amount Ex Tax (${totalAmountExTax.toFixed(2)}) cannot exceed the GRN total amount (${grnTotalAmount.toFixed(2)}).`;
                showErrorMessage(message);
                if (submitBtn) {
                    submitBtn.disabled = true;
                }
                totalAmountExTaxDisplay.style.backgroundColor = '#f8d7da';
                totalAmountExTaxDisplay.style.color = '#721c24';
            } else {
                hideErrorMessage();
                if (submitBtn) {
                    submitBtn.disabled = false;
                }
                totalAmountExTaxDisplay.style.backgroundColor = '';
                totalAmountExTaxDisplay.style.color = '';
            }
        }

        // The validateInvoiceDate function and its related event listener have been removed.
        // This means no date validation will be performed.

        function setupLineEvents(lineRow) {
            if (!lineRow) return;

            const calcFields = lineRow.querySelectorAll('.amount-field, .tax-field, .other-tax-field');
            calcFields.forEach(field => {
                const handleCalculation = () => {
                    calculateLineTotal(lineRow);
                    updateOverallTotals();
                };
                field.removeEventListener('input', handleCalculation);
                field.addEventListener('input', handleCalculation);
            });

            // The invoice date event listener has been removed from here.

            calculateLineTotal(lineRow);
        }

        // The handleDateValidation function has been removed.

        function reindexDetails() {
            document.querySelectorAll('.detail-row').forEach((row, index) => {
                row.querySelectorAll('input, select, textarea').forEach(input => {
                    if (input.name) {
                        input.name = input.name.replace(/Details\[\d+\]/g, `Details[${index}]`);
                    }
                });
            });
            updateOverallTotals();
        }

        function handleSelfPayeeToggle() {
            if (!selfApplicantSwitch) return;

            const isSelfPayee = selfApplicantSwitch.checked;
            const supplierField = document.getElementById('supplier-field');
            const payeeNameInput = document.getElementById('PayeeName');
            const supplierSelect = supplierField ? supplierField.querySelector('select[name="SupplierId"]') : null;

            // CRITICAL: Get the full name passed from the controller's ViewModel property (CurrentUserName)
            const currentUserName = initialData.currentUserName;

            // Check the existing value in the input field BEFORE changing it.
            // This handles the case where the form loads with the correct server-side value.
            const initialPayeeValue = payeeNameInput.value;

            if (isSelfPayee) {
                if (supplierField) {
                    supplierField.style.display = 'none';
                    if (supplierSelect) {
                        supplierSelect.value = '';
                        supplierSelect.removeAttribute('required');
                    }
                }
                if (payeeNameInput) {
                    // Set Payee Name to current user's full name, and make it read-only
                    payeeNameInput.value = currentUserName || '';
                    payeeNameInput.readOnly = true;
                    payeeNameInput.style.backgroundColor = '#f8f9fa';
                }
            } else {
                if (supplierField) {
                    supplierField.style.display = 'block';
                    if (supplierSelect) {
                        supplierSelect.setAttribute('required', 'required');
                    }
                }
                if (payeeNameInput) {
                    // Only clear the payee name if it currently holds the self-applicant's name.
                    // If the user name is currently displayed, we assume it was set by the toggle and clear it.
                    if (payeeNameInput.value === currentUserName) {
                        payeeNameInput.value = '';
                    }
                    payeeNameInput.readOnly = false;
                    payeeNameInput.style.backgroundColor = '';
                }
            }
        }


        function addDetailRow(detail = {}) {
            if (!detailsContainer) return;

            const newIndex = document.querySelectorAll('.detail-row').length;

            // Calculate the date range
            const today = new Date();
            const sixMonthsAgo = new Date();
            sixMonthsAgo.setMonth(today.getMonth() - 6);

            // Format dates to YYYY-MM-DD for the input attributes
            const todayFormatted = today.toISOString().slice(0, 10);
            const sixMonthsAgoFormatted = sixMonthsAgo.toISOString().slice(0, 10);

            const detailHtml = `
        <div class="card card-body mb-2 detail-row">
            <input type="hidden" name="Details[${newIndex}].Id" value="${detail.id || 0}" />
            <div class="row">
                <div class="col-md-3 mb-3">
                    <label class="form-label">Invoice Number <span class="text-danger">*</span></label>
                    <div class="input-group">
                        <span class="input-group-text custom-blue"><i class="fas fa-file-invoice"></i></span>
                        <input name="Details[${newIndex}].InvoiceNo" class="form-control" value="${detail.invoiceNo || ''}" required />
                    </div>
                </div>
                <div class="col-md-3 mb-3">
                    <label class="form-label">Invoice Date <span class="text-danger">*</span></label>
                    <div class="input-group">
                        <span class="input-group-text custom-blue"><i class="fas fa-calendar-alt"></i></span>
                        <input 
                            name="Details[${newIndex}].InvoiceDate" 
                            type="date" 
                            class="form-control invoice-date-field" 
                            value="${detail.invoiceDate ? new Date(detail.invoiceDate).toISOString().slice(0, 10) : ''}" 
                            required 
                            min="${sixMonthsAgoFormatted}" 
                            max="${todayFormatted}" />
                    </div>
                </div>
                <div class="col-4 mb-3">
                    <label class="form-label">Description <span class="text-danger">*</span></label>
                    <div class="input-group">
                        <span class="input-group-text custom-blue"><i class="fas fa-comment-dots"></i></span>
                        <textarea name="Details[${newIndex}].Description" class="form-control" rows="1" required>${detail.description || ''}</textarea>
                    </div>
                </div>
                <div class="col-md-2 mb-3 d-flex align-items-end">
                    <button type="button" class="btn btn-danger remove-detail-btn w-100">
                        <i class="fas fa-trash me-1"></i> Remove
                    </button>
                </div>
            </div>
            <div class="row">
                <div class="col-md-3 mb-3">
                    <label class="form-label">Amount Ex Tax <span class="text-danger">*</span></label>
                    <div class="input-group">
                        <span class="input-group-text custom-blue"><i class="fas fa-dollar-sign"></i></span>
                        <input name="Details[${newIndex}].AmountExTax" type="number" step="0.01" class="form-control amount-field" value="${detail.amountExTax || ''}" required />
                    </div>
                </div>
                <div class="col-md-3 mb-3">
                    <label class="form-label">Sales Tax Rate (%)</label>
                    <div class="input-group">
                        <span class="input-group-text custom-blue"><i class="fas fa-percent"></i></span>
                        <input name="Details[${newIndex}].STRate" type="number" step="0.01" class="form-control tax-field" value="${detail.stRate || ''}" />
                    </div>
                </div>
                <div class="col-md-3 mb-3">
                    <label class="form-label">Other Tax</label>
                    <div class="input-group">
                        <span class="input-group-text custom-blue"><i class="fas fa-dollar-sign"></i></span>
                        <input name="Details[${newIndex}].OtherTax" type="number" step="0.01" class="form-control other-tax-field" value="${detail.otherTax || ''}" />
                    </div>
                </div>
                <div class="col-md-3 mb-3">
                    <label class="form-label">Total Amount</label>
                    <div class="input-group">
                        <span class="input-group-text custom-blue"><i class="fas fa-dollar-sign"></i></span>
                        <input type="text" class="form-control line-total-display" value="${formatCurrency(detail.totalAmount)}" readonly />
                        <input type="hidden" name="Details[${newIndex}].TotalAmount" class="line-total-hidden" value="${detail.totalAmount || 0}" />
                    </div>
                </div>
            </div>
        </div>
    `;
            detailsContainer.insertAdjacentHTML('beforeend', detailHtml);
            const newRow = detailsContainer.lastElementChild;
            setupLineEvents(newRow);
        }

        // --- Event listeners setup ---
        document.querySelectorAll('.detail-row').forEach(row => {
            setupLineEvents(row);
        });

        if (selfApplicantSwitch) {
            handleSelfPayeeToggle();
            selfApplicantSwitch.addEventListener('change', handleSelfPayeeToggle);
        }

        if (supplierSelect && payeeNameInput) {
            supplierSelect.addEventListener('change', function () {
                if (!selfApplicantSwitch || !selfApplicantSwitch.checked) {
                    const selectedOption = this.options[this.selectedIndex];
                    payeeNameInput.value = (selectedOption && selectedOption.text && selectedOption.value) ? selectedOption.text : '';
                }
            });
        }

        if (addDetailBtn) {
            addDetailBtn.addEventListener('click', (e) => {
                e.preventDefault();
                addDetailRow();
                reindexDetails();
            });
        }

        if (detailsContainer) {
            detailsContainer.addEventListener('click', function (e) {
                if (e.target.closest('.remove-detail-btn')) {
                    e.preventDefault();
                    const detailRows = detailsContainer.querySelectorAll('.detail-row');
                    if (detailRows.length > 1) {
                        e.target.closest('.detail-row').remove();
                        reindexDetails();
                    } else {
                        alert('At least one payment detail line is required.');
                    }
                }
            });
        }

        // Initial setup for the entire form
        setTimeout(updateOverallTotals, 100);
        console.log('Edit GRN Based Payment Request initialized successfully');
    };

    if (typeof window.editGRNInitialData !== 'undefined') {
        document.addEventListener('DOMContentLoaded', function () {
            window.initializeEditGRNBasedPaymentRequest(window.editGRNInitialData);
        });
    }
})();