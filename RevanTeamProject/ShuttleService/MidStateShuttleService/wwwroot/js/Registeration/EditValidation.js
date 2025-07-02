document.addEventListener('DOMContentLoaded', function () {
    //const studentId = document.getElementById('StudentId'); // Removing per kathy on 2025-01-15
    const name = document.getElementById('firstName');
    const lastName = document.getElementById('lastName');
    const phone = document.getElementById('phone');
    const email = document.getElementById('email');

    // Form submission event listener
    document.querySelector('#editForm').addEventListener('submit', function (event) {
        let isFormValid = validateForm();
        if (!isFormValid) {
            event.preventDefault();
        }
    });

    // Validation functions
    function validateForm() {
        let isFormValid = true;
        //isFormValid &= validateString(studentId); // Removing per kathy on 2025-01-15
        isFormValid &= validateString(name);
        isFormValid &= validateString(lastName);
        isFormValid &= validateString(phone);
        isFormValid &= validateString(email);
        return isFormValid;
    }
});

function validateString(item) {
    if (!item.value) {
        displayValidationMessage(item, "Please make sure this field is filled out");
        return false;
    } else {
        clearValidationMessage(item);
        return true;
    }
}

function validateRoutes(route, returnRoute, display) {
    if (route.value == returnRoute.value) {
        displayRouteValidation(display, "Route and Return route cannot be the same");
        return false;
    } else {
        clearRouteValidation(display);
        return true;
    }
}

function displayValidationMessage(element, message) {
    const id = element.id;
    const elementId = id.concat("-", "validation-message")
    let validationMessageElement = document.getElementById(elementId);
    validationMessageElement.innerText = message;
    validationMessageElement.style.display = 'block'; // Show validation message
    element.classList.add('is-invalid');
}

function clearValidationMessage(element) {
    const id = element.id;
    const elementId = id.concat("-", "validation-message")
    let validationMessageElement = document.getElementById(elementId);
    validationMessageElement.innerText = '';
    validationMessageElement.style.display = 'none'; // Hide validation message
    element.classList.remove('is-invalid');
}

function displayRouteValidation(display, message) {
    const divDisplay = display + "-validation-message";
    let validationMessageElement = document.getElementById(divDisplay);
    validationMessageElement.innerText = message;
    validationMessageElement.style.display = 'block'; // Show validation message
    element.classList.add('is-invalid');
}