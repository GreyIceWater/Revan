// Wait until the page is fully loaded
document.addEventListener("DOMContentLoaded", () => {

    // Listen for changes anywhere in the document
    // This automatically supports dynamically added rides
    document.addEventListener("change", handleChange);

    // Initialize routes for rides that already exist on the page
    setTimeout(initializeExistingRides, 50);
});


/*
 Handles all change events and determines what action to take.
*/
async function handleChange(e) {
    const target = e.target;

    if (target.classList.contains("route-select")) {
        toggleField(target, ".time-select");
        return;
    }

    if (target.classList.contains("time-select")) {
        toggleField(target, ".route-select");
        return;
    }

    // If pickup, dropoff, or day select changed → refresh routes
    if (
        target.name?.includes("PickUpLocationID") ||
        target.name?.includes("DropOffLocationID") ||
        target.classList.contains("day-select")
    ) {
        // If it's a day select, refresh all rides under this day
        if (target.classList.contains("day-select")) {
            const dayCard = target.closest(".day-card");
            const rows = dayCard.querySelectorAll(".ride-row");
            rows.forEach(row => updateRoutes(row));
        } else {
            const row = target.closest(".ride-row");
            if (row) updateRoutes(row);
        }
    }
}


/*
 Enables/disables the opposite dropdown so users
 cannot choose both a route and a manual time.
*/
function toggleField(source, selector) {

    const row = source.closest(".ride-row");
    if (!row) return;

    const other = row.querySelector(selector);
    if (!other) return;

    other.disabled = source.value !== "";
}


/*
 Fetch routes based on pickup, dropoff, and weekday.
*/
async function updateRoutes(source) {

    // Get the ride row
    const row = source.closest?.(".ride-row") || source;
    if (!row) return;

    // Get parent day card
    const dayCard = row.closest(".day-card");
    if (!dayCard) return;

    // Get weekday
    const weekday = dayCard.querySelector(".day-select")?.value;
    if (!weekday) return;

    // Get pickup/dropoff
    const pickup = row.querySelector("[name*='PickUpLocationID']")?.value;
    const dropoff = row.querySelector("[name*='DropOffLocationID']")?.value;

    if (!pickup || !dropoff) return;

    const routeDropdown = row.querySelector(".route-select");
    if (!routeDropdown) return;

    // Save currently selected route
    const selectedRoute = routeDropdown.value;

    // Show loading state
    routeDropdown.innerHTML = `<option value="">Loading routes...</option>`;

    // Build API query
    const params = new URLSearchParams({
        pickupId: pickup,
        dropoffId: dropoff,
        dayOfWeek: weekday
    });

    const url = `${window.location.origin}/Routes/GetRoutes?${params}`;

    try {

        const response = await fetch(url);

        if (!response.ok)
            throw new Error(response.statusText);

        const routes = await response.json();

        populateRoutes(routeDropdown, routes, selectedRoute);

    } catch (err) {

        console.error("Route fetch failed:", err);

        routeDropdown.innerHTML =
            `<option value="">Unable to load routes</option>`;

    }

}


/*
 Populate the route dropdown with API results.
*/
function populateRoutes(select, routes, selectedRoute) {

    select.innerHTML = "";

    const defaultOption = document.createElement("option");
    defaultOption.value = "";
    defaultOption.textContent = "Select Route (Optional)";
    select.appendChild(defaultOption);

    const fragment = document.createDocumentFragment();

    routes.forEach(route => {

        const option = document.createElement("option");

        option.value = route.id;
        option.textContent =
            `${route.pickupTime} → ${route.dropoffTime}`;

        if (route.id.toString() === selectedRoute)
            option.selected = true;

        fragment.appendChild(option);

    });

    select.appendChild(fragment);
}


/*
 When editing an existing registration, rides already have
 pickup and dropoff values but routes were rendered by Razor.

 This function refreshes them so the dropdown always reflects
 the API results.
*/
function initializeExistingRides() {

    const rows = document.querySelectorAll(".ride-row");

    rows.forEach(row => {

        const pickup = row.querySelector("[name*='PickUpLocationID']");
        const dropoff = row.querySelector("[name*='DropOffLocationID']");

        if (pickup && dropoff && pickup.value && dropoff.value) {
            updateRoutes(row);
        }

    });

}