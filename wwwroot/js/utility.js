// utility.js
function getUrl(area, page, action, id) {
    const baseUrl = window.baseUrl || '/';
    if (id) {
        return `${baseUrl}${area}/${page}/${action}/${id}`;
    }
    return `${baseUrl}${area}/${page}/${action}`;
}