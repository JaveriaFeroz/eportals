// File: wwwroot/js/purchase-request-edit.js

// Function to attach the form submission handler
function attachPurchaseRequestEditFormHandler() {
    const editForm = document.getElementById('purchaseRequestEditForm');

    if (editForm) {
        editForm.addEventListener('submit', function (event) {
            event.preventDefault();

            const form = event.target;
            const formData = new FormData(form);

            fetch(form.action, {
                method: form.method,
                body: formData,
                headers: {
                    'RequestVerificationToken': document.getElementsByName('__RequestVerificationToken')[0].value
                }
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok.');
                    }
                    return response.json();
                })
                .then(data => {
                    if (data.success) {
                        toastr.success(data.message);
                        const modalElement = document.getElementById('purchaseRequestEditModal');
                        const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
                        modal.hide();
                        setTimeout(() => {
                            window.location.href = data.redirectUrl;
                        }, 300);
                    } else {
                        toastr.error(data.message);
                        document.querySelectorAll('.text-danger').forEach(span => {
                            span.textContent = '';
                        });
                        if (data.errors) {
                            for (const key in data.errors) {
                                const errorMessages = data.errors[key];
                                const validationSpan = document.querySelector(`span[data-valmsg-for="${key}"]`);
                                if (validationSpan) {
                                    validationSpan.textContent = errorMessages.join(' ');
                                }
                            }
                        }
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    toastr.error("An unexpected error occurred while saving changes.");
                });
        });
    }
}