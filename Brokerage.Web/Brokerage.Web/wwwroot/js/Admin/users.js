document.addEventListener(
    "DOMContentLoaded",
    function() {
        const searchInput =
            document.getElementById(
                "userSearch"
            );

        const filterForm =
            document.getElementById(
                "userFilterForm"
            );

        let searchTimer;

        searchInput?.addEventListener(
            "input",
            function() {
                clearTimeout(
                    searchTimer
                );

                searchTimer =
                    setTimeout(
                        function() {
                            filterForm.submit();
                        },
                        350);
            });

        const toggleButtons =
            document.querySelectorAll(
                ".user-row-toggle, .user-role-toggle"
            );

        toggleButtons.forEach(
            function(toggleButton) {
                toggleButton.addEventListener(
                    "click",
                    function() {
                        toggleUserDetails(
                            toggleButton
                        );
                    });
            });

        const actionButtons =
            document.querySelectorAll(
                ".user-action-btn"
            );

        actionButtons.forEach(
            function(actionButton) {
                actionButton.addEventListener(
                    "click",
                    async function() {
                        const card =
                            actionButton.closest(
                                ".user-action-card"
                            );

                        if (!card) {
                            return;
                        }

                        const action =
                            actionButton.dataset
                                .statusAction;

                        if (
                            action ===
                            "cancel"
                        ) {
                            closeUserDetails(
                                card
                            );

                            return;
                        }

                        if (
                            action ===
                            "confirm"
                        ) {
                            await confirmUserStatusChange(
                                card,
                                actionButton
                            );
                        }
                    });
            });

        function toggleUserDetails(
            toggleButton
        ) {
            const userId =
                toggleButton.dataset.userId;

            const detailsRow =
                document.getElementById(
                    `user-details-${userId}`
                );

            if (!detailsRow) {
                return;
            }

            const isOpen =
                toggleButton
                    .classList
                    .contains("expanded");

            document
                .querySelectorAll(
                    ".user-row-toggle.expanded"
                )
                .forEach(
                    function(otherButton) {
                        if (
                            otherButton !==
                            toggleButton
                        ) {
                            closeToggle(
                                otherButton
                            );
                        }
                    });

            if (isOpen) {
                closeToggle(
                    toggleButton
                );
            }
            else {
                openToggle(
                    toggleButton,
                    detailsRow
                );
            }
        }

        function openToggle(
            toggleButton,
            detailsRow
        ) {
            toggleButton
                .classList
                .add("expanded");

            toggleButton.setAttribute(
                "aria-expanded",
                "true"
            );

            detailsRow.hidden =
                false;
        }

        function closeToggle(
            toggleButton
        ) {
            const userId =
                toggleButton.dataset.userId;

            const detailsRow =
                document.getElementById(
                    `user-details-${userId}`
                );

            toggleButton
                .classList
                .remove("expanded");

            toggleButton.setAttribute(
                "aria-expanded",
                "false"
            );

            if (detailsRow) {
                detailsRow.hidden =
                    true;
            }
        }

        function closeUserDetails(
            card
        ) {
            const userId =
                card.dataset.userId;

            const toggleButton =
                document.querySelector(
                    `.user-row-toggle[data-user-id="${userId}"]`
                );

            if (toggleButton) {
                closeToggle(
                    toggleButton
                );
            }
        }

        async function confirmUserStatusChange(
            card,
            actionButton
        ) {
            const userId =
                Number(
                    card.dataset.userId
                );

            const row =
                document.querySelector(
                    `.user-row[data-user-id="${userId}"]`
                );

            if (!row) {
                return;
            }

            const currentStatus =
                row.dataset.isActive ===
                "true";

            const newStatus =
                !currentStatus;

            const token =
                document.querySelector(
                    'input[name="__RequestVerificationToken"]'
                )?.value;

            if (!token) {
                alert(
                    "Security token was not found."
                );

                return;
            }

            const formData =
                new FormData();

            formData.append(
                "userId",
                userId
            );

            formData.append(
                "isActive",
                newStatus
            );

            actionButton.disabled =
                true;

            try {
                const url =
                    new URL(
                        window.location.href
                    );

                url.searchParams.set(
                    "handler",
                    "UpdateStatus"
                );

                const response =
                    await fetch(
                        url.toString(),
                        {
                            method: "POST",
                            headers: {
                                "RequestVerificationToken":
                                    token
                            },
                            body: formData
                        });

                const result =
                    await response.json();

                if (
                    !response.ok ||
                    !result.success
                ) {
                    throw new Error(
                        result.message ||
                        "Unable to update user status."
                    );
                }

                window.location.reload();
            }
            catch (error) {
                alert(
                    error.message ||
                    "Unable to update user status."
                );

                actionButton.disabled =
                    false;
            }
        }
    });