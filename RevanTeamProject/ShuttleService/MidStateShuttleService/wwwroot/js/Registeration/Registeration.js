
//hiding One way or round trip fields based on the selection
document.addEventListener("DOMContentLoaded", function () {

    // Function to toggle visibility of additional fields
    function toggleAdditionalFields(display) {
        document.querySelectorAll('.additional-fields').forEach(function (element) {
            element.style.display = display;
        });
    }

    // Initially hide the additional fields
    toggleAdditionalFields('none');

    // Event listener for changes in trip type
    document.querySelectorAll('input[name="TripType"]').forEach(function (radio) {
        radio.addEventListener('change', function () {
            if (this.value === 'RoundTrip' || this.value === 'OneWay') {
                toggleAdditionalFields('');
            } else {
                toggleAdditionalFields('none');
            }
        });
    });
});

document.addEventListener('DOMContentLoaded', function () {
    var pickUpLocationSelect = document.getElementById('PickUpLocation');
    var dropOffLocationSelect = document.getElementById('DropOffLocation');
    var otherSpecialRequestSection = document.querySelector('.other-special-request');
    var otherSpecialRequestTimePreferences = document.querySelector('.other-special-request-time-preferences');
    var otherPickupLocationDiv = document.querySelector('.other-pickup');
    var otherDropoffLocationDiv = document.querySelector('.other-dropoff');
    var otherSpecialYes = document.getElementById('otherSpecialYes');
    var otherSpecialNo = document.getElementById('otherSpecialNo');
    var otherMustLeaveAfterContainer = document.querySelector('.other-leave-after');
    var oneWayRadio = document.getElementById('OneWay');
    var roundTripRadio = document.getElementById('RoundTrip');

    //fucntion used to ccontrol the display of the special request section in a route shecudling system
    function toggleSpecialRequestDisplay() {
        // Get the selected option text for both pickup and dropoff locations
        var pickUpSelectedOptionText = pickUpLocationSelect.options[pickUpLocationSelect.selectedIndex].text;
        var dropOffSelectedOptionText = dropOffLocationSelect.options[dropOffLocationSelect.selectedIndex].text;
        // Check if the selected text in 'other' (in a case-insensitive manner)
        var displayState = (pickUpSelectedOptionText.toLowerCase() === 'other' || dropOffSelectedOptionText.toLowerCase() === 'other') ? 'block' : 'none';

        // Applies the displayState to the otherSpecialRequestSection's display style, determining its visibility.
        otherSpecialRequestSection.style.display = displayState;
        // Sets the display style of otherPickupLocationDiv and  otherDropoffLocationDiv to 'block' if location are 'other', or 'none' if not.
        otherPickupLocationDiv.style.display = (pickUpSelectedOptionText.toLowerCase() === 'other') ? 'block' : 'none';
        otherDropoffLocationDiv.style.display = (dropOffSelectedOptionText.toLowerCase() === 'other') ? 'block' : 'none';
        //Calls the function with the argument otherSpecialYes.checked, which is a boolean representing whether the "Yes" option for other special requests is selected.
        toggleOtherSpecialRequestVisibility(otherSpecialYes.checked);
    }

    function toggleOtherSpecialRequestVisibility(show) {
// Determines whether the "Yes" radio button for other special requests is selected
        otherSpecialRequestTimePreferences.style.display = show ? '' : 'none';
    }

    function toggleOtherMustLeaveAfterVisibility() {
        // Determines whether the "One Way" radio button is selected
        var isOneWaySelected = oneWayRadio.checked;
// Determines whether the "Yes" radio button for other special requests is selected
        var isOtherSpecialYesSelected = otherSpecialYes.checked;
        otherMustLeaveAfterContainer.style.display = (isOneWaySelected && isOtherSpecialYesSelected) ? 'none' : 'block';
    }

    // Event listeners for changing display based on dropdown selections
    pickUpLocationSelect.addEventListener('change', toggleSpecialRequestDisplay);
    dropOffLocationSelect.addEventListener('change', toggleSpecialRequestDisplay);

    // Event listeners for "Other Special Request" Yes/No radio buttons
    otherSpecialYes.addEventListener('change', function () {
        toggleOtherSpecialRequestVisibility(this.checked);
        toggleOtherMustLeaveAfterVisibility();
    });
    otherSpecialNo.addEventListener('change', function () {
        toggleOtherSpecialRequestVisibility(false);
        toggleOtherMustLeaveAfterVisibility();
    });

    // Event listeners for trip type radio buttons
    oneWayRadio.addEventListener('change', toggleOtherMustLeaveAfterVisibility);
    roundTripRadio.addEventListener('change', toggleOtherMustLeaveAfterVisibility);

    // Initial checks
    toggleSpecialRequestDisplay();
    toggleOtherMustLeaveAfterVisibility();
});


//Friday Special Request Selection
document.addEventListener("DOMContentLoaded", function () {
    // Existing function to toggle additional fields
    function toggleAdditionalFields(display) {
        document.querySelectorAll('.additional-fields').forEach(function (element) {
            element.style.display = display;
        });
    }

    // New function to toggle Special Request section
    function toggleSpecialRequest(display) {
        document.querySelector('.special-request').style.display = display;
    }

    // Event listener for changes in trip type
    document.querySelectorAll('input[name="TripType"]').forEach(function (radio) {
        radio.addEventListener('change', function () {
            if (this.value === 'Friday') {
                toggleAdditionalFields('none');
                toggleSpecialRequest('');
            } else {
                toggleAdditionalFields('');
                toggleSpecialRequest('none');
            }
        });
    });
});

//Show and hide speical request time preferences section based on the selection
document.addEventListener("DOMContentLoaded", function () {
    // Function to toggle visibility of sections based on the Special Request selection
    function toggleSectionsBasedOnSpecialRequest() {
        var specialRequestYes = document.getElementById('specialYes').checked;
        var specialRequestTimePreferences = document.querySelector('.special-request-time-preferences');

        // Toggle Special Request Time Preferences section
        if (specialRequestTimePreferences) {
            specialRequestTimePreferences.style.display = specialRequestYes ? 'block' : 'none';
        }
    }

    // Event listener for changes in Special Request radio buttons
    document.querySelectorAll('input[name="SpecialRequest"]').forEach(function (radio) {
        radio.addEventListener('change', toggleSectionsBasedOnSpecialRequest);
    });

    // Optionally, trigger the change event on page load if "Yes" is already checked
    // This ensures the correct sections are shown or hidden based on the preselected value
    toggleSectionsBasedOnSpecialRequest();
});


//Hide can leave after field based on Friday Special Request time preferences selection
document.addEventListener('DOMContentLoaded', function () {
    // Function to toggle the visibility of the Can Leave After field
    function toggleCanLeaveAfterVisibility() {
        var specialRequestYes = document.getElementById('specialYes').checked;
        var fridayOneWay = document.getElementById('FridayOneWay').checked;

        // Logic to determine if the Can Leave After field should be hidden
        if (specialRequestYes && fridayOneWay) {
            document.querySelector('.leave-after').style.display = 'none'; // Hides the Can Leave After field
        } else {
            document.querySelector('.leave-after').style.display = 'block'; // Shows the Can Leave After field
        }
    }

    // Attach event listeners to the Special Request and FridayTripType radio buttons
    document.getElementById('specialYes').addEventListener('change', toggleCanLeaveAfterVisibility);
    document.getElementById('specialNo').addEventListener('change', toggleCanLeaveAfterVisibility);
    document.getElementById('FridayRoundTrip').addEventListener('change', toggleCanLeaveAfterVisibility);
    document.getElementById('FridayOneWay').addEventListener('change', toggleCanLeaveAfterVisibility);

    // Initial call to set the correct state when the page loads
    toggleCanLeaveAfterVisibility();
});

//Show and hide contact preference section based on the selection
//document.addEventListener('DOMContentLoaded', function () {
//    // Function to toggle the contact preference display based on trip type selection
//    function toggleContactPreferenceDisplay() {
//        // Select the contact preference section
//        var contactPreferenceSection = document.querySelector('.contact-preference');

//        // Check if any trip type is selected
//        var isAnyTripTypeSelected = document.getElementById('RoundTrip').checked ||
//            document.getElementById('OneWay').checked ||
//            document.getElementById('Friday').checked;

//        // Show the contact preference section if any trip type is selected
//        contactPreferenceSection.style.display = isAnyTripTypeSelected ? 'block' : 'none';
//    }

//    // Get the trip type radio buttons by ID
//    var roundTripRadio = document.getElementById('RoundTrip');
//    var oneWayRadio = document.getElementById('OneWay');
//    var fridayRadio = document.getElementById('Friday');

//    // Add event listeners to trip type radio buttons to toggle the contact preference display
//    [roundTripRadio, oneWayRadio, fridayRadio].forEach(function (radioButton) {
//        radioButton.addEventListener('change', toggleContactPreferenceDisplay);
//    });

//    // Initial check to set the correct display state when the page loads
//    toggleContactPreferenceDisplay();
//});


//Slider toggle
document.addEventListener('DOMContentLoaded', function () {
    var toggleInput = document.getElementById('additionalStopsToggle');
    var toggleNoLabel = document.getElementById('toggleNo');
    var toggleYesLabel = document.getElementById('toggleYes');

    // Function to update label styles based on the checkbox state
    function updateToggleLabels() {
        if (toggleInput.checked) {
            toggleYesLabel.classList.add('active');
            toggleNoLabel.classList.remove('active');
        } else {
            toggleNoLabel.classList.add('active');
            toggleYesLabel.classList.remove('active');
        }
    }

    // Event listener for the toggle input change event
    toggleInput.addEventListener('change', updateToggleLabels);

    // Initial state update
    updateToggleLabels();
});

//Show and hide additional stops section based on the selection
document.addEventListener('DOMContentLoaded', function () {
    var otherSpecialYes = document.getElementById('otherSpecialYes');
    var additionalStopsToggle = document.getElementById('additionalStopsToggle');
    var stopsAvailableSection = document.querySelector('.stops-available');

    function toggleStopsAvailable() {
        if (otherSpecialYes.checked && additionalStopsToggle.checked) {
            stopsAvailableSection.style.display = 'block';
        } else {
            stopsAvailableSection.style.display = 'none';
        }
    }

    // Event listeners for the other special request option and the toggle switch
    otherSpecialYes.addEventListener('change', toggleStopsAvailable);
    additionalStopsToggle.addEventListener('change', toggleStopsAvailable);

    // Call the function on initial load to set the correct display state
    toggleStopsAvailable();
});

document.addEventListener("DOMContentLoaded", function () {
    function fetchClientIpAddress() {
        fetch('https://api.ipify.org?format=json')
            .then(response => response.json())
            .then(data => {
                setIpAddressField(data.ip);
            })
            .catch(error => {
                setIpAddressField(null);
            });
    }

    function setIpAddressField(ip) {
        const ipField = document.getElementById('DeviceIpAddress');
        if (ipField) {
            ipField.value = ip || ''; // Set to an empty string if null
        }
    }

    fetchClientIpAddress();
});

