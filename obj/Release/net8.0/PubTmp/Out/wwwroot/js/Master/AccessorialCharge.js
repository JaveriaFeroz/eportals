// Master // AccessorialCharge Script (Final Working Version)
document.addEventListener('DOMContentLoaded', function () {
    // --- SETUP ---
    const filterInputs = document.querySelectorAll('[data-filter]');
    const dataRows = Array.from(document.querySelectorAll('.user-row'));
    const clearButton = document.querySelector('.clear-button');

    // --- FILTER FUNCTION ---
    function filterData() {
        const filters = Array.from(filterInputs).map(input => ({
            key: input.getAttribute('data-filter'),
            value: (input.value || '').trim().toLowerCase()
        })).filter(f => f.value);

        dataRows.forEach(row => {
            let isVisible = true;

            // Check if the row matches all active filters
            for (const filter of filters) {
                const cell = row.querySelector(`[data-column="${filter.key}"]`);
                const cellValue = cell?.textContent.trim().toLowerCase() || '';

                let rowMatchesFilter = false;

                switch (filter.key) {
                    case 'accessorialName':
                    case 'accessorialCode':
                        if (cellValue.includes(filter.value)) {
                            rowMatchesFilter = true;
                        }
                        break;
                    case 'status':
                        if (cellValue === filter.value) {
                            rowMatchesFilter = true;
                        }
                        break;
                }

                if (!rowMatchesFilter) {
                    isVisible = false;
                    break;
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
            filterInputs.forEach(input => {
                input.value = ''; // Clears text inputs
                if (input.tagName === 'SELECT') {
                    input.selectedIndex = 0; // Resets dropdowns to the first option (e.g., "All")
                }
            });
            filterData(); // Re-run the filter to display all rows after clearing inputs
        });
    }

    filterData();
});
