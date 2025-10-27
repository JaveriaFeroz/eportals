// --- JQUERY SLIDING PAGINATION & FILTER SCRIPT ---
// Yeh script client-side filtering aur advanced pagination dono ko handle karti hai.

$(document).ready(function () {
    // --- CONFIGURATION ---
    const rows = $('.user-row'); // Woh items jin par pagination/filter lagana hai.
    const filterInputs = $('[data-filter]'); // Search ke saare inputs.
    const clearButton = $('.clear-button');
    const controlsContainer = $('#pagination-controls'); // Sirf yeh ek div HTML mein hona zaroori hai.
    const itemsPerPage = 10; // Har page par kitne items dikhane hain.
    const maxPagesToShow = 5; // Ek baar mein kitne page number dikhane hain.

    // Agar koi rows ya container nahi hai, to script ko na chalayein.
    if (rows.length === 0 || controlsContainer.length === 0) {
        return;
    }

    // --- HTML STRUCTURE INJECT KARNE KA CODE ---
    const paginationHtml = `
        <div class="row mt-3">
            <div class="col-md-6 d-flex align-items-center">
                <div id="pagination-info" class="text-muted"></div>
            </div>
            <div class="col-md-6">
                <div id="pagination-container"></div>
            </div>
        </div>
    `;
    controlsContainer.html(paginationHtml);

    const paginationContainer = $('#pagination-container');
    const paginationInfoContainer = $('#pagination-info');
    let visibleRows = rows; // Shuru mein saari rows visible hain.

    // --- MAIN UPDATE FUNCTION ---
    // Yeh function filter aur pagination dono ko refresh karta hai.
    function updateTable() {
        const filters = [];
        filterInputs.each(function () {
            const value = $(this).val().trim().toLowerCase();
            if (value) {
                filters.push({ key: $(this).data('filter'), value: value });
            }
        });

        if (filters.length > 0) {
            visibleRows = rows.filter(function () {
                let row = $(this);
                let isMatch = true;
                for (const filter of filters) {
                    const cell = row.find(`[data-column="${filter.key}"]`);
                    const cellValue = cell.text().trim().toLowerCase();
                    if (!cellValue.includes(filter.value)) {
                        isMatch = false;
                        break;
                    }
                }
                return isMatch;
            });
        } else {
            visibleRows = rows;
        }

        showPage(1);
    }

    // --- PAGE DISPLAY FUNCTION ---
    function showPage(pageNum) {
        rows.hide();

        const totalItems = visibleRows.length;
        const totalPages = Math.ceil(totalItems / itemsPerPage);

        pageNum = Math.max(1, Math.min(pageNum, totalPages || 1));

        const startIndex = (pageNum - 1) * itemsPerPage;
        const endIndex = Math.min(startIndex + itemsPerPage, totalItems);

        visibleRows.slice(startIndex, endIndex).show();

        renderPaginationControls(pageNum, totalPages, totalItems, startIndex, endIndex);
    }

    // --- PAGINATION CONTROLS RENDER FUNCTION ---
    // Yeh function sliding page numbers aur buttons banata hai.
    function renderPaginationControls(currentPage, totalPages, totalItems, startIndex, endIndex) {
        paginationContainer.empty();
        paginationInfoContainer.text(`Showing ${totalItems > 0 ? startIndex + 1 : 0} to ${endIndex} of ${totalItems} results`);

        if (totalPages <= 1) return;

        const paginationList = $('<ul class="pagination justify-content-end"></ul>');

        // First aur Previous buttons (with Font Awesome Icons)
        paginationList.append(`<li class="page-item ${currentPage === 1 ? 'disabled' : ''}" id="first-page"><a class="page-link" href="#"><i class="fas fa-angle-double-left"></i></a></li>`);
        paginationList.append(`<li class="page-item ${currentPage === 1 ? 'disabled' : ''}" id="prev-page"><a class="page-link" href="#"><i class="fas fa-angle-left"></i></a></li>`);

        // Page numbers calculate karne ka logic
        let startPage = Math.max(1, currentPage - Math.floor(maxPagesToShow / 2));
        let endPage = Math.min(totalPages, startPage + maxPagesToShow - 1);

        if (endPage - startPage + 1 < maxPagesToShow) {
            startPage = Math.max(1, endPage - maxPagesToShow + 1);
        }

        if (startPage > 1) {
            paginationList.append('<li class="page-item disabled"><a class="page-link" href="#">...</a></li>');
        }

        for (let i = startPage; i <= endPage; i++) {
            paginationList.append(`<li class="page-item page-num ${i === currentPage ? 'active' : ''}"><a class="page-link" href="#">${i}</a></li>`);
        }

        if (endPage < totalPages) {
            paginationList.append('<li class="page-item disabled"><a class="page-link" href="#">...</a></li>');
        }

        // Next aur Last buttons (with Font Awesome Icons)
        paginationList.append(`<li class="page-item ${currentPage === totalPages ? 'disabled' : ''}" id="next-page"><a class="page-link" href="#"><i class="fas fa-angle-right"></i></a></li>`);
        paginationList.append(`<li class="page-item ${currentPage === totalPages ? 'disabled' : ''}" id="last-page"><a class="page-link" href="#"><i class="fas fa-angle-double-right"></i></a></li>`);

        paginationContainer.append(paginationList);
    }

    // --- EVENT BINDING ---
    filterInputs.on('input change', updateTable);

    clearButton.on('click', function () {
        filterInputs.val('');
        updateTable();
    });

    paginationContainer.on('click', '.page-link', function (e) {
        e.preventDefault();
        const button = $(this).parent();
        if (button.hasClass('disabled') || button.hasClass('active')) return;

        let currentPage = parseInt(paginationContainer.find('.page-num.active a').text());
        const totalPages = Math.ceil(visibleRows.length / itemsPerPage);

        if (button.attr('id') === 'first-page') {
            currentPage = 1;
        } else if (button.attr('id') === 'prev-page') {
            currentPage--;
        } else if (button.attr('id') === 'next-page') {
            currentPage++;
        } else if (button.attr('id') === 'last-page') {
            currentPage = totalPages;
        } else {
            currentPage = parseInt($(this).text());
        }

        showPage(currentPage);
    });

    // --- INITIALIZATION ---
    updateTable();
});
