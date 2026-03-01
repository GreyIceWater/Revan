(function () {
    const searchEl = document.getElementById("regSearch");
    const filterEl = document.getElementById("regFilter");
    const sortEl = document.getElementById("regSort");
    const countEl = document.getElementById("regCount");

    const cards = Array.from(document.querySelectorAll(".reg-card"));

    function normalize(s) {
        return (s || "").toString().trim().toLowerCase();
    }

    function matchesSearch(card, q) {
        if (!q) return true;
        const name = normalize(card.dataset.name);
        const sid = normalize(card.dataset.studentid);
        const term = normalize(card.dataset.term);
        return name.includes(q) || sid.includes(q) || term.includes(q);
    }

    function matchesFilter(card, filter) {
        const isCustom = card.dataset.iscustom === "true";
        if (filter === "custom") return isCustom;
        if (filter === "standard") return !isCustom;
        return true;
    }

    function applySort(sortMode) {
        const parent = cards[0]?.parentElement;
        if (!parent) return;

        const visibleCards = cards.filter(c => c.style.display !== "none");

        visibleCards.sort((a, b) => {
            const aCreated = parseInt(a.dataset.created || "0", 10);
            const bCreated = parseInt(b.dataset.created || "0", 10);
            const aRides = parseInt(a.dataset.rides || "0", 10);
            const bRides = parseInt(b.dataset.rides || "0", 10);

            switch (sortMode) {
                case "oldest": return aCreated - bCreated;
                case "newest": return bCreated - aCreated;
                case "ridesAsc": return aRides - bRides;
                case "ridesDesc": return bRides - aRides;
                default: return 0;
            }
        });

        visibleCards.forEach(c => parent.appendChild(c));
    }

    function update() {
        const q = normalize(searchEl.value);
        const f = filterEl.value;

        let shown = 0;

        cards.forEach(card => {
            const show = matchesSearch(card, q) && matchesFilter(card, f);
            card.style.display = show ? "" : "none";
            if (show) shown++;
        });

        applySort(sortEl.value);

        if (countEl) {
            countEl.textContent = `${shown} of ${cards.length} request(s) shown`;
        }
    }

    if (searchEl) searchEl.addEventListener("input", update);
    if (filterEl) filterEl.addEventListener("change", update);
    if (sortEl) sortEl.addEventListener("change", update);

    update();
})();