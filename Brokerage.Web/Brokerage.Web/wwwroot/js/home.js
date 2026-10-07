document.addEventListener("DOMContentLoaded", function () {

    /*
     * Login Modal Logic
     */

    const modal = document.getElementById("loginModalOverlay");
    const closeBtn = document.getElementById("modalCloseBtn");
    const progressBar = document.getElementById("modalProgressBar");

    let hideTimer = null;
    const DISAPPEAR_DELAY = 1800;

    function showModal() {
        if (!modal || !progressBar) {
            return;
        }

        if (hideTimer) {
            clearTimeout(hideTimer);
            hideTimer = null;
        }

        progressBar.style.transition = "none";
        progressBar.style.width = "100%";

        modal.classList.add("active");

        setTimeout(function () {
            progressBar.style.transition = `width ${DISAPPEAR_DELAY}ms linear`;
            progressBar.style.width = "0%";
        }, 20);

        hideTimer = setTimeout(function () {
            hideModal();
        }, DISAPPEAR_DELAY);
    }

    function hideModal() {
        if (!modal || !progressBar) {
            return;
        }

        if (hideTimer) {
            clearTimeout(hideTimer);
            hideTimer = null;
        }

        modal.classList.remove("active");

        progressBar.style.transition = "none";
        progressBar.style.width = "100%";
    }

    closeBtn?.addEventListener("click", function (event) {
        event.stopPropagation();
        hideModal();
    });

    modal?.addEventListener("click", function (event) {
        if (event.target === modal) {
            hideModal();
        }
    });

    /*
     * Property display & fetching
     */

    const propertyGrid = document.getElementById("propertyGrid");
    const propertyPills = document.querySelectorAll(".pill-btn");

    /*
     * Load properties for selected city
     */

    async function loadProperties(city) {
        if (!propertyGrid) {
            return;
        }

        propertyGrid.innerHTML = `
            <div class="property-loading">
                Loading properties...
            </div>
        `;

        try {
            const url = city === ""
                ? "/?handler=Properties"
                : `/?handler=Properties&city=${encodeURIComponent(city)}`;

            const response = await fetch(url, {
                method: "GET",
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!response.ok) {
                throw new Error("Unable to load properties.");
            }

            const properties = await response.json();
            renderProperties(properties);

        } catch (error) {
            console.error("Property loading error:", error);

            propertyGrid.innerHTML = `
                <div class="property-empty-state">
                    Unable to load properties.
                </div>
            `;
        }
    }

    /*
     * City filter buttons (Explicity allowed)
     */

    propertyPills.forEach(function (pill) {
        pill.addEventListener("click", function (event) {
            /*
             * Prevent filter pill clicks from bubbling up
             * to any parent card click handlers.
             */
            event.stopPropagation();

            propertyPills.forEach(function (item) {
                item.classList.remove("active");
            });

            pill.classList.add("active");

            const city = pill.dataset.city || "";
            loadProperties(city);
        });
    });

    /*
     * Render property cards
     */

    function renderProperties(properties) {
        if (!propertyGrid) {
            return;
        }

        if (!properties || properties.length === 0) {
            propertyGrid.innerHTML = `
                <div class="property-empty-state">
                    No approved properties found.
                </div>
            `;
            return;
        }

        propertyGrid.innerHTML = properties
            .map(function (property) {
                return createPropertyCard(property);
            })
            .join("");

        // Re-attach restricted login triggers to newly created cards
        attachLoginRequiredHandlers();
    }

    /*
     * Create property card
     */

    function createPropertyCard(property) {
        const listingType = property.listingType || "PROPERTY";

        const location = property.locationAddress
            ? `${property.locationAddress}, ${property.city}`
            : property.city;

        const price = Number(property.price || 0).toLocaleString();

        const imageUrl = property.coverImagePath
            ? `/?handler=PropertyImage&path=${encodeURIComponent(property.coverImagePath)}`
            : "";

        const imageHtml = property.coverImagePath
            ? `
                <img
                    src="${escapeHtml(imageUrl)}"
                    alt="${escapeHtml(property.propertyTitle)}"
                    class="card-image"
                />
              `
            : `
                <div class="card-image-placeholder">
                    <i class="fa-solid fa-house"></i>
                </div>
              `;

        return `
            <div class="property-card">
                <div class="card-image-wrapper">
                    ${imageHtml}
                    <span class="card-badge">
                        ${escapeHtml(listingType.toUpperCase())}
                    </span>
                    <button
                        type="button"
                        class="btn-favorite"
                        aria-label="Favorite property">
                        <svg
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="2">
                            <path
                                d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z">
                            </path>
                        </svg>
                    </button>
                </div>

                <div class="card-content">
                    <div class="card-header-row">
                        <h3 class="property-title">
                            ${escapeHtml(property.propertyTitle)}
                        </h3>
                        <span class="property-price">
                            ${price}
                        </span>
                    </div>

                    <div class="property-location">
                        <svg
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="2">
                            <path
                                d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z">
                            </path>
                            <circle cx="12" cy="10" r="3"></circle>
                        </svg>
                        <span>${escapeHtml(location)}</span>
                    </div>

                    <div class="property-specs">
                        <div class="spec-item">
                            <span>${escapeHtml(property.bedrooms)} Beds</span>
                        </div>
                        <div class="spec-item">
                            <span>${property.bathrooms} Baths</span>
                        </div>
                        <div class="spec-item">
                            <span>${property.area} ${escapeHtml(property.areaUnit)}</span>
                        </div>
                    </div>

                    <a href="/Listings/Details/${property.propertyID}" class="btn-view-details">
                        View Details
                    </a>
                </div>
            </div>
        `;
    }

    /*
     * Login restriction handler
     */

    function attachLoginRequiredHandlers() {
        const protectedElements = document.querySelectorAll(
            ".property-card, " +
            ".btn-view-details, " +
            ".btn-favorite, " +
            ".property-location, " +
            ".btn-filter, " +
            ".btn-explore"
        );

        protectedElements.forEach(function (element) {
            /*
             * Never apply restriction to filter pill buttons or items within filter-pills.
             */
            if (element.closest(".filter-pills") || element.classList.contains("pill-btn")) {
                return;
            }

            if (element.dataset.loginHandlerAttached === "true") {
                return;
            }

            element.dataset.loginHandlerAttached = "true";

            element.addEventListener("click", function (event) {
                /*
                 * Safety check: allow clicks inside filter pills to go through cleanly.
                 */
                if (event.target.closest(".filter-pills") || event.target.closest(".pill-btn")) {
                    return;
                }

                event.preventDefault();
                event.stopPropagation();

                showModal();
            });
        });
    }

    /*
     * Initial execution
     */

    attachLoginRequiredHandlers();
    loadProperties("");

    /*
     * HTML escaping utility
     */

    function escapeHtml(value) {
        return String(value ?? "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }

});