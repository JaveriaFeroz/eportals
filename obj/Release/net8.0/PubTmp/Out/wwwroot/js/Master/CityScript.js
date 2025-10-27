// Master // City Script// Master // City Script (Final Working Version)
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
                // This line now correctly finds the div using the 'data-column' attribute
                const cell = row.querySelector(`[data-column="${filter.key}"]`);
                const cellValue = cell?.textContent.trim().toLowerCase() || '';

                let rowMatchesFilter = false;

                // This logic handles all filters correctly
                switch (filter.key) {
                    case 'cityName':
                    case 'cityCode':
                    case 'region':
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
                input.value = '';
                if (input.tagName === 'SELECT') {
                    input.selectedIndex = 0;
                }
            });
            filterData();
        });
    }
});

