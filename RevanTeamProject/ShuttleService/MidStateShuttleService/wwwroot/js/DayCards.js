let dayIndex = 0

document.getElementById("addDayBtn").addEventListener("click", () => {
    const template = document.getElementById("dayCardTemplate").innerHTML
        .replaceAll("__dayIndex__", dayIndex)

    const wrapper = document.createElement("div")
    wrapper.innerHTML = template

    const card = wrapper.firstElementChild
    card.dataset.rideCount = 0

    document.getElementById("dayCardsContainer").appendChild(card)
    dayIndex++
})

document.addEventListener("click", function (e) {
    if (!e.target.classList.contains("addRideBtn")) return

    const card = e.target.closest(".day-card")
    const dayIdx = card.dataset.dayIndex
    const rideIdx = card.dataset.rideCount

    let template = document.getElementById("rideTemplate").innerHTML
        .replaceAll("__dayIndex__", dayIdx)
        .replaceAll("__rideIndex__", rideIdx)

    const wrapper = document.createElement("div")
    wrapper.innerHTML = template

    const rideRow = wrapper.firstElementChild
    card.querySelector(".ridesContainer").appendChild(rideRow)

    card.dataset.rideCount++
})

document.addEventListener("click", function (e) {
    if (!e.target.classList.contains("removeDayBtn")) return

    const card = e.target.closest(".day-card")
    card.remove()
})

document.addEventListener("click", function (e) {
    if (!e.target.classList.contains("removeRideBtn")) return

    const rideRow = e.target.closest(".ride-row")
    rideRow.remove()
})