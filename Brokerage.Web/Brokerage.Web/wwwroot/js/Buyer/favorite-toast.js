(function () {
    let hideTimer;
    let removeTimer;

    window.showFavoriteToast = function (
        title,
        message,
        isError = false
    ) {
        const toast = document.getElementById("favoriteToast");
        const icon = document.getElementById("favoriteToastIcon");
        const titleElement = document.getElementById("favoriteToastTitle");
        const messageElement = document.getElementById("favoriteToastMessage");

        if (!toast || !icon || !titleElement || !messageElement) {
            return;
        }

        clearTimeout(hideTimer);
        clearTimeout(removeTimer);

        toast.classList.remove("show", "hide", "error");

        if (isError) {
            toast.classList.add("error");
            icon.textContent = "!";
        } else {
            icon.textContent = "✓";
        }

        titleElement.textContent = title;
        messageElement.textContent = message;

        // Restart the entry animation.
        void toast.offsetWidth;

        toast.classList.add("show");

        hideTimer = setTimeout(function () {
            toast.classList.remove("show");
            toast.classList.add("hide");
        }, 2200);

        removeTimer = setTimeout(function () {
            toast.classList.remove("hide", "error");
        }, 2550);
    };
})();