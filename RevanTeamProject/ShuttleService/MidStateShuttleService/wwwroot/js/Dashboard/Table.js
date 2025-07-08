$(function () {
    // Function to hide all tables
    function hideAllTables() {
        $('.recentFeedback').hide();
    }

    // Initial setup to show all tables
    hideAllTables();
    $('.recentFeedback.messages').show();


    
    // Click event for the Routes button
    $('#order').click(function () {
        hideAllTables();
        $('.recentFeedback.messages').show();
        return false;
    });

    // Click event for the Employees button
    $('#routes').click(function () {
        hideAllTables();
        $('.recentFeedback.route').show(); // Show the location table
        return false;
    });

    $('#location').click(function () {
        hideAllTables();
        $('.recentFeedback.location').show(); // Show the location table
        return false;
    });
    // Click event for the Drivers button
    $('#driver').click(function () {
        hideAllTables();
        $('.recentFeedback.driver').show(); // Show the driver table
        return false;
    });

    // Click event for the Shuttles button
    $('#shuttle').click(function () {
        hideAllTables();
        $('.recentFeedback.shuttle').show(); // Show the shuttle table
        return false;
    });

    // Click event for the Check Ins button
    $('#check').click(function () {
        hideAllTables();
        $('.recentFeedback.check').show(); // Show the check ins table
        return false;
    });

    // Click event for the Feedback button
    $('#feedback').click(function () {
        hideAllTables();
        $('.recentFeedback.feedback').show(); // Show the feedback table
        return false;
    });

    $('#special').click(function () {
        hideAllTables();
        $('.recentFeedback.request').show(); // Show the request table
        return false;
    });

});
