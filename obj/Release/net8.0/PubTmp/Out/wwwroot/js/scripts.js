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
