$(document).ready(function () {
    $('.btn--submit').click(function (e) {
        e.preventDefault(); // Prevent form submit

        // Gather form data
        var schoolTerm = $('#SchoolTerm').val();
        var studentId = $('#StudentId').val();
        var firstName = $('#FirstName').val();
        var lastName = $('#LastName').val();

        // grabbing the isadult value and setting string yes or no
        var isAdult = $('input[name="IsAdult"]:checked').val();
        var displayText = isAdult === "True" ? "Yes" : "No";

        var email = $('#Email').val();
        var phoneNumber = $('#PhoneNumber').val();
        var tripType = $('[name="TripType"]:checked').val();

        //Round Trip variables
        var otherSpecialRequest = $('#otherSpecialYes').is(':checked') ? 'Yes' : 'No'; 
        var specialPickUpLocation = $('#other-pickup-location').val() || 'Not specified';
        var specialDropOffLocation = $('#other-dropoff-location').val() || 'Not specified';
        var otherMustArriveBy = $('#otherMustArriveBy').val() || 'Not specified';
        var otherCanLeaveAfter = $('#otherCanLeaveAfter').val() || 'Not specified';
        var needTransportation = $('#NeedTransportation').val() || 'Not specified';
        //var contactPreference = $('[name="ContactPreference"]:checked').val();

        // Fetch selected location names directly from the option text
        var pickUpLocationName = $('#PickUpLocation option:selected').text();
        var dropOffLocationName = $('#DropOffLocation option:selected').text();

        // Ensure "Select Pick-Up Location" or "Select Drop-Off Location" is not treated as valid selections
        pickUpLocationName = pickUpLocationName === "Select Pick-Up Location" ? "Not specified" : pickUpLocationName;
        dropOffLocationName = dropOffLocationName === "Select Drop-Off Location" ? "Not specified" : dropOffLocationName;
        
        var initialRoute = $('input[name="SelectedRouteDetail"]:checked + span').text();
        var returnRoute = $('input[name="ReturnSelectedRouteDetail"]:checked + span').text();

        var selectedDaysOfWeek = [];
        $('input[name="SelectedDaysOfWeek"]:checked').each(function () {
            selectedDaysOfWeek.push($(this).val());
        });
        var daysOfWeekFormatted = selectedDaysOfWeek.join(', ');

        var firstDayExpectingToRide = $('#FirstDayExpectingToRide').val() || 'Not specified';

        //Friday Speical Request Variables
        var fridaySpecialRequest = $('#specialYes').is(':checked') ? 'Yes' : 'No'; // Assuming you have a similar ID for the Friday special request radio button
        var fridayTripType = $('[name="FridayTripType"]:checked').val(); // Fetching the value for the Friday trip type

        // Fetch selected Friday location names directly from the option text
        var fridayPickUpLocationName = $('#FridayPickUpLocation option:selected').text();
        var fridayDropOffLocationName = $('#FridayDropOffLocation option:selected').text();

        // Ensure "Select Pick-Up Location" or "Select Drop-Off Location" is not treated as valid selections for Friday locations
        fridayPickUpLocationName = fridayPickUpLocationName === "Select Pick-Up Location" ? "Not specified" : fridayPickUpLocationName;
        fridayDropOffLocationName = fridayDropOffLocationName === "Select Drop-Off Location" ? "Not specified" : fridayDropOffLocationName;


        var fridayMustArriveTime = $('#friday-special-arrive').val() || 'Not specified';
        var fridayCanLeaveTime = $('#friday-special-leave').val() || 'Not specified';
        var whichFriday = $('#WhichFriday').val() || 'Not specified';


        // Start building the confirmationContent
        var confirmationContent = `
            <p>School Term: ${schoolTerm}</p>
            <p>Student ID: ${studentId}</p>
            <p>First Name: ${firstName}</p>
            <p>Last Name: ${lastName}</p>
            <p>I am 18 years of age or older: ${displayText}</p>
            <p>Email: ${email}</p>
            <p>Phone Number: ${phoneNumber}</p>
            <p>Trip Type: ${tripType}</p>
        `;

        //Round trip No Speical Request
        if (tripType === 'RoundTrip') {
            if (otherSpecialRequest === "No") {
                confirmationContent += `
                    <p>Special Request: ${otherSpecialRequest}</p>
                    <p>Initial Route: ${initialRoute}</p>
                    <p>Return Route: ${returnRoute}</p>
                    <p>Days of the Week Needed: ${daysOfWeekFormatted}</p>
                    <p>First Day Expecting to Ride: ${firstDayExpectingToRide}</p>
                `;
            }
            else if (otherSpecialRequest === "Yes" && pickUpLocationName === 'Other' && dropOffLocationName !== 'Other') {
                confirmationContent += `
                <p>Special Request: ${otherSpecialRequest}</p>
                <p>Must Arrive Time: ${otherMustArriveBy}</p>
                <p>Can Leave Time: ${otherCanLeaveAfter}</p>
                <p>Special Pick Up Location: ${specialPickUpLocation}</p>
                <p>Drop Off Location: ${dropOffLocationName}</p>
                <p>Need Transportation Detail: ${needTransportation}</p>
            `;
            }
            else if (otherSpecialRequest === "Yes" && dropOffLocationName === 'Other' && pickUpLocationName !== 'Other') {
                confirmationContent += `
                <p>Special Request: ${otherSpecialRequest}</p>
                <p>Must Arrive Time: ${otherMustArriveBy}</p>
                <p>Can Leave Time: ${otherCanLeaveAfter}</p>
                <p>Pick Up Location: ${pickUpLocationName}</p>
                <p>Special Drop Off Location: ${specialDropOffLocation}</p>
                <p>Need Transportation Detail: ${needTransportation}</p>
            `;
            }
            else if (otherSpecialRequest === "Yes" && pickUpLocationName === 'Other' && dropOffLocationName === 'Other') {
                confirmationContent += `
                <p>Special Request: ${otherSpecialRequest}</p>
                <p>Must Arrive Time: ${otherMustArriveBy}</p>
                <p>Can Leave Time: ${otherCanLeaveAfter}</p>
                <p>Special Pick Up Location: ${specialPickUpLocation}</p>
                <p>Special Drop Off Location: ${specialDropOffLocation}</p>
                <p>Need Transportation Detail: ${needTransportation}</p>
            `;
            }
            else if (otherSpecialRequest === "Yes" && (pickUpLocationName && dropOffLocationName) && !initialRoute) {
                confirmationContent += `
                <p>Special Request: ${otherSpecialRequest}</p>
                <p>Pick Up Location: ${pickUpLocationName}</p>
                <p>Drop Off Location: ${dropOffLocationName}</p>
                <p>Must Arrive Time: ${otherMustArriveBy}</p>
                <p>Can Leave Time: ${otherCanLeaveAfter}</p>
                <p>Need Transportation Detail: ${needTransportation}</p>
            `;
            }
            else if (otherSpecialRequest === "Yes" && initialRoute && !returnRoute) {
                confirmationContent += `
                <p>Special Request: ${otherSpecialRequest}</p>
                <p>Initial Route: ${initialRoute}</p>
                <p>Must Arrive Time: ${otherMustArriveBy}</p>
                <p>Can Leave Time: ${otherCanLeaveAfter}</p>
                <p>Need Transportation Detail: ${needTransportation}</p>
            `;
            }
        }

        //One Way No Speical Request
        if (tripType === 'OneWay') {
            if (otherSpecialRequest === "No") {
                confirmationContent += `
                    <p>Initial Route: ${initialRoute}</p>
                    <p>Days of the Week Needed: ${daysOfWeekFormatted}</p>
                    <p>First Day Expecting to Ride: ${firstDayExpectingToRide}</p>
                `;
            }
            else if (otherSpecialRequest === "Yes" && pickUpLocationName && dropOffLocationName && !initialRoute) {
                confirmationContent += `
                <p>Special Request: ${otherSpecialRequest}</p>
                <p>Pick Up Location: ${pickUpLocationName}</p>
                <p>Drop Off Location: ${dropOffLocationName}</p>
                <p>Must Arrive Time: ${otherMustArriveBy}</p>
                <p>Need Transportation Detail: ${needTransportation}</p>
            `;
            }
            else if (otherSpecialRequest === "Yes" && pickUpLocationName === 'Other' && dropOffLocationName !== 'Other') {
                confirmationContent += `
                <p>Special Request: ${otherSpecialRequest}</p>
                <p>Must Arrive Time: ${otherMustArriveBy}</p>
                <p>Special Pick Up Location: ${specialPickUpLocation}</p>
                <p>Drop Off Location: ${dropOffLocationName}</p>
                <p>Need Transportation Detail: ${needTransportation}</p>
            `;
            }
            else if (otherSpecialRequest === "Yes" && dropOffLocationName === 'Other' && pickUpLocationName !== 'Other') {
                confirmationContent += `
                <p>Special Request: ${otherSpecialRequest}</p>
                <p>Must Arrive Time: ${otherMustArriveBy}</p>
                <p>Pick Up Location: ${pickUpLocationName}</p>
                <p>Special Drop Off Location: ${specialDropOffLocation}</p>
                <p>Need Transportation Detail: ${needTransportation}</p>
            `;
            }
            else if (otherSpecialRequest === "Yes" && pickUpLocationName === 'Other' && dropOffLocationName === 'Other') {
                confirmationContent += `
                <p>Special Request: ${otherSpecialRequest}</p>
                <p>Must Arrive Time: ${otherMustArriveBy}</p>
                <p>Special Pick Up Location: ${specialPickUpLocation}</p>
                <p>Special Drop Off Location: ${specialDropOffLocation}</p>
                <p>Need Transportation Detail: ${needTransportation}</p>
            `;
            }
        }

        //One Way No Speical Request
        if (tripType === 'Friday') {
            if (fridaySpecialRequest === "Yes" && fridayTripType == "RoundTrip") {
                confirmationContent += `
                    <p>Special Request: ${fridaySpecialRequest}</p>
                    <p>Friday Trip Choice: ${fridayTripType}</p>
                    <p>Must Arrive Time: ${fridayMustArriveTime}</p>
                    <p>Can Leave After: ${fridayCanLeaveTime}</p>
                    <p>Pick Up Location: ${fridayPickUpLocationName}</p>
                    <p>Drop Off Location: ${fridayDropOffLocationName}</p>
                    <p>Which Friday Detail: ${whichFriday}</p>
                `;
            }
            else if (fridaySpecialRequest === "Yes" && fridayTripType == "OneWay") {
                confirmationContent += `
                    <p>Special Request: ${fridaySpecialRequest}</p>
                    <p>Friday Trip Choice: ${fridayTripType}</p>
                    <p>Must Arrive Time: ${fridayMustArriveTime}</p>
                    <p>Pick Up Location: ${fridayPickUpLocationName}</p>
                    <p>Drop Off Location: ${fridayDropOffLocationName}</p>
                    <p>Which Friday Detail: ${whichFriday}</p>
                `;
            }
        }

        /*confirmationContent += `<p>Contact Preference: ${contactPreference}</p>`;*/

        // Display the constructed confirmation content in the modal body
        $('.modal-body').html(confirmationContent);

        // Show the modal
        $('#confirmationModal').modal('show');
    });

    // Handle the final confirmation button click
    $('#confirmSubmit').click(function () {
        // Actual form submission
        $('#registrationForm').submit();
    });
});
