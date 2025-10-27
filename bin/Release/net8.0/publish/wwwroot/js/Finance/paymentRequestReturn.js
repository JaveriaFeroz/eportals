// paymentRequestReturn.js
console.log('Payment Request return functionality JavaScript loaded');

(function ($) {
    'use strict';

    let returnToSpecificUserModal;

    $(document).ready(function () {
        console.log('✅ Payment Request return functionality initialization complete');

        returnToSpecificUserModal = $('#returnToSpecificUserModal');

        // Event listener for the "Return" button on the Details page
        // It now looks for a data attribute named 'payment-request-id'
        $(document).on('click', '#returnButton, .btn-return', function (e) {
            e.preventDefault();
            console.log('Return button clicked');
            const paymentRequestId = $(this).data('payment-request-id');

            if (!paymentRequestId) {
                console.error('No Payment Request ID found on button.');
                showError('Could not find the ID of the Payment Request.');
                return;
            }

            showReturnModalAndLoadUsers(paymentRequestId);
        });

        // Event listener for user selection change inside the modal
        $(document).on('change', '#returnToUserId', function () {
            const selectedValue = $(this).val();
            const selectedText = $(this).find('option:selected').text();

            if (selectedValue) {
                $('#selectedUserPreview').html(`<strong>${selectedText}</strong>`);
                $('#confirmReturnButton').prop('disabled', false);
            } else {
                $('#selectedUserPreview').html('<em>Select a user above</em>');
                $('#confirmReturnButton').prop('disabled', true);
            }
        });

        // Event listener for comments character count
        $(document).on('input', '#returnComments', function () {
            const commentsField = $(this);
            const currentLength = commentsField.val().length;
            const maxLength = 1000;
            const charCountElement = $('#commentCharCount');

            charCountElement.text(currentLength);

            if (currentLength > maxLength) {
                commentsField.addClass('is-invalid');
                charCountElement.addClass('text-danger');
            } else {
                commentsField.removeClass('is-invalid');
                charCountElement.removeClass('text-danger');
            }
        });

        // Event listener for form submission validation
        $(document).on('submit', '#returnForm', function (e) {
            const userId = $('#returnToUserId').val();
            const comments = $('#returnComments').val().trim();
            const commentsLength = comments.length;
            const maxLength = 1000;

            if (!userId) {
                e.preventDefault();
                showError('Please select a user to return the request to.');
                return false;
            }

            if (!comments) {
                e.preventDefault();
                showError('Please provide comments for the return.');
                return false;
            }

            if (commentsLength > maxLength) {
                e.preventDefault();
                showError('Comments cannot exceed 1000 characters.');
                return false;
            }

            return true;
        });

        // Reset modal state when it's hidden
        returnToSpecificUserModal.on('hidden.bs.modal', function () {
            resetModalState();
        });
    });

    /**
     * Shows the return modal and handles the entire process of loading users via AJAX.
     * @param {number} paymentRequestId The ID of the payment request.
     */
    // Corrected JavaScript function
    function showReturnModalAndLoadUsers(paymentRequestId) {
        console.log('🎯 Showing return modal for PRQ ID:', paymentRequestId);

        clearMessages();
        resetModalState();
        returnToSpecificUserModal.modal('show');
        showLoadingState(); // Show the spinner and hide the form section

        const url = '/Finance/PaymentRequest/GetPreviousWorkflowUsers';
        const requestData = { paymentRequestId: paymentRequestId };

        console.log('🌐 Making AJAX request to:', url);
        console.log('📝 With parameters:', requestData);

        $.ajax({
            url: url,
            type: 'GET',
            data: requestData,
            dataType: 'json',
            timeout: 30000,
            success: function (data) {
                console.log('✅ AJAX Success:', data);
                if (data && data.success) {
                    populateUserDropdown(data.users);
                    // CORRECTED: Set the value of the hidden input with ID 'Id'
                    $('#Id').val(paymentRequestId);
                } else {
                    showError(data?.message || 'Failed to load previous users');
                }
            },
            error: function (xhr, status, error) {
                console.error('❌ AJAX Error Details:', {
                    status: status,
                    error: error,
                    responseStatus: xhr.status,
                    responseText: xhr.responseText
                });

                let errorMessage = 'An error occurred while loading previous users';
                try {
                    if (xhr.responseText) {
                        const errorResponse = JSON.parse(xhr.responseText);
                        errorMessage = errorResponse.message || errorMessage;
                    }
                } catch (parseError) {
                    console.warn('Could not parse error response:', parseError);
                    errorMessage = `Server Error (${xhr.status}): ${error || xhr.statusText}`;
                }
                showError(errorMessage);
            },
            complete: function () {
                hideLoadingState(); // Hide the spinner and show the form section
            }
        });
    }

    /**
     * Populates the user dropdown list with data from the server.
     * @param {Array<object>} users An array of user objects to populate the dropdown.
     */
    function populateUserDropdown(users) {
        console.log('🔄 Populating dropdown with users:', users);
        const dropdown = $('#returnToUserId');
        dropdown.empty();
        dropdown.append('<option value="">Select a user...</option>');

        if (!users || users.length === 0) {
            dropdown.append('<option value="" disabled>No previous users found</option>');
            dropdown.prop('disabled', true);
            console.warn('⚠️ No users to populate in dropdown');
            return;
        }

        const groupedUsers = {};
        users.forEach(user => {
            const groupName = user.group?.name || 'Other';
            if (!groupedUsers[groupName]) {
                groupedUsers[groupName] = [];
            }
            groupedUsers[groupName].push(user);
        });

        Object.keys(groupedUsers).forEach(groupName => {
            const optgroup = $(`<optgroup label="${groupName}"></optgroup>`);
            groupedUsers[groupName].forEach(user => {
                optgroup.append(`<option value="${user.value}">${user.text}</option>`);
            });
            dropdown.append(optgroup);
        });

        dropdown.prop('disabled', false);
        console.log('✅ Dropdown populated successfully with', users.length, 'users');
    }

    function showLoadingState() {
        console.log('Showing loading state');
        $('#loadingSpinner').show();
        $('#returnUserSection').hide();
        $('#confirmReturnButton').prop('disabled', true);
        $('#returnComments').prop('disabled', true);
        $('#returnToUserId').prop('disabled', true);
    }

    function hideLoadingState() {
        console.log('Hiding loading state');
        $('#loadingSpinner').hide();
        $('#returnUserSection').show();
        $('#returnToUserId').prop('disabled', false);
        $('#returnComments').prop('disabled', false);
    }

    function showError(message) {
        console.error('Showing error:', message);
        const errorDiv = $('#returnToSpecificUserModalError');
        errorDiv.find('.alert-text').text(message);
        errorDiv.removeClass('d-none');
        setTimeout(() => {
            errorDiv.addClass('d-none');
        }, 10000);
    }

    function clearMessages() {
        $('#returnToSpecificUserModalError').addClass('d-none');
    }

    function resetModalState() {
        $('#returnToUserId').empty();
        $('#returnComments').val('');
        $('#commentCharCount').text('0');
        $('#selectedUserPreview').html('<em>Select a user above</em>');

        $('#returnToUserId').prop('disabled', true);
        $('#returnComments').prop('disabled', true);
        $('#confirmReturnButton').prop('disabled', true);

        hideLoadingState();
        clearMessages();
    }
})(jQuery);