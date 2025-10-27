(function () {
    // --- Global Data Hook ---
    let initialData = {};

    // --- Helper Functions (Definitions) ---

    /** Toggles the visibility and required status of the Cost Allocation section. */
    function toggleCostAllocationSection(id) {
        const costAllocationCard = document.getElementById('costAllocationCard');
        const costAllocationContainer = document.getElementById('cost-allocation-container');
        if (!costAllocationCard) return;

        const shouldHide = (id === 1);

        if (shouldHide) {
            costAllocationCard.style.display = 'none';
            if (costAllocationContainer) {
                costAllocationContainer.querySelectorAll('input, select, textarea').forEach(input => {
                    input.disabled = true;
                    input.removeAttribute('required');
                });
            }
        } else {
            costAllocationCard.style.display = 'block';
            if (costAllocationContainer) {
                costAllocationContainer.querySelectorAll('input, select, textarea').forEach(input => {
                    input.disabled = false;
                    if (input.tagName === 'SELECT' || (input.classList.contains('rate-field') && input.value === '')) {
                        input.setAttribute('required', 'required');
                    }
                });
            }
        }
    }

    /** Toggles the state of the supplier and payee name fields based on the self applicant switch. */
    function toggleSelfApplicantFields() {
        const selfApplicantSwitch = document.getElementById('selfApplicantSwitch');
        const supplierField = document.getElementById('supplier-field');
        const payeeNameInput = document.getElementById('PayeeName');
        const supplierSelect = document.querySelector('select[name="SupplierId"]');
        const departmentSelect = document.querySelector('select[name="DepartmentCode"]');
        const branchSelect = document.querySelector('select[name="BranchCode"]');

        if (!selfApplicantSwitch) return;
        const isSelfPayee = selfApplicantSwitch.checked;
        const currentUserName = initialData.currentUserName || 'Not Available';

        // Logic for Self Payee (Disable Supplier fields, populate PayeeName)
        if (isSelfPayee) {
            if (supplierField) { supplierField.style.display = 'none'; }
            if (supplierSelect) {
                supplierSelect.value = '';
                supplierSelect.removeAttribute('required');
                supplierSelect.setAttribute('disabled', 'disabled');
            }
            if (payeeNameInput) {
                payeeNameInput.value = currentUserName;
                payeeNameInput.readOnly = true;
                payeeNameInput.style.backgroundColor = '#f8f9fa';
            }
            if (departmentSelect) departmentSelect.setAttribute('disabled', 'disabled');
            if (branchSelect) branchSelect.setAttribute('disabled', 'disabled');
        }
        // Logic for External Payee (Enable Supplier fields, clear PayeeName if it matches current user)
        else {
            if (supplierField) { supplierField.style.display = 'block'; }
            if (supplierSelect) {
                supplierSelect.setAttribute('required', 'required');
                supplierSelect.removeAttribute('disabled');
            }
            if (payeeNameInput) {
                if (payeeNameInput.value === currentUserName) { payeeNameInput.value = ''; }
                payeeNameInput.readOnly = false;
                payeeNameInput.style.backgroundColor = '';
            }
            if (departmentSelect) departmentSelect.removeAttribute('disabled');
            if (branchSelect) branchSelect.removeAttribute('disabled');
        }
    }

    /** Formats a number as a currency string with two decimal places. */
    function formatCurrency(value) {
        if (value === null || value === undefined || isNaN(value)) return '0.00';
        return parseFloat(value).toFixed(2);
    }

    /** Calculates the total for a single detail row. */
    function calculateLineTotal(row) {
        const amountField = row.querySelector('.amount-field');
        const taxField = row.querySelector('.tax-field');
        const otherTaxField = row.querySelector('.other-tax-field');
        const lineTotalDisplay = row.querySelector('.line-total-display');

        if (!amountField || !lineTotalDisplay) return 0;

        const amount = parseFloat(amountField.value) || 0;
        const taxRate = parseFloat(taxField ? taxField.value : 0) || 0;
        const otherTax = parseFloat(otherTaxField ? otherTaxField.value : 0) || 0;

        const total = amount + (amount * taxRate / 100) + otherTax;

        lineTotalDisplay.value = formatCurrency(total);
        return total;
    }

    /** Recalculates and updates the overall totals for the entire form, including GRN validation. */
    function updateOverallTotals() {
        let totalAmountExTax = 0;
        let totalOtherTax = 0;
        let grandTotal = 0;

        const totalAmountExTaxDisplay = document.getElementById('totalAmountExTaxDisplay');
        const totalOtherTaxDisplay = document.getElementById('totalOtherTaxDisplay');
        const grandTotalDisplay = document.getElementById('grandTotalDisplay');
        const submitBtn = document.getElementById('submitBtn');
        const grnTotalAmount = initialData.grnTotalAmount || 0;

        document.querySelectorAll('.detail-row').forEach(row => {
            const amount = parseFloat(row.querySelector('.amount-field')?.value) || 0;
            const otherTax = parseFloat(row.querySelector('.other-tax-field')?.value) || 0;
            const stRate = parseFloat(row.querySelector('.tax-field')?.value) || 0;
            const stTax = (amount * stRate / 100);

            const lineTotal = amount + stTax + otherTax;

            totalAmountExTax += amount;
            totalOtherTax += otherTax;
            grandTotal += lineTotal;

            const lineTotalDisplay = row.querySelector('.line-total-display');
            if (lineTotalDisplay) {
                lineTotalDisplay.value = formatCurrency(lineTotal);
            }
        });

        if (totalAmountExTaxDisplay) totalAmountExTaxDisplay.value = formatCurrency(totalAmountExTax);
        if (totalOtherTaxDisplay) totalOtherTaxDisplay.value = formatCurrency(totalOtherTax);
        if (grandTotalDisplay) grandTotalDisplay.value = formatCurrency(grandTotal);

        // GRN Total Amount validation
        if (grnTotalAmount > 0 && totalAmountExTax > grnTotalAmount) {
            alert(`The total Amount Ex Tax (${totalAmountExTax.toFixed(2)}) cannot exceed the GRN total amount (${grnTotalAmount.toFixed(2)}).`);
            if (submitBtn) { submitBtn.disabled = true; }
            if (totalAmountExTaxDisplay) {
                totalAmountExTaxDisplay.style.backgroundColor = '#f8d7da';
                totalAmountExTaxDisplay.style.color = '#721c24';
            }
        } else {
            if (submitBtn) { submitBtn.disabled = false; }
            if (totalAmountExTaxDisplay) {
                totalAmountExTaxDisplay.style.backgroundColor = '';
                totalAmountExTaxDisplay.style.color = '';
            }
        }
    }

    /** Sets up event listeners for a new detail row. */
    function setupLineEvents(lineRow) {
        if (!lineRow) return;
        const calcFields = lineRow.querySelectorAll('.amount-field, .tax-field, .other-tax-field');
        calcFields.forEach(field => {
            // Note: Using updateOverallTotals here correctly triggers calculateLineTotal inside
            field.addEventListener('input', updateOverallTotals);
        });
        calculateLineTotal(lineRow); // Ensure line total is calculated on setup
        updateOverallTotals(); // Ensure grand totals are updated on setup
    }

    /** Re-indexes input names for detail rows. */
    function reindexDetails() {
        const detailsContainer = document.getElementById('details-container');
        document.querySelectorAll('.detail-row').forEach((row, index) => {
            row.querySelectorAll('input, select, textarea').forEach(input => {
                const name = input.name;
                if (name) {
                    input.name = name.replace(/Details\[\d+\]/g, `Details[${index}]`);
                }
            });
        });
        updateOverallTotals();
    }

    /** Adds a new detail row to the form. */
    function addDetailRow(detail = {}) {
        const detailsContainer = document.getElementById('details-container');
        if (!detailsContainer) return;

        const newIndex = detailsContainer.querySelectorAll('.detail-row').length;
        const today = new Date();
        const sixMonthsAgo = new Date();
        sixMonthsAgo.setMonth(today.getMonth() - 6);

        // Format dates to YYYY-MM-DD for the input attributes
        const todayFormatted = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`;
        const sixMonthsAgoFormatted = `${sixMonthsAgo.getFullYear()}-${String(sixMonthsAgo.getMonth() + 1).padStart(2, '0')}-${String(sixMonthsAgo.getDate()).padStart(2, '0')}`;

        // Generating the new compact Bootstrap grid row (div.row)
        const detailHtml = `
        <div class="row detail-row border-bottom py-2 align-items-center">
            <input type="hidden" name="Details[${newIndex}].Id" value="${detail.id || 0}" />
            
            <div class="col-md-2">
                <input name="Details[${newIndex}].InvoiceNo" 
                       class="form-control form-control-sm" 
                       placeholder="Invoice No" 
                       required 
                       maxlength="20" 
                       value="${detail.invoiceNo || ''}" />
            </div>

            <div class="col-md-2">
                <input name="Details[${newIndex}].InvoiceDate" 
                       type="date" 
                       class="form-control form-control-sm invoice-date-field"
                       required 
                       min="${sixMonthsAgoFormatted}" 
                       max="${todayFormatted}" 
                       value="${detail.invoiceDate ? new Date(detail.invoiceDate).toISOString().slice(0, 10) : ''}" />
            </div>

            <div class="col-md-3">
                <textarea name="Details[${newIndex}].Description" class="form-control form-control-sm" placeholder="Description" required rows="1">${detail.description || ''}</textarea>
            </div>

            <div class="col-md-1">
                <input name="Details[${newIndex}].AmountExTax" type="number" step="0.01" class="form-control form-control-sm amount-field text-end" placeholder="0.00" value="${detail.amountExTax || '0'}" required />
            </div>

            <div class="col-md-1">
                <input name="Details[${newIndex}].STRate" type="number" step="0.01" class="form-control form-control-sm tax-field text-end" placeholder="%" value="${detail.stRate || '0'}" />
            </div>

            <div class="col-md-1">
                <input name="Details[${newIndex}].OtherTax" type="number" step="0.01" class="form-control form-control-sm other-tax-field text-end" placeholder="0.00" value="${detail.otherTax || '0'}" />
            </div>

            <div class="col-md-1">
                <input type="text" class="form-control form-control-sm line-total-display text-end" readonly />
                <input type="hidden" name="Details[${newIndex}].TotalAmount" class="line-total-hidden" />
            </div>
            <div class="col-md-1 text-center">
                <button type="button" class="btn btn-danger btn-sm remove-detail-btn" title="Remove Item">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        </div>
        `;

        detailsContainer.insertAdjacentHTML('beforeend', detailHtml);
        const newRow = detailsContainer.lastElementChild;
        setupLineEvents(newRow);
        calculateLineTotal(newRow);
    }

    // --- Main Initialization Function ---
    window.initializePaymentRequestScript = function (data) {
        console.log('Initializing Payment Request Script', data);
        const detailsContainer = document.getElementById('details-container');
        const addDetailBtn = document.getElementById('addDetailBtn');
        const selfApplicantSwitch = document.getElementById('selfApplicantSwitch');

        initialData = data || {};
        initialData.details = initialData.details || [];
        initialData.costAllocations = initialData.costAllocations || [];

        // Clear and Populate initial details
        detailsContainer.innerHTML = '';
        initialData.details.forEach(addDetailRow);

        if (initialData.details.length === 0) {
            addDetailRow();
        }

        // Attach event listeners for Add button and Self-Applicant switch
        if (addDetailBtn) {
            addDetailBtn.addEventListener('click', () => {
                addDetailRow();
                reindexDetails();
            });
        }

        if (selfApplicantSwitch) {
            toggleSelfApplicantFields();
            selfApplicantSwitch.addEventListener('change', toggleSelfApplicantFields);
        }

        // Set up events for initial/existing rows
        document.querySelectorAll('.detail-row').forEach(setupLineEvents);

        // ⭐ CRITICAL FIX: Event Delegation for Remove Button ⭐
        if (detailsContainer) {
            detailsContainer.addEventListener('click', function (e) {
                // Check if the clicked element or its closest ancestor is the remove button
                if (e.target.closest('.remove-detail-btn')) {
                    e.preventDefault();

                    const rowToRemove = e.target.closest('.detail-row');
                    const detailRows = detailsContainer.querySelectorAll('.detail-row');

                    // Enforce minimum of one detail row
                    if (detailRows.length > 1) {
                        rowToRemove.remove();
                        reindexDetails(); // Re-index names and recalculate totals
                    } else {
                        alert('At least one invoice detail line is required.');
                    }
                }
            });
        }

        // Final initial calculation
        setTimeout(updateOverallTotals, 100);

        console.log('Payment Request Script initialized successfully');
    };

    // Auto-initialize if initialData is available globally
    if (typeof window.grnInitialData !== 'undefined') {
        document.addEventListener('DOMContentLoaded', function () {
            window.initializePaymentRequestScript(window.grnInitialData);
        });
    }

})();