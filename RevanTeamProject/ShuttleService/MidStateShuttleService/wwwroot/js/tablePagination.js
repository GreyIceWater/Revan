document.addEventListener("DOMContentLoaded", function () {

    function setupPagination({
        containerId,
        itemSelector,
        paginationId,
        itemsPerPage = 10,
        maxButtonsToShow = 10,
        isTableRow = true
    }) {
        const container = document.getElementById(containerId);
        if (!container) return;

        const items = container.querySelectorAll(itemSelector);
        const paginationControls = document.getElementById(paginationId);
        if (!paginationControls) return;

        // Calculate pages
        let pageCount = Math.ceil(items.length / itemsPerPage);
        pageCount = pageCount > maxButtonsToShow ? maxButtonsToShow : pageCount;

        // Clear existing buttons
        paginationControls.innerHTML = '';

        // Create buttons
        for (let i = 1; i <= pageCount; i++) {
            const button = document.createElement('button');
            button.className = 'btn btn-sm btn-midstate me-1';
            button.textContent = i;
            paginationControls.appendChild(button);
        }

        function showPage(page) {
            items.forEach((item, index) => {
                const isVisible = index >= (page - 1) * itemsPerPage && index < page * itemsPerPage;
                item.style.display = isVisible
                    ? (isTableRow ? 'table-row' : 'block')
                    : 'none';
            });
        }

        const buttons = paginationControls.querySelectorAll('button');
        buttons.forEach(button => {
            button.addEventListener('click', function () {
                showPage(parseInt(this.textContent));
            });
        });

        // Show first page initially
        if (buttons.length > 0) showPage(1);
    }

    // Example usage for multiple tables/cards:

    setupPagination({
        containerId: 'routesTableBody',
        itemSelector: '.route-card',
        paginationId: 'pagination-controls-routes',
        isTableRow: false
    });

    setupPagination({
        containerId: 'checkInsTableBody',
        itemSelector: '.checkin-card',
        paginationId: 'pagination-controls-checkIns',
        isTableRow: false
    });

    setupPagination({
        containerId: 'driverTableBody',
        itemSelector: '.driver-card',
        paginationId: 'pagination-controls-driver',
        isTableRow: false
    });

    setupPagination({
        containerId: 'feedbackTableBody',
        itemSelector: '.feedback-card',
        paginationId: 'pagination-controls-feedback',
        isTableRow: false
    });

    setupPagination({
        containerId: 'locationsTableBody',
        itemSelector: '.location-card',
        paginationId: 'pagination-controls-location',
        isTableRow: false
    });

    setupPagination({
        containerId: 'shuttleTableBody',
        itemSelector: '.shuttle-card',
        paginationId: 'pagination-controls-shuttle',
        isTableRow: false
    });

    setupPagination({
        containerId: 'registrationTableBody',
        itemSelector: '.registration-card',
        paginationId: 'pagination-controls-registration',
        isTableRow: false
    });

});