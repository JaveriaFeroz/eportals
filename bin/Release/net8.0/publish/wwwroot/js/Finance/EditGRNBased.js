(function () {
    window.initializeEditGRNBasedPaymentRequest = function (initialData) {
        console.log('Initializing Edit GRN Based Payment Request', initialData);

        // Ensure initialData has a default structure to prevent errors
        initialData = initialData || {};
        initialData.details = initialData.details || [];
        initialData.costAllocations = initialData.costAllocations || [];
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
        const costAllocationContainer = document.getElementById('cost-allocation-container');
        const addDetailBtn = document.getElementById('addDetailBtn');
        const addCostAllocationBtn = document.getElementById('addCostAllocationBtn');
        const totalAmountExTaxDisplay = document.getElementById('totalAmountExTaxDisplay');
        const totalOtherTaxDisplay = document.getElementById('totalOtherTaxDisplay');
        const grandTotalDisplay = document.getElementById('grandTotalDisplay');
        const totalRateDisplay = document.getElementById('totalRateDisplay');

        // Check for required elements
        if (!detailsContainer) {
            console.error('Details container not found');
            return;
        }
        if (!costAllocationContainer) {
            console.error('Cost allocation container not found');
            return;
        }

        // --- Helper functions for calculations and UI updates ---

        /**
         * Formats a number as a currency string with two decimal places.
         * @param {number|string} value The value to format.
         * @returns {string} The formatted currency string.
         */
        function formatCurrency(value) {
            const num = parseFloat(value);
            return (isNaN(num) || num === null) ? '0.00' : num.toFixed(2);
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
         * Recalculates and updates the overall totals for the entire form.
         */
        function updateOverallTotals() {
            let totalAmountExTax = 0;
            let totalOtherTax = 0;
            let grandTotal = 0;
            let totalRate = 0;

            // Calculate totals from detail rows
            document.querySelectorAll('.detail-row').forEach(row => {
                totalAmountExTax += parseFloat(row.querySelector('.amount-field')?.value) || 0;
                totalOtherTax += parseFloat(row.querySelector('.other-tax-field')?.value) || 0;
                grandTotal += parseFloat(row.querySelector('.line-total-display')?.value) || 0;
            });

            // Calculate total rate from allocation rows
            document.querySelectorAll('.allocation-row .rate-field').forEach(field => {
                totalRate += parseFloat(field.value) || 0;
            });

            // Update display fields
            if (totalAmountExTaxDisplay) totalAmountExTaxDisplay.value = formatCurrency(totalAmountExTax);
            if (totalOtherTaxDisplay) totalOtherTaxDisplay.value = formatCurrency(totalOtherTax);
            if (grandTotalDisplay) grandTotalDisplay.value = formatCurrency(grandTotal);

            // Update total rate display with visual feedback
            if (totalRateDisplay) {
                totalRateDisplay.value = totalRate.toFixed(2);
                const isRateValid = Math.abs(totalRate - 100) < 0.01;
                totalRateDisplay.style.backgroundColor = isRateValid ? '#d4edda' : '#f8d7da';
                totalRateDisplay.style.color = isRateValid ? '#155724' : '#721c24';
            }
        }

        /**
         * Sets up event listeners for a newly added detail row.
         * @param {HTMLElement} lineRow The detail row element.
         */
        function setupLineEvents(lineRow) {
            if (!lineRow) return;

            const calcFields = lineRow.querySelectorAll('.amount-field, .tax-field, .other-tax-field');
            calcFields.forEach(field => {
                // Use a dedicated function to allow removal
                const handleCalculation = () => {
                    calculateLineTotal(lineRow);
                    updateOverallTotals();
                };
                field.removeEventListener('input', handleCalculation);
                field.addEventListener('input', handleCalculation);
            });
            calculateLineTotal(lineRow); // Initial calculation for new rows
        }

        /**
         * Sets up event listeners for a newly added cost allocation row.
         * @param {HTMLElement} allocationRow The allocation row element.
         */
        function setupAllocationEvents(allocationRow) {
            if (!allocationRow) return;
            const rateField = allocationRow.querySelector('.rate-field');
            if (rateField) {
                // Use a dedicated function to allow removal
                const handleUpdate = () => updateOverallTotals();
                rateField.removeEventListener('input', handleUpdate);
                rateField.addEventListener('input', handleUpdate);
            }
        }

        // --- Re-indexing functions after row removal ---

        /**
         * Re-indexes the names of the input fields for all detail rows.
         */
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

        /**
         * Re-indexes the names of the input fields for all cost allocation rows.
         */
        function reindexAllocations() {
            document.querySelectorAll('.allocation-row').forEach((row, index) => {
                row.querySelectorAll('input, select').forEach(input => {
                    if (input.name) {
                        input.name = input.name.replace(/CostAllocations\[\d+\]/g, `CostAllocations[${index}]`);
                    }
                });
            });
            updateOverallTotals();
        }

        // --- Self Payee Logic ---

        /**
         * Toggles the state of the supplier and payee name fields based on the self applicant switch.
         */
        function handleSelfPayeeToggle() {
            if (!selfApplicantSwitch) return;

            const isSelfPayee = selfApplicantSwitch.checked;
            const supplierField = document.getElementById('supplier-field');
            const payeeNameInput = document.getElementById('PayeeName');
            const supplierSelect = supplierField ? supplierField.querySelector('select[name="SupplierId"]') : null;

            if (isSelfPayee) {
                // Hide supplier field and disable it
                if (supplierField) {
                    supplierField.style.display = 'none';
                    if (supplierSelect) {
                        supplierSelect.value = '';
                        supplierSelect.removeAttribute('required');
                    }
                }
                // Auto-populate payee name with current user and make it read-only
                if (payeeNameInput) {
                    payeeNameInput.value = initialData.currentUserName || 'Not Available';
                    payeeNameInput.readOnly = true;
                    payeeNameInput.style.backgroundColor = '#f8f9fa';
                }
            } else {
                // Show supplier field and make it required
                if (supplierField) {
                    supplierField.style.display = 'block';
                    if (supplierSelect) {
                        supplierSelect.setAttribute('required', 'required');
                    }
                }
                // Clear and enable payee name field
                if (payeeNameInput) {
                    payeeNameInput.value = '';
                    payeeNameInput.readOnly = false;
                    payeeNameInput.style.backgroundColor = '';
                }
            }
        }

        // --- Add new row functions ---

        /**
         * Adds a new detail row to the form.
         * @param {object} detail The detail object to pre-populate the row.
         */
        function addDetailRow(detail = {}) {
            if (!detailsContainer) return;

            const newIndex = document.querySelectorAll('.detail-row').length;
            const detailHtml = `
                <div class="card card-body mb-2 detail-row">
                    <input type="hidden" name="Details[${newIndex}].Id" value="${detail.id || 0}" />
                    <div class="row">
                        <div class="col-md-3 mb-3">
                            <label class="form-label">Invoice Number</label>
                            <div class="input-group">
                                <span class="input-group-text custom-blue"><i class="fas fa-file-invoice"></i></span>
                                <input name="Details[${newIndex}].InvoiceNo" class="form-control" value="${detail.invoiceNo || ''}" />
                            </div>
                        </div>
                        <div class="col-md-3 mb-3">
                            <label class="form-label">Invoice Date</label>
                            <div class="input-group">
                                <span class="input-group-text custom-blue"><i class="fas fa-calendar-alt"></i></span>
                                <input name="Details[${newIndex}].InvoiceDate" type="date" class="form-control" value="${detail.invoiceDate ? new Date(detail.invoiceDate).toISOString().slice(0, 10) : ''}" />
                            </div>
                        </div>
                        <div class="col-4 mb-3">
                            <label class="form-label">Description</label>
                            <div class="input-group">
                                <span class="input-group-text custom-blue"><i class="fas fa-comment-dots"></i></span>
                                <textarea name="Details[${newIndex}].Description" class="form-control" rows="1">${detail.description || ''}</textarea>
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
                                <input type="text" class="form-control line-total-display" value="${detail.totalAmount || ''}" readonly />
                            </div>
                        </div>
                    </div>
                </div>
            `;
            detailsContainer.insertAdjacentHTML('beforeend', detailHtml);
            const newRow = detailsContainer.lastElementChild;
            setupLineEvents(newRow);
        }

        /**
         * Adds a new cost allocation row to the form.
         * @param {object} allocation The allocation object to pre-populate the row.
         */
        function addCostAllocationRow(allocation = {}) {
            if (!costAllocationContainer) return;

            const newIndex = document.querySelectorAll('.allocation-row').length;
            const departmentOptions = initialData.departments.map(d =>
                `<option value="${d.value}" ${allocation.departmentCode === d.value ? 'selected' : ''}>${d.text}</option>`
            ).join('');
            const branchOptions = initialData.branches.map(b =>
                `<option value="${b.value}" ${allocation.branchCode === b.value ? 'selected' : ''}>${b.text}</option>`
            ).join('');

            const allocationHtml = `
                <div class="card card-body mb-2 allocation-row">
                    <input type="hidden" name="CostAllocations[${newIndex}].Id" value="${allocation.id || 0}" />
                    <div class="row">
                        <div class="col-md-4 mb-3">
                            <label class="form-label">Department</label>
                            <div class="input-group">
                                <span class="input-group-text custom-blue"><i class="fas fa-building"></i></span>
                                <select name="CostAllocations[${newIndex}].DepartmentCode" class="form-select department-select">
                                    <option value="">Select Department</option>
                                    ${departmentOptions}
                                </select>
                            </div>
                        </div>
                        <div class="col-md-4 mb-3">
                            <label class="form-label">Branch</label>
                            <div class="input-group">
                                <span class="input-group-text custom-blue"><i class="fas fa-map-marker-alt"></i></span>
                                <select name="CostAllocations[${newIndex}].BranchCode" class="form-select branch-select">
                                    <option value="">Select Branch</option>
                                    ${branchOptions}
                                </select>
                            </div>
                        </div>
                        <div class="col-md-2 mb-3">
                            <label class="form-label">Rate (%) <span class="text-danger">*</span></label>
                            <div class="input-group">
                                <span class="input-group-text custom-blue"><i class="fas fa-percent"></i></span>
                                <input name="CostAllocations[${newIndex}].Rate" type="number" step="0.01" min="0" max="100" class="form-control rate-field" value="${allocation.rate || ''}" required />
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

        // --- Event listeners setup ---

        // Set up events for existing rows first
        document.querySelectorAll('.detail-row').forEach(setupLineEvents);
        document.querySelectorAll('.allocation-row').forEach(setupAllocationEvents);

        // Self applicant switch functionality
        if (selfApplicantSwitch) {
            handleSelfPayeeToggle(); // Set initial state
            selfApplicantSwitch.addEventListener('change', handleSelfPayeeToggle);
        }

        // Supplier select change event - update payee name if not self payee
        if (supplierSelect && payeeNameInput) {
            supplierSelect.addEventListener('change', function () {
                if (!selfApplicantSwitch || !selfApplicantSwitch.checked) {
                    const selectedOption = this.options[this.selectedIndex];
                    payeeNameInput.value = (selectedOption && selectedOption.text && selectedOption.value) ? selectedOption.text : '';
                }
            });
        }

        // Add new row buttons
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

        // Final initial calculation after all elements and event listeners are set up
        setTimeout(updateOverallTotals, 100);

        console.log('Edit GRN Based Payment Request initialized successfully');
    };

    // Auto-initialize if initialData is available globally
    if (typeof window.editGRNInitialData !== 'undefined') {
        document.addEventListener('DOMContentLoaded', function () {
            window.initializeEditGRNBasedPaymentRequest(window.editGRNInitialData);
        });
    }
})();