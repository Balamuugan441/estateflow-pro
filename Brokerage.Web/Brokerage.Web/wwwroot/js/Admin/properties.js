document.addEventListener("DOMContentLoaded", function () {
    const filterForm = document.getElementById("propertiesFilterForm") || document.querySelector("form[method='get']");
    const minPriceInput = document.getElementById("MinPrice") || document.querySelector("input[name='MinPrice']");
    const maxPriceInput = document.getElementById("MaxPrice") || document.querySelector("input[name='MaxPrice']");
    const sortBySelect = document.getElementById("SortBy") || document.querySelector("select[name='SortBy']");

    if (!filterForm) return;

    // Price range validation: MinPrice cannot be greater than MaxPrice
    function validatePriceRange() {
        const minVal = parseFloat(minPriceInput?.value);
        const maxVal = parseFloat(maxPriceInput?.value);

        if (!isNaN(minVal) && !isNaN(maxVal) && minVal > maxVal) {
            alert("Minimum price cannot be greater than Maximum price.");
            minPriceInput.focus();
            return false;
        }
        return true;
    }

    // Intercept form submit to perform validation
    filterForm.addEventListener("submit", function (event) {
        if (!validatePriceRange()) {
            event.preventDefault();
        }
    });

    // Auto-submit filter form when Sort By dropdown value changes
    if (sortBySelect) {
        sortBySelect.addEventListener("change", function () {
            if (validatePriceRange()) {
                filterForm.submit();
            }
        });
    }
});