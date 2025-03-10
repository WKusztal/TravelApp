document.addEventListener("DOMContentLoaded", function () {
    const filterValueInput = document.getElementById("filterValue");
    const filtersSection = document.getElementById("filtersSection");
    const sortDropdownItems = document.querySelectorAll(".dropdown-item");
    const toggleFilterButtons = document.querySelectorAll(".show-filter-option");

    if (filterValueInput.value.trim()) {
        console.log("Aktywny filtr - sekcja widoczna");
        filtersSection.classList.remove("hidden");
    }

    toggleFilterButtons.forEach((button) => {
        button.addEventListener("click", function () {
            const filterType = button.dataset.filter;

            if (filterType === "tag") {
                filtersSection.classList.remove("hidden");
                console.log("Pokazanie sekcji 'Filtruj po tagu'");
            }
        });
    });

    sortDropdownItems.forEach((item) => {
        item.addEventListener("click", function (event) {
            if (!item.classList.contains("show-filter-option")) {
                filtersSection.classList.add("hidden");
                console.log("Ukrywanie sekcji filtrów przy wyborze innej opcji");
            }
        });
    });
});
