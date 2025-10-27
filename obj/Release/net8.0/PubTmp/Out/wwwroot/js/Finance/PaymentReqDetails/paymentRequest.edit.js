    $(document).ready(function () {
        // Helper function to build URLs
        function getUrl(area, controller, action, id) {
            return `/${area}/${controller}/${action}${id ? '/' + id : ''}`;
        }

        const paymentRequestId = @Model.Id;

    // --- Function to handle form submission via AJAX ---
    // This function is generic and can be used for both edit forms.
    async function handlePaymentRequestEditFormSubmit(event) {
        event.preventDefault();

    const form = event.target;
    const modalElement = form.closest('.modal');
    const modalBodyElement = modalElement.querySelector('.modal-body');

    if (!modalBodyElement) {
        console.error('Modal body element not found for form submission.');
    return;
            }

    // Check for client-side validation errors
    if (window.jQuery && window.jQuery.validator && !$(form).valid()) {
                if (typeof toastr !== 'undefined') {
        toastr.error("Please correct the validation errors.");
                }
    return;
            }

    // Show a loading state
    const originalContent = modalBodyElement.innerHTML;
    modalBodyElement.innerHTML = '<div class="text-center py-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div> Submitting...</div>';

    try {
                const response = await fetch(form.action, {
        method: form.method,
    body: new FormData(form),
                });

    const contentType = response.headers.get("content-type");

    if (response.ok) {
                    // A successful response (200-299)
                    if (contentType && contentType.includes("application/json")) {
                        const jsonResponse = await response.json();
    if (jsonResponse.success) {
                            const modalInstance = bootstrap.Modal.getInstance(modalElement);
    if (modalInstance) {
        modalInstance.hide();
                            }
    if (typeof toastr !== 'undefined') {
        toastr.success(jsonResponse.message || "Payment request updated successfully!");
                            }
    if (jsonResponse.redirectUrl) {
        window.location.href = jsonResponse.redirectUrl;
                            } else {
        window.location.reload();
                            }
                        } else {
                            if (typeof toastr !== 'undefined') {
        toastr.error(jsonResponse.message || "An error occurred.");
                            }
    // Revert to the form, but with server-side validation messages
    modalBodyElement.innerHTML = await response.text();
    window.attachDynamicFormHandlers(modalBodyElement);
                        }
                    } else {
        // A non-JSON response from a successful action (e.g., a full view)
        window.location.reload();
                    }
                } else {
                    // A non-ok response (e.g., 400, 403, 500)
                    const errorMessage = await response.text();
    modalBodyElement.innerHTML = `<div class="alert alert-danger" role="alert"><i class="fas fa-exclamation-triangle me-2"></i> ${errorMessage}</div>`;
    if (typeof toastr !== 'undefined') {
        toastr.error(errorMessage || "Server error.");
                    }
                }
            } catch (error) {
        console.error("AJAX submission failed:", error);
    modalBodyElement.innerHTML = originalContent;
    if (typeof toastr !== 'undefined') {
        toastr.error("An unexpected error occurred during submission.");
                }
            }
        }

    // --- SCRIPT FOR THE GRN-BASED EDIT MODAL ---
    $('#paymentRequestEditGRNBasedModal').on('show.bs.modal', async function (event) {
            const button = event.relatedTarget;
    const prqId = button.getAttribute('data-id');
    const modalBody = this.querySelector('.modal-body');
    const url = getUrl('Finance', 'PaymentRequest', 'EditGRNBased', prqId);

    if (modalBody) {
        modalBody.innerHTML = '<div class="text-center py-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div> Loading...</div>';
            }

    try {
                const response = await fetch(url, {
        headers: {'X-Requested-With': 'XMLHttpRequest' }
                });

    if (response.ok) {
                    const html = await response.text();
    if (modalBody) {
        modalBody.innerHTML = html;
    // Attach the submission handler to the newly loaded form
    const form = modalBody.querySelector('form');
    if (form) {
        form.addEventListener('submit', handlePaymentRequestEditFormSubmit);
                        }
    window.attachDynamicFormHandlers(modalBody);
                    }
                } else {
        modalBody.innerHTML = `<div class="alert alert-danger" role="alert"><i class="fas fa-exclamation-triangle me-2"></i> Error loading form.</div>`;
                }
            } catch (error) {
        console.error("AJAX load failed:", error);
    modalBody.innerHTML = `<div class="alert alert-danger" role="alert"><i class="fas fa-exclamation-triangle me-2"></i> An unexpected error occurred.</div>`;
            }
        });

    // --- SCRIPT FOR THE DIRECT-BASED EDIT MODAL ---
    $('#paymentRequestEditDirectModal').on('show.bs.modal', async function (event) {
            const button = event.relatedTarget;
    const prqId = button.getAttribute('data-id');
    const modalBody = this.querySelector('.modal-body');
    const url = getUrl('Finance', 'PaymentRequest', 'EditDirect', prqId);

    if (modalBody) {
        modalBody.innerHTML = '<div class="text-center py-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div> Loading...</div>';
            }

    try {
                const response = await fetch(url, {
        headers: {'X-Requested-With': 'XMLHttpRequest' }
                });

    if (response.ok) {
                    const html = await response.text();
    if (modalBody) {
        modalBody.innerHTML = html;
    // Attach the submission handler to the newly loaded form
    const form = modalBody.querySelector('form');
    if (form) {
        form.addEventListener('submit', handlePaymentRequestEditFormSubmit);
                        }
    window.attachDynamicFormHandlers(modalBody);
                    }
                } else {
        modalBody.innerHTML = `<div class="alert alert-danger" role="alert"><i class="fas fa-exclamation-triangle me-2"></i> Error loading form.</div>`;
                }
            } catch (error) {
        console.error("AJAX load failed:", error);
    modalBody.innerHTML = `<div class="alert alert-danger" role="alert"><i class="fas fa-exclamation-triangle me-2"></i> An unexpected error occurred.</div>`;
            }
        });
    });
