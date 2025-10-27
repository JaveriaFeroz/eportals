// --- DYNAMIC BREADCRUMB SCRIPT ---
// This script dynamically handles breadcrumb navigation based on current URL and context

$(document).ready(function () {
    initializeDynamicBreadcrumbs();
});

function initializeDynamicBreadcrumbs() {
    // Get current URL information
    const currentPath = window.location.pathname;
    const pathSegments = currentPath.split('/').filter(segment => segment !== '');
    const currentController = getControllerFromPath();
    const currentAction = getActionFromPath();

    // Dynamic breadcrumb click handling
    $('.breadcrumb-item a').on('click', function (e) {
        e.preventDefault();

        const clickedText = $(this).text().trim();
        const breadcrumbLevel = $(this).data('breadcrumb-level') || clickedText.toLowerCase();

        // Add visual feedback
        addClickFeedback($(this));

        // Handle click dynamically
        handleDynamicBreadcrumbClick(breadcrumbLevel, clickedText);
    });

    // Add hover effects
    addBreadcrumbHoverEffects();

    // Initialize breadcrumb context
    setBreadcrumbContext();
}

function getControllerFromPath() {
    const path = window.location.pathname;
    const segments = path.split('/').filter(s => s !== '');

    // Common controller detection patterns
    if (segments.length >= 1) {
        const potentialController = segments[0].toLowerCase();
        return potentialController;
    }
    return 'home';
}

function getActionFromPath() {
    const path = window.location.pathname;
    const segments = path.split('/').filter(s => s !== '');

    if (segments.length >= 2) {
        return segments[1].toLowerCase();
    }
    return 'index';
}

function handleDynamicBreadcrumbClick(level, displayText) {
    const currentController = getControllerFromPath();
    const currentAction = getActionFromPath();

    showLoadingIndicator();

    switch (level) {
        case 'home':
            // Always go to home/dashboard
            setTimeout(() => {
                window.location.href = window.basePath || '/';
            }, 300);
            break;

        case 'master':
            handleMasterBreadcrumb(currentAction);
            break;

        case 'procurement':
            handleProcurementBreadcrumb(currentAction);
            break;

        case 'finance':
            handleFinanceBreadcrumb(currentAction);
            break;

        case 'user management':
        case 'usermanagement':
            handleUserManagementBreadcrumb(currentAction);
            break;

        case 'insurance':
            handleInsuranceBreadcrumb(currentAction);
            break;

        case 'reports':
            handleReportsBreadcrumb(currentAction);
            break;

        default:
            // Dynamic handling based on current context
            handleGenericBreadcrumb(level, displayText);
            break;
    }
}

function handleMasterBreadcrumb(currentAction) {
    const masterRoutes = {
        'product': '/Master/Product',
        'supplier': '/Master/Supplier',
        'category': '/Master/Category',
        'location': '/Master/Location'
    };

    // If we're in master section, refresh current page
    if (getControllerFromPath() === 'master') {
        setTimeout(() => window.location.reload(), 300);
    } else {
        // Navigate to master index or most relevant master page
        const targetRoute = masterRoutes[currentAction] || '/Master';
        setTimeout(() => {
            window.location.href = window.basePath + targetRoute;
        }, 300);
    }
}

function handleProcurementBreadcrumb(currentAction) {
    const procurementRoutes = {
        'purchaseorder': '/Procurement/PurchaseOrder',
        'vendor': '/Procurement/Vendor',
        'request': '/Procurement/Request'
    };

    if (getControllerFromPath() === 'procurement') {
        setTimeout(() => window.location.reload(), 300);
    } else {
        const targetRoute = procurementRoutes[currentAction] || '/Procurement';
        setTimeout(() => {
            window.location.href = window.basePath + targetRoute;
        }, 300);
    }
}

function handleFinanceBreadcrumb(currentAction) {
    const financeRoutes = {
        'invoice': '/Finance/Invoice',
        'payment': '/Finance/Payment',
        'budget': '/Finance/Budget'
    };

    if (getControllerFromPath() === 'finance') {
        setTimeout(() => window.location.reload(), 300);
    } else {
        const targetRoute = financeRoutes[currentAction] || '/Finance';
        setTimeout(() => {
            window.location.href = window.basePath + targetRoute;
        }, 300);
    }
}

function handleUserManagementBreadcrumb(currentAction) {
    const userRoutes = {
        'users': '/UserManagement/Users',
        'roles': '/UserManagement/Roles',
        'permissions': '/UserManagement/Permissions'
    };

    if (getControllerFromPath() === 'usermanagement') {
        setTimeout(() => window.location.reload(), 300);
    } else {
        const targetRoute = userRoutes[currentAction] || '/UserManagement';
        setTimeout(() => {
            window.location.href = window.basePath + targetRoute;
        }, 300);
    }
}

function handleInsuranceBreadcrumb(currentAction) {
    const insuranceRoutes = {
        'policy': '/Insurance/Policy',
        'claim': '/Insurance/Claim',
        'premium': '/Insurance/Premium'
    };

    if (getControllerFromPath() === 'insurance') {
        setTimeout(() => window.location.reload(), 300);
    } else {
        const targetRoute = insuranceRoutes[currentAction] || '/Insurance';
        setTimeout(() => {
            window.location.href = window.basePath + targetRoute;
        }, 300);
    }
}

function handleReportsBreadcrumb(currentAction) {
    const reportRoutes = {
        'financial': '/Reports/Financial',
        'inventory': '/Reports/Inventory',
        'user': '/Reports/User'
    };

    if (getControllerFromPath() === 'reports') {
        setTimeout(() => window.location.reload(), 300);
    } else {
        const targetRoute = reportRoutes[currentAction] || '/Reports';
        setTimeout(() => {
            window.location.href = window.basePath + targetRoute;
        }, 300);
    }
}

function handleGenericBreadcrumb(level, displayText) {
    // Try to construct route dynamically
    const currentController = getControllerFromPath();

    // If the breadcrumb matches current controller, refresh
    if (level === currentController) {
        setTimeout(() => window.location.reload(), 300);
        return;
    }

    // Try to navigate to the breadcrumb section
    const potentialRoute = `/${capitalizeFirst(level)}`;

    // Check if route exists (you can customize this logic)
    setTimeout(() => {
        window.location.href = window.basePath + potentialRoute;
    }, 300);
}

function setBreadcrumbContext() {
    // Add dynamic classes and data attributes based on current context
    const currentController = getControllerFromPath();
    const currentAction = getActionFromPath();

    $('.breadcrumb-item').each(function () {
        const $item = $(this);
        const $link = $item.find('a');
        const itemText = $item.text().trim().toLowerCase();

        // Add context classes
        if (itemText === currentController) {
            $item.addClass('current-section');
        }

        if ($link.length > 0) {
            // Add dynamic data attributes
            $link.attr('data-controller', currentController);
            $link.attr('data-action', currentAction);
            $link.attr('data-breadcrumb-level', itemText);
        }
    });
}

function addClickFeedback($element) {
    $element.css({
        'opacity': '0.6',
        'transform': 'scale(0.95)',
        'transition': 'all 0.15s ease'
    });

    setTimeout(() => {
        $element.css({
            'opacity': '1',
            'transform': 'scale(1)'
        });
    }, 150);
}

function addBreadcrumbHoverEffects() {
    $('.breadcrumb-item a').hover(
        function () {
            $(this).css({
                'text-decoration': 'underline',
                'transform': 'translateY(-1px)',
                'transition': 'all 0.2s ease'
            });
        },
        function () {
            $(this).css({
                'text-decoration': 'none',
                'transform': 'translateY(0)',
                'transition': 'all 0.2s ease'
            });
        }
    );
}

function showLoadingIndicator() {
    // Remove existing indicator
    $('#breadcrumb-loading').remove();

    // Add loading indicator
    const loadingHtml = `
        <div id="breadcrumb-loading" style="
            position: fixed; 
            top: 20px; 
            right: 20px; 
            z-index: 9999; 
            background: rgba(0,123,255,0.9); 
            color: white; 
            padding: 8px 16px; 
            border-radius: 20px;
            font-size: 14px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.2);
        ">
            <i class="fas fa-spinner fa-spin"></i> Loading...
        </div>
    `;

    $('body').append(loadingHtml);

    // Auto remove after 3 seconds (failsafe)
    setTimeout(() => {
        $('#breadcrumb-loading').fadeOut(() => {
            $('#breadcrumb-loading').remove();
        });
    }, 3000);
}

function capitalizeFirst(str) {
    return str.charAt(0).toUpperCase() + str.slice(1);
}

// Dynamic route existence checker (optional enhancement)
function checkRouteExists(route, callback) {
    $.ajax({
        url: route,
        type: 'HEAD',
        success: function () {
            callback(true);
        },
        error: function () {
            callback(false);
        }
    });
}

// Export functions for external use
window.breadcrumbUtils = {
    refresh: () => window.location.reload(),
    navigate: (route) => window.location.href = route,
    getCurrentController: getControllerFromPath,
    getCurrentAction: getActionFromPath
};