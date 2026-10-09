(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        const messageLink = document.querySelector(
            ".message-header-link[data-unread-messages-url]"
        );

        if (!messageLink) {
            return;
        }

        const indicator = messageLink.querySelector(
            ".message-unread-indicator"
        );
        const unreadUrl = messageLink.dataset.unreadMessagesUrl;
        const scope = messageLink.dataset.messageIndicatorScope || "default";

        if (!indicator || !unreadUrl) {
            return;
        }

        const storageKey = `estateflow-message-seen-count-${scope}`;
        const isMessagesPage = window.location.pathname
            .toLowerCase()
            .includes("/messages");

        let seenUnreadCount = readSeenCount();
        let initializedOnMessagesPage = false;

        function readSeenCount() {
            try {
                const stored = window.localStorage.getItem(storageKey);
                if (stored === null) {
                    return 0;
                }

                const count = Number(stored);
                return Number.isFinite(count) && count >= 0 ? count : 0;
            } catch {
                return 0;
            }
        }

        function saveSeenCount(count) {
            try {
                window.localStorage.setItem(storageKey, String(count));
            } catch {
                // The indicator still works for the current page if storage is unavailable.
            }
        }

        function setIndicatorVisible(visible) {
            indicator.hidden = !visible;
        }

        async function checkUnreadMessages() {
            try {
                const response = await fetch(unreadUrl, {
                    method: "GET",
                    headers: { "Accept": "application/json" },
                    cache: "no-store",
                    credentials: "same-origin"
                });

                if (!response.ok) {
                    throw new Error("Unable to check unread messages.");
                }

                const result = await response.json();
                const unreadCount = Math.max(0, Number(result.unreadCount) || 0);

                // Opening the Messages section clears the visual indicator.
                // Remember the current count once, so a later increase shows the dot again.
                if (isMessagesPage) {
                    if (!initializedOnMessagesPage) {
                        seenUnreadCount = unreadCount;
                        saveSeenCount(seenUnreadCount);
                        initializedOnMessagesPage = true;
                    }

                    setIndicatorVisible(false);
                    return;
                }

                // If reading a conversation reduced the unread count, update the baseline.
                if (unreadCount < seenUnreadCount) {
                    seenUnreadCount = unreadCount;
                    saveSeenCount(seenUnreadCount);
                }

                setIndicatorVisible(unreadCount > seenUnreadCount);
            } catch (error) {
                console.error("Message indicator check failed:", error);
            }
        }

        // Hide the dot immediately when the user clicks the Messages icon.
        messageLink.addEventListener("click", function () {
            setIndicatorVisible(false);
        });

        checkUnreadMessages();
        window.setInterval(checkUnreadMessages, 5000);
    });
})();
