    $(document).ready(function () {
        const paymentRequestId = @Model.Id;
    const currentApprovalSeq = @(Model.CurrentApprovalSequence);

    // --- SCRIPT FOR THE SUBMIT MODAL ---
    const submitModal = document.getElementById('submitModal');
    const submitToUserIdSelect = document.getElementById('submitToUserId');
    const submitButton = document.getElementById('submitButton');

    if (submitModal) {
        submitModal.addEventListener('show.bs.modal', function () {
            submitToUserIdSelect.innerHTML = '<option value="">Loading users...</option>';
            submitToUserIdSelect.disabled = true;
            submitButton.disabled = true;

            const nextApprovalSeqForSubmit = 1;
            fetch(`/Finance/PaymentRequest/GetApprovers?paymentRequestId=${paymentRequestId}&approvalSeq=${nextApprovalSeqForSubmit}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }
                    return response.json();
                })
                .then(data => {
                    if (data.success) {
                        submitToUserIdSelect.innerHTML = '';
                        const defaultOption = document.createElement('option');
                        defaultOption.value = '';
                        defaultOption.textContent = '-- Select an Approver --';
                        submitToUserIdSelect.appendChild(defaultOption);
                        data.users.forEach(user => {
                            const option = document.createElement('option');
                            option.value = user.value;
                            option.textContent = user.text;
                            submitToUserIdSelect.appendChild(option);
                        });
                        submitToUserIdSelect.disabled = false;
                        submitButton.disabled = false;
                    } else {
                        submitToUserIdSelect.innerHTML = `<option value="">${data.message}</option>`;
                        submitToUserIdSelect.disabled = true;
                        submitButton.disabled = true;
                        if (typeof toastr !== 'undefined') {
                            toastr.error(data.message || 'No approvers found for this step.');
                        }
                    }
                })
                .catch(error => {
                    console.error('Error fetching users:', error);
                    submitToUserIdSelect.innerHTML = '<option value="">Error loading users</option>';
                    submitToUserIdSelect.disabled = true;
                    submitButton.disabled = true;
                    if (typeof toastr !== 'undefined') {
                        toastr.error('An unexpected error occurred while loading approvers.');
                    }
                });
        });
        }

    // --- SCRIPT FOR THE APPROVE MODAL ---
    const approveModal = document.getElementById('approveModal');
    const approveToUserIdSelect = document.getElementById('approveToUserId');
    const approveButton = document.getElementById('approveButton');
    const nextApproverSelectionDiv = document.getElementById('approverSelection-approve');

    if (approveModal) {
        approveModal.addEventListener('show.bs.modal', function () {
            approveToUserIdSelect.innerHTML = '<option value="">Loading next approvers...</option>';
            approveToUserIdSelect.disabled = true;
            approveButton.disabled = true;

            const nextApprovalSeqForApprove = currentApprovalSeq + 1;
            fetch(`/Finance/PaymentRequest/GetApprovers?paymentRequestId=${paymentRequestId}&approvalSeq=${nextApprovalSeqForApprove}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }
                    return response.json();
                })
                .then(data => {
                    if (data.success && data.users.length > 0) {
                        approveToUserIdSelect.innerHTML = '';
                        const defaultOption = document.createElement('option');
                        defaultOption.value = '';
                        defaultOption.textContent = '-- Select Next Approver --';
                        approveToUserIdSelect.appendChild(defaultOption);

                        data.users.forEach(user => {
                            const option = document.createElement('option');
                            option.value = user.value;
                            option.textContent = user.text;
                            approveToUserIdSelect.appendChild(option);
                        });
                        approveToUserIdSelect.disabled = false;
                        approveButton.disabled = false;
                        nextApproverSelectionDiv.style.display = 'block';
                        approveToUserIdSelect.setAttribute('required', 'required');
                    } else {
                        nextApproverSelectionDiv.style.display = 'none';
                        approveToUserIdSelect.disabled = true;
                        approveToUserIdSelect.removeAttribute('required');
                        approveButton.disabled = false;
                        if (typeof toastr !== 'undefined') {
                            toastr.info('No further approvers found. This will be the final approval.');
                        }
                    }
                })
                .catch(error => {
                    console.error('Error fetching next approvers:', error);
                    approveToUserIdSelect.innerHTML = '<option value="">Error loading approvers</option>';
                    approveToUserIdSelect.disabled = true;
                    approveButton.disabled = true;
                    if (typeof toastr !== 'undefined') {
                        toastr.error('An unexpected error occurred while loading approvers.');
                    }
                });
        });
        }

    // --- SCRIPT FOR THE PIV MODAL ---
    document.addEventListener('DOMContentLoaded', function () {
            var pivModal = document.getElementById('pivModal');
    if (pivModal) {
        pivModal.addEventListener('show.bs.modal', function (event) {
            var button = event.relatedTarget;
            var pivNumber = button.getAttribute('data-pivno');
            var pivInput = pivModal.querySelector('#PIVNo');
            if (pivInput) {
                pivInput.value = pivNumber || '';
            }
        });
            }
        });

    // --- SCRIPT FOR THE CS MODAL ---
    document.addEventListener('DOMContentLoaded', function () {
            var csModal = document.getElementById('csModal');
    if (csModal) {
        csModal.addEventListener('show.bs.modal', function (event) {
            var button = event.relatedTarget;
            var csNumber = button.getAttribute('data-csno');
            var csInput = csModal.querySelector('#CSNo');
            if (csInput) {
                csInput.value = csNumber || '';
            }
        });
            }
        });
    });
