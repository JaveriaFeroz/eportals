// ====================================================================
// PRCreate.js - Centralized logic for Purchase Request forms (Create & Edit)
// All functions are attached to the 'window' object for global access.
// ====================================================================

// --- Global Variables (now attached to window object) ---
window.detailRowIndex = 0;

window.getUrl = function (area, controller, action, id = null) {
    let url = `${baseUrl}${area}/${controller}/${action}`;
    if (id !== null) {
        url += `/${id}`;
    }
    return url;
};

// --- Utility Functions ---

/**
 * Validates a form after re-parsing its validation rules.
 * @param {jQuery} $form The jQuery object for the form.
 * @returns {boolean} True if the form is valid, false otherwise.
 */


window.validateForm = function ($form) {
    // Re-parses the validation rules for dynamically added content
    window.reparseFormValidation($form);

    // Ensure jQuery validation is available
    if (typeof $.validator === 'undefined') {
        console.error('jQuery validation is not loaded');
        return true; // Allow submission if validation library is missing
    }

    // Get or create validator instance
    var validator = $form.data('validator') || $form.validate();

    // Trigger validation and return result
    return validator.form();
};

/**
 * Formats a Date object into a string suitable for a datetime-local input.
 * @param {Date} date The date to format.
 * @returns {string} The formatted date string (e.g., '2025-08-05T15:30').
 */

window.formatDateTimeLocal = function (date) {
    const year = date.getFullYear();
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const day = date.getDate().toString().padStart(2, '0');
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
};

/**
 * Clears the fields of a single item row.
 * @param {jQuery} $row The jQuery object for the item row.
 * @param {jQuery} $form The parent form's jQuery object.
 */
window.clearAllItemRowFields = function ($row, $form) {
    $row.find('.product-dropdown').val('');
    $row.find('.service-dropdown').val('');
    $row.find('.item-name-input').val('');
    $row.find('.item-code-hidden').val('');
    $row.find('.unit-price-input').val(0);
    $row.find('.uom-id-hidden').val('');
    $row.find('.uom-name').val('');
    $row.find('.narration-input').val('');
    window.calculateItemAmounts($row, $form);
};

/**
 * Calculates the amounts for a single item row and updates the fields.
 * @param {jQuery} rowElement The jQuery object for the item row.
 * @param {jQuery} $form The parent form's jQuery object.
 */
window.calculateItemAmounts = function (rowElement, $form) {
    const quantity = parseFloat(rowElement.find('.quantity-input').val()) || 0;
    const unitPrice = parseFloat(rowElement.find('.unit-price-input').val()) || 0;
    const gstRate = parseFloat(rowElement.find('.gst-rate-input').val()) || 0;
    const vatRate = parseFloat(rowElement.find('.vat-rate-input').val()) || 0;

    const grossAmount = quantity * unitPrice;
    const gstAmount = grossAmount * (gstRate / 100);
    const vatAmount = grossAmount * (vatRate / 100);
    const netAmount = grossAmount + gstAmount + vatAmount;

    rowElement.find('.gross-amount-display').val(grossAmount.toFixed(2));
    rowElement.find('.gst-amount-display').val(gstAmount.toFixed(2));
    rowElement.find('.vat-amount-display').val(vatAmount.toFixed(2));
    rowElement.find('.net-amount-output').val(netAmount.toFixed(2));

    rowElement.find('.gross-amount-hidden').val(grossAmount.toFixed(2));
    rowElement.find('.gst-amount-hidden').val(gstAmount.toFixed(2));
    rowElement.find('.vat-amount-hidden').val(vatAmount.toFixed(2));
    rowElement.find('.total-price-hidden').val(netAmount.toFixed(2));

    window.reparseFormValidation($form);
};

/**
 * Populates the product or service dropdowns for a single item row based on type and nature ID.
 * @param {jQuery} rowElement The jQuery object for the item row.
 * @param {jQuery} $form The parent form's jQuery object.
 * @param {string} currentPurchaseItemType The selected purchase item type ('1' for Goods, '2' for Service).
 * @param {string} currentNatureId The selected nature ID.
 * @param {string|null} selectedProductId Pre-selected product ID (for edit).
 * @param {string|null} selectedServiceId Pre-selected service ID (for edit).
 */
window.populateItemDropdowns = function (rowElement, $form, currentPurchaseItemType, currentNatureId, selectedProductId = null, selectedServiceId = null) {
    const $productDropdown = rowElement.find('.product-dropdown');
    const $serviceDropdown = rowElement.find('.service-dropdown');
    const $productGroup = rowElement.find('.product-group-item-row');
    const $serviceGroup = rowElement.find('.service-group-item-row');

    $productDropdown.empty().append($('<option></option>').val('').text('-- Select Product --'));
    $serviceDropdown.empty().append($('<option></option>').val('').text('-- Select Service --'));

    rowElement.find('span[data-valmsg-for^="Items"][data-valmsg-for$="ProductId"]').empty();
    rowElement.find('span[data-valmsg-for^="Items"][data-valmsg-for$="ServiceId"]').empty();

    if (currentPurchaseItemType === '1') { // Goods
        $productGroup.show();
        $productDropdown.prop('disabled', false).attr('data-val-required', 'Product is required.');

        $serviceGroup.hide();
        $serviceDropdown.prop('disabled', true).removeAttr('data-val-required').val('');

        if (currentNatureId) {
            $.ajax({
                url: getUrl('Procurement', 'PurchaseRequest', 'GetProductsByNatureId'),
                type: 'GET',
                data: { natureId: currentNatureId },
                success: function (data) {
                    $.each(data, function (idx, item) {
                        $productDropdown.append($('<option></option>').val(item.value).text(item.text));
                    });
                    if (selectedProductId && $productDropdown.find('option[value="' + selectedProductId + '"]').length) {
                        $productDropdown.val(selectedProductId);
                    }
                    window.reparseFormValidation($form);
                },
                error: function () {
                    console.error("Failed to load Products.");
                }
            });
        }
    } else if (currentPurchaseItemType === '2') { // Service
        $serviceGroup.show();
        $serviceDropdown.prop('disabled', false).attr('data-val-required', 'Service is required.');

        $productGroup.hide();
        $productDropdown.prop('disabled', true).removeAttr('data-val-required').val('');

        if (currentNatureId) {
            $.ajax({
                url: getUrl('Procurement', 'PurchaseRequest', 'GetServicesByNatureId'),
                type: 'GET',
                data: { natureId: currentNatureId },
                success: function (data) {
                    $.each(data, function (idx, item) {
                        $serviceDropdown.append($('<option></option>').val(item.value).text(item.text));
                    });
                    if (selectedServiceId && $serviceDropdown.find('option[value="' + selectedServiceId + '"]').length) {
                        $serviceDropdown.val(selectedServiceId);
                    }
                    window.reparseFormValidation($form);
                },
                error: function () {
                    console.error("Failed to load Services.");
                }
            });
        }
    } else { // Neither Goods nor Service selected
        $productGroup.hide();
        $serviceGroup.hide();
        $productDropdown.prop('disabled', true).removeAttr('data-val-required').val('');
        $serviceDropdown.prop('disabled', true).removeAttr('data-val-required').val('');
        window.clearAllItemRowFields(rowElement, $form);
    }
    window.reparseFormValidation($form);
};

/**
 * Toggles the visibility and validation of product/service dropdowns for a single row.
 * @param {string} purchaseItemType The selected purchase item type.
 * @param {jQuery} $row The jQuery object for the item row.
 * @param {jQuery} $form The parent form's jQuery object.
 */
window.toggleItemDropdownsForSingleRow = function (purchaseItemType, $row, $form) {
    const $productGroup = $row.find('.product-group-item-row');
    const $serviceGroup = $row.find('.service-group-item-row');
    const $productDropdown = $productGroup.find('.product-dropdown');
    const $serviceDropdown = $serviceGroup.find('.service-dropdown');

    $row.find('span[data-valmsg-for^="Items"][data-valmsg-for$="ProductId"]').empty();
    $row.find('span[data-valmsg-for^="Items"][data-valmsg-for$="ServiceId"]').empty();

    if (purchaseItemType === '1') {
        $productGroup.show();
        $productDropdown.prop('disabled', false).attr('data-val-required', 'Product is required.');
        $serviceGroup.hide();
        $serviceDropdown.prop('disabled', true).removeAttr('data-val-required').val('');
    } else if (purchaseItemType === '2') {
        $productGroup.hide();
        $productDropdown.prop('disabled', true).removeAttr('data-val-required').val('');
        $serviceGroup.show();
        $serviceDropdown.prop('disabled', false).attr('data-val-required', 'Service is required.');
    } else {
        $productGroup.hide();
        $serviceGroup.hide();
        $productDropdown.prop('disabled', true).removeAttr('data-val-required').val('');
        $serviceDropdown.prop('disabled', true).removeAttr('data-val-required').val('');
    }
    window.clearAllItemRowFields($row, $form);
    window.reparseFormValidation($form);
};

/**
 * Binds all necessary events to an item row (input changes, dropdown selections).
 * @param {jQuery} $row The jQuery object for the item row.
 * @param {jQuery} $form The parent form's jQuery object.
 */
window.bindItemRowEvents = function ($row, $form) {
    $row.find('.quantity-input, .unit-price-input, .gst-rate-input, .vat-rate-input').on('input', function () {
        window.calculateItemAmounts($(this).closest('.purchase-request-item-row'), $form);
        window.updatePurchaseRequestTotal($form);
    });

    $row.find('.product-dropdown').on('change', function () {
        const $currentRow = $(this).closest('.purchase-request-item-row');
        const productId = $(this).val();
        if (productId) {
            $.ajax({
                url: getUrl('Procurement', 'PurchaseRequest', 'GetProductDetails'),
                type: 'GET',
                data: { productId: productId },
                success: function (data) {
                    if (data && data.success) {
                        $currentRow.find('.item-name-input').val(data.name);
                        $currentRow.find('.item-code-hidden').val(data.code || '');
                        $currentRow.find('.unit-price-input').val(data.unitPrice || 0);
                        $currentRow.find('.uom-id-hidden').val(data.uoMId || '');
                        $currentRow.find('.uom-name').val(data.uoMName || '');
                        window.calculateItemAmounts($currentRow, $form);
                        window.updatePurchaseRequestTotal($form);
                    } else {
                        console.warn("Product details not found or failed: " + (data ? data.message : "unknown error"));
                        $currentRow.find('.item-name-input, .item-code-hidden, .unit-price-input, .uom-id-hidden, .uom-name').val('');
                        $currentRow.find('.narration-input').val('');
                        window.calculateItemAmounts($currentRow, $form);
                        window.updatePurchaseRequestTotal($form);
                    }
                    window.reparseFormValidation($form);
                },
                error: function () {
                    console.error("AJAX Error loading product details.");
                    $currentRow.find('.item-name-input, .item-code-hidden, .unit-price-input, .uom-id-hidden, .uom-name').val('');
                    $currentRow.find('.narration-input').val('');
                    window.calculateItemAmounts($currentRow, $form);
                    window.updatePurchaseRequestTotal($form);
                    window.reparseFormValidation($form);
                }
            });
        } else {
            window.clearAllItemRowFields($currentRow, $form);
            window.updatePurchaseRequestTotal($form);
            window.reparseFormValidation($form);
        }
    });

    $row.find('.service-dropdown').on('change', function () {
        const $currentRow = $(this).closest('.purchase-request-item-row');
        const serviceId = $(this).val();
        if (serviceId) {
            $.ajax({
                url: getUrl('Procurement', 'PurchaseRequest', 'GetServiceDetails'),
                type: 'GET',
                data: { serviceId: serviceId },
                success: function (data) {
                    if (data && data.success) {
                        $currentRow.find('.item-name-input').val(data.name);
                        $currentRow.find('.item-code-hidden').val(data.code || '');
                        $currentRow.find('.unit-price-input').val(data.unitPrice || 0);
                        $currentRow.find('.uom-id-hidden').val(data.uoMId || '');
                        $currentRow.find('.uom-name').val(data.uoMName || '');
                        window.calculateItemAmounts($currentRow, $form);
                        window.updatePurchaseRequestTotal($form);
                    } else {
                        console.warn("Service details not found or failed: " + (data ? data.message : "unknown error"));
                        $currentRow.find('.item-name-input, .item-code-hidden, .unit-price-input, .uom-id-hidden, .uom-name').val('');
                        $currentRow.find('.narration-input').val('');
                        window.calculateItemAmounts($currentRow, $form);
                        window.updatePurchaseRequestTotal($form);
                    }
                    window.reparseFormValidation($form);
                },
                error: function () {
                    console.error("AJAX Error loading service details.");
                    $currentRow.find('.item-name-input, .item-code-hidden, .unit-price-input, .uom-id-hidden, .uom-name').val('');
                    $currentRow.find('.narration-input').val('');
                    window.calculateItemAmounts($currentRow, $form);
                    window.updatePurchaseRequestTotal($form);
                    window.reparseFormValidation($form);
                }
            });
        } else {
            window.clearAllItemRowFields($currentRow, $form);
            window.updatePurchaseRequestTotal($form);
            window.reparseFormValidation($form);
        }
    });

    $row.find('.narration-input').on('input', function () {
        window.reparseFormValidation($form);
    });
};

/**
 * Re-indexes the item rows to maintain correct model binding for the server.
 * @param {jQuery} $form The parent form's jQuery object.
 */
window.reIndexDetailRows = function ($form) {
    let newDetailRowIndex = 0;
    $form.find('.purchase-request-item-row').each(function (i) {
        const $row = $(this);
        $row.attr('data-row-index', i);
        $row.find('[name^="Items["]').each(function () {
            const oldName = $(this).attr('name');
            if (oldName) {
                const newName = oldName.replace(/Items\[\d+\]/, `Items[${i}]`);
                $(this).attr('name', newName);
            }
        });
        $row.find('[id^="Items_"]').each(function () {
            const oldId = $(this).attr('id');
            if (oldId) {
                const newId = oldId.replace(/Items_\d+__/, `Items_${i}__`);
                $(this).attr('id', newId);
            }
        });
        $row.find('span[data-valmsg-for^="Items["]').each(function () {
            const oldDataValMsgFor = $(this).attr('data-valmsg-for');
            if (oldDataValMsgFor) {
                const newDataValMsgFor = oldDataValMsgFor.replace(/Items\[\d+\]/, `Items[${i}]`);
                $(this).attr('data-val-msg-for', newDataValMsgFor);
            }
        });
        newDetailRowIndex++;
    });
    window.detailRowIndex = newDetailRowIndex;
    window.reparseFormValidation($form);
};

/**
 * Updates the total amount for the entire purchase request form.
 * @param {jQuery} $form The parent form's jQuery object.
 */
window.updatePurchaseRequestTotal = function ($form) {
    let total = 0;
    $form.find('.purchase-request-item-row').each(function () {
        const netAmount = parseFloat($(this).find('.net-amount-output').val()) || 0;
        total += netAmount;
    });
    $form.find('#TotalAmount').val(total.toFixed(2));
};

/**
 * Updates the main nature dropdowns based on the selected purchase type.
 * @param {jQuery} $form The parent form's jQuery object.
 */
window.updateMainNatureDropdowns = function ($form) {
    const selectedPurchaseNatureType = $form.find('.purchase-nature-type-radio:checked').val();
    const selectedPurchaseItemType = $form.find('.purchase-item-type-radio:checked').val();

    const $productNatureDropdown = $form.find('#productNatureDropdown');
    const $serviceNatureDropdown = $form.find('#serviceNatureDropdown');
    const $productNatureGroup = $form.find('#productNatureGroup');
    const $serviceNatureGroup = $form.find('#serviceNatureGroup');

    $productNatureDropdown.empty().append($('<option></option>').val('').text('-- Select Product Nature --'));
    $serviceNatureDropdown.empty().append($('<option></option>').val('').text('-- Select Service Nature --'));
    $productNatureGroup.hide();
    $serviceNatureGroup.hide();

    $form.find('span[data-valmsg-for="ProductNatureId"]').empty();
    $form.find('span[data-valmsg-for="ServiceNatureId"]').empty();

    if (!selectedPurchaseNatureType || !selectedPurchaseItemType) {
        window.reparseFormValidation($form);
        return;
    }

    if (selectedPurchaseItemType === '1') { // Goods
        $productNatureGroup.show();
        $productNatureDropdown.prop('disabled', false).attr('data-val-required', 'Product Nature is required when Purchase Item Type is Goods');
        $serviceNatureDropdown.prop('disabled', true).removeAttr('data-val-required').val('');

        $.ajax({
            url: getUrl('Procurement', 'PurchaseRequest', 'GetProductNaturesByPurchaseNatureType'),
            type: 'GET',
            data: { type: selectedPurchaseNatureType },
            success: function (data) {
                $.each(data, function (index, item) {
                    $productNatureDropdown.append($('<option></option>').val(item.value).text(item.text));
                });
                const preSelectedProductNatureId = String($productNatureDropdown.data('initial-value')) || '';
                if (preSelectedProductNatureId && $productNatureDropdown.find('option[value="' + preSelectedProductNatureId + '"]').length) {
                    $productNatureDropdown.val(preSelectedProductNatureId);
                }
                window.reparseFormValidation($form);
            }
        });
    } else if (selectedPurchaseItemType === '2') { // Service
        $serviceNatureGroup.show();
        $serviceNatureDropdown.prop('disabled', false).attr('data-val-required', 'Service Nature is required when Purchase Item Type is Service');
        $productNatureDropdown.prop('disabled', true).removeAttr('data-val-required').val('');

        $.ajax({
            url: getUrl('Procurement', 'PurchaseRequest', 'GetServiceNaturesByPurchaseNatureType'),
            type: 'GET',
            data: { type: selectedPurchaseNatureType },
            success: function (data) {
                $.each(data, function (index, item) {
                    $serviceNatureDropdown.append($('<option></option>').val(item.value).text(item.text));
                }); const preSelectedServiceNatureId = String($serviceNatureDropdown.data('initial-value')) || '';

                if (preSelectedServiceNatureId && $serviceNatureDropdown.find('option[value="' + preSelectedServiceNatureId + '"]').length) {
                    $serviceNatureDropdown.val(preSelectedServiceNatureId);
                }
                window.reparseFormValidation($form);
            }
        });
    }
    window.reparseFormValidation($form);
    // Check if there are items and disable the main form selections if needed
    if ($form.find('.purchase-request-item-row').length === 0) {
        window.enableMainFormSelections($form);
    }
};

/**
 * Updates the text of the "Add Item" button based on the selected item type.
 * @param {jQuery} $form The parent form's jQuery object.
 */
window.updateAddButtonText = function ($form) {
    const $addDetailRowBtn = $form.find('#addDetailRowBtn');
    const selectedPurchaseItemType = $form.find('.purchase-item-type-radio:checked').val();
    if (selectedPurchaseItemType === '1') {
        $addDetailRowBtn.html('<i class="fas fa-plus me-1"></i> Add Product');
    } else if (selectedPurchaseItemType === '2') {
        $addDetailRowBtn.html('<i class="fas fa-plus me-1"></i> Add Service');
    } else {
        $addDetailRowBtn.html('<i class="fas fa-plus me-1"></i> Add Product/Service');
    }
};

/**
 * Disables the main form selection fields to prevent changes once items are added.
 * @param {jQuery} $form The parent form's jQuery object.
 */
window.disableMainFormSelections = function ($form) {
    const selectedPurchaseNatureType = $form.find('.purchase-nature-type-radio:checked').val();
    const selectedPurchaseItemType = $form.find('.purchase-item-type-radio:checked').val();
    const selectedProductNatureId = $form.find('#productNatureDropdown').val();
    const selectedServiceNatureId = $form.find('#serviceNatureDropdown').val();

    console.log('Disabling form - Current values:', {
        purchaseNatureType: selectedPurchaseNatureType,
        purchaseItemType: selectedPurchaseItemType,
        productNatureId: selectedProductNatureId,
        serviceNatureId: selectedServiceNatureId
    });

    // Update hidden fields with current values
    $form.find('#PurchaseNatureType_Hidden').val(selectedPurchaseNatureType);
    $form.find('#PurchaseItemType_Hidden').val(selectedPurchaseItemType);
    $form.find('#ProductNatureId_Hidden').val(selectedProductNatureId || '');
    $form.find('#ServiceNatureId_Hidden').val(selectedServiceNatureId || '');

    // Verify hidden fields were set
    console.log('Hidden field values after setting:', {
        purchaseNatureType: $form.find('#PurchaseNatureType_Hidden').val(),
        purchaseItemType: $form.find('#PurchaseItemType_Hidden').val(),
        productNatureId: $form.find('#ProductNatureId_Hidden').val(),
        serviceNatureId: $form.find('#ServiceNatureId_Hidden').val()
    });

    // Disable visible controls
    $form.find('.purchase-nature-type-radio').prop('disabled', true);
    $form.find('.purchase-item-type-radio').prop('disabled', true);
    $form.find('#productNatureDropdown').prop('disabled', true);
    $form.find('#serviceNatureDropdown').prop('disabled', true);
};

/**
 * Enables the main form selection fields.
 * @param {jQuery} $form The parent form's jQuery object.
 */
window.enableMainFormSelections = function ($form) {
    console.log('Enabling form selections');

    // Don't clear hidden fields, just enable controls
    // The controls will update the hidden fields when changed

    // Enable visible controls
    $form.find('.purchase-nature-type-radio').prop('disabled', false);
    $form.find('.purchase-item-type-radio').prop('disabled', false);
    $form.find('#productNatureDropdown').prop('disabled', false);
    $form.find('#serviceNatureDropdown').prop('disabled', false);
};
/**
 * Re-parses unobtrusive validation for a dynamic form or form part.
 * @param {jQuery} $form The form's jQuery object.
 */
window.reparseFormValidation = function ($form) {
    if (window.jQuery && window.jQuery.validator && window.jQuery.validator.unobtrusive) {
        $form.removeData('validator');
        $form.removeData('unobtrusiveValidation');
        $.validator.unobtrusive.parse($form);
        $form.valid();
    }
};

/**
 * The main initialization function for both Create and Edit forms.
 * @param {jQuery} $form The form's jQuery object.
 * @param {number} initialItemCount The number of items already present in the form.
 */
window.initializePurchaseRequestForm = function ($form, initialItemCount) {
    window.detailRowIndex = initialItemCount;

    // Date Field Initialization
    (function initDateFields() {
        const now = new Date();
        const requestDateInput = $form.find('input[name="RequestDate"]');
        if (!requestDateInput.val()) {
            requestDateInput.val(window.formatDateTimeLocal(now));
        }
        const requiredDateInput = $form.find('input[name="RequiredDate"]');
        if (!requiredDateInput.val()) {
            const sevenDaysLater = new Date(now.getTime() + (7 * 24 * 60 * 60 * 1000));
            requiredDateInput.val(window.formatDateTimeLocal(sevenDaysLater));
        }
    })();

    // Store initial values from the server-rendered HTML for logic to use
    const initialPurchaseNatureType = $form.find('input[name="PurchaseNatureType"]:checked').val();
    const initialPurchaseItemType = $form.find('input[name="PurchaseItemType"]:checked').val();

    // Bind event listeners for main form controls
    $form.find('.purchase-nature-type-radio').off('change').on('change', function () {
        window.updateMainNatureDropdowns($form);
        const selectedPurchaseItemType = $form.find('.purchase-item-type-radio:checked').val();
        const relevantNatureIdForItems = (selectedPurchaseItemType === '1')
            ? $form.find('#productNatureDropdown').val()
            : $form.find('#serviceNatureDropdown').val();
        $form.find('.purchase-request-item-row').each(function () {
            const $row = $(this);
            window.clearAllItemRowFields($row, $form);
            window.populateItemDropdowns($row, $form, selectedPurchaseItemType, relevantNatureIdForItems);
        });
        window.updatePurchaseRequestTotal($form);
    });

    $form.find('.purchase-item-type-radio').off('change').on('change', function () {
        window.updateMainNatureDropdowns($form);
        window.updateAddButtonText($form);
        const selectedPurchaseItemType = $(this).val();
        const relevantNatureIdForItems = (selectedPurchaseItemType === '1')
            ? $form.find('#productNatureDropdown').val()
            : $form.find('#serviceNatureDropdown').val();
        $form.find('.purchase-request-item-row').each(function () {
            const $row = $(this);
            window.toggleItemDropdownsForSingleRow(selectedPurchaseItemType, $row, $form);
            window.populateItemDropdowns($row, $form, selectedPurchaseItemType, relevantNatureIdForItems);
        });
        window.updatePurchaseRequestTotal($form);
    });

    $form.find('#productNatureDropdown').off('change').on('change', function () {
        const selectedProductNatureId = $(this).val();
        const selectedPurchaseItemType = $form.find('.purchase-item-type-radio:checked').val();
        $form.find('.purchase-request-item-row').each(function () {
            const $row = $(this);
            if (selectedPurchaseItemType === '1') {
                $row.find('.product-dropdown').val('');
                window.clearAllItemRowFields($row, $form);
                window.populateItemDropdowns($row, $form, selectedPurchaseItemType, selectedProductNatureId);
            }
        });
        window.updatePurchaseRequestTotal($form);
    });

    $form.find('#serviceNatureDropdown').off('change').on('change', function () {
        const selectedServiceNatureId = $(this).val();
        const selectedPurchaseItemType = $form.find('.purchase-item-type-radio:checked').val();
        $form.find('.purchase-request-item-row').each(function () {
            const $row = $(this);
            if (selectedPurchaseItemType === '2') {
                $row.find('.service-dropdown').val('');
                window.clearAllItemRowFields($row, $form);
                window.populateItemDropdowns($row, $form, selectedPurchaseItemType, selectedServiceNatureId);
            }
        });
        window.updatePurchaseRequestTotal($form);
    });

    // Add/Remove Detail Rows
    $form.find('#addDetailRowBtn').off('click').on('click', function () {
        const selectedPurchaseNatureType = $form.find('.purchase-nature-type-radio:checked').val();
        const selectedPurchaseItemType = $form.find('.purchase-item-type-radio:checked').val();
        const selectedProductNatureId = $form.find('#productNatureDropdown').val();
        const selectedServiceNatureId = $form.find('#serviceNatureDropdown').val();

        let isValid = true;
        let errorMessage = '';
        if (!selectedPurchaseNatureType) { errorMessage += 'Please select Purchase Nature Type (Opex/Capex).\n'; isValid = false; }
        if (!selectedPurchaseItemType) { errorMessage += 'Please select Purchase Item Type (Goods/Service).\n'; isValid = false; }
        else if (selectedPurchaseItemType === '1' && !selectedProductNatureId) { errorMessage += 'Please select Product Nature.\n'; isValid = false; }
        else if (selectedPurchaseItemType === '2' && !selectedServiceNatureId) { errorMessage += 'Please select Service Nature.\n'; isValid = false; }
        if (!isValid) {
            alert(errorMessage);
            return;
        }

        const relevantNatureIdForNewRow = (selectedPurchaseItemType === '1') ? selectedProductNatureId : selectedServiceNatureId;
        $.ajax({
            url: getUrl('Procurement', 'PurchaseRequest', 'AddPurchaseRequestDetailRow') + '?index=' + window.detailRowIndex,
            type: 'POST',
            success: function (html) {
                const $newRow = $(html).appendTo($form.find('#purchaseRequisitionDetailsContainer'));
                window.reIndexDetailRows($form);
                window.bindItemRowEvents($newRow, $form);
                window.toggleItemDropdownsForSingleRow(selectedPurchaseItemType, $newRow, $form);
                window.populateItemDropdowns($newRow, $form, selectedPurchaseItemType, relevantNatureIdForNewRow);
                window.updatePurchaseRequestTotal($form);
                if (window.detailRowIndex > 0) { window.disableMainFormSelections($form); }
            }
        });
    });

    $form.find('#purchaseRequisitionDetailsContainer').off('click', '.remove-detail-row-btn').on('click', '.remove-detail-row-btn', function () {
        $(this).closest('.purchase-request-item-row').remove();
        window.reIndexDetailRows($form);
        window.updatePurchaseRequestTotal($form);
        if (window.detailRowIndex === 0) {
            window.enableMainFormSelections($form);
            $form.find('#productNatureDropdown').val('');
            $form.find('#serviceNatureDropdown').val('');
            window.updateMainNatureDropdowns($form);
        }
    });

    // **CRITICAL FIX: Initial setup logic for both Create and Edit**
    // This block runs every time the form is initialized.
    window.updateMainNatureDropdowns($form);
    window.updateAddButtonText($form);
    window.updatePurchaseRequestTotal($form);

    // Bind events for existing rows if this is an Edit form
    $form.find('.purchase-request-item-row').each(function () {
        const $row = $(this);
        window.bindItemRowEvents($row, $form);
    });

    // **CRITICAL FIX: Disable/Enable logic on initial load**
    if (initialItemCount > 0) {
        window.disableMainFormSelections($form);
    } else {
        window.enableMainFormSelections($form);
    }

    window.reparseFormValidation($form);
};

// --- Main Document Ready Handlers ---

// This handler initializes the form on the main Create page.
$(document).ready(function () {
    const $createForm = $('#purchaseRequestForm');
    if ($createForm.length > 0) {
        // Check if there are any validation errors on the page
        const initialItemCount = $createForm.find('.purchase-request-item-row').length;

        // This is the critical part: Re-initialize the form state
        // on both initial load and postback with errors.
        window.initializePurchaseRequestForm($createForm, initialItemCount);

        console.log("Create form initialized successfully.");
    }
});

// This function will be triggered when a user clicks an edit button
// It handles fetching and displaying the edit modal.
window.openEditPurchaseRequestModal = function (purchaseRequestId) {
    const modalElement = document.getElementById('purchaseRequestEditModal');
    const modalBody = document.getElementById('purchaseRequestEditModalBody');
    const editModal = new bootstrap.Modal(modalElement);

    modalBody.innerHTML = `
        <div class="text-center py-5">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="mt-2 text-muted">Loading Purchase Request...</p>
        </div>
    `;

    editModal.show();

    $.ajax({
        url: getUrl('Procurement', 'PurchaseRequest', 'Edit', purchaseRequestId),
        type: 'GET',
        success: function (html) {
            modalBody.innerHTML = html;

            const $form = $('#purchaseRequestEditForm');
            if ($form.length > 0) {
                const initialItemCount = $form.find('.purchase-request-item-row').length;
                window.initializePurchaseRequestForm($form, initialItemCount);
                $form.off('submit', window.handlePurchaseRequestEditFormSubmit).on('submit', window.handlePurchaseRequestEditFormSubmit);
                console.log("Edit form loaded and initialized successfully.");
            }
        },
        error: function (xhr, status, error) {
            console.error("Failed to load edit form:", error);
            modalBody.innerHTML = `<div class="alert alert-danger" role="alert">
                <i class="fas fa-exclamation-triangle me-2"></i>
                Failed to load the purchase request data. Please try again.
            </div>`;
        }
    });
};

/**
 * Handles the AJAX submission for the edit form.
 * This function updates the main page content without a full reload.
 * @param {Event} event The form submission event.
 */
window.handlePurchaseRequestEditFormSubmit = async function (event) {
    event.preventDefault();
    const form = event.target;
    const $form = $(form);

    if ($form.find('#productNatureDropdown').prop('disabled')) {
        const selectedProductNatureId = $form.find('#productNatureDropdown').val();
        $form.find('#ProductNatureId_Hidden').val(selectedProductNatureId || '');
        console.log("Submitting disabled ProductNatureId:", selectedProductNatureId);
    }

    // Agar Service Nature dropdown disabled hai, toh hidden field update karein
    if ($form.find('#serviceNatureDropdown').prop('disabled')) {
        const selectedServiceNatureId = $form.find('#serviceNatureDropdown').val();
        $form.find('#ServiceNatureId_Hidden').val(selectedServiceNatureId || '');
        console.log("Submitting disabled ServiceNatureId:", selectedServiceNatureId);
    }

    // Check if validation libraries are loaded before validating
    if (typeof $.validator !== 'undefined' && typeof $.validator.unobtrusive !== 'undefined') {
        window.reparseFormValidation($form);

        if (!window.validateForm($form)) {
            alert("Please fix the validation errors before submitting.");
            $form.find('.input-validation-error:first').focus();
            return;
        }
    } else {
        console.warn('jQuery validation not available, skipping client-side validation');
    }

    const formData = new FormData(form);
    const submitBtn = $form.find('button[type="submit"]');
    const originalBtnHtml = submitBtn.html();

    submitBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Saving...');

    try {
        const response = await fetch(form.action, {
            method: form.method,
            body: formData,
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            }
        });

        // Check if response is ok before trying to parse
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();

        console.log('Server response:', result); // Debug log

        if (result.success) {
            alert(result.message || "Purchase Request updated successfully.");

            // Check if data exists before trying to use it
            if (result.data) {
                const updatedModel = result.data;

                // Safely update main details card
                if (updatedModel.requestNumber) {
                    $(`div.col-md-6 table tr:contains('Request Number') td`).text(updatedModel.requestNumber);
                }
                if (updatedModel.requestDate) {
                    $(`div.col-md-6 table tr:contains('Request Date') td`).text(new Date(updatedModel.requestDate).toLocaleDateString());
                }
                if (updatedModel.requiredDate) {
                    $(`div.col-md-6 table tr:contains('Required Date') td`).text(new Date(updatedModel.requiredDate).toLocaleDateString());
                }
                if (updatedModel.requestedByUser?.userName) {
                    $(`div.col-md-6 table tr:contains('Requested By') td`).text(updatedModel.requestedByUser.userName);
                }
                if (updatedModel.department?.departmentName) {
                    $(`div.col-md-6 table tr:contains('Department') td`).text(updatedModel.department.departmentName);
                }
                if (updatedModel.branch?.branchName) {
                    $(`div.col-md-6 table tr:contains('Branch') td`).text(updatedModel.branch.branchName);
                }
                if (updatedModel.owner) {
                    $(`div.col-md-6 table tr:contains('Current Owner') td`).text(updatedModel.owner);
                }
                if (updatedModel.currentApprovalSequence !== undefined) {
                    $(`div.col-md-6 table tr:contains('Approval Sequence') td`).text(updatedModel.currentApprovalSequence);
                }

                // Update description
                if (updatedModel.description) {
                    $(`h5:contains('Description')`).next('p').text(updatedModel.description);
                }

                // Update the items table
                const $itemsTableBody = $(`div.card-body table.table tbody`);
                $itemsTableBody.empty();
                let grandTotal = 0;

                if (updatedModel.items && updatedModel.items.length > 0) {
                    updatedModel.items.forEach(item => {
                        grandTotal += item.totalAmount || 0;
                        const newRow = `
                            <tr>
                                <td>${item.itemName || ''}</td>
                                <td>${item.narration || ''}</td>
                                <td>${item.quantity || 0}</td>
                                <td>${(item.unitPrice || 0).toLocaleString('en-US', { style: 'currency', currency: 'USD' })}</td>
                                <td>${(item.totalAmount || 0).toLocaleString('en-US', { style: 'currency', currency: 'USD' })}</td>
                            </tr>`;
                        $itemsTableBody.append(newRow);
                    });
                } else {
                    $itemsTableBody.html('<tr><td colspan="5" class="text-center text-muted">No items added to this request.</td></tr>');
                }

                // Update grand total
                $(`div.card-body table.table tfoot tr.table-info td:last-child strong`).text(
                    grandTotal.toLocaleString('en-US', { style: 'currency', currency: 'USD' })
                );
            }

            // Dismiss the modal
            const modalElement = document.getElementById('purchaseRequestEditModal');
            const modal = bootstrap.Modal.getInstance(modalElement);
            if (modal) {
                modal.hide();
            } else {
                // If modal instance not found, try to hide it directly
                $('#purchaseRequestEditModal').modal('hide');
            }

            // Optionally reload the page to ensure all data is fresh
            // setTimeout(() => location.reload(), 1000);

        } else {
            alert(result.message || "Failed to update Purchase Request.");
            if (result.errors) {
                console.error("Server-side errors:", result.errors);
            }
        }
    } catch (error) {
        console.error("AJAX submission failed:", error);
        alert("An unexpected error occurred during submission: " + error.message);
    } finally {
        submitBtn.prop('disabled', false).html(originalBtnHtml);
    }
};
