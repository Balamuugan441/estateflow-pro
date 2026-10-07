document.addEventListener('DOMContentLoaded', () => {
    const fromDateInput = document.querySelector('input[name="fromDate"]');
    const toDateInput = document.querySelector('input[name="toDate"]');
    const filterForm = document.querySelector('.transactions-filter-card');

    // 1. Dynamic Date Restriction Logic
    const updateToDateMinConstraint = () => {
        if (!fromDateInput || !toDateInput) return;

        if (fromDateInput.value) {
            // Set min property on toDate input to grey out earlier dates in the UI calendar picker
            toDateInput.min = fromDateInput.value;

            // Reset or adjust toDate if current value is earlier than selected start date
            if (toDateInput.value && toDateInput.value < fromDateInput.value) {
                toDateInput.value = fromDateInput.value;
            }
        } else {
            toDateInput.removeAttribute('min');
        }
    };

    // Apply constraint on initial page load (if query string values exist)
    updateToDateMinConstraint();

    // Re-apply when start date changes
    if (fromDateInput) {
        fromDateInput.addEventListener('change', updateToDateMinConstraint);
        fromDateInput.addEventListener('input', updateToDateMinConstraint);
    }

    // Form submit validation fallback
    if (filterForm && fromDateInput && toDateInput) {
        filterForm.addEventListener('submit', (e) => {
            if (fromDateInput.value && toDateInput.value) {
                if (new Date(fromDateInput.value) > new Date(toDateInput.value)) {
                    e.preventDefault();
                    alert('Completion "From Date" cannot be later than "To Date".');
                }
            }
        });
    }

    // 2. Action Dropdown Toggle Logic
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