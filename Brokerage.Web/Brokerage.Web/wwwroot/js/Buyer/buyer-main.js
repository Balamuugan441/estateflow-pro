document.addEventListener("DOMContentLoaded", function () {

    const propertyGrid =
        document.getElementById("propertyGrid");

    const propertyCount =
        document.getElementById("propertyCount");

    const paginationContainer =
        document.getElementById("paginationContainer");

    const propertySearch =
        document.getElementById("propertySearch");

    const locationSearch =
        document.getElementById("locationSearch");

    const locationType =
        document.getElementById("locationType");

    const locationValue =
        document.getElementById("locationValue");

    const locationSuggestions =
        document.getElementById("locationSuggestions");

    const minPriceInput =
        document.querySelector(
            ".price-inputs input:nth-child(1)"
        );

    const maxPriceInput =
        document.querySelector(
            ".price-inputs input:nth-child(3)"
        );

    const propertyTypeButtons =
        document.querySelectorAll(
            "[data-property-type]"
        );

    const bedroomButtons =
        document.querySelectorAll(
            ".chip-btn[data-bedrooms]"
        );

    const listingTypeFilters =
        document.querySelectorAll(
            ".listing-type-filter"
        );

    const applyFilterButton =
        document.getElementById(
            "applyFilterBtn"
        );

    const resetButton =
        document.querySelector(
            ".btn-reset"
        );

    const sortSelect =
        document.querySelector(
            ".sort-select"
        );

    let currentPage = 1;

    let searchTimer = null;

    let locationTimer = null;

    function buildSearchRequest(pageNumber) {

        const activePropertyType =
            document.querySelector(
                "[data-property-type].active"
            );

        const activeBedroom =
            document.querySelector(
                ".chip-btn[data-bedrooms].active"
            );

        const listingTypes =
            Array.from(
                document.querySelectorAll(
                    ".listing-type-filter:checked"
                )
            ).map(function (item) {
                return item.value;
            });

        const minPrice =
            parseNumber(minPriceInput?.value);

        const maxPrice =
            parseNumber(maxPriceInput?.value);
        [minPriceInput, maxPriceInput].forEach(function (input) {
            if (!input) return;

            input.addEventListener("input", function () {
                this.value = this.value.replace(/[^0-9]/g, "");
            });
        });

        const minBedrooms = activeBedroom ? Number(activeBedroom.dataset.bedrooms) : null;

        return {
            pageNumber: pageNumber,

            search:
                propertySearch?.value.trim()
                || null,

            locationType:
                locationType?.value
                || null,

            locationValue:
                locationValue?.value
                || null,

            minPrice: minPrice,

            maxPrice: maxPrice,

            propertyType:
                activePropertyType
                    ?.dataset.propertyType
                || null,

            minBedrooms:
                minBedrooms,

            listingTypes:
                listingTypes,

            sortBy:
                sortSelect?.value || "latest"
        };
    }

    async function loadProperties(pageNumber) {

        currentPage =
            pageNumber < 1
                ? 1
                : pageNumber;

        showLoading();

        const request =
            buildSearchRequest(currentPage);

        const query =
            new URLSearchParams();

        query.set(
            "pageNumber",
            request.pageNumber
        );

        addParameter(
            query,
            "search",
            request.search
        );

        addParameter(
            query,
            "locationType",
            request.locationType
        );

        addParameter(
            query,
            "locationValue",
            request.locationValue
        );

        addParameter(
            query,
            "minPrice",
            request.minPrice
        );

        addParameter(
            query,
            "maxPrice",
            request.maxPrice
        );

        addParameter(
            query,
            "propertyType",
            request.propertyType
        );

        addParameter(
            query,
            "minBedrooms",
            request.minBedrooms
        );

        request.listingTypes.forEach(
            function (listingType) {
                query.append(
                    "listingTypes",
                    listingType
                );
            }
        );

        addParameter(
            query,
            "sortBy",
            request.sortBy
        );

        try {

            const response =
                await fetch(
                    `?handler=SearchProperties&${query.toString()}`,
                    {
                        method: "GET",
                        headers: {
                            "Accept": "application/json"
                        }
                    }
                );

            if (!response.ok) {
                throw new Error(
                    "Unable to load properties."
                );
            }

            const result =
                await response.json();

            renderProperties(
                result.properties
            );

            renderPagination(
                result.pageNumber,
                result.totalPages
            );

            if (propertyCount) {
                propertyCount.textContent =
                    `${result.totalRecords} Properties`;
            }

        } catch (error) {

            console.error(
                "Buyer property loading error:",
                error
            );

            propertyGrid.innerHTML = `
                <div class="property-empty-state">
                    Unable to load properties.
                </div>
            `;

            paginationContainer.innerHTML = "";
        }
    }

    function renderProperties(properties) {

        if (!properties ||
            properties.length === 0) {

            propertyGrid.innerHTML = `
                <div class="property-empty-state">
                    No approved properties match your filters.
                </div>
            `;

            return;
        }

        propertyGrid.innerHTML =
    properties
        .map(createPropertyCard)
        .join("");

attachPropertyPreviewHandlers();

attachFavoriteHandlers();
    }

    function createPropertyCard(property) {

        const location =
            [
                property.locationAddress,
                property.city,
                property.state
            ]
                .filter(Boolean)
                .join(", ");

        const imageUrl =
            property.coverImagePath
                ? `?handler=PropertyImage&path=${encodeURIComponent(
                    property.coverImagePath
                )}`
                : "";

        const listingType =
            property.listingType || "";

        const price =
            Number(
                property.price || 0
            ).toLocaleString("en-IN");

        const priceSuffix =
            listingType.toLowerCase() ===
                "for rent"
                ? "<small>/month</small>"
                : "";

        const imageHtml =
            imageUrl
                ? `
                    <img
                        src="${escapeHtml(imageUrl)}"
                        alt="${escapeHtml(
                            property.propertyTitle
                        )}"
                        class="card-image"
                        loading="lazy" />
                  `
                : `
                    <div class="card-image">
                        No Image
                    </div>
                  `;

        return `
   <div class="property-card"
     data-property-guid="${property.propertyGUID}"
     role="link"
     tabindex="0">

                <div class="card-image-wrapper">

                    ${imageHtml}

                    <div class="badge-group">
                        <span class="badge badge-sale">
                            ${escapeHtml(
                                listingType.toUpperCase()
                            )}
                        </span>
                    </div>

                   <button
    type="button"
    class="btn-favorite ${property.isFavorite ? "is-favorite" : ""}"
    data-property-guid="${property.propertyGUID}"
    aria-label="${
        property.isFavorite
            ? "Remove property from favorites"
            : "Add property to favorites"
    }"
    aria-pressed="${property.isFavorite}">

    <svg
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        stroke-width="2">

        <path
            d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67
               l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06
               L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0
               0-7.78z">
        </path>

    </svg>
</button>

                </div>

                <div class="card-body">

                    <div class="card-header">

                        <div>

                            <h3 class="property-title">
                                ${escapeHtml(
                                    property.propertyTitle
                                )}
                            </h3>

                            <div class="property-location">

                                <svg
                                    viewBox="0 0 24 24"
                                    fill="none"
                                    stroke="currentColor"
                                    stroke-width="2">

                                    <path
                                        d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z">
                                    </path>

                                    <circle
                                        cx="12"
                                        cy="10"
                                        r="3">
                                    </circle>

                                </svg>

                                <span>
                                    ${escapeHtml(location)}
                                </span>

                            </div>

                        </div>

                        <span class="property-price">
                            ${price}
                            ${priceSuffix}
                        </span>

                    </div>

                    <div class="property-specs">

                        <div class="spec-item">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16">
                                <path d="M3 7v11M21 7v11M3 13h18M3 10h18M7 10V7a1 1 0 0 1 1-1h8a1 1 0 0 1 1 1v3"/>
                            </svg>
                            <span>
                                ${escapeHtml(property.bedrooms)} Beds
                            </span>
                        </div>

                        <div class="spec-item">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16">
                                <path d="M4 12h16a1 1 0 0 1 1 1v3a4 4 0 0 1-4 4H7a4 4 0 0 1-4-4v-3a1 1 0 0 1 1-1z"/>
                                <path d="M6 12V5a2 2 0 0 1 2-2h3v3"/>
                            </svg>
                            <span>
                                ${property.bathrooms} Baths
                            </span>
                        </div>

                        <div class="spec-item">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16">
                                <rect x="3" y="3" width="18" height="18" rx="2"/>
                                <path d="M3 9h18M9 21V9"/>
                            </svg>
                            <span>
                                ${property.area} ${escapeHtml(property.areaUnit || "")}
                            </span>
                        </div>

                    </div>

                    <div class="card-action">
                        <span>Get Full Details &rarr;</span>
                    </div>

                </div>

            </div>
        `;
    }
    function getRequestVerificationToken() {

    const token =
        document.querySelector(
            'input[name="__RequestVerificationToken"]'
        );

    return token?.value || "";
}
async function toggleFavorite(button) {

    const propertyGuid =
        button.dataset.propertyGuid;

    if (!propertyGuid) {
        return;
    }

    const currentlyFavorite =
        button.classList.contains(
            "is-favorite"
        );

    const nextFavorite =
        !currentlyFavorite;

    button.disabled = true;

    const body =
        new URLSearchParams();

    body.set(
        "propertyGuid",
        propertyGuid
    );

    body.set(
        "favorite",
        nextFavorite
    );

    try {

        const response =
            await fetch(
                "?handler=Favorite",
                {
                    method: "POST",

                    headers: {
                        "Accept": "application/json",

                        "Content-Type":
                            "application/x-www-form-urlencoded; charset=UTF-8",

                        "RequestVerificationToken":
                            getRequestVerificationToken()
                    },

                    body: body.toString()
                }
            );

        const result =
            await response.json();

        if (!response.ok ||
            !result.success) {

            throw new Error(
                result.message ||
                "Unable to update favorite."
            );
        }

        button.classList.toggle(
            "is-favorite",
            result.isFavorite
        );

        button.setAttribute(
            "aria-pressed",
            result.isFavorite
        );

        button.setAttribute(
            "aria-label",
            result.isFavorite
                ? "Remove property from favorites"
                : "Add property to favorites"
        );

    }
    catch (error) {

        console.error(
            "Favorite update error:",
            error
        );
    }
    finally {

        button.disabled = false;
    }
}
function attachFavoriteHandlers() {

    document
        .querySelectorAll(
            ".btn-favorite"
        )
        .forEach(function (button) {

            button.addEventListener(
                "click",
                function (event) {

                    event.stopPropagation();

                    toggleFavorite(
                        button
                    );
                }
            );
        });
}
    function attachPropertyPreviewHandlers() {

        const previewUrlElement =
            document.getElementById(
                "buyerPreviewUrl"
            );

        if (!previewUrlElement) {
            return;
        }

        const previewUrl =
            previewUrlElement.dataset.url;

        document
            .querySelectorAll(".property-card")
            .forEach(function (card) {

                function openPreview() {

                    const propertyGuid =
                        card.dataset.propertyGuid;

                    if (!propertyGuid) {
                        return;
                    }

                    window.location.href =
                        `${previewUrl}?propertyGuid=${encodeURIComponent(
                            propertyGuid
                        )}`;
                }

                card.addEventListener(
                    "click",
                    function (event) {

                        if (
                            event.target.closest(
                                ".btn-favorite"
                            )
                        ) {
                            return;
                        }

                        openPreview();
                    }
                );

                card.addEventListener(
                    "keydown",
                    function (event) {

                        if (
                            event.key === "Enter" ||
                            event.key === " "
                        ) {
                            event.preventDefault();

                            openPreview();
                        }
                    }
                );
            });
    }

    function renderPagination(
        pageNumber,
        totalPages
    ) {

        paginationContainer.innerHTML = "";

        if (totalPages <= 1) {
            return;
        }

        const previousButton =
            createPageButton(
                pageNumber - 1,
                "previous",
                pageNumber === 1
            );

        paginationContainer.appendChild(
            previousButton
        );

        const pageNumbers =
            getVisiblePages(
                pageNumber,
                totalPages
            );

        pageNumbers.forEach(
            function (page) {

                if (page === "...") {

                    const ellipsis =
                        document.createElement(
                            "span"
                        );

                    ellipsis.className =
                        "page-ellipsis";

                    ellipsis.textContent =
                        "...";

                    paginationContainer.appendChild(
                        ellipsis
                    );

                    return;
                }

                const button =
                    createPageButton(
                        page,
                        "page",
                        false,
                        page === pageNumber
                    );

                paginationContainer.appendChild(
                    button
                );
            }
        );

        const nextButton =
            createPageButton(
                pageNumber + 1,
                "next",
                pageNumber === totalPages
            );

        paginationContainer.appendChild(
            nextButton
        );
    }

    function getVisiblePages(
        current,
        total
    ) {

        if (total <= 0) {
            return [];
        }

        if (total <= 7) {
            return Array.from(
                { length: total },
                (_, index) => index + 1
            );
        }

        let start = current - 2;
        let end = current + 2;

        if (start <= 3) {
            start = 1;
            end = 5;
        } else if (end >= total - 2) {
            start = total - 4;
            end = total;
        }

        const pages = [1];

        if (start > 2) {
            pages.push("...");
        }

        for (let i = Math.max(2, start); i <= Math.min(total - 1, end); i++) {
            pages.push(i);
        }

        if (end < total - 1) {
            pages.push("...");
        }

        pages.push(total);

        return pages;
    }

    function createPageButton(
        page,
        type,
        disabled,
        active = false
    ) {

        const button =
            document.createElement("button");

        button.type = "button";

        button.className =
            "page-btn";

        if (type === "previous" ||
            type === "next") {

            button.classList.add(
                "page-nav"
            );
        }

        if (active) {
            button.classList.add(
                "active"
            );
        }

        button.disabled =
            disabled;

        if (type === "previous") {
            button.innerHTML = "‹";
        }
        else if (type === "next") {
            button.innerHTML = "›";
        }
        else {
            button.textContent =
                page;
        }

        if (!disabled) {

            button.addEventListener(
                "click",
                function () {
                    loadProperties(page);
                }
            );
        }

        return button;
    }

    propertyTypeButtons.forEach(
        function (button) {

            button.addEventListener(
                "click",
                function () {

                    propertyTypeButtons.forEach(
                        function (item) {
                            item.classList.remove(
                                "active"
                            );
                        }
                    );

                    button.classList.add(
                        "active"
                    );
                }
            );
        }
    );

    bedroomButtons.forEach(
        function (button) {

            button.addEventListener(
                "click",
                function () {

                    bedroomButtons.forEach(
                        function (item) {
                            item.classList.remove(
                                "active"
                            );
                        }
                    );

                    button.classList.add(
                        "active"
                    );
                }
            );
        }
    );

    applyFilterButton?.addEventListener(
        "click",
        function () {
            loadProperties(1);
        }
    );

    resetButton?.addEventListener(
        "click",
        function () {

            if (propertySearch) {
                propertySearch.value = "";
            }

            if (locationSearch) {
                locationSearch.value = "";
            }

            if (locationType) {
                locationType.value = "";
            }

            if (locationValue) {
                locationValue.value = "";
            }

            if (minPriceInput) {
                minPriceInput.value = "";
            }

            if (maxPriceInput) {
                maxPriceInput.value = "";
            }

            propertyTypeButtons.forEach(
                function (button) {
                    button.classList.remove(
                        "active"
                    );
                }
            );

            bedroomButtons.forEach(
                function (button) {
                    button.classList.remove(
                        "active"
                    );
                }
            );

            listingTypeFilters.forEach(
                function (checkbox) {
                    checkbox.checked = false;
                }
            );

            if (sortSelect) {
                sortSelect.value =
                    "latest";
            }

            locationSuggestions.innerHTML =
                "";

            loadProperties(1);
        }
    );

    propertySearch?.addEventListener(
        "input",
        function () {

            clearTimeout(
                searchTimer
            );

            searchTimer =
                setTimeout(
                    function () {
                        loadProperties(1);
                    },
                    350
                );
        }
    );

    locationSearch?.addEventListener(
        "input",
        function () {

            const search =
                locationSearch.value.trim();

            locationType.value = "";
            locationValue.value = "";

            clearTimeout(
                locationTimer
            );

            if (search.length < 2) {

                locationSuggestions.innerHTML =
                    "";

                return;
            }

            locationTimer =
                setTimeout(
                    function () {
                        loadLocationSuggestions(
                            search
                        );
                    },
                    250
                );
        }
    );

    sortSelect?.addEventListener(
        "change",
        function () {
            loadProperties(1);
        }
    );

    async function loadLocationSuggestions(
        search
    ) {

        try {

            const response =
                await fetch(
                    `?handler=LocationSearch&search=${encodeURIComponent(
                        search
                    )}`,
                    {
                        method: "GET",
                        headers: {
                            "Accept": "application/json"
                        }
                    }
                );

            if (!response.ok) {
                throw new Error(
                    "Unable to load locations."
                );
            }

            const locations =
                await response.json();

            renderLocationSuggestions(
                locations
            );

        } catch (error) {

            console.error(
                "Location search error:",
                error
            );

            locationSuggestions.innerHTML =
                "";
        }
    }

    function renderLocationSuggestions(
        locations
    ) {

        locationSuggestions.innerHTML = "";

        if (!locations ||
            locations.length === 0) {
            return;
        }

        locations.forEach(
            function (location) {

                const button =
                    document.createElement(
                        "button"
                    );

                button.type = "button";

                button.className =
                    "location-suggestion";

                button.innerHTML = `
                    <span class="location-type">
                        ${escapeHtml(
                            location.type
                        )}
                    </span>
                    <span class="location-name">
                        ${escapeHtml(
                            location.name
                        )}
                    </span>
                `;

                button.addEventListener(
                    "click",
                    function () {

                        locationSearch.value =
                            location.name;

                        locationType.value =
                            location.type;

                        locationValue.value =
                            location.name;

                        locationSuggestions.innerHTML =
                            "";

                        loadProperties(1);
                    }
                );

                locationSuggestions.appendChild(
                    button
                );
            }
        );
    }

    function showLoading() {

        propertyGrid.innerHTML = `
            <div class="property-empty-state">
                Loading properties...
            </div>
        `;
    }

    function addParameter(
        params,
        key,
        value
    ) {

        if (
            value !== null &&
            value !== undefined &&
            value !== ""
        ) {
            params.set(
                key,
                value
            );
        }
    }

    function parseNumber(value) {

        if (!value) {
            return null;
        }

        const number =
            Number(
                String(value)
                    .replaceAll(",", "")
                    .trim()
            );

        return Number.isFinite(number)
            ? number
            : null;
    }

    function escapeHtml(value) {

        return String(
            value ?? ""
        )
            .replaceAll(
                "&",
                "&amp;"
            )
            .replaceAll(
                "<",
                "&lt;"
            )
            .replaceAll(
                ">",
                "&gt;"
            )
            .replaceAll(
                '"',
                "&quot;"
            )
            .replaceAll(
                "'",
                "&#039;"
            );
    }

    loadProperties(1);
});