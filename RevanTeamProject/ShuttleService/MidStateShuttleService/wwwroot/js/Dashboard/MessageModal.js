// MessageModal.js
$(document).ready(function () {

    // Initialize the modal (Bootstrap 5)
    var messageModal = new bootstrap.Modal(document.getElementById('messageDetailsModal'), {
        keyboard: true
    });

    // Click handler for "View" buttons
    $('.viewButton').click(function (e) {
        e.preventDefault();

        // Find the closest message card
        var card = $(this).closest('.message-card');

        // Grab the name from <h4> inside the card
        var name = card.find('h4').text();

        // Grab responseRequired and contactInfo from spans/divs (adjust selectors)
        var responseRequired = card.find('strong:contains("Response Required")').parent().text().replace('Response Required:', '').trim();
        var contactInfo = card.find('strong:contains("Contact")').parent().text().replace('Contact:', '').trim();

        var fullMessage = $(this).data('full-message');

        $('#messageDetailsContent').html(
            '<p><strong>Name:</strong> ' + name + '</p>' +
            '<p><strong>Message:</strong> ' + fullMessage + '</p>' +
            '<p><strong>Response Required:</strong> ' + responseRequired + '</p>' +
            '<p><strong>Contact Info:</strong> ' + (contactInfo ? contactInfo : "N/A") + '</p>'
        );

        messageModal.show();
    });

    // Clear modal content on close
    $('#messageDetailsModal').on('hidden.bs.modal', function () {
        $('#messageDetailsContent').html('');
    });

});