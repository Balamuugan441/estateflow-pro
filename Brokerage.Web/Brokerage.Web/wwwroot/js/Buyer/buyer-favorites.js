document.addEventListener("DOMContentLoaded", function () {
    attachPropertyPreviewHandlers();
    attachFavoriteRemovalHandlers();
});

function attachPropertyPreviewHandlers() {
    const previewElement = document.getElementById("buyerFavoritePreviewUrl");

    if (!previewElement) {
        return;
    }

    const previewUrl = previewElement.dataset.url;

    document.querySelectorAll(".favorites-property-card").forEach(function (card) {
        card.addEventListener("click", function (event) {
            if (event.target.closest(".btn-favorite")) {
                return;
            }

            const propertyGuid = card.dataset.propertyGuid;

            if (!propertyGuid) {
                return;
            }

            window.location.href = `${previewUrl}?propertyGuid=${encodeURIComponent(propertyGuid)}&source=favorites`;
        });

        card.addEventListener("keydown", function (event) {
            if (event.key !== "Enter" && event.key !== " ") {
                return;
            }

            if (event.target.closest(".btn-favorite")) {
                return;
            }

            event.preventDefault();

            const propertyGuid = card.dataset.propertyGuid;

            if (!propertyGuid) {
                return;
            }

            window.location.href = `${previewUrl}?propertyGuid=${encodeURIComponent(propertyGuid)}`;
        });
    });
}

function getRequestVerificationToken() {
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    return token?.value || "";
}

function attachFavoriteRemovalHandlers() {
    document.querySelectorAll(".favorites-property-card .btn-favorite").forEach(function (button) {
        button.addEventListener("click", function (event) {
            event.preventDefault();
            event.stopPropagation();
            removeFavorite(button);
        });
    });
}

async function removeFavorite(button) {
    const propertyGuid = button.dataset.propertyGuid;

    if (!propertyGuid) {
        return;
    }

    const card = button.closest(".favorites-property-card");

    if (!card) {
        return;
    }

    button.disabled = true;

    const body = new URLSearchParams();
    body.set("propertyGuid", propertyGuid);

    try {
        const response = await fetch("?handler=Favorite", {
            method: "POST",
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8",
                "RequestVerificationToken": getRequestVerificationToken()
            },
            body: body.toString()
        });

        const result = await response.json();

        if (!response.ok || !result.success) {
            throw new Error(result.message || "Unable to remove favorite.");
        }
        window.showFavoriteToast(
    "Success",
    "Property removed successfully from favorites."
);

        card.remove();

        updateFavoriteCount();
    } catch (error) {
        console.error("Favorite removal error:", error);
        button.disabled = false;
    }
}

function updateFavoriteCount() {
    const cards = document.querySelectorAll(".favorites-property-card");

    // Update standard count text if present
    const countElement = document.querySelector(".favorites-count");
    if (countElement) {
        const count = cards.length;
        countElement.textContent = `${count} ${count === 1 ? "Property" : "Properties"}`;
    }

    // Update badge elements if using the enhanced UI
    const countNumberEl = document.querySelector(".favorites-count-badge .count-number");
    const countLabelEl = document.querySelector(".favorites-count-badge .count-label");
    if (countNumberEl && countLabelEl) {
        const count = cards.length;
        countNumberEl.textContent = count;
        countLabelEl.textContent = count === 1 ? "Property Saved" : "Properties Saved";
    }

    // Smart Pagination Redirection when current page becomes empty
    if (cards.length === 0) {
        const urlParams = new URLSearchParams(window.location.search);
        const currentPage = parseInt(urlParams.get("pageNumber") || "1", 10);

        if (currentPage > 1) {
            // Redirect to the previous page where remaining items exist
            urlParams.set("pageNumber", currentPage - 1);
            window.location.href = `${window.location.pathname}?${urlParams.toString()}`;
        } else {
            // On Page 1, reload to let Razor render the true empty state
            window.location.reload();
        }
    }
}