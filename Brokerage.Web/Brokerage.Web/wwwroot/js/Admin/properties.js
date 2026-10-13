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
document.addEventListener("DOMContentLoaded", () => {
    const card = document.getElementById("statusCard");
    const form = document.getElementById("statusForm");

    document.querySelectorAll(".status-change-row").forEach(row => {
        row.addEventListener("click", () => {
            const current = row.dataset.listingStatus;
            const target = current === "Approved"
                ? "Rejected"
                : "Approved";

            document.getElementById("propertyId").value =
                row.dataset.propertyId;

            document.getElementById("targetStatus").value = target;

            document.getElementById("statusMessage").textContent =
                `Do you want to ${target === "Rejected"
                    ? "reject" : "approve"} "${row.dataset.propertyTitle}"?`;

            card.hidden = false;
        });
    });

    document.getElementById("cancelStatus").addEventListener("click", () => {
        card.hidden = true;
    });

    form.addEventListener("submit", async event => {
        event.preventDefault();

        const response = await fetch(form.action, {
            method: "POST",
            body: new FormData(form)
        });

        if (response.ok) {
            window.location.reload();
        } else {
            alert("Unable to update the property status.");
        }
    });
});