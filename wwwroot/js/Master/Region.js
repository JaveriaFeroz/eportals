// Master // Region Script
document.addEventListener('DOMContentLoaded', function () {
    // --- SETUP ---
    const filterInputs = document.querySelectorAll('[data-filter]');
    const dataRows = Array.from(document.querySelectorAll('.user-row')); // FIX: Changed from .role-row to .user-row
    const clearButton = document.querySelector('.clear-button');

    // --- FILTER FUNCTION ---
    function filterData() {
        const filters = Array.from(filterInputs).map(input => ({
            key: input.getAttribute('data-filter'),
            value: (input.value || '').trim().toLowerCase()
        })).filter(f => f.value); // Only keep filters that have a value

        dataRows.forEach(row => {
            let isVisible = true;

            // Check if the row matches ALL active filters
            for (const filter of filters) {
                let fieldValue = '';
                let rowMatchesFilter = false;

                switch (filter.key) {
                    case 'regionname':
                        fieldValue = row.querySelector('.col-2.fw-bold')?.textContent.trim().toLowerCase() || '';
                        if (fieldValue.includes(filter.value)) {
                            rowMatchesFilter = true;
                        }
                        break;

                    case 'statusSearch':
                        const badge = row.querySelector('.col-1.text-center .badge');
                        const isActive = badge?.textContent.trim().toLowerCase() === 'active';
                        if (
                            (value === 'true' && !isActive) ||
                            (value === 'false' && isActive)
                        ) isVisible = false;
                        break;
                }

                if (!rowMatchesFilter) {
                    isVisible = false;
                    break; // Exit the loop early if one filter doesn't match
                }
            }

            row.style.display = isVisible ? '' : 'none';
        });
    }

    // --- EVENT BINDING ---
    filterInputs.forEach(input => {
        const eventType = input.tagName === 'SELECT' ? 'change' : 'input';
        input.addEventListener(eventType, filterData);
    });

    if (clearButton) {
        clearButton.addEventListener('click', () => {
            filterInputs.forEach(input => (input.value = ''));
            filterData();
        });
    }

    // Initial filter run on page load
    filterData();
});




