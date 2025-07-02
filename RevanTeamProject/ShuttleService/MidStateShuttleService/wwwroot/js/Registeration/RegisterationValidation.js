// ** Commented out the some validation fields to work with the rest of the form in next sprint **//
//This file does not seem to be used, use RegisterValidation.js

document.addEventListener('DOMContentLoaded', function () {
    const schoolTerm = document.getElementById('SchoolTerm');
    //const studentId = document.getElementById('StudentId');
    const firstName = document.getElementById('FirstName');
    const lastName = document.getElementById('LastName');
    const email = document.getElementById('Email');
    const phoneNumber = document.getElementById('PhoneNumber');
    //const tripTypeRadios = document.querySelectorAll('input[name="TripType"]');

    // Attach event listeners for real-time validation
    schoolTerm.addEventListener('change', () => validateDropdown(schoolTerm, 'SchoolTerm'));
    phoneNumber.addEventListener('input', () => validateField(phoneNumber, /^[0-9]{10}$/, 'PhoneNumber'));
    //studentId.addEventListener('input', () => validateField(studentId, /^\d{8}$/, 'StudentId'));
    firstName.addEventListener('input', () => validateField(firstName, /^[A-Za-z\s'-]+$/, 'FirstName'));
    lastName.addEventListener('input', () => validateField(lastName, /^[A-Za-z\s'-]+$/, 'LastName'));
    email.addEventListener('input', () => validateField(email, /^[^\s@]+@[^\s@]+\.[^\s@]+$/, 'Email')); // Email validation regex
    //tripTypeRadios.forEach(radio => radio.addEventListener('change', validateTripType));

    if (!schoolTerm) {
        console.error('SchoolTerm element not found!');
    } else {
        console.log('SchoolTerm dropdown found!');
    }

    document.querySelector('#registrationForm').addEventListener('submit', function (event) {
        event.preventDefault(); // Always prevent default submission initially

        if (validateForm()) {
            console.log("Form is valid, showing confirmation modal.");
            $('#confirmationModal').modal('show'); // Correctly show confirmation modal only if valid
        } else {
            console.log("Form is not valid, submission prevented.");
            scrollToFirstInvalid();
        }
    });

    // Make sure it returns false if any validation fails temporarily crossed out vlaidation fields for submit button
    function validateForm() {
        const isSchoolTermValid = validateDropdown(schoolTerm, 'SchoolTerm');
        const isPhoneNumberValid = validateField(phoneNumber, /^[0-9]{10}$/, 'PhoneNumber');
        //const isStudentIdValid = validateField(studentId, /^\d{8}$/, 'StudentId');
        const isFirstNameValid = validateField(firstName, /^[A-Za-z\s'-]+$/, 'FirstName');
        const isLastNameValid = validateField(lastName, /^[A-Za-z\s'-]+$/, 'LastName');
        const isEmailValid = validateField(email, /^[^\s@]+@[^\s@]+\.[^\s@]+$/, 'Email');
        //const isTripTypeValid = validateTripType(); // Correctly placed outside the array

        // Combine all validations for overall form validity when click submit
        //return isSchoolTermValid && isPhoneNumberValid && isFirstNameValid && isLastNameValid && isEmailValid &&
        //    isPickUpLocationValid && isDropOffLocationValid && isDateValid &&
        //    isPickUpTimeValid && isDropOffTimeValid && isTripTypeValid;
    }

    function validateField(element, regex, fieldName) {
        const isValid = regex.test(element.value);
        const validationMessageElement = document.getElementById(`${fieldName}-validation-message`);
        if (isValid) {
            validationMessageElement.style.display = 'none';
            element.classList.remove('is-invalid');
        } else {
            validationMessageElement.style.display = 'block';
            element.classList.add('is-invalid');
        }
        return isValid;
    }

    function validateDropdown(dropdown, fieldName) {
        const isValid = dropdown.value !== ""; // "" is the default "Select Term" option
        const validationMessageElement = document.getElementById(`${fieldName}-validation-message`);
        if (isValid) {
            validationMessageElement.style.display = 'none';
            dropdown.classList.remove('is-invalid');
        } else {
            validationMessageElement.style.display = 'block';
            dropdown.classList.add('is-invalid');
        }
        return isValid;
    }


    //to improve user experience by bringing the first invalid input into view if the form is not valid.
    function scrollToFirstInvalid() {
        const firstInvalidElement = document.querySelector('.is-invalid');
        if (firstInvalidElement) {
            firstInvalidElement.scrollIntoView({
                behavior: 'smooth',
                block: 'center'
            });
        }
    }
});