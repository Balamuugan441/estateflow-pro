document.addEventListener(
    "DOMContentLoaded",
    function () {

        const notificationButton =
            document.querySelector(
                ".notification-center-button"
            );

        if (!notificationButton) {
            return;
        }


        const unreadUrl =
            notificationButton.dataset.unreadUrl;

        const notificationsUrl =
            notificationButton.dataset.notificationsUrl;

        const readUrl =
            notificationButton.dataset.readUrl;

        const scope =
            notificationButton.dataset.notificationScope ||
            "default";


        const popup =
            document.getElementById(
                "notificationPopup"
            );

        const popupTitle =
            document.getElementById(
                "notificationPopupTitle"
            );

        const popupMessage =
            document.getElementById(
                "notificationPopupMessage"
            );

        const popupCount =
            document.getElementById(
                "notificationPopupCount"
            );

        const popupView =
            document.getElementById(
                "notificationPopupView"
            );

        const popupDismiss =
            document.getElementById(
                "notificationPopupDismiss"
            );

        const popupClose =
            document.getElementById(
                "notificationPopupClose"
            );

        const popupBackdrop =
            document.querySelector(
                "[data-notification-close]"
            );

        const badge =
            document.querySelector(
                ".notification-center-badge"
            );


        /*
         * The newest notification ID is used only
         * to prevent the same popup from appearing
         * repeatedly.
         */
        const storageKey =
            `estateflow-notification-${scope}`;


        function getUnreadNotifications()
        {
            return fetch(
                unreadUrl,
                {
                    method: "GET",

                    headers: {
                        "Accept":
                            "application/json"
                    },

                    cache: "no-store"
                }
            )
                .then(
                    function (response)
                    {
                        if (!response.ok)
                        {
                            throw new Error(
                                "Unable to load notifications."
                            );
                        }

                        return response.json();
                    }
                );
        }


        function updateBadge(count)
        {
            if (!badge) {
                return;
            }

            badge.textContent =
                count > 99
                    ? "99+"
                    : count;

            badge.hidden =
                count === 0;
        }


        function getLastPromptedNotificationId()
        {
            return Number(
                localStorage.getItem(
                    storageKey
                ) || "0"
            );
        }


        function setLastPromptedNotificationId(
            notificationId
        )
        {
            localStorage.setItem(
                storageKey,
                String(notificationId)
            );
        }


        function showNewNotificationPopup(
            notifications
        )
        {
            if (
                !popup ||
                notifications.length === 0
            ) {
                return;
            }


            /*
             * Do not show the popup while the user
             * is already on the Notifications page.
             */
            if (
                window.location.pathname
                    .toLowerCase()
                    .includes("/notifications")
            ) {
                return;
            }


            const newestNotificationId =
                Number(
                    notifications[0]
                        .notificationID
                );


            const lastPromptedId =
                getLastPromptedNotificationId();


            if (
                newestNotificationId <=
                lastPromptedId
            ) {
                return;
            }


            setLastPromptedNotificationId(
                newestNotificationId
            );


            popupTitle.textContent =
                "You Have New Notifications";


            popupMessage.textContent =
                "You have new notifications. Tap to view them or dismiss.";


            if (popupCount)
            {
                popupCount.textContent =
                    notifications.length === 1
                        ? "1 new notification"
                        : `${notifications.length} new notifications`;
            }


            popup.hidden = false;
        }


        function closePopup()
        {
            if (!popup) {
                return;
            }

            popup.hidden = true;
        }


        async function checkNotifications()
        {
            try
            {
                const notifications =
                    await getUnreadNotifications();

                updateBadge(
                    notifications.length
                );

                showNewNotificationPopup(
                    notifications
                );
            }
            catch (error)
            {
                console.error(
                    "Notification polling error:",
                    error
                );
            }
        }


        popupClose?.addEventListener(
            "click",
            closePopup
        );


        popupDismiss?.addEventListener(
            "click",
            closePopup
        );


        popupBackdrop?.addEventListener(
            "click",
            closePopup
        );


        popupView?.addEventListener(
            "click",
            function ()
            {
                if (notificationsUrl)
                {
                    window.location.href =
                        notificationsUrl;
                }
            }
        );


        /*
         * Clicking the notification bell always
         * opens the notification history page.
         */
        notificationButton.addEventListener(
            "click",
            function ()
            {
                if (notificationsUrl)
                {
                    window.location.href =
                        notificationsUrl;
                }
            }
        );


        /*
         * Mark individual notification cards as read.
         */
        document
            .querySelectorAll(
                ".notification-card[data-notification-id]"
            )
            .forEach(
                function (card)
                {
                    card.addEventListener(
                        "click",
                        async function ()
                        {
                            const notificationId =
                                card.dataset
                                    .notificationId;

                            if (
                                !notificationId ||
                                !readUrl ||
                                card.dataset.unread !== "true"
                            ) {
                                return;
                            }


                            try
                            {
                                const body =
                                    new URLSearchParams();

                                body.set(
                                    "notificationId",
                                    notificationId
                                );


                                const token =
                                    document.querySelector(
                                        'input[name="__RequestVerificationToken"]'
                                    )?.value || "";


                                const response =
                                    await fetch(
                                        readUrl,
                                        {
                                            method: "POST",

                                            headers: {
                                                "Accept":
                                                    "application/json",

                                                "Content-Type":
                                                    "application/x-www-form-urlencoded; charset=UTF-8",

                                                "RequestVerificationToken":
                                                    token
                                            },

                                            body:
                                                body.toString()
                                        }
                                    );


                                if (!response.ok)
                                {
                                    throw new Error(
                                        "Unable to mark notification as read."
                                    );
                                }


                                card.dataset.unread =
                                    "false";

                                card.classList.remove(
                                    "notification-unread"
                                );


                                const unreadDot =
                                    card.querySelector(
                                        ".unread-dot"
                                    );

                                unreadDot?.remove();
                            }
                            catch (error)
                            {
                                console.error(
                                    "Notification read error:",
                                    error
                                );
                            }
                        }
                    );
                }
            );


        checkNotifications();


        setInterval(
            checkNotifications,
            10000
        );
    }
);