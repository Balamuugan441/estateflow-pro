document.addEventListener("DOMContentLoaded", function () {
    const filterForm = document.getElementById("propertiesFilterForm") || document.querySelector("form[method='get']");
    const minPriceInput = document.getElementById("MinPrice") || document.querySelector("input[name='MinPrice']");
    const maxPriceInput = document.getElementById("MaxPrice") || document.querySelector("input[name='MaxPrice']");
    const priceErrorMessage = document.getElementById("priceErrorMessage");

    if (!filterForm) return;

    // Prevent negative numbers and scientific notation (e/E/-)
    const priceInputs = [minPriceInput, maxPriceInput].filter(Boolean);

    priceInputs.forEach(input => {
        // Block typing minus (-) or scientific notation (e/E)
        input.addEventListener("keydown", function (e) {
            if (e.key === "-" || e.key === "e" || e.key === "E") {
                e.preventDefault();
            }
        });

        // Sanitize pasted or typed negative values
        input.addEventListener("input", function () {
            if (this.value !== "" && parseFloat(this.value) < 0) {
                this.value = "";
            }
            validatePriceRange();
        });
    });

    // Price range validation: displays red inline text if Min > Max
    function validatePriceRange() {
        const minVal = parseFloat(minPriceInput?.value);
        const maxVal = parseFloat(maxPriceInput?.value);

        if (!isNaN(minVal) && !isNaN(maxVal) && minVal > maxVal) {
            if (priceErrorMessage) {
                priceErrorMessage.style.display = "block";
            }
            return false;
        }

        if (priceErrorMessage) {
            priceErrorMessage.style.display = "none";
        }
        return true;
    }

    // Form submit validation check
    filterForm.addEventListener("submit", function (event) {
        if (!validatePriceRange()) {
            event.preventDefault();
        }
    });
});