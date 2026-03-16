// Adds a message notification item to the notification list
function addMessageNotification(count, message = 'You have a new message!') {

    // Get the notification list container
    let notificationList = $('#notificationMessageDropdown');

    // Create the HTML for the message notification
    let newNotificationHtml = `
        <li class="list-group-item message-notification d-flex align-items-start"
            data-url="/Communicate/ViewAll">

            <!-- Icon -->
            <i class="bi bi-envelope-fill text-warning me-3 fs-5"></i>

            <!-- Notification content -->
            <div>
                <strong>New Message (${count})</strong>
                <div class="text-muted small">${message}</div>
            </div>

        </li>
    `;

    // Add the notification to the top of the list
    notificationList.prepend(newNotificationHtml);
}


// Adds a feedback notification item
function addFeedbackNotification(count, message = 'You have new feedback!') {

    // Get the notification list container
    let notificationList = $('#notificationMessageDropdown');

    // Create the HTML for the feedback notification
    let newNotificationHtml = `
        <li class="list-group-item feedback-notification d-flex align-items-start"
            data-url="/Feedback/ViewAll">

            <!-- Icon -->
            <i class="bi bi-chat-dots-fill text-warning me-3 fs-5"></i>

            <!-- Notification content -->
            <div>
                <strong>New Feedback (${count})</strong>
                <div class="text-muted small">${message}</div>
            </div>

        </li>
    `;

    // Add the notification to the top of the list
    notificationList.prepend(newNotificationHtml);
}


$(document).ready(function () {

    // Grab the notification container
    let container = $('#notificationMessageDropdown');

    // Read counts from data attributes (provided by Razor ViewData)
    let messageCount = parseInt(container.data('message-count')) || 0;
    let feedbackCount = parseInt(container.data('feedback-count')) || 0;

    // Read the last message / feedback text
    let lastMessage = container.data('last-message') || 'You have a new message!';
    let lastFeedback = container.data('last-feedback') || 'You have new feedback!';

    // Debugging output (check browser console if things don't show)
    console.log("Message Count:", messageCount);
    console.log("Feedback Count:", feedbackCount);
    console.log("Last Message:", lastMessage);
    console.log("Last Feedback:", lastFeedback);

    // Remove any placeholder items in the list
    container.empty();

    // Add message notification if messages exist
    if (messageCount > 0) {
        addMessageNotification(messageCount, lastMessage);
    }

    // Add feedback notification if feedback exists
    if (feedbackCount > 0) {
        addFeedbackNotification(feedbackCount, lastFeedback);
    }

    // If there are no notifications, show a default message
    if (messageCount === 0 && feedbackCount === 0) {

        container.append(`
            <li class="list-group-item text-muted text-center">
                No new notifications
            </li>
        `);

    }

});


// Handle clicking a notification
// Redirects user to the page defined in data-url
$(document).on('click', '.message-notification, .feedback-notification', function () {

    // Get redirect URL
    let url = $(this).data('url');

    console.log("Notification clicked, redirecting to:", url);

    // Navigate if URL exists
    if (url) {
        window.location.href = url;
    }

});







