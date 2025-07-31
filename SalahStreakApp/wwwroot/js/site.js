// Simple utility functions for data tables
window.exportTableToCSV = function(tableId) {
    const table = document.getElementById(tableId);
    const rows = table.getElementsByTagName('tr');
    let csv = [];
    
    for (let i = 0; i < rows.length; i++) {
        const row = rows[i];
        const cols = row.querySelectorAll('td, th');
        const rowData = [];
        
        for (let j = 0; j < cols.length; j++) {
            rowData.push('"' + cols[j].textContent.replace(/"/g, '""') + '"');
        }
        
        csv.push(rowData.join(','));
    }
    
    const csvContent = csv.join('\n');
    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'data.csv';
    a.click();
    window.URL.revokeObjectURL(url);
};

window.exportTableToPDF = function(tableId) {
    // Simple PDF export using window.print()
    window.print();
};

window.refreshTableData = function(tableId, url) {
    fetch(url)
        .then(response => response.json())
        .then(data => {
            // This function can be overridden by individual tables
            console.log('Refresh data:', data);
        })
        .catch(error => console.error('Error refreshing table:', error));
};

window.toggleSearch = function(tableId) {
    const searchBox = document.getElementById(tableId + 'Search');
    if (searchBox) {
        searchBox.style.display = searchBox.style.display === 'none' ? 'block' : 'none';
    }
};
