// Procurement Category Index script
//document.addEventListener('DOMContentLoaded', function () {

//    const filterInputs = document.querySelectorAll('[data-filter]');
//    const dataRows = Array.from(document.querySelectorAll('.procurementcategory-row'));
//    const clearButton = document.querySelector('.clear-button');

//    function filterData() {
//        dataRows.forEach(row => {
//            let isVisible = true;

//            filterInputs.forEach(input => {
//                const key = input.getAttribute('data-filter');
//                const value = (input.value || '').trim().toLowerCase();
//                if (!value) return;

//                let fieldValue = '';

//                switch (key) {
//                    case 'procurementcategoryname':
//                        fieldValue = row.querySelector('.col-2.fw-bold')?.textContent.trim().toLowerCase() || '';
//                        if (!fieldValue.includes(value)) isVisible = false;
//                        break;
//                    case 'parentcategory':
//                        fieldValue = row.querySelector('.col-2.text-start')?.textContent.trim().toLowerCase() || '';
//                        if (!fieldValue.includes(value)) isVisible = false;
//                        break;
//                    case 'procurementstatus':
//                        const badge = row.querySelector('.col-2.text-center > .badge');
//                        if (!badge) {
//                            isVisible = false;
//                            return;
//                        }

//                    case 'adjustmentnumber':
//                        fieldValue = row.querySelector('.col-3.fw-bold')?.textContent.trim().toLowerCase() || '';
//                        if (!fieldValue.includes(value)) isVisible = false;
//                        break;
//                    case 'adjustmentreason':
//                        fieldValue = row.querySelector('.col-3.text-start')?.textContent.trim().toLowerCase() || '';
//                        if (!fieldValue.includes(value)) isVisible = false;
//                        break;
//                    case 'adjustmentstatus':
//                        const badge = row.querySelector('.col-2.text-center > .badge');
//                        if (!badge) {
//                            isVisible = false;
//                            return;
//                        }

//                        const statusText = badge.textContent.trim().toLowerCase();

//                        // Match the value to approved/pending correctly
//                        const isApproved = statusText === 'approved';

//                        if (
//                            (value === 'true' && !isApproved) ||
//                            (value === 'false' && isApproved)
//                        ) {
//                            isVisible = false;
//                        }
//                        break;
//                    default:
//                        const statusText = badge.textContent.trim().toLowerCase();

//                        // Match the value to active/inactive correctly
//                        const isActive = statusText === 'active';

//                        if (
//                            (value === 'true' && !isActive) ||
//                            (value === 'false' && isActive)
//                        ) {
//                            isVisible = false;
//                        }
//                        break;
//                    default:
//                }
//            });

//            // Set row visibility
//            row.style.display = isVisible ? '' : 'none';
//        });
//    }

//    // Event listeners for inputs
//    filterInputs.forEach(input => {
//        const eventType = input.tagName === 'SELECT' ? 'change' : 'input';
//        input.addEventListener(eventType, filterData);
//    });

//    // Clear button to reset filters
//    if (clearButton) {
//        clearButton.addEventListener('click', () => {
//            filterInputs.forEach(input => (input.value = ''));
//            filterData();
//        });
//    }

//    // Initial filter to display all data
//    filterData();
//});



//    // Event listeners for inputs
//    filterInputs.forEach(input => {
//        const eventType = input.tagName === 'SELECT' ? 'change' : 'input';
//        input.addEventListener(eventType, filterData);
//    });

//    // Clear button to reset filters
//    if (clearButton) {
//        clearButton.addEventListener('click', () => {
//            filterInputs.forEach(input => (input.value = ''));
//            filterData();
//        });
//    }

//    // Initial filter to display all data
//    filterData();
//});

//// FInventoryAdjustment
//document.addEventListener('DOMContentLoaded', function () {

//    const filterInputs = document.querySelectorAll('[data-filter]');
//    const dataRows = Array.from(document.querySelectorAll('.adjustment-row'));
//    const clearButton = document.querySelector('.clear-button');

//    function filterData() {
//        dataRows.forEach(row => {
//            let isVisible = true;

//            filterInputs.forEach(input => {
//                const key = input.getAttribute('data-filter');
//                const value = (input.value || '').trim().toLowerCase();
//                if (!value) return;

//                let fieldValue = '';

//                switch (key) {
//                    case 'adjustmentnumber':
//                        fieldValue = row.querySelector('.col-3.fw-bold')?.textContent.trim().toLowerCase() || '';
//                        if (!fieldValue.includes(value)) isVisible = false;
//                        break;
//                    case 'adjustmentreason':
//                        fieldValue = row.querySelector('.col-3.text-start')?.textContent.trim().toLowerCase() || '';
//                        if (!fieldValue.includes(value)) isVisible = false;
//                        break;
//                    case 'adjustmentstatus':
//                        const badge = row.querySelector('.col-2.text-center > .badge');
//                        if (!badge) {
//                            isVisible = false;
//                            return;
//                        }

//                        const statusText = badge.textContent.trim().toLowerCase();

//                        // Match the value to approved/pending correctly
//                        const isApproved = statusText === 'approved';

//                        if (
//                            (value === 'true' && !isApproved) ||
//                            (value === 'false' && isApproved)
//                        ) {
//                            isVisible = false;
//                        }
//                        break;
//                    default:
//                }
//            });

//            // Set row visibility
//            row.style.display = isVisible ? '' : 'none';
//        });
//    }

//    // Event listeners for inputs
//    filterInputs.forEach(input => {
//        const eventType = input.tagName === 'SELECT' ? 'change' : 'input';
//        input.addEventListener(eventType, filterData);
//    });

//    // Clear button to reset filters
//    if (clearButton) {
//        clearButton.addEventListener('click', () => {
//            filterInputs.forEach(input => (input.value = ''));
//            filterData();
//        });
//    }

//    // Initial filter to display all data
//    filterData();
//});

document.addEventListener('DOMContentLoaded', function () {
    const filterInputs = document.querySelectorAll('[data-filter]');
    const dataRows = Array.from(document.querySelectorAll('.procurementcategory-row'));
    const clearButton = document.querySelector('.clear-button');

    function filterData() {
        dataRows.forEach(row => {
            let isVisible = true;

            filterInputs.forEach(input => {
                const key = input.getAttribute('data-filter');
                const value = (input.value || '').trim().toLowerCase();
                if (!value) return;

                let fieldValue = '';

                switch (key) {
                    case 'procurementcategoryname':
                        fieldValue = row.querySelector('.col-2.fw-bold')?.textContent.trim().toLowerCase() || '';
                        if (!fieldValue.includes(value)) isVisible = false;
                        break;

                    case 'parentcategory':
                        fieldValue = row.querySelector('.col-2.text-start')?.textContent.trim().toLowerCase() || '';
                        if (!fieldValue.includes(value)) isVisible = false;
                        break;

                    case 'procurementstatus':
                        const procurementBadge = row.querySelector('.col-2.text-center > .badge');
                        if (procurementBadge) {
                            const statusText = procurementBadge.textContent.trim().toLowerCase();
                            const isActive = statusText === 'active';
                            if ((value === 'true' && !isActive) || (value === 'false' && isActive)) {
                                isVisible = false;
                            }
                        } else {
                            isVisible = false;
                        }
                        break;

                    case 'adjustmentnumber':
                        fieldValue = row.querySelector('.col-3.fw-bold')?.textContent.trim().toLowerCase() || '';
                        if (!fieldValue.includes(value)) isVisible = false;
                        break;

                    case 'adjustmentreason':
                        fieldValue = row.querySelector('.col-3.text-start')?.textContent.trim().toLowerCase() || '';
                        if (!fieldValue.includes(value)) isVisible = false;
                        break;

                    case 'adjustmentstatus':
                        const adjustmentBadge = row.querySelector('.col-2.text-center > .badge');
                        if (adjustmentBadge) {
                            const statusText = adjustmentBadge.textContent.trim().toLowerCase();

                            // Check if the status text matches the selected value
                            if (value && statusText !== value) {
                                isVisible = false;
                            }
                        } else {
                            isVisible = false;
                        }
                        break;


                    default:
                        break;
                }
            });

            // Set row visibility
            row.style.display = isVisible ? '' : 'none';
        });
    }

    // Event listeners for inputs
    filterInputs.forEach(input => {
        const eventType = input.tagName === 'SELECT' ? 'change' : 'input';
        input.addEventListener(eventType, filterData);
    });

    // Clear button to reset filters
    if (clearButton) {
        clearButton.addEventListener('click', () => {
            filterInputs.forEach(input => (input.value = ''));
            filterData();
        });
    }

    // Initial filter to display all data
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
