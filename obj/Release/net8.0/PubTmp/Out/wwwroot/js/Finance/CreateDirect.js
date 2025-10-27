(function () {
    // --- Global Data Hook ---
    let initialData = {};

    // --- Helper Functions ---

    /**
     * Toggles the visibility and required status of the Cost Allocation section.
     * @param {number} id The currently selected Payment Nature ID.
     */
    function toggleCostAllocationSection(id) {
        const costAllocationCard = document.getElementById('costAllocationCard');
        const costAllocationContainer = document.getElementById('cost-allocation-container');

        if (!costAllocationCard || !costAllocationContainer) return;

        // Payment Nature '1' corresponds to 'Operational Payments' which should not have cost allocation.
        const shouldHide = (id === 1);

        costAllocationCard.style.display = shouldHide ? 'none' : 'block';

        // Toggle required and disabled attributes
        costAllocationContainer.querySelectorAll('input, select, textarea').forEach(input => {
            if (shouldHide) {
                input.disabled = true;
                input.removeAttribute('required');
            } else {
                input.disabled = false;
                // Re-add 'required' attribute to the select fields if they are visible
                if (input.tagName === 'SELECT' || (input.classList.contains('rate-field') && input.value === '')) {
                    input.setAttribute('required', 'required');
                }
            }
        });
    }

    /**
     * Toggles the state of the supplier and payee name fields based on the self applicant switch.
     */
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

        if (isSelfPayee) {
            if (supplierField) {
                supplierField.style.display = 'none';
                if (supplierSelect) {
                    supplierSelect.value = '';
                    supplierSelect.removeAttribute('required');
                    supplierSelect.setAttribute('disabled', 'disabled');
                }
            }
            if (payeeNameInput) {
                payeeNameInput.value = currentUserName;
                payeeNameInput.readOnly = true;
                payeeNameInput.style.backgroundColor = '#f8f9fa';
            }
            if (departmentSelect) departmentSelect.setAttribute('disabled', 'disabled');
            if (branchSelect) branchSelect.setAttribute('disabled', 'disabled');

        } else {
            if (supplierField) {
                supplierField.style.display = 'block';
                if (supplierSelect) {
                    supplierSelect.setAttribute('required', 'required');
                    supplierSelect.removeAttribute('disabled');
                }
            }
            if (payeeNameInput) {
                if (payeeNameInput.value === currentUserName) {
                    payeeNameInput.value = '';
                }
                payeeNameInput.readOnly = false;
                payeeNameInput.style.backgroundColor = '';
            }
            if (departmentSelect) departmentSelect.removeAttribute('disabled');
            if (branchSelect) branchSelect.removeAttribute('disabled');
        }
    }


    // --- Other Helper Functions ---
    /**
     * Formats a number to a currency string with two decimal places.
     * @param {number} value The number to format.
     * @returns {string} The formatted currency string.
     */
    function formatCurrency(value) {
        const val = parseFloat(value);
        return isNaN(val) ? '0.00' : val.toFixed(2);
    }

    /**
     * Calculates the total for a single detail row.
     * @param {HTMLElement} row The detail row element.
     * @returns {number} The calculated total amount.
     */
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

    /**
     * Recalculates and updates the overall totals for the entire form, including the allocation rate.
     */
    function updateOverallTotals() {
        let totalAmountExTax = 0;
        let totalOtherTax = 0;
        let grandTotal = 0;
        let totalRate = 0;

        document.querySelectorAll('.detail-row').forEach(row => {
            const amount = parseFloat(row.querySelector('.amount-field')?.value) || 0;
            const taxRate = parseFloat(row.querySelector('.tax-field')?.value) || 0;
            const otherTax = parseFloat(row.querySelector('.other-tax-field')?.value) || 0;
            const stTax = (amount * taxRate / 100);
            const lineTotal = amount + stTax + otherTax;

            totalAmountExTax += amount;
            totalOtherTax += otherTax;
            grandTotal += lineTotal;

            const lineTotalDisplay = row.querySelector('.line-total-display');
            if (lineTotalDisplay) {
                lineTotalDisplay.value = formatCurrency(lineTotal);
            }
        });

        document.querySelectorAll('.allocation-row .rate-field').forEach(field => {
            totalRate += parseFloat(field.value) || 0;
        });

        if (document.getElementById('totalAmountExTaxDisplay')) document.getElementById('totalAmountExTaxDisplay').value = formatCurrency(totalAmountExTax);
        if (document.getElementById('totalOtherTaxDisplay')) document.getElementById('totalOtherTaxDisplay').value = formatCurrency(totalOtherTax);
        if (document.getElementById('grandTotalDisplay')) document.getElementById('grandTotalDisplay').value = formatCurrency(grandTotal);

        if (document.getElementById('totalRateDisplay')) {
            const totalRateDisplay = document.getElementById('totalRateDisplay');
            totalRateDisplay.value = totalRate.toFixed(2);
            const isRateValid = Math.abs(totalRate - 100) < 0.01;
            totalRateDisplay.style.backgroundColor = isRateValid ? '#d4edda' : '#f8d7da';
            totalRateDisplay.style.color = isRateValid ? '#155724' : '#721c24';
        }
    }

    /**
     * Sets up event listeners for a single detail row.
     * @param {HTMLElement} lineRow The detail row element.
     */
    function setupLineEvents(lineRow) {
        const fields = lineRow.querySelectorAll('.amount-field, .tax-field, .other-tax-field');
        fields.forEach(field => {
            field.addEventListener('input', () => {
                calculateLineTotal(lineRow);
                updateOverallTotals();
            });
        });
    }

    /**
     * Sets up event listeners for a single cost allocation row.
     * @param {HTMLElement} allocationRow The cost allocation row element.
     */
    function setupAllocationEvents(allocationRow) {
        const rateField = allocationRow.querySelector('.rate-field');
        if (rateField) {
            rateField.addEventListener('input', updateOverallTotals);
        }
    }

    /**
     * Re-indexes the detail rows and updates the overall totals.
     */
    function reindexDetails() {
        document.querySelectorAll('.detail-row').forEach((row, index) => {
            row.querySelectorAll('[name^="Details["]').forEach(input => {
                const oldName = input.getAttribute('name');
                const newName = oldName.replace(/Details\[\d+\]/, `Details[${index}]`);
                input.setAttribute('name', newName);
            });
        });
        updateOverallTotals();
    }

    /**
     * Re-indexes the cost allocation rows and updates the overall totals.
     */
    function reindexAllocations() {
        document.querySelectorAll('.allocation-row').forEach((row, index) => {
            row.querySelectorAll('[name^="CostAllocations["]').forEach(input => {
                const oldName = input.getAttribute('name');
                const newName = oldName.replace(/CostAllocations\[\d+\]/, `CostAllocations[${index}]`);
                input.setAttribute('name', newName);
            });
        });
        updateOverallTotals();
    }

    /**
     * Adds a new detail row to the form.
     * @param {object} detail The detail object to pre-populate the row.
     */
    function addDetailRow(detail = {}) {
        const detailsContainer = document.getElementById('details-container');
        if (!detailsContainer) return;

        const newIndex = detailsContainer.querySelectorAll('.detail-row').length; // Ensure this counts the rows correctly
        const today = new Date();
        const sixMonthsAgo = new Date();
        sixMonthsAgo.setMonth(today.getMonth() - 6);

        const todayFormatted = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`;
        const sixMonthsAgoFormatted = `${sixMonthsAgo.getFullYear()}-${String(sixMonthsAgo.getMonth() + 1).padStart(2, '0')}-${String(sixMonthsAgo.getDate()).padStart(2, '0')}`;

        // ⭐ FIX: Generate the compact Bootstrap grid row structure
        const detailHtml = `
        <div class="row detail-row border-bottom py-2 align-items-center">
            <input type="hidden" name="Details[${newIndex}].Id" value="${detail.id || 0}" />
            
            <div class="col-md-2">
                <input name="Details[${newIndex}].InvoiceNo" class="form-control form-control-sm" placeholder="Invoice No" required maxlength="20" value="${detail.invoiceNo || ''}" />
            </div>
            
            <div class="col-md-2">
                <input name="Details[${newIndex}].InvoiceDate" type="date" class="form-control form-control-sm invoice-date-field" 
                       required min="${sixMonthsAgoFormatted}" max="${todayFormatted}" value="${detail.invoiceDate ? new Date(detail.invoiceDate).toISOString().slice(0, 10) : ''}" />
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
        // Ensure setupLineEvents is adapted to find fields within this new <div> structure
        setupLineEvents(newRow);
        calculateLineTotal(newRow);
    }
    /**
     * Adds a new cost allocation row to the form.
     */
    function addCostAllocationRow(allocation = {}) {
        const costAllocationContainer = document.getElementById('cost-allocation-container');
        if (!costAllocationContainer) return;

        const newIndex = costAllocationContainer.querySelectorAll('.allocation-row').length;
        const departmentOptions = (initialData.departments || []).map(d => `<option value="${d.value}" ${d.value === allocation.departmentCode ? 'selected' : ''}>${d.text}</option>`).join('');
        const branchOptions = (initialData.branches || []).map(b => `<option value="${b.value}" ${b.value === allocation.branchCode ? 'selected' : ''}>${b.text}</option>`).join('');

        const allocationHtml = `
            <div class="card card-body mb-2 allocation-row">
                <div class="row">
                    <div class="col-md-4 mb-3">
                        <label class="form-label">Department</label>
                        <div class="input-group">
                            <span class="input-group-text custom-blue"><i class="fas fa-building"></i></span>
                            <select name="CostAllocations[${newIndex}].DepartmentCode" class="form-select department-select" required>
                                <option value="">Select Department</option>
                                ${departmentOptions}
                            </select>
                        </div>
                    </div>
                    <div class="col-md-4 mb-3">
                        <label class="form-label">Branch</label>
                        <div class="input-group">
                            <span class="input-group-text custom-blue"><i class="fas fa-map-marker-alt"></i></span>
                            <select name="CostAllocations[${newIndex}].BranchCode" class="form-select branch-select" required>
                                <option value="">Select Branch</option>
                                ${branchOptions}
                            </select>
                        </div>
                    </div>
                    <div class="col-md-2 mb-3">
                        <label class="form-label">Rate (%)</label>
                        <div class="input-group">
                            <span class="input-group-text custom-blue"><i class="fas fa-percent"></i></span>
                            <input name="CostAllocations[${newIndex}].Rate" type="number" step="0.01" min="0" max="100" class="form-control rate-field" value="${allocation.rate || '0'}" required />
                        </div>
                    </div>
                    <div class="col-md-2 mb-3 d-flex align-items-end">
                        <button type="button" class="btn btn-danger remove-allocation-btn w-100">
                            <i class="fas fa-trash me-1"></i> Remove
                        </button>
                    </div>
                </div>
            </div>
        `;
        costAllocationContainer.insertAdjacentHTML('beforeend', allocationHtml);
        const newRow = costAllocationContainer.lastElementChild;
        setupAllocationEvents(newRow);
    }
    // --- End Helper Functions ---


    /**
     * Initializes the form with data and sets up event listeners.
     * @param {object} data The initial data to pre-populate the form.
     */
    window.initializePaymentRequestScript = function (data) {
        console.log('Initializing Create Direct Payment Request', data);

        // Populate global initialData variable
        initialData = data || {};
        initialData.details = initialData.details || [];
        initialData.costAllocations = initialData.costAllocations || [];
        initialData.departments = initialData.departments || [];
        initialData.branches = initialData.branches || [];

        // --- DOM Elements ---
        const selfApplicantSwitch = document.getElementById('selfApplicantSwitch');
        const paymentNatureSelect = document.querySelector('select[name="PaymentNatureId"]');
        const addDetailBtn = document.getElementById('addDetailBtn');
        const addCostAllocationBtn = document.getElementById('addCostAllocationBtn');
        const detailsContainer = document.getElementById('details-container');
        const costAllocationContainer = document.getElementById('cost-allocation-container');


        // --- Initialization Logic ---

        // Pre-populate details and cost allocations based on initialData
        initialData.details.forEach(addDetailRow);
        initialData.costAllocations.forEach(addCostAllocationRow);

        // If no details or allocations are provided, add one empty row
        if (initialData.details.length === 0) {
            addDetailRow();
        }
        if (initialData.costAllocations.length === 0) {
            addCostAllocationRow();
        }

        // Set initial state for self-applicant and cost allocation visibility
        if (selfApplicantSwitch) {
            toggleSelfApplicantFields();
            selfApplicantSwitch.addEventListener('change', toggleSelfApplicantFields);
        }

        if (paymentNatureSelect) {
            toggleCostAllocationSection(parseInt(paymentNatureSelect.value, 10));
            paymentNatureSelect.addEventListener('change', function () {
                toggleCostAllocationSection(parseInt(this.value, 10));
                // Recalculate totals as some fields might become disabled
                updateOverallTotals();
            });
        }

        // Set up event listeners for existing and new rows
        document.querySelectorAll('.detail-row').forEach(setupLineEvents);
        document.querySelectorAll('.allocation-row').forEach(setupAllocationEvents);


        // --- Event Listeners Setup ---
        if (addDetailBtn) {
            addDetailBtn.addEventListener('click', () => {
                addDetailRow();
                reindexDetails();
            });
        }
        if (addCostAllocationBtn) {
            addCostAllocationBtn.addEventListener('click', () => {
                addCostAllocationRow();
                reindexAllocations();
            });
        }

        // Delegated event listeners for remove buttons
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
        if (costAllocationContainer) {
            costAllocationContainer.addEventListener('click', function (e) {
                if (e.target.closest('.remove-allocation-btn')) {
                    e.preventDefault();
                    const allocationRows = costAllocationContainer.querySelectorAll('.allocation-row');
                    if (allocationRows.length > 1) {
                        e.target.closest('.allocation-row').remove();
                        reindexAllocations();
                    } else {
                        alert('At least one cost allocation line is required.');
                    }
                }
            });
        }

        // Final initial calculation after everything is set up
        updateOverallTotals();

        console.log('Create Direct Payment Request initialized successfully');
    };

    // Auto-initialize if initialData is available globally
    if (typeof window.directInitialData !== 'undefined') {
        document.addEventListener('DOMContentLoaded', function () {
            window.initializePaymentRequestScript(window.directInitialData);
        });
    }
})();