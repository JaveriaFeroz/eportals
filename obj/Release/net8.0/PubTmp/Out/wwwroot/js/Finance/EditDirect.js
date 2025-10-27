(function () {
    // --- Global Helper Functions (Accessible throughout the script) ---

    /**
     * Toggles the state of the supplier and payee name fields based on the self applicant switch.
     * @param {object} initialData The initial data for the form.
     */
    window.handleSelfPayeeToggle = function (initialData) {
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
                    supplierSelect.disabled = true;
                }
            }
            if (payeeNameInput) {
                payeeNameInput.value = currentUserName;
                payeeNameInput.readOnly = true;
                payeeNameInput.style.backgroundColor = '#f8f9fa';
            }
            if (departmentSelect) departmentSelect.disabled = true;
            if (branchSelect) branchSelect.disabled = true;
        } else {
            if (supplierField) {
                supplierField.style.display = 'block';
                if (supplierSelect) {
                    supplierSelect.setAttribute('required', 'required');
                    supplierSelect.disabled = false;
                }
            }
            if (payeeNameInput) {
                if (payeeNameInput.value === currentUserName) {
                    payeeNameInput.value = '';
                }
                payeeNameInput.readOnly = false;
                payeeNameInput.style.backgroundColor = '';
            }
            if (departmentSelect) departmentSelect.disabled = false;
            if (branchSelect) branchSelect.disabled = false;
        }
    };

    /**
     * Formats a number as a currency string with two decimal places.
     * @param {number|string} value The value to format.
     * @returns {string} The formatted currency string.
     */
    window.formatCurrency = function (value) {
        const num = parseFloat(value);
        return (isNaN(num) || num === null) ? '0.00' : num.toFixed(2);
    };

    /**
     * Calculates the total for a single detail row.
     * @param {HTMLElement} row The detail row element.
     */
    window.calculateLineTotal = function (row) {
        const amountField = row.querySelector('.amount-field');
        const taxField = row.querySelector('.tax-field');
        const otherTaxField = row.querySelector('.other-tax-field');
        const lineTotalDisplay = row.querySelector('.line-total-display');
        const lineTotalHidden = row.querySelector('.line-total-hidden');

        if (!amountField || !lineTotalDisplay) return;

        const amount = parseFloat(amountField.value) || 0;
        const taxRate = parseFloat(taxField?.value || 0) || 0;
        const otherTax = parseFloat(otherTaxField?.value || 0) || 0;
        const total = amount + (amount * taxRate / 100) + otherTax;

        lineTotalDisplay.value = window.formatCurrency(total);
        if (lineTotalHidden) {
            lineTotalHidden.value = total.toFixed(2);
        }
    };

    /**
     * Recalculates and updates the overall totals for the entire form.
     */
    window.updateOverallTotals = function () {
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
                lineTotalDisplay.value = window.formatCurrency(lineTotal);
            }
        });

        document.querySelectorAll('.allocation-row .rate-field').forEach(field => {
            totalRate += parseFloat(field.value) || 0;
        });

        if (document.getElementById('totalAmountExTaxDisplay')) document.getElementById('totalAmountExTaxDisplay').value = window.formatCurrency(totalAmountExTax);
        if (document.getElementById('totalOtherTaxDisplay')) document.getElementById('totalOtherTaxDisplay').value = window.formatCurrency(totalOtherTax);
        if (document.getElementById('grandTotalDisplay')) document.getElementById('grandTotalDisplay').value = window.formatCurrency(grandTotal);

        if (document.getElementById('totalRateDisplay')) {
            const totalRateDisplay = document.getElementById('totalRateDisplay');
            totalRateDisplay.value = totalRate.toFixed(2);
            const isRateValid = Math.abs(totalRate - 100) < 0.01;
            totalRateDisplay.style.backgroundColor = isRateValid ? '#d4edda' : '#f8d7da';
            totalRateDisplay.style.color = isRateValid ? '#155724' : '#721c24';
        }
    };

    /**
     * Sets up event listeners for a new detail row.
     * @param {HTMLElement} lineRow The detail row element.
     */
    window.setupLineEvents = function (lineRow) {
        if (!lineRow) return;
        const calcFields = lineRow.querySelectorAll('.amount-field, .tax-field, .other-tax-field');
        calcFields.forEach(field => {
            field.addEventListener('input', window.updateOverallTotals);
        });
        window.calculateLineTotal(lineRow);
    };

    /**
     * Sets up event listeners for a newly added cost allocation row.
     * @param {HTMLElement} allocationRow The allocation row element.
     */
    window.setupAllocationEvents = function (allocationRow) {
        const rateField = allocationRow.querySelector('.rate-field');
        if (rateField) {
            rateField.addEventListener('input', window.updateOverallTotals);
        }
    };

    /**
     * Re-indexes input names for detail rows.
     */
    window.reindexDetails = function () {
        document.querySelectorAll('.detail-row').forEach((row, index) => {
            row.querySelectorAll('input, select, textarea').forEach(input => {
                if (input.name) {
                    input.name = input.name.replace(/Details\[\d+\]/g, `Details[${index}]`);
                }
            });
        });
        window.updateOverallTotals();
    };

    /**
     * Re-indexes input names for cost allocation rows.
     */
    window.reindexAllocations = function () {
        document.querySelectorAll('.allocation-row').forEach((row, index) => {
            row.querySelectorAll('input, select').forEach(input => {
                if (input.name) {
                    input.name = input.name.replace(/CostAllocations\[\d+\]/g, `CostAllocations[${index}]`);
                }
            });
        });
        window.updateOverallTotals();
    };

    /**
     * Adds a new detail row to the form.
     * @param {object} detail The detail object to pre-populate the row.
     */
    window.addDetailRow = function (detail = {}) {
        const detailsContainer = document.getElementById('details-container');
        if (!detailsContainer) return;

        const newIndex = document.querySelectorAll('.detail-row').length;
        const today = new Date();
        const sixMonthsAgo = new Date();
        sixMonthsAgo.setMonth(today.getMonth() - 6);

        // Fix: Use local date components to avoid timezone issues.
        const todayFormatted = today.getFullYear() + '-' + String(today.getMonth() + 1).padStart(2, '0') + '-' + String(today.getDate()).padStart(2, '0');
        const sixMonthsAgoFormatted = sixMonthsAgo.getFullYear() + '-' + String(sixMonthsAgo.getMonth() + 1).padStart(2, '0') + '-' + String(sixMonthsAgo.getDate()).padStart(2, '0');

        const detailHtml = `
            <div class="card card-body mb-2 detail-row">
                <input type="hidden" name="Details[${newIndex}].Id" value="${detail.id || 0}" />
                <div class="row">
                    <div class="col-md-3 mb-3">
                        <label class="form-label">Invoice Number</label>
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
                        <label class="form-label">Description</label>
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
                            <input name="Details[${newIndex}].AmountExTax" type="number" step="0.01" class="form-control amount-field" value="${detail.amountExTax || '0'}" required />
                        </div>
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label">Sales Tax Rate (%)</label>
                        <div class="input-group">
                            <span class="input-group-text custom-blue"><i class="fas fa-percent"></i></span>
                            <input name="Details[${newIndex}].STRate" type="number" step="0.01" class="form-control tax-field" value="${detail.stRate || '0'}" />
                        </div>
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label">Other Tax</label>
                        <div class="input-group">
                            <span class="input-group-text custom-blue"><i class="fas fa-dollar-sign"></i></span>
                            <input name="Details[${newIndex}].OtherTax" type="number" step="0.01" class="form-control other-tax-field" value="${detail.otherTax || '0'}" />
                        </div>
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label">Total Amount</label>
                        <div class="input-group">
                            <span class="input-group-text custom-blue"><i class="fas fa-dollar-sign"></i></span>
                            <input type="text" class="form-control line-total-display" readonly />
                            <input type="hidden" name="Details[${newIndex}].TotalAmount" class="line-total-hidden" />
                        </div>
                    </div>
                </div>
            </div>
        `;
        detailsContainer.insertAdjacentHTML('beforeend', detailHtml);
        const newRow = detailsContainer.lastElementChild;
        window.setupLineEvents(newRow);
    };

    /**
     * Adds a new cost allocation row to the form.
     * @param {object} allocation The allocation object to pre-populate the row.
     */
    window.addCostAllocationRow = function (allocation = {}) {
        const costAllocationContainer = document.getElementById('cost-allocation-container');
        if (!costAllocationContainer) return;

        const newIndex = document.querySelectorAll('.allocation-row').length;

        // Use the global window.initialData object for options
        const departmentOptions = (window.initialData.departments || []).map(d =>
            `<option value="${d.value}" ${allocation.departmentCode === d.value ? 'selected' : ''}>${d.text}</option>`
        ).join('');
        const branchOptions = (window.initialData.branches || []).map(b =>
            `<option value="${b.value}" ${allocation.branchCode === b.value ? 'selected' : ''}>${b.text}</option>`
        ).join('');

        const allocationHtml = `
            <div class="card card-body mb-2 allocation-row">
                <input type="hidden" name="CostAllocations[${newIndex}].Id" value="${allocation.id || 0}" />
                <div class="row">
                    <div class="col-md-4 mb-3">
                     <label class="form-label">Department</label>
                     <div class="input-group">
                         <span class="input-group-text custom-blue"><i class="fas fa-sitemap"></i></span>
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
                        <label class="form-label">Rate (%) <span class="text-danger">*</span></label>
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
        window.setupAllocationEvents(newRow);
    };

    // --- Main Initialization Function ---

    /**
     * Initializes the form for editing a direct payment request.
     * @param {object} initialData The initial data to pre-populate the form.
     */
    window.initializeEditDirectPaymentRequest = function (initialData) {
        console.log('Initializing Edit Direct Payment Request', initialData);

        // Store initial data globally for other functions to access
        window.initialData = initialData || {};

        // Get key elements
        const form = document.getElementById('editDirectForm');
        const selfApplicantSwitch = document.getElementById('selfApplicantSwitch');
        const supplierSelect = form.querySelector('select[name="SupplierId"]');
        const payeeNameInput = document.getElementById('PayeeName');
        const detailsContainer = document.getElementById('details-container');
        const costAllocationContainer = document.getElementById('cost-allocation-container');

        // Clear existing content and populate with initial data
        detailsContainer.innerHTML = '';
        (window.initialData.details || []).forEach(window.addDetailRow);
        if (window.initialData.details?.length === 0) {
            window.addDetailRow();
        }

        costAllocationContainer.innerHTML = '';
        (window.initialData.costAllocations || []).forEach(window.addCostAllocationRow);
        if (window.initialData.costAllocations?.length === 0) {
            window.addCostAllocationRow();
        }

        // Set up event listeners for dynamic form elements
        document.querySelectorAll('.detail-row').forEach(window.setupLineEvents);
        document.querySelectorAll('.allocation-row').forEach(window.setupAllocationEvents);

        // Set up the self-payee toggle
        if (selfApplicantSwitch) {
            window.handleSelfPayeeToggle(window.initialData);
            selfApplicantSwitch.addEventListener('change', () => window.handleSelfPayeeToggle(window.initialData));
        }

        // Update payee name based on supplier selection
        if (supplierSelect && payeeNameInput) {
            supplierSelect.addEventListener('change', function () {
                if (!selfApplicantSwitch.checked) {
                    const selectedOption = this.options[this.selectedIndex];
                    payeeNameInput.value = selectedOption.text || '';
                }
            });
        }

        // Delegated event listeners for remove buttons
        const addDetailBtn = document.getElementById('addDetailBtn');
        if (addDetailBtn) {
            addDetailBtn.addEventListener('click', () => {
                window.addDetailRow();
                window.reindexDetails();
            });
        }

        const addCostAllocationBtn = document.getElementById('addCostAllocationBtn');
        if (addCostAllocationBtn) {
            addCostAllocationBtn.addEventListener('click', () => {
                window.addCostAllocationRow();
                window.reindexAllocations();
            });
        }

        // Use delegated event listeners for the remove buttons
        detailsContainer.addEventListener('click', function (e) {
            if (e.target.closest('.remove-detail-btn')) {
                e.preventDefault();
                const detailRows = detailsContainer.querySelectorAll('.detail-row');
                if (detailRows.length > 1) {
                    e.target.closest('.detail-row').remove();
                    window.reindexDetails();
                } else {
                    alert('At least one payment detail line is required.');
                }
            }
        });

        costAllocationContainer.addEventListener('click', function (e) {
            if (e.target.closest('.remove-allocation-btn')) {
                e.preventDefault();
                const allocationRows = costAllocationContainer.querySelectorAll('.allocation-row');
                if (allocationRows.length > 1) {
                    e.target.closest('.allocation-row').remove();
                    window.reindexAllocations();
                } else {
                    alert('At least one cost allocation line is required.');
                }
            }
        });

        // Final initial calculation
        setTimeout(window.updateOverallTotals, 100);

        console.log('Edit Direct Payment Request initialized successfully');
    };
})();