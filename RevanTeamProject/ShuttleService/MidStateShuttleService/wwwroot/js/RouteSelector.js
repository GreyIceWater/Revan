


document.addEventListener("change", async function (e) {

    if (
        !e.target.name.includes("PickUpLocationID") &&
        !e.target.name.includes("DropOffLocationID")
    ) return

    const row = e.target.closest(".ride-row")

    const pickup = row.querySelector("[name*='PickUpLocationID']").value
    const dropoff = row.querySelector("[name*='DropOffLocationID']").value

    if (!pickup || !dropoff) return

    const routeDropdown = row.querySelector(".route-select")

    const response = await fetch(`/Routes/GetRoutes?pickupId=${pickup}&dropoffId=${dropoff}`)
    const routes = await response.json()

    routeDropdown.innerHTML = `<option value="">Select Route (Optional)</option>`

    routes.forEach(r => {
        routeDropdown.innerHTML += `<option value="${r.id}">${r.pickupTime} → ${r.dropoffTime}</option>`
    })
})

document.addEventListener("change", function (e) {

    if (!e.target.classList.contains("route-select")) return

    const row = e.target.closest(".ride-row")
    const timeSelect = row.querySelector(".time-select")

    if (e.target.value !== "")
        timeSelect.disabled = true
    else
        timeSelect.disabled = false
})

document.addEventListener("change", function (e) {

    if (!e.target.classList.contains("time-select")) return

    const row = e.target.closest(".ride-row")
    const routeSelect = row.querySelector(".route-select")

    if (e.target.value !== "")
        routeSelect.disabled = true
    else
        routeSelect.disabled = false
})