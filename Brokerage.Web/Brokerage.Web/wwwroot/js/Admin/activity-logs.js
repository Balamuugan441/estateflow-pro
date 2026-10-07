        document.addEventListener("DOMContentLoaded", function () {

            // Activity details modal

            const modal =
                document.getElementById(
                    "activityDetailsModal");

            const closeButton =
                document.querySelector(
                    ".details-modal-close");

            const overlay =
                document.querySelector(
                    ".activity-details-overlay");

            const detailsAction =
                document.getElementById(
                    "detailsAction");

            const detailsTimestamp =
                document.getElementById(
                    "detailsTimestamp");

            const detailsActor =
                document.getElementById(
                    "detailsActor");

            const detailsObject =
                document.getElementById(
                    "detailsObject");

            const detailsResult =
                document.getElementById(
                    "detailsResult");

            const detailsDescription =
                document.getElementById(
                    "detailsDescription");

            const detailButtons =
                document.querySelectorAll(
                    ".btn-details");

            detailButtons.forEach(
                function (button) {

                    button.addEventListener(
                        "click",
                        function () {

                            detailsAction.textContent =
                                button.dataset.action ||
                                "Activity";

                            detailsTimestamp.textContent =
                                button.dataset.timestamp ||
                                "N/A";

                            detailsActor.textContent =
                                button.dataset.actor ||
                                "System";

                            detailsObject.textContent =
                                button.dataset.object ||
                                "N/A";

                            detailsResult.textContent =
                                button.dataset.result ||
                                "N/A";

                            detailsDescription.textContent =
                                button.dataset.details ||
                                "No additional details available.";

                            modal.classList.add("show");

                            modal.setAttribute(
                                "aria-hidden",
                                "false");
                        });
                });


            function closeModal() {

                modal.classList.remove("show");

                modal.setAttribute(
                    "aria-hidden",
                    "true");
            }


            closeButton?.addEventListener(
                "click",
                closeModal);

            overlay?.addEventListener(
                "click",
                closeModal);


            document.addEventListener(
                "keydown",
                function (event) {

                    if (event.key === "Escape") {
                        closeModal();
                    }
                });


            // Date range validation

            const startDate =
                document.getElementById("FromDate");

            const endDate =
                document.getElementById("ToDate");

            const filterForm =
                document.getElementById(
                    "activityLogFilterForm");

            if (!startDate || !endDate) {
                return;
            }


            function validateDateRange() {

                if (!startDate.value) {

                    endDate.removeAttribute("min");

                    return;
                }

                endDate.min =
                    startDate.value;

                if (
                    endDate.value &&
                    endDate.value < startDate.value
                ) {
                    endDate.value =
                        startDate.value;
                }
            }


            startDate.addEventListener(
                "change",
                validateDateRange);

            startDate.addEventListener(
                "input",
                validateDateRange);


            endDate.addEventListener(
                "change",
                validateDateRange);


            filterForm?.addEventListener(
                "submit",
                function (event) {

                    if (
                        startDate.value &&
                        endDate.value &&
                        endDate.value < startDate.value
                    ) {
                        event.preventDefault();

                        endDate.value =
                            startDate.value;

                        endDate.focus();
                    }
                });


            validateDateRange();
        });