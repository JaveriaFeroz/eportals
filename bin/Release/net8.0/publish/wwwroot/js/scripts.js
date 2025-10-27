 // Branch/Dept Index Script
document.addEventListener('DOMContentLoaded', function () {
    const codeInput = document.getElementById('codeSearch');
    const nameInput = document.getElementById('nameSearch');
    const statusSelect = document.getElementById('statusSearch');
    const dataRows = Array.from(document.querySelectorAll('.card-body > div[tabindex="0"]'));

    // Data filtering function - Dynamically adjust based on available filters
    function filterData() {
        const codeQuery = (codeInput?.value || '').trim().toLowerCase();
        const nameQuery = (nameInput?.value || '').trim().toLowerCase();
        const statusQuery = statusSelect ? statusSelect.value : '';

        dataRows.forEach(row => {
            const code = row.querySelector('.col-2.fw-bold')?.textContent.trim().toLowerCase() || '';
            const name = row.querySelector('.col-3.text-start')?.textContent.trim().toLowerCase() || '';
            const statusElement = row.querySelector('.col-1.text-center > span.badge');
            const statusText = statusElement ? statusElement.textContent.trim().toLowerCase() : '';
            const isActive = statusText === 'active';

            const codeMatch = codeQuery === '' || code.includes(codeQuery);
            const nameMatch = nameQuery === '' || name.includes(nameQuery);
            const statusMatch = statusQuery === '' || (statusQuery === 'true' && isActive) || (statusQuery === 'false' && !isActive);

            // Adjust visibility based on all active filters
            row.style.display = (codeMatch && nameMatch && statusMatch) ? '' : 'none';
        });
    }

    // Add event listeners for filtering
    if (codeInput) codeInput.addEventListener('input', filterData);
    if (nameInput) nameInput.addEventListener('input', filterData);
    if (statusSelect) statusSelect.addEventListener('change', filterData);

    // Clear input fields and apply filters
    const clearButton = document.querySelector('.clear-button');
    if (clearButton) {
        clearButton.addEventListener('click', () => {
            if (codeInput) codeInput.value = '';
            if (nameInput) nameInput.value = '';
            if (statusSelect) statusSelect.value = '';
            filterData();
        });
    }

    // Initial filter when DOM is loaded (in case fields are prefilled)
    filterData();
});

// user index script.js
document.addEventListener('DOMContentLoaded', function () {
    const filterInputs = document.querySelectorAll('[data-filter]');  // Select all inputs with data-filter attribute
    const dataRows = Array.from(document.querySelectorAll('.user-row'));
    const clearButton = document.querySelector('.clear-button');  // Select the clear button

    // Filter the data rows based on input values
    function filterData() {
        dataRows.forEach(row => {
            let isVisible = true;

            filterInputs.forEach(input => {
                const key = input.getAttribute('data-filter');  // e.g., "username", "status"
                const value = (input.value || '').trim().toLowerCase();  // Get the trimmed and lowercase value of input field
                if (!value) return;  // Skip if no value in input

                let fieldValue = '';

                switch (key) {
                    case 'username':
                        fieldValue = row.querySelector('.col-2.fw-bold')?.textContent.trim().toLowerCase() || '';
                        if (!fieldValue.includes(value)) isVisible = false;
                        break;
                    case 'email':
                        const emailCol = row.querySelectorAll('.col-2')[2]; // username is first .col-2, email is second
                        fieldValue = emailCol?.textContent.trim().toLowerCase() || '';
                        if (!fieldValue.includes(value)) isVisible = false;
                        break;
                    case 'status':
                        const badge = row.querySelector('.col-1.text-center .badge');
                        const isActive = badge?.textContent.trim().toLowerCase() === 'active';
                        if (
                            (value === 'true' && !isActive) ||
                            (value === 'false' && isActive)
                        ) isVisible = false;
                        break;
                    // Add more cases if needed
                }
            });

            // Show or hide the row based on visibility
            row.style.display = isVisible ? '' : 'none';
        });
    }

    // Bind input events to trigger filtering dynamically
    filterInputs.forEach(input => {
        const eventType = input.tagName === 'SELECT' ? 'change' : 'input';  // 'change' for selects, 'input' for text fields
        input.addEventListener(eventType, filterData);
    });

    // Clear button functionality
    if (clearButton) {
        clearButton.addEventListener('click', () => {
            // Reset all filter inputs
            filterInputs.forEach(input => (input.value = ''));
            // Trigger filtering again with no filters (i.e., show all rows)
            filterData();
        });
    }

    // Run the filter initially in case any filters are already applied
    filterData();
});

// Role Index script
document.addEventListener('DOMContentLoaded', function () {
    const filterInputs = document.querySelectorAll('[data-filter]');  // Select all inputs with data-filter attribute
    const dataRows = Array.from(document.querySelectorAll('.role-row'));
    const clearButton = document.querySelector('.clear-button');  // Select the clear button

    // Filter the data rows based on input values
    function filterData() {
        dataRows.forEach(row => {
            let isVisible = true;

            filterInputs.forEach(input => {
                const key = input.getAttribute('data-filter');  // e.g., "username", "status"
                const value = (input.value || '').trim().toLowerCase();  // Get the trimmed and lowercase value of input field
                if (!value) return;  // Skip if no value in input

                let fieldValue = '';

                switch (key) {
                    case 'rolename':
                        fieldValue = row.querySelector('.col-3.fw-bold')?.textContent.trim().toLowerCase() || '';
                        if (!fieldValue.includes(value)) isVisible = false;
                        break;
                    case 'status':
                        const badge = row.querySelector('.col-1.text-center .badge');
                        const isActive = badge?.textContent.trim().toLowerCase() === 'active';
                        if (
                            (value === 'true' && !isActive) ||
                            (value === 'false' && isActive)
                        ) isVisible = false;
                        break;
                }
            });

            // Show or hide the row based on visibility
            row.style.display = isVisible ? '' : 'none';
        });
    }

    // Bind input events to trigger filtering dynamically
    filterInputs.forEach(input => {
        const eventType = input.tagName === 'SELECT' ? 'change' : 'input';  // 'change' for selects, 'input' for text fields
        input.addEventListener(eventType, filterData);
    });

    // Clear button functionality
    if (clearButton) {
        clearButton.addEventListener('click', () => {
            // Reset all filter inputs
            filterInputs.forEach(input => (input.value = ''));
            // Trigger filtering again with no filters (i.e., show all rows)
            filterData();
        });
    }

    // Run the filter initially in case any filters are already applied
    filterData();
});


// createIcon.js
document.addEventListener('DOMContentLoaded', function () {
    const createUserIcons = document.querySelectorAll('.create-user-icon');
    const createTextButtons = document.querySelectorAll('.create-user-text');

    // Toggle visibility for icon-text button pairs
    createUserIcons.forEach((icon, index) => {
        const textButton = createTextButtons[index];
        textButton.style.display = 'none';

        icon.addEventListener('click', () => {
            const iconElement = icon.querySelector('i');
            const isOpening = iconElement.classList.contains('fa-plus');

            // Reset all buttons
            createUserIcons.forEach((icn, idx) => {
                icn.querySelector('i').classList.remove('fa-times');
                icn.querySelector('i').classList.add('fa-plus');
                createTextButtons[idx].style.display = 'none';
            });

            if (isOpening) {
                iconElement.classList.remove('fa-plus');
                iconElement.classList.add('fa-times');
                textButton.style.display = 'inline-flex';
            }
        });

        textButton.addEventListener('click', () => {
            const url = textButton.getAttribute('data-url');
            window.location.href = url;
        });

        // Hover effects
        icon.addEventListener('mouseover', () => {
            icon.style.opacity = '0.7';
        });
        icon.addEventListener('mouseout', () => {
            icon.style.opacity = '1';
        });

        textButton.addEventListener('mouseover', () => {
            textButton.style.backgroundColor = '#071e49';
        });
        textButton.addEventListener('mouseout', () => {
            textButton.style.backgroundColor = '#092963';
        });
    });
});
