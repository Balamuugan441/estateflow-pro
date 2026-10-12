document.addEventListener('DOMContentLoaded', () => {
    const fromDateInput = document.querySelector('input[name="fromDate"]');
    const toDateInput = document.querySelector('input[name="toDate"]');
    const minPriceInput = document.getElementById('minPrice') || document.querySelector('input[name="minPrice"]');
    const maxPriceInput = document.getElementById('maxPrice') || document.querySelector('input[name="maxPrice"]');
    const priceErrorMessage = document.getElementById('priceErrorMessage');
    const filterForm = document.querySelector('.transactions-filter-card');

    // 1. Dynamic Date Restriction Logic
    const updateToDateMinConstraint = () => {
        if (!fromDateInput || !toDateInput) return;

        if (fromDateInput.value) {
            toDateInput.min = fromDateInput.value;

            if (toDateInput.value && toDateInput.value < fromDateInput.value) {
                toDateInput.value = fromDateInput.value;
            }
        } else {
            toDateInput.removeAttribute('min');
        }
    };

    updateToDateMinConstraint();

    if (fromDateInput) {
        fromDateInput.addEventListener('change', updateToDateMinConstraint);
        fromDateInput.addEventListener('input', updateToDateMinConstraint);
    }

    // 2. Prevent Negative Numbers on Price Fields
    const priceInputs = [minPriceInput, maxPriceInput].filter(Boolean);

    priceInputs.forEach(input => {
        // Block typing minus key (-)
        input.addEventListener('keydown', (e) => {
            if (e.key === '-' || e.key === 'e') {
                e.preventDefault();
            }
        });

        // Clear value if a negative number is somehow pasted/entered
        input.addEventListener('input', () => {
            if (input.value !== '' && parseFloat(input.value) < 0) {
                input.value = '';
            }
            validatePriceRange();
        });
    });

    // 3. Price Range Validation (Min <= Max)
    const validatePriceRange = () => {
        if (!minPriceInput || !maxPriceInput) return true;

        const minVal = parseFloat(minPriceInput.value);
        const maxVal = parseFloat(maxPriceInput.value);

        if (!isNaN(minVal) && !isNaN(maxVal) && minVal > maxVal) {
            if (priceErrorMessage) {
                priceErrorMessage.style.display = 'block';
            }
            return false;
        }

        if (priceErrorMessage) {
            priceErrorMessage.style.display = 'none';
        }
        return true;
    };

    // Form submit validation
    if (filterForm) {
        filterForm.addEventListener('submit', (e) => {
            let isValid = true;

            // Date check
            if (fromDateInput && toDateInput && fromDateInput.value && toDateInput.value) {
                if (new Date(fromDateInput.value) > new Date(toDateInput.value)) {
                    isValid = false;
                }
            }

            // Price check
            if (!validatePriceRange()) {
                isValid = false;
            }

            if (!isValid) {
                e.preventDefault();
            }
        });
    }

    // 4. Action Dropdown Toggle Logic
    const actionButtons = document.querySelectorAll('[data-action-toggle]');

    actionButtons.forEach(button => {
        button.addEventListener('click', (e) => {
            e.stopPropagation();
            const parentTd = button.closest('td');
            const dropdown = parentTd.querySelector('.action-dropdown');

            document.querySelectorAll('.action-dropdown').forEach(menu => {
                if (menu !== dropdown) {
                    menu.classList.add('hidden');
                }
            });

            dropdown.classList.toggle('hidden');
        });
    });

    // Close action dropdowns on clicking outside
    document.addEventListener('click', () => {
        document.querySelectorAll('.action-dropdown').forEach(menu => {
            menu.classList.add('hidden');
        });
    });
});