document.addEventListener("DOMContentLoaded", function () {

    // Login modal elements
    const modal = document.getElementById("loginModalOverlay");
    const closeBtn = document.getElementById("modalCloseBtn");
    const progressBar = document.getElementById("modalProgressBar");

    let hideTimer = null;
    const DISAPPEAR_DELAY = 1800;

    // Show the login modal for restricted property actions
    function showModal() {
        if (!modal || !progressBar) {
            return;
        }

        clearTimeout(hideTimer);

        progressBar.style.transition = "none";
        progressBar.style.width = "100%";

        modal.classList.add("active");

        setTimeout(function () {
            progressBar.style.transition =
                `width ${DISAPPEAR_DELAY}ms linear`;

            progressBar.style.width = "0%";
        }, 20);

        hideTimer = setTimeout(function () {
            hideModal();
        }, DISAPPEAR_DELAY);
    }

    // Hide the login modal
    function hideModal() {
        if (!modal || !progressBar) {
            return;
        }

        clearTimeout(hideTimer);
        hideTimer = null;

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


    // Home page elements
    const propertyGrid = document.getElementById("propertyGrid");
    const propertyPills = document.querySelectorAll(".pill-btn");

    const cityInput = document.getElementById("citySearchInput");
    const citySuggestions = document.getElementById("citySuggestions");
    const homeSearchForm = document.getElementById("homeSearchForm");
    const featuredListings = document.getElementById("featuredListings");

    // Carousel elements
    const previousButton = document.getElementById("previousProperties");
    const nextButton = document.getElementById("nextProperties");
    const carouselPageLabel = document.getElementById("carouselPageLabel");

    // Filter dropdown elements
    const filterToggleButton = document.getElementById("filterToggleButton");
    const homeFilterPanel = document.getElementById("homeFilterPanel");

    const filterBedrooms = document.getElementById("filterBedrooms");
    const filterBathrooms = document.getElementById("filterBathrooms");
    const filterListingType = document.getElementById("filterListingType");
    const filterMinPrice = document.getElementById("filterMinPrice");
    const filterMaxPrice = document.getElementById("filterMaxPrice");

    const clearHomeFilters = document.getElementById("clearHomeFilters");
    const filterValidationMessage =
        document.getElementById("filterValidationMessage");

    // Allow our JavaScript to display the price validation messages
    if (homeSearchForm) {
        homeSearchForm.noValidate = true;
    }

    // Only these five cities are available in the home page search
    const cities = [
        "Delhi",
        "Mumbai",
        "Kolkata",
        "Bengaluru",
        "Chennai"
    ];

    // Show three properties at a time
    const PAGE_SIZE = 3;

    // Properties returned by the server for the current search
    let allProperties = [];

    // Current carousel page, starting from zero
    let currentPage = 0;

    // One selected city value is shared by search and city pills
    let selectedCity = "";

    // These are the filters that have actually been applied
    let activeHomeFilters = {
        bedrooms: "",
        bathrooms: "",
        listingType: "",
        minPrice: "",
        maxPrice: ""
    };


    // Display a filter validation message
    function showFilterValidation(message) {
        if (!filterValidationMessage) {
            return;
        }

        filterValidationMessage.textContent = message;
        filterValidationMessage.hidden = false;
    }

    // Clear the filter validation message
    function clearFilterValidation() {
        if (!filterValidationMessage) {
            return;
        }

        filterValidationMessage.textContent = "";
        filterValidationMessage.hidden = true;
    }


    // Open or close the filter dropdown
    filterToggleButton?.addEventListener("click", function () {
        if (!homeFilterPanel) {
            return;
        }

        const isOpening = homeFilterPanel.hidden;

        homeFilterPanel.hidden = !isOpening;

        filterToggleButton.setAttribute(
            "aria-expanded",
            String(isOpening)
        );
    });


    // Read the selected filter values and validate the prices
    function collectHomeFilters() {
        clearFilterValidation();

        const minText = filterMinPrice?.value.trim() || "";
        const maxText = filterMaxPrice?.value.trim() || "";

        const minValue = minText === ""
            ? null
            : Number(minText);

        const maxValue = maxText === ""
            ? null
            : Number(maxText);

        // Reject a negative or invalid minimum price
        if (
            minValue !== null &&
            (!Number.isFinite(minValue) || minValue < 0)
        ) {
            showFilterValidation(
                "Minimum price cannot be negative."
            );

            return null;
        }

        // Reject a negative or invalid maximum price
        if (
            maxValue !== null &&
            (!Number.isFinite(maxValue) || maxValue < 0)
        ) {
            showFilterValidation(
                "Maximum price cannot be negative."
            );

            return null;
        }

        // The minimum price must not exceed the maximum price
        if (
            minValue !== null &&
            maxValue !== null &&
            minValue > maxValue
        ) {
            showFilterValidation(
                "Minimum price cannot exceed maximum price."
            );

            return null;
        }

        // Return the values without applying them yet
        return {
            bedrooms: filterBedrooms?.value || "",
            bathrooms: filterBathrooms?.value || "",
            listingType: filterListingType?.value || "",
            minPrice: minText,
            maxPrice: maxText
        };
    }


    // Clear the dropdown fields
    // The changes take effect when Explore Listings is clicked
    clearHomeFilters?.addEventListener("click", function () {
        if (filterBedrooms) filterBedrooms.value = "";
        if (filterBathrooms) filterBathrooms.value = "";
        if (filterListingType) filterListingType.value = "";
        if (filterMinPrice) filterMinPrice.value = "";
        if (filterMaxPrice) filterMaxPrice.value = "";

        clearFilterValidation();
    });


    // Update the active city pill
    function setActiveCityPill(city) {
        propertyPills.forEach(function (pill) {
            const pillCity = pill.dataset.city || "";

            pill.classList.toggle(
                "active",
                pillCity === city
            );
        });
    }


    // Load properties using the selected city and applied filters
    async function loadProperties(
        city = selectedCity,
        filters = activeHomeFilters
    ) {
        if (!propertyGrid) {
            return false;
        }

        // Keep the selected city consistent across the page
        selectedCity = city || "";

        // Show a loading message while the server fetches properties
        propertyGrid.innerHTML = `
            <div class="property-loading">
                Loading properties...
            </div>
        `;

        try {
            const query = new URLSearchParams();

            // If no city is selected, search all cities
            if (selectedCity) {
                query.set("city", selectedCity);
            }

            // Add only the filters that have values
            if (filters.bedrooms) {
                query.set("bedrooms", filters.bedrooms);
            }

            if (filters.bathrooms) {
                query.set("bathrooms", filters.bathrooms);
            }

            if (filters.listingType) {
                query.set("listingType", filters.listingType);
            }

            if (filters.minPrice !== "") {
                query.set("minPrice", filters.minPrice);
            }

            if (filters.maxPrice !== "") {
                query.set("maxPrice", filters.maxPrice);
            }

            // Build the URL for the Razor Page handler
            const queryString = query.toString();

            const url = queryString
                ? `/?handler=Properties&${queryString}`
                : "/?handler=Properties";

            // The browser calls the Web application, not the API directly
            const response = await fetch(url, {
                method: "GET",
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!response.ok) {
                throw new Error(
                    `Property request failed with status ${response.status}.`
                );
            }

            // The response contains all matching properties
            const properties = await response.json();

            renderProperties(properties);

            return true;
        } catch (error) {
            console.error("Property loading error:", error);

            propertyGrid.innerHTML = `
                <div class="property-empty-state">
                    Unable to load properties. Please try again.
                </div>
            `;

            allProperties = [];
            currentPage = 0;

            updateCarouselControls();

            return false;
        }
    }


    // Store the complete result and start the carousel from its first page
    function renderProperties(properties) {
        allProperties = Array.isArray(properties)
            ? properties
            : [];

        currentPage = 0;

        renderCurrentPage();
    }


    // Display the current group of three properties
    function renderCurrentPage() {
        if (!propertyGrid) {
            return;
        }

        // Show a message when no properties match the search
        if (allProperties.length === 0) {
            propertyGrid.innerHTML = `
                <div class="property-empty-state">
                    No approved properties match your search.
                </div>
            `;

            updateCarouselControls();

            return;
        }

        const startIndex = currentPage * PAGE_SIZE;

        const visibleProperties = allProperties.slice(
            startIndex,
            startIndex + PAGE_SIZE
        );

        // Rebuild the cards for the current carousel page
        propertyGrid.innerHTML = visibleProperties
            .map(function (property) {
                return createPropertyCard(property);
            })
            .join("");

        updateCarouselControls();

        // Newly created cards still require the login restrictions
        attachLoginRequiredHandlers();
    }


    // Update the carousel arrows and page number
    function updateCarouselControls() {
        const totalPages = Math.ceil(
            allProperties.length / PAGE_SIZE
        );

        if (previousButton) {
            previousButton.disabled =
                totalPages === 0 || currentPage === 0;
        }

        if (nextButton) {
            nextButton.disabled =
                totalPages === 0 || currentPage >= totalPages - 1;
        }

        if (carouselPageLabel) {
            carouselPageLabel.textContent = totalPages === 0
                ? "0 / 0"
                : `${currentPage + 1} / ${totalPages}`;
        }
    }


    // Show the previous three properties
    previousButton?.addEventListener("click", function () {
        if (currentPage > 0) {
            currentPage--;
            renderCurrentPage();
        }
    });


    // Show the next three properties
    nextButton?.addEventListener("click", function () {
        const totalPages = Math.ceil(
            allProperties.length / PAGE_SIZE
        );

        if (currentPage < totalPages - 1) {
            currentPage++;
            renderCurrentPage();
        }
    });


    // City pills load properties using the last applied filters
    propertyPills.forEach(function (pill) {
        pill.addEventListener("click", async function (event) {
            event.stopPropagation();

            selectedCity = pill.dataset.city || "";

            if (cityInput) {
                cityInput.value = selectedCity;
            }

            setActiveCityPill(selectedCity);
            hideCitySuggestions();

            await loadProperties(
                selectedCity,
                activeHomeFilters
            );
        });
    });


    // Hide the city suggestion dropdown
    function hideCitySuggestions() {
        if (citySuggestions) {
            citySuggestions.hidden = true;
        }

        cityInput?.setAttribute("aria-expanded", "false");
    }


    // Display cities that begin with the entered letters
    function showCitySuggestions(query) {
        if (!citySuggestions || !cityInput) {
            return;
        }

        const normalizedQuery = query.toLowerCase();

        const matchingCities = cities.filter(function (city) {
            return city.toLowerCase().startsWith(normalizedQuery);
        });

        // Show a message when no city matches the search text
        if (matchingCities.length === 0) {
            citySuggestions.innerHTML = `
                <div class="city-no-results">
                    No cities found
                </div>
            `;
        } else {
            citySuggestions.innerHTML = matchingCities
                .map(function (city) {
                    return `
                        <button type="button"
                                class="city-suggestion"
                                role="option"
                                data-city="${city}">
                            ${city}
                        </button>
                    `;
                })
                .join("");
        }

        citySuggestions.hidden = false;

        cityInput.setAttribute(
            "aria-expanded",
            "true"
        );
    }


    // Changing the search text means the city must be selected again
    cityInput?.addEventListener("input", function () {
        selectedCity = "";

        showCitySuggestions(cityInput.value.trim());
    });


    // Selecting a suggestion fills the input but does not load properties yet
    citySuggestions?.addEventListener("click", function (event) {
        const suggestion = event.target.closest(".city-suggestion");

        if (!suggestion) {
            return;
        }

        selectedCity = suggestion.dataset.city || "";
        cityInput.value = selectedCity;

        hideCitySuggestions();
    });


    // Hide city suggestions when clicking outside the search field
    document.addEventListener("click", function (event) {
        if (!event.target.closest(".search-input-wrapper")) {
            hideCitySuggestions();
        }
    });


    // Explore Listings is the single place where all filters are applied
    homeSearchForm?.addEventListener(
        "submit",
        async function (event) {
            event.preventDefault();

            const cityText = cityInput?.value.trim() || "";

            // An empty city means All Listings
            if (cityText === "") {
                selectedCity = "";
            } else {
                // Accept only one of the five supported cities
                const exactCity = cities.find(function (city) {
                    return city.toLowerCase() === cityText.toLowerCase();
                });

                if (!exactCity) {
                    showCitySuggestions(cityText);
                    return;
                }

                selectedCity = exactCity;
                cityInput.value = exactCity;
            }

            // Validate and collect every filter before sending the request
            const filters = collectHomeFilters();

            if (filters === null) {
                return;
            }

            // Save these values as the currently applied filters
            activeHomeFilters = filters;

            // Activate the city pill that matches the search
            setActiveCityPill(selectedCity);

            // Close the dropdowns before displaying results
            hideCitySuggestions();

            if (homeFilterPanel) {
                homeFilterPanel.hidden = true;
            }

            filterToggleButton?.setAttribute(
                "aria-expanded",
                "false"
            );

            // Fetch properties for the selected city and all filters
            const loaded = await loadProperties(
                selectedCity,
                activeHomeFilters
            );

            // Scroll to the selected city pill or the listings section
            if (loaded) {
                const selectedPill = Array.from(propertyPills).find(
                    function (pill) {
                        return (pill.dataset.city || "") === selectedCity;
                    }
                );

                const scrollTarget = selectedPill || featuredListings;

                scrollTarget?.scrollIntoView({
                    behavior: "smooth",
                    block: "center"
                });
            }
        }
    );


    // Create the HTML for one property card
    function createPropertyCard(property) {
        const listingType = property.listingType || "PROPERTY";

        const location = property.locationAddress
            ? `${property.locationAddress}, ${property.city}`
            : property.city;

        const price = Number(property.price || 0).toLocaleString();

        const imageUrl = property.coverImagePath
            ? `/?handler=PropertyImage&path=${encodeURIComponent(property.coverImagePath)}`
            : "";

        // Use the uploaded cover photo, or show a house placeholder
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
                            ₹ ${price}
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
                            <span>
              
                                ${escapeHtml(property.bedrooms)} Beds
                            </span>
                        </div>

                        <div class="spec-item">
                            <span>
                            
                                ${escapeHtml(property.bathrooms)} Baths
                            </span>
                        </div>

                        <div class="spec-item">
                            <span>
                                ${escapeHtml(property.area)}
                                ${escapeHtml(property.areaUnit)}
                            </span>
                        </div>
                    </div>

                    <a href="/Listings/Details/${encodeURIComponent(property.propertyID)}"
                       class="btn-view-details">
                        View Details
                    </a>
                </div>
            </div>
        `;
    }

    // Keep property interactions restricted for visitors who are not logged in
    function attachLoginRequiredHandlers() {
        const protectedElements = document.querySelectorAll(
            ".property-card, " +
            ".btn-view-details, " +
            ".btn-favorite, " +
            ".property-location"
        );

        protectedElements.forEach(function (element) {
            // Do not attach a second handler to the same element
            if (element.dataset.loginHandlerAttached === "true") {
                return;
            }

            element.dataset.loginHandlerAttached = "true";

            element.addEventListener("click", function (event) {
                // Prevent the protected action and show the login modal
                event.preventDefault();
                event.stopPropagation();

                showModal();
            });
        });
    }


    // Convert dynamic values to safe HTML text
    function escapeHtml(value) {
        return String(value ?? "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }


    // Set up the login restrictions and load all properties initially
    attachLoginRequiredHandlers();

    loadProperties("", activeHomeFilters);

});