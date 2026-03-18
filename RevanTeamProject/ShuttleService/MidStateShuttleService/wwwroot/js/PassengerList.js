document.addEventListener("DOMContentLoaded", function () {

    var printButton = document.getElementById('printButton');
    if (printButton) {
        printButton.addEventListener('click', function () {
            // Directly print the table
            var table = document.getElementById("passengerTable");
            if (!table) return;

            var printContent = table.outerHTML;

            var printWindow = window.open('', '', 'height=600,width=800');
            printWindow.document.write('<html><head><title>Passenger List</title>');
            printWindow.document.write('<style>');
            printWindow.document.write('table {width: 100%; border-collapse: collapse; font-size: 12px;}');
            printWindow.document.write('th, td {border: 1px solid #000; padding: 6px; text-align: left;}');
            printWindow.document.write('thead {background-color: #000; color: #fff;}');
            printWindow.document.write('body {margin: 20px;}');
            printWindow.document.write('</style></head><body>');
            printWindow.document.write(printContent);
            printWindow.document.write('</body></html>');
            printWindow.document.close();
            printWindow.focus();
            printWindow.print();
            printWindow.close();
        });
    }

});