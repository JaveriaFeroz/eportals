document.addEventListener('DOMContentLoaded', function () {
    // --- General Form Functionality ---
    const selfApplicantSwitch = document.getElementById('selfApplicantSwitch');
    const supplierField = document.getElementById('supplier-field');
    const payeeNameInput = document.getElementById('PayeeName');
    const supplierSelect = document.querySelector('select[name="SupplierId"]');
    const departmentSelect = document.querySelector('select[name="DepartmentCode"]');
    const branchSelect = document.querySelector('select[name="BranchCode"]');

    // Select the Payment Nature dropdown
    const paymentNatureSelect = document.querySelector('select[name="PaymentNatureId"]');
    // Get the Cost Allocation card element by its new, reliable ID
    const costAllocationCard = document.getElementById('costAllocationCard');
    const submitBtn = document.getElementById('submitBtn'); // Get the submit button

    // These values will be passed from the Razor View
    let initialPayeeName = '';
    let initialDepartmentCode = '';
    let initialBranchCode = '';
    let departments = [];
    let branches = [];
    let paymentNatureId = 0;
    let grnTotalAmount = 0; // Add this variable to hold the GRN total amount

    // Function to initialize the script with data from the view
    window.initializePaymentRequestScript = function (data) {
        initialPayeeName = data.initialPayeeName;
        initialDepartmentCode = data.initialDepartmentCode;
        initialBranchCode = data.initialBranchCode;
        departments = data.departments;
        branches = data.branches;
        paymentNatureId = data.paymentNatureId;
        grnTotalAmount = data.grnTotalAmount; // Set the GRN total amount

        // Call the initial state setup after data is loaded
        toggleSelfApplicantFields();
        toggleCostAllocationSection(paymentNatureId);

        // Re-enable event listeners after initialization
        if (supplierSelect) {
            supplierSelect.addEventListener('change', function () {
                if (!selfApplicantSwitch.checked) {
                    const selectedOption = this.options[this.selectedIndex];
                    const supplierName = selectedOption.text === 'Select Supplier' ? '' : selectedOption.text;
                    payeeNameInput.value = supplierName;
                    payeeNameInput.setAttribute('readonly', 'readonly');
                }
            });
        }
        if (selfApplicantSwitch) {
            selfApplicantSwitch.addEventListener('change', toggleSelfApplicantFields);
        }

        // This event listener will dynamically hide/show the card if the nature changes
        if (paymentNatureSelect) {
            paymentNatureSelect.addEventListener('change', function () {
                toggleCostAllocationSection(parseInt(this.value, 10));
            });
        }

        const addDetailBtn = document.getElementById('addDetailBtn');
        const detailsContainer = document.getElementById('details-container');

        if (addDetailBtn) {
            addDetailBtn.addEventListener('click', addDetailRow);
        }
        if (detailsContainer) {
            detailsContainer.addEventListener('click', function (e) {
                if (e.target.classList.contains('remove-detail-btn') || e.target.closest('.remove-detail-btn')) {
                    const button = e.target.closest('.remove-detail-btn');
                    const detailRow = button.closest('.detail-row');
                    if (detailsContainer.children.length > 1) {
                        detailRow.remove();
                        reindexDetails();
                    } else {
                        alert('At least one payment detail line is required.');
                    }
                }
            });

            if (detailsContainer.querySelectorAll('.detail-row').length === 0) {
                addDetailRow();
            } else {
                document.querySelectorAll('.detail-row').forEach(setupLineEvents);
                updateOverallTotals();
            }
        }

        const addCostAllocationBtn = document.getElementById('addCostAllocationBtn');
        const costAllocationContainer = document.getElementById('cost-allocation-container');

        if (costAllocationContainer) {
            if (addCostAllocationBtn) {
                addCostAllocationBtn.addEventListener('click', addCostAllocationRow);
            }
            costAllocationContainer.addEventListener('click', function (e) {
                if (e.target.closest('.remove-allocation-btn')) {
                    const button = e.target.closest('.remove-allocation-btn');
                    const allocationRow = button.closest('.allocation-row');
                    if (costAllocationContainer.children.length > 1) {
                        allocationRow.remove();
                        reindexAllocations();
                    } else {
                        alert('At least one cost allocation line is required.');
                    }
                }
            });

            if (costAllocationContainer.querySelectorAll('.allocation-row').length === 0) {
                addCostAllocationRow();
            } else {
                document.querySelectorAll('.allocation-row').forEach(setupAllocationEvents);
                updateOverallRate();
            }
        }
    };

    // Function to toggle the visibility of the cost allocation section based on payment nature
    function toggleCostAllocationSection(id) {
        if (costAllocationCard) {
            // Payment Nature '1' corresponds to 'Operational Payments' which do not have cost allocation.
            if (id === 1) {
                costAllocationCard.style.display = 'none';
                const inputs = costAllocationCard.querySelectorAll('input, select, textarea');
                inputs.forEach(input => input.disabled = true);
            } else {
                costAllocationCard.style.display = 'block';
                const inputs = costAllocationCard.querySelectorAll('input, select, textarea');
                inputs.forEach(input => input.disabled = false);
            }
        }
    }

    function toggleSelfApplicantFields() {
        const departmentHiddenInput = document.querySelector('input[name="DepartmentCode"][type="hidden"]');
        const branchHiddenInput = document.querySelector('input[name="BranchCode"][type="hidden"]');
        const supplierHiddenInput = document.querySelector('input[name="SupplierId"][type="hidden"]');

        if (selfApplicantSwitch && supplierField && payeeNameInput && departmentSelect && branchSelect) {
            if (selfApplicantSwitch.checked) {
                supplierField.style.display = 'none';
                payeeNameInput.value = initialPayeeName;
                payeeNameInput.removeAttribute('disabled');
                payeeNameInput.setAttribute('readonly', 'readonly');
                payeeNameInput.setAttribute('required', 'required');

                supplierSelect.value = '';
                supplierSelect.removeAttribute('required');
                supplierSelect.setAttribute('disabled', 'disabled');

                departmentSelect.value = initialDepartmentCode;
                departmentSelect.setAttribute('disabled', 'disabled');
                branchSelect.value = initialBranchCode;
                branchSelect.setAttribute('disabled', 'disabled');

                if (departmentHiddenInput) departmentHiddenInput.value = initialDepartmentCode;
                if (branchHiddenInput) branchHiddenInput.value = initialBranchCode;
                if (supplierHiddenInput) supplierHiddenInput.value = '';
            } else {
                supplierField.style.display = 'block';
                payeeNameInput.value = '';
                payeeNameInput.removeAttribute('readonly');
                payeeNameInput.removeAttribute('disabled');
                payeeNameInput.setAttribute('required', 'required');

                supplierSelect.removeAttribute('disabled');
                supplierSelect.setAttribute('required', 'required');

                departmentSelect.removeAttribute('disabled');
                branchSelect.removeAttribute('disabled');

                if (departmentHiddenInput) departmentHiddenInput.value = '';
                if (branchHiddenInput) branchHiddenInput.value = '';
            }
        }
    }

    const detailsContainer = document.getElementById('details-container');
    const totalAmountExTaxDisplay = document.getElementById('totalAmountExTaxDisplay');
    const totalOtherTaxDisplay = document.getElementById('totalOtherTaxDisplay');
    const grandTotalDisplay = document.getElementById('grandTotalDisplay');

    function formatCurrency(value) {
        if (value === null || value === undefined || isNaN(value)) return '0.00';
        return parseFloat(value).toFixed(2);
    }

    function updateOverallTotals() {
        let totalAmountExTax = 0;
        let totalSTTax = 0;
        let totalOtherTax = 0;
        let grandTotal = 0;

        document.querySelectorAll('.detail-row').forEach(row => {
            const amount = parseFloat(row.querySelector('.amount-field')?.value) || 0;
            const otherTax = parseFloat(row.querySelector('.other-tax-field')?.value) || 0;
            const stRate = parseFloat(row.querySelector('.tax-field')?.value) || 0;
            const stTax = (amount * stRate / 100);

            const lineTotal = amount + stTax + otherTax;

            totalAmountExTax += amount;
            totalSTTax += stTax;
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

        // --- New Validation Logic ---
        if (grnTotalAmount > 0 && totalAmountExTax > grnTotalAmount) {
            alert(`The total Amount Ex Tax (${totalAmountExTax.toFixed(2)}) cannot exceed the GRN total amount (${grnTotalAmount.toFixed(2)}).`);
            if (submitBtn) {
                submitBtn.disabled = true; // Disable the submit button
            }
            totalAmountExTaxDisplay.style.backgroundColor = '#f8d7da';
            totalAmountExTaxDisplay.style.color = '#721c24';
        } else {
            if (submitBtn) {
                submitBtn.disabled = false; // Re-enable the submit button
            }
            totalAmountExTaxDisplay.style.backgroundColor = '';
            totalAmountExTaxDisplay.style.color = '';
        }
        // --- End New Validation Logic ---
    }

    function setupLineEvents(lineRow) {
        const calcFields = lineRow.querySelectorAll('.amount-field, .tax-field, .other-tax-field');
        calcFields.forEach(field => {
            field.addEventListener('input', updateOverallTotals);
        });
        updateOverallTotals();
    }

    function reindexDetails() {
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

    function addDetailRow() {
        const newIndex = detailsContainer.querySelectorAll('.detail-row').length;
        const detailHtml = `
            <div class="card card-body mb-2 detail-row">
                <div class="row">
                    <div class="col-md-3 mb-3">
                        <label class="form-label">Invoice Number <span class="text-danger">*</span></label>
                        <div class="input-group">
                            <span class="input-group-text custom-blue"><i class="fas fa-file-invoice"></i></span>
                            <input name="Details[${newIndex}].InvoiceNo" class="form-control" placeholder="Enter invoice number" required/>
                        </div>
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label">Invoice Date <span class="text-danger">*</span></label>
                        <div class="input-group">
                            <span class="input-group-text custom-blue"><i class="fas fa-calendar-alt"></i></span>
                            <input name="Details[${newIndex}].InvoiceDate" type="date" class="form-control" required/>
                        </div>
                    </div>
                    <div class="col-4 mb-3">
                        <label class="form-label">Description <span class="text-danger">*</span></label>
                        <div class="input-group">
                            <span class="input-group-text custom-blue"><i class="fas fa-comment-dots"></i></span>
                            <textarea name="Details[${newIndex}].Description" class="form-control" placeholder="Enter item description" required rows="1"></textarea>
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
                            <input name="Details[${newIndex}].AmountExTax" type="number" step="0.01" class="form-control amount-field" value="0" required />
                        </div>
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label">Sales Tax Rate (%)</label>
                        <div class="input-group">
                            <span class="input-group-text custom-blue"><i class="fas fa-percent"></i></span>
                            <input name="Details[${newIndex}].STRate" type="number" step="0.01" class="form-control tax-field" value="0" />
                        </div>
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label">Other Tax</label>
                        <div class="input-group">
                            <span class="input-group-text custom-blue"><i class="fas fa-dollar-sign"></i></span>
                            <input name="Details[${newIndex}].OtherTax" type="number" step="0.01" class="form-control other-tax-field" value="0" />
                        </div>
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label">Total Amount</label>
                        <div class="input-group">
                            <span class="input-group-text custom-blue"><i class="fas fa-dollar-sign"></i></span>
                            <input type="text" class="form-control line-total-display" readonly />
                        </div>
                    </div>
                </div>
            </div>
        `;
        detailsContainer.insertAdjacentHTML('beforeend', detailHtml);
        const newRow = detailsContainer.lastElementChild;
        setupLineEvents(newRow);
    }

    const costAllocationContainer = document.getElementById('cost-allocation-container');
    const totalRateDisplay = document.getElementById('totalRateDisplay');

    function updateOverallRate() {
        let totalRate = 0;
        document.querySelectorAll('.allocation-row .rate-field').forEach(field => {
            totalRate += parseFloat(field.value) || 0;
        });
        if (totalRateDisplay) {
            totalRateDisplay.value = totalRate.toFixed(2);

            if (totalRate !== 100) {
                totalRateDisplay.style.backgroundColor = '#f8d7da';
                totalRateDisplay.style.color = '#721c24';
            } else {
                totalRateDisplay.style.backgroundColor = '#d4edda';
                totalRateDisplay.style.color = '#155724';
            }
        }
    }

    function setupAllocationEvents(allocationRow) {
        const rateField = allocationRow.querySelector('.rate-field');
        if (rateField) {
            rateField.addEventListener('input', updateOverallRate);
        }
    }

    function reindexAllocations() {
        document.querySelectorAll('.allocation-row').forEach((row, index) => {
            row.querySelectorAll('input, select').forEach(input => {
                const name = input.name;
                if (name) {
                    input.name = name.replace(/CostAllocations\[\d+\]/g, `CostAllocations[${index}]`);
                }
            });
        });
        updateOverallRate();
    }

    function addCostAllocationRow() {
        const newIndex = costAllocationContainer.querySelectorAll('.allocation-row').length;
        const departmentOptions = departments.map(d => `<option value="${d.value}">${d.text}</option>`).join('');
        const branchOptions = branches.map(b => `<option value="${b.value}">${b.text}</option>`).join('');

        const allocationHtml = `
            <div class="card card-body mb-2 allocation-row">
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
                        <label class="form-label">Rate (%)</label>
                        <div class="input-group">
                            <span class="input-group-text custom-blue"><i class="fas fa-percent"></i></span>
                            <input name="CostAllocations[${newIndex}].Rate" type="number" step="0.01" min="0" max="100" class="form-control rate-field" value="0" />
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
});