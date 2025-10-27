// Master // Asset Script (Final Working Version)
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

            for (const filter of filters) {
                let cellValue = '';
                const cell = row.querySelector(`[data-column="${filter.key}"]`);

                // Determine cellValue based on the filter key for specific cases
                if (filter.key === 'status') {
                    // For status, read from the data-raw-status attribute
                    cellValue = cell?.getAttribute('data-raw-status') || '';
                } else {
                    // For other filters, read from textContent
                    cellValue = cell?.textContent.trim().toLowerCase() || '';
                }

                let rowMatchesFilter = false;

                // This logic handles all filters correctly
                switch (filter.key) {
                    case 'assetNumber':
                    case 'assettypes':
                        if (cellValue.includes(filter.value)) {
                            rowMatchesFilter = true;
                        }
                        break;
                    case 'status':
                        // Now we are comparing 'active'/'inactive' from the dropdown
                        // with 'active'/'inactive' from data-raw-status attribute
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
                input.value = '';
                if (input.tagName === 'SELECT') {
                    input.selectedIndex = 0;
                }
            });
            filterData();
        });
    }

    // Initial filter execution to ensure correct display on page load
    filterData();
});