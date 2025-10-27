// purchase-order-calculations.js

(function () {

    // =======================================================
    // CORE CALCULATION AND VALIDATION HELPERS (Internal)
    // =======================================================

    // --- 1. UNIT PRICE VALIDATION (Prevents price from increasing past original) ---
    function validateUnitPrice(itemRow) {
        const unitPriceInput = itemRow.querySelector('.unit-price-input');
        const newPrice = parseFloat(unitPriceInput.value) || 0;
        // Read the original price from the data attribute (CRITICAL for this validation)
        const originalPrice = parseFloat(unitPriceInput.dataset.originalPrice) || 0;

        // Enforce price cannot be raised
        if (newPrice > originalPrice) {
            alert('The unit price cannot be increased above the original price: ' + originalPrice.toFixed(2));
            // Revert the value to the original price
            unitPriceInput.value = originalPrice.toFixed(2);
        } else if (newPrice < 0) {
            // Prevent negative prices
            unitPriceInput.value = '0.00';
        }

        // Proceed with calculating totals using the (potentially corrected) price
        calculateItemTotalPrice(itemRow);
    }

    // --- 2. SUPPLIER CHANGE HANDLER ---
    function handleSupplierChange(vendorSelect, vendorAddressTextarea, vendorContactInput) {
        const supplierId = vendorSelect.value;
        const url = vendorSelect.dataset.url;
        if (supplierId) {
            fetch(`${url}?supplierId=${supplierId}`)
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        vendorAddressTextarea.value = data.address || '';
                        vendorContactInput.value = data.contact || '';
                        vendorAddressTextarea.setAttribute('readonly', 'readonly');
                        vendorContactInput.setAttribute('readonly', 'readonly');
                    } else {
                        console.error('Failed to fetch supplier details:', data.message);
                        vendorAddressTextarea.value = '';
                        vendorContactInput.value = '';
                        vendorAddressTextarea.removeAttribute('readonly');
                        vendorContactInput.removeAttribute('readonly');
                    }
                })
                .catch(error => {
                    console.error('Error during fetch:', error);
                    vendorAddressTextarea.value = '';
                    vendorContactInput.value = '';
                    vendorAddressTextarea.removeAttribute('readonly');
                    vendorContactInput.removeAttribute('readonly');
                });
        } else {
            vendorAddressTextarea.value = '';
            vendorContactInput.value = '';
            vendorAddressTextarea.removeAttribute('readonly');
            vendorContactInput.removeAttribute('readonly');
        }
    }

    // --- 3. DISCOUNT FIELD TOGGLE ---
    function updateDiscountFields(itemRow) {
        const discRateInput = itemRow.querySelector('.disc-rate-input');
        const discAmountInput = itemRow.querySelector('.item-disc-amount-display');
        const selectedRadio = itemRow.querySelector('input[name^="discountType"]:checked');
        const isPercentSelected = selectedRadio?.value === 'percent';
        if (isPercentSelected) {
            discRateInput.removeAttribute('readonly');
            discAmountInput.setAttribute('readonly', 'readonly');
        } else {
            discAmountInput.removeAttribute('readonly');
            discRateInput.setAttribute('readonly', 'readonly');
        }
        calculateDiscountValues(itemRow);
    }

    // --- 4. DISCOUNT VALUE CALCULATION ---
    function calculateDiscountValues(itemRow) {
        const discRateInput = itemRow.querySelector('.disc-rate-input');
        const discAmountInput = itemRow.querySelector('.item-disc-amount-display');
        const amountExclTax = parseFloat(itemRow.querySelector('.amount-ex-tax-display').value) || 0;
        const selectedRadio = itemRow.querySelector('input[name^="discountType"]:checked');
        const isPercentSelected = selectedRadio?.value === 'percent';
        let calculatedDiscAmount = 0;
        if (isPercentSelected) {
            const discRate = parseFloat(discRateInput.value) || 0;
            calculatedDiscAmount = amountExclTax * (discRate / 100);
            discAmountInput.value = calculatedDiscAmount.toFixed(2);
        } else {
            calculatedDiscAmount = parseFloat(discAmountInput.value) || 0;
        }
        if (calculatedDiscAmount > amountExclTax) {
            calculatedDiscAmount = amountExclTax;
            alert('Discount amount cannot exceed the amount excluding tax.');
            if (isPercentSelected) {
                discRateInput.value = (100).toFixed(2);
            }
            discAmountInput.value = calculatedDiscAmount.toFixed(2);
        }

        const newDiscRate = (amountExclTax > 0) ? (calculatedDiscAmount / amountExclTax) * 100 : 0;
        discRateInput.value = isFinite(newDiscRate) ? newDiscRate.toFixed(2) : 0;

        calculateItemTotalPrice(itemRow);
    }

    // --- 5. ITEM TOTAL CALCULATION ---
    function calculateItemTotalPrice(itemRow) {
        const quantityInput = itemRow.querySelector('.quantity-input');
        const unitPriceInput = itemRow.querySelector('.unit-price-input');
        const discAmountInput = itemRow.querySelector('.item-disc-amount-display');
        const gstRateInput = itemRow.querySelector('.gst-rate-input');
        const amountExclTaxDisplay = itemRow.querySelector('.amount-ex-tax-display');
        const itemGstAmountDisplay = itemRow.querySelector('.item-gst-amount-display');
        const totalPriceDisplay = itemRow.querySelector('.total-price-display');

        const quantity = parseFloat(quantityInput.value) || 0;
        const unitPrice = parseFloat(unitPriceInput.value) || 0;
        const discAmount = parseFloat(discAmountInput.value) || 0;
        let gstRate = parseFloat(gstRateInput.value) || 0;
        if (gstRate < 0 || gstRate > 100) {
            alert('GST Rate must be between 0 and 100.');
            gstRate = Math.min(Math.max(gstRate, 0), 100);
            gstRateInput.value = gstRate;
        }
        const amountExclTax = quantity * unitPrice;
        const subtotalAfterDiscount = amountExclTax - discAmount;
        const gstAmount = subtotalAfterDiscount * (gstRate / 100.00);
        const totalAmount = subtotalAfterDiscount + gstAmount;

        amountExclTaxDisplay.value = amountExclTax.toFixed(2);
        itemGstAmountDisplay.value = gstAmount.toFixed(2);
        totalPriceDisplay.value = totalAmount.toFixed(2);
        updateOverallTotals();
    }

    // --- 6. OVERALL TOTALS ---
    function updateOverallTotals() {
        let totalDiscount = 0;
        let totalGst = 0;
        let grandTotal = 0;

        // Use document since these elements are likely outside the formContainer in the parent view
        document.querySelectorAll('.po-item-row').forEach(row => {
            totalDiscount += parseFloat(row.querySelector('.item-disc-amount-display')?.value) || 0;
            totalGst += parseFloat(row.querySelector('.item-gst-amount-display')?.value) || 0;
            grandTotal += parseFloat(row.querySelector('.total-price-display')?.value) || 0;
        });

        // Look up elements by ID globally since they are outside the item loop
        const totalDiscountAmountDisplay = document.getElementById('totalDiscountAmountDisplay');
        const totalGstAmountDisplay = document.getElementById('totalGstAmountDisplay');
        const grandTotalDisplay = document.getElementById('grandTotalDisplay');
        const mappedAmountHidden = document.getElementById('mappedAmountHidden');

        if (totalDiscountAmountDisplay) totalDiscountAmountDisplay.value = totalDiscount.toFixed(2);
        if (totalGstAmountDisplay) totalGstAmountDisplay.value = totalGst.toFixed(2);
        if (grandTotalDisplay) grandTotalDisplay.value = grandTotal.toFixed(2);
        if (mappedAmountHidden) mappedAmountHidden.value = grandTotal.toFixed(2);
    }

    // --- 7. ATTACH EVENTS PER ROW ---
    function setupItemRowEvents(itemRow) {
        const quantityInput = itemRow.querySelector('.quantity-input');
        const unitPriceInput = itemRow.querySelector('.unit-price-input');
        const discRateInput = itemRow.querySelector('.disc-rate-input');
        const gstRateInput = itemRow.querySelector('.gst-rate-input');
        const discRadios = itemRow.querySelectorAll('.disc-radio');
        const discAmountInput = itemRow.querySelector('.item-disc-amount-display');

        if (quantityInput) {
            quantityInput.addEventListener('input', () => calculateItemTotalPrice(itemRow));
        }

        // ** FIX: Attach validateUnitPrice to input and blur events for price validation **
        if (unitPriceInput) {
            unitPriceInput.addEventListener('input', () => validateUnitPrice(itemRow));
            unitPriceInput.addEventListener('blur', () => validateUnitPrice(itemRow));
        }

        if (gstRateInput) {
            gstRateInput.addEventListener('input', () => calculateItemTotalPrice(itemRow));
            gstRateInput.addEventListener('blur', () => calculateItemTotalPrice(itemRow));
        }

        discRadios.forEach(radio => {
            radio.addEventListener('change', () => updateDiscountFields(itemRow));
        });

        // Consolidating blur event handlers for inputs
        const blurHandler = function () {
            const value = parseFloat(this.value);
            // Ensure values are formatted to 2 decimal places on blur
            this.value = isNaN(value) ? '0.00' : value.toFixed(2);
            calculateDiscountValues(itemRow);
        };

        if (discRateInput) {
            discRateInput.addEventListener('input', () => calculateDiscountValues(itemRow));
            discRateInput.addEventListener('blur', blurHandler);
        }
        if (discAmountInput) {
            discAmountInput.addEventListener('input', () => calculateDiscountValues(itemRow));
            discAmountInput.addEventListener('blur', blurHandler);
        }

        updateDiscountFields(itemRow);
    }

    // =======================================================
    // GLOBAL EXPOSED FUNCTION
    // =======================================================

    /**
     * Initializes dynamic behavior (calculations, validation, vendor fetching)
     * for the Purchase Order form, especially useful for content loaded via AJAX.
     * @param {HTMLElement} formContainer The root element containing the form elements.
     */
    window.initPurchaseOrderForm = function (formContainer) {
        // Find elements within the specified form container
        const totalDiscountAmountDisplay = formContainer.querySelector('#totalDiscountAmountDisplay');
        const totalGstAmountDisplay = formContainer.querySelector('#totalGstAmountDisplay');
        const grandTotalDisplay = formContainer.querySelector('#grandTotalDisplay');
        const mappedAmountHidden = formContainer.querySelector('#mappedAmountHidden');
        const vendorSelect = formContainer.querySelector('#vendorSelect');
        const vendorAddressTextarea = formContainer.querySelector('#vendorAddressTextarea');
        const vendorContactInput = formContainer.querySelector('#vendorContactInput');
        const dateInput = formContainer.querySelector('#expectedDeliveryDateInput');
        const dateContainer = dateInput ? dateInput.closest('.input-group') : null;
        // The list of variables above is complete as requested.

        // 1. Setup Vendor/Date logic
        if (vendorSelect) {
            vendorSelect.removeEventListener('change', handleSupplierChange);

            vendorSelect.addEventListener('change', () => handleSupplierChange(vendorSelect, vendorAddressTextarea, vendorContactInput));
            if (vendorSelect.value) {
                handleSupplierChange(vendorSelect, vendorAddressTextarea, vendorContactInput);
            }
        }
        if (dateContainer && dateInput) {
            dateContainer.addEventListener('click', function () {
                dateInput.showPicker();
            });
        }

        // 2. Setup Item Row Events
        formContainer.querySelectorAll('.po-item-row').forEach(setupItemRowEvents);

        // 3. Initial calculation for totals
        updateOverallTotals();
    }

    // This is the IIFE enclosure. The function is exposed above.
})();