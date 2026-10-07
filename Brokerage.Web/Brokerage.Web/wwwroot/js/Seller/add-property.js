document.addEventListener("DOMContentLoaded", function () {
    const coverPhotoInput = document.getElementById("coverPhotoInput");
    const reviewCoverImage = document.getElementById("reviewCoverImage");
    const reviewCoverPlaceholder =
        document.getElementById("reviewCoverPlaceholder");

    const wizard =
        document.getElementById("propertyWizard");

    let currentStep =
        Number(
            wizard?.dataset.currentStep || 1
        );

    const steps =
        document.querySelectorAll(
            ".wizard-step"
        );

    const continueButton =
        document.getElementById(
            "continueButton"
        );

    const backButton =
        document.getElementById(
            "backButton"
        );

    const saveDraftButton =
        document.getElementById(
            "saveDraftButton"
        );

    const form =
        document.getElementById(
            "addPropertyForm"
        );

    const formAction =
        document.getElementById(
            "formAction"
        );

    const wizardMessage =
        document.getElementById(
            "wizardMessage"
        );

    const reviewDataElement =
        document.getElementById(
            "propertyReviewData"
        );

    let serverReview = null;

    let existingCoverRemoved = false;

    let removedMediaIds = [];

    let galleryReplacementIds = [];

    let videoReplacementIds = [];

    let floorPlanReplacementIds = [];

    let documentReplacementIds = [];

    let pendingGalleryReplacementId = 0;

    let pendingVideoReplacementId = 0;

    let pendingFloorPlanReplacementId = 0;

    let pendingDocumentReplacementId = 0;

    if (reviewDataElement) {
        try {
            const reviewJson =
                reviewDataElement
                    .textContent
                    ?.trim();

            if (reviewJson) {
                serverReview =
                    JSON.parse(
                        reviewJson
                    );

                console.log(
                    "SERVER REVIEW DATA:",
                    serverReview
                );
            }
        } catch (error) {
            console.error(
                "Unable to read property review data:",
                error
            );
        }
    }

    const urlParams =
        new URLSearchParams(
            window.location.search
        );

    const success =
        (
            urlParams.get("success") ||
            ""
        ).toLowerCase() === "true";

    const successOverlay =
        document.getElementById(
            "successOverlay"
        );

    const myPropertiesUrl =
        form?.dataset.myPropertiesUrl;

    if (success) {
        console.log(
            "PROPERTY SUBMISSION SUCCESSFUL"
        );

        if (successOverlay) {
            successOverlay.classList.add(
                "show"
            );
        }

        setTimeout(function () {
            console.log(
                "Redirecting to My Properties:",
                myPropertiesUrl
            );

            window.location.assign(
                myPropertiesUrl
            );
        }, 3000);
    }

    function showMessage(message) {
        if (!wizardMessage) {
            return;
        }

        wizardMessage.textContent =
            message;

        wizardMessage.classList.add(
            "show"
        );
    }

    function hideMessage() {
        if (!wizardMessage) {
            return;
        }

        wizardMessage.textContent = "";

        wizardMessage.classList.remove(
            "show"
        );
    }

    function updateStepIndicator() {
        const stepItems =
            document.querySelectorAll(
                ".step-item"
            );

        stepItems.forEach(function (item) {
            const stepNumber =
                Number(
                    item.dataset.step
                );

            item.classList.remove(
                "active",
                "completed"
            );

            if (
                stepNumber ===
                currentStep
            ) {
                item.classList.add(
                    "active"
                );
            }
            else if (
                stepNumber <
                currentStep
            ) {
                item.classList.add(
                    "completed"
                );
            }
        });
    }

    function restoreBathroomsIfNeeded() {
        const bathroomsField =
            document.getElementById(
                "Details_Bathrooms"
            );

        if (
            !bathroomsField ||
            bathroomsField.value !== ""
        ) {
            return;
        }

        const bathroomValue =
            serverReview
                ?.property
                ?.bathrooms;

        if (
            bathroomValue === null ||
            bathroomValue === undefined
        ) {
            return;
        }

        const normalizedValue =
            Number(
                bathroomValue
            ).toString();

        const matchingOption =
            Array.from(
                bathroomsField.options
            ).find(function (option) {
                return (
                    option.value ===
                    normalizedValue
                );
            });

        if (matchingOption) {
            bathroomsField.value =
                normalizedValue;

            console.log(
                "[AddProperty] Restored Bathrooms:",
                normalizedValue
            );
        }
    }

    const possessionDateInput =
        document.getElementById(
            "Details_PossessionDate"
        );

    if (possessionDateInput) {
        possessionDateInput.addEventListener(
            "click",
            function () {
                if (
                    typeof this.showPicker ===
                    "function"
                ) {
                    this.showPicker();
                }
            }
        );
    }

    function showStep(stepNumber) {
        const targetStep =
            document.getElementById(
                `step-${stepNumber}`
            );

        if (!targetStep) {
            showMessage(
                "This step will be added next."
            );

            return;
        }

        steps.forEach(function (step) {
            step.classList.remove(
                "active"
            );
        });

        targetStep.classList.add(
            "active"
        );

        currentStep =
            stepNumber;

        if (backButton) {
            backButton.hidden =
                currentStep === 1;
        }

        if (
            currentStep === 2
        ) {
            restoreBathroomsIfNeeded();
        }

        if (
            currentStep === 4
        ) {
            restoreMediaOnStepFour();
        }

        if (
            currentStep === 5
        ) {
            const text =
                continueButton?.querySelector(
                    "#continueButtonText"
                );

            if (text) {
                text.textContent =
                    "Post Property";
            }

            renderServerReview();
        }
        else {
            const text =
                continueButton?.querySelector(
                    "#continueButtonText"
                );

            if (text) {
                text.textContent =
                    "Continue";
            }
        }

        updateStepIndicator();

        hideMessage();

        window.scrollTo({
            top: 0,
            behavior: "smooth"
        });
    }

    const listingTypeField =
        document.getElementById(
            "Input_ListingType"
        );

    const priceField =
        document.getElementById(
            "Details_Price"
        );

    const priceFieldLabel =
        document.getElementById(
            "priceFieldLabel"
        );

    function updatePriceFieldForListingType() {
        if (
            !listingTypeField ||
            !priceField ||
            !priceFieldLabel
        ) {
            return;
        }

        if (
            listingTypeField.value ===
            "For Sale"
        ) {
            priceFieldLabel.textContent =
                "Sale amount";

            priceField.placeholder =
                "Enter sale amount";
        }
        else {
            priceFieldLabel.textContent =
                "Rent price / month";

            priceField.placeholder =
                "Enter rent price";
        }
    }

    listingTypeField?.addEventListener(
        "change",
        updatePriceFieldForListingType
    );

    updatePriceFieldForListingType();

    continueButton?.addEventListener(
        "click",
        function () {

            if (
                currentStep === 1
            ) {
                form.noValidate =
                    true;

                formAction.value =
                    "ValidateStep1";

                form.requestSubmit();

                return;
            }

            if (
                currentStep === 2
            ) {
                form.noValidate =
                    true;

                formAction.value =
                    "ValidateStep2";

                form.requestSubmit();

                return;
            }

            if (
                currentStep === 3
            ) {
                form.noValidate =
                    true;

                formAction.value =
                    "ValidateStep3";

                form.requestSubmit();

                return;
            }

            if (
                currentStep === 4
            ) {
                if (
                    !validateStepFour()
                ) {
                    return;
                }

                form.noValidate =
                    true;

                formAction.value =
                    "SaveForReview";

                form.requestSubmit();

                return;
            }

            if (
                currentStep === 5
            ) {
                form.noValidate =
                    true;

                formAction.value =
                    "SubmitProperty";

                form.requestSubmit();
            }
        }
    );

    backButton?.addEventListener(
        "click",
        function () {
            if (
                currentStep > 1
            ) {
                showStep(
                    currentStep - 1
                );
            }
        }
    );

    saveDraftButton?.addEventListener(
        "click",
        function () {
            savePropertyDraftToLocalStorage();

            showMessage(
                "Property draft saved locally."
            );
        }
    );

    const baseUrl =
        form?.dataset.locationApiBaseUrl;

    const countrySearch =
        document.getElementById(
            "countrySearch"
        );

    const countryOptions =
        document.getElementById(
            "countryOptions"
        );

    const selectedCountry =
        document.getElementById(
            "selectedCountry"
        );

    const stateSearch =
        document.getElementById(
            "stateSearch"
        );

    const stateOptions =
        document.getElementById(
            "stateOptions"
        );

    const selectedState =
        document.getElementById(
            "selectedState"
        );

    const citySearch =
        document.getElementById(
            "citySearch"
        );

    const cityOptions =
        document.getElementById(
            "cityOptions"
        );

    const selectedCity =
        document.getElementById(
            "selectedCity"
        );

    let countries = [];

    let states = [];

    let cities = [];

    async function fetchJson(url) {
        const response =
            await fetch(url);

        if (!response.ok) {
            throw new Error(
                `Location API failed: ${response.status}`
            );
        }

        return await response.json();
    }

    function normaliseName(item) {
        if (
            typeof item ===
            "string"
        ) {
            return item;
        }

        return item?.name ?? "";
    }

    function renderOptions(
        items,
        container,
        input,
        onSelect
    ) {
        if (
            !container ||
            !input
        ) {
            return;
        }

        container.innerHTML = "";

        const searchTerm =
            input.value
                .trim()
                .toLowerCase();

        const matches =
            items
                .filter(function (item) {
                    const name =
                        normaliseName(
                            item
                        ).toLowerCase();

                    if (!searchTerm) {
                        return true;
                    }

                    return name.startsWith(
                        searchTerm
                    );
                })
                .slice(0, 10);

        if (
            matches.length ===
            0
        ) {
            const empty =
                document.createElement(
                    "div"
                );

            empty.className =
                "search-option no-results";

            empty.textContent =
                "No matching records found.";

            container.appendChild(
                empty
            );

            container.classList.add(
                "show"
            );

            return;
        }

        matches.forEach(
            function (item) {

                const name =
                    normaliseName(
                        item
                    );

                const option =
                    document.createElement(
                        "div"
                    );

                option.className =
                    "search-option";

                option.textContent =
                    name;

                option.setAttribute(
                    "role",
                    "option"
                );

                option.addEventListener(
                    "mousedown",
                    function (event) {

                        event.preventDefault();

                        input.value =
                            name;

                        onSelect(
                            name
                        );

                        container.classList.remove(
                            "show"
                        );
                    }
                );

                container.appendChild(
                    option
                );
            }
        );

        container.classList.add(
            "show"
        );
    }

    function showLoading(container) {
        if (!container) {
            return;
        }

        container.innerHTML = `
            <div class="search-option loading">
                Loading...
            </div>
        `;

        container.classList.add(
            "show"
        );
    }

    async function loadCountries() {
        if (
            !baseUrl ||
            !countryOptions ||
            !countrySearch
        ) {
            return;
        }

        try {
            const data =
                await fetchJson(
                    `${baseUrl}/countries/iso`
                );

            if (data.error) {
                throw new Error(
                    data.msg
                );
            }

            countries =
                Array.isArray(
                    data.data
                )
                    ? data.data
                    : [];

            renderOptions(
                countries,
                countryOptions,
                countrySearch,
                selectCountry
            );

            if (
                selectedCountry &&
                selectedCountry.value
            ) {

                countrySearch.value =
                    selectedCountry.value;

                if (stateSearch) {
                    stateSearch.disabled =
                        false;

                    stateSearch.placeholder =
                        "Search state";
                }

                await loadStates(
                    selectedCountry.value
                );

                if (
                    selectedState &&
                    selectedState.value
                ) {

                    if (stateSearch) {
                        stateSearch.value =
                            selectedState.value;
                    }

                    if (citySearch) {
                        citySearch.disabled =
                            false;

                        citySearch.placeholder =
                            "Search city";
                    }

                    await loadCities(
                        selectedCountry.value,
                        selectedState.value
                    );

                    if (
                        selectedCity &&
                        selectedCity.value
                    ) {
                        citySearch.value =
                            selectedCity.value;
                    }
                }

                countryOptions.classList.remove(
                    "show"
                );

                stateOptions?.classList.remove(
                    "show"
                );

                cityOptions?.classList.remove(
                    "show"
                );
            }
        }
        catch (error) {
            console.error(
                "Country API error:",
                error
            );

            countryOptions.innerHTML = `
                <div class="search-option no-results">
                    Unable to load countries.
                </div>
            `;

            countryOptions.classList.add(
                "show"
            );
        }
    }

    function selectCountry(
        countryName
    ) {
        if (selectedCountry) {
            selectedCountry.value =
                countryName;
        }

        states = [];

        cities = [];

        if (selectedState) {
            selectedState.value =
                "";
        }

        if (selectedCity) {
            selectedCity.value =
                "";
        }

        if (stateSearch) {
            stateSearch.value =
                "";

            stateSearch.disabled =
                false;

            stateSearch.placeholder =
                "Search state";
        }

        if (citySearch) {
            citySearch.value =
                "";

            citySearch.disabled =
                true;

            citySearch.placeholder =
                "Select state first";
        }

        loadStates(
            countryName
        );
    }

    countrySearch?.addEventListener(
        "focus",
        function () {
            renderOptions(
                countries,
                countryOptions,
                countrySearch,
                selectCountry
            );
        }
    );

    countrySearch?.addEventListener(
        "input",
        function () {
            renderOptions(
                countries,
                countryOptions,
                countrySearch,
                selectCountry
            );
        }
    );

    async function loadStates(
        countryName
    ) {
        if (
            !baseUrl ||
            !stateOptions ||
            !stateSearch
        ) {
            return;
        }

        states = [];

        stateOptions.innerHTML =
            "";

        showLoading(
            stateOptions
        );

        try {
            const url =
                `${baseUrl}/countries/states/q?country=${encodeURIComponent(countryName)}`;

            const data =
                await fetchJson(
                    url
                );

            if (data.error) {
                throw new Error(
                    data.msg
                );
            }

            states =
                Array.isArray(
                    data?.data?.states
                )
                    ? data.data.states
                    : [];

            renderOptions(
                states,
                stateOptions,
                stateSearch,
                selectState
            );
        }
        catch (error) {
            console.error(
                "State API error:",
                error
            );

            stateOptions.innerHTML = `
                <div class="search-option no-results">
                    Unable to load states.
                </div>
            `;

            stateOptions.classList.add(
                "show"
            );
        }
    }

    function selectState(
        stateName
    ) {
        if (selectedState) {
            selectedState.value =
                stateName;
        }

        cities = [];

        if (selectedCity) {
            selectedCity.value =
                "";
        }

        if (citySearch) {
            citySearch.value =
                "";

            citySearch.disabled =
                false;

            citySearch.placeholder =
                "Search city";
        }

        loadCities(
            selectedCountry?.value,
            stateName
        );
    }

    stateSearch?.addEventListener(
        "focus",
        function () {
            renderOptions(
                states,
                stateOptions,
                stateSearch,
                selectState
            );
        }
    );

    stateSearch?.addEventListener(
        "input",
        function () {
            renderOptions(
                states,
                stateOptions,
                stateSearch,
                selectState
            );
        }
    );

    async function loadCities(
        countryName,
        stateName
    ) {
        if (
            !baseUrl ||
            !cityOptions ||
            !citySearch
        ) {
            return;
        }

        cities = [];

        cityOptions.innerHTML =
            "";

        showLoading(
            cityOptions
        );

        try {
            const url =
                `${baseUrl}/countries/state/cities/q?country=${encodeURIComponent(countryName)}&state=${encodeURIComponent(stateName)}`;

            const data =
                await fetchJson(
                    url
                );

            if (data.error) {
                throw new Error(
                    data.msg
                );
            }

            cities =
                Array.isArray(
                    data.data
                )
                    ? data.data
                    : [];

            renderOptions(
                cities,
                cityOptions,
                citySearch,
                selectCity
            );
        }
        catch (error) {
            console.error(
                "City API error:",
                error
            );

            cityOptions.innerHTML = `
                <div class="search-option no-results">
                    Unable to load cities.
                </div>
            `;

            cityOptions.classList.add(
                "show"
            );
        }
    }

    function selectCity(
        cityName
    ) {
        if (selectedCity) {
            selectedCity.value =
                cityName;
        }
    }

    citySearch?.addEventListener(
        "focus",
        function () {
            renderOptions(
                cities,
                cityOptions,
                citySearch,
                selectCity
            );
        }
    );

    citySearch?.addEventListener(
        "input",
        function () {
            renderOptions(
                cities,
                cityOptions,
                citySearch,
                selectCity
            );
        }
    );

    document.addEventListener(
        "click",
        function (event) {
            if (
                !event.target.closest(
                    ".searchable-select"
                )
            ) {
                document
                    .querySelectorAll(
                        ".search-options"
                    )
                    .forEach(
                        function (dropdown) {
                            dropdown.classList.remove(
                                "show"
                            );
                        }
                    );
            }
        }
    );

    const yearBuiltField =
        document.getElementById(
            "Details_YearBuilt"
        );

    const propertyAgeField =
        document.getElementById(
            "Details_PropertyAgeYears"
        );

    function updatePropertyAge(
        forceUpdate
    ) {
        if (
            !yearBuiltField ||
            !propertyAgeField
        ) {
            return;
        }

        const yearBuilt =
            Number(
                yearBuiltField.value
            );

        if (!yearBuilt) {
            propertyAgeField.value =
                "";

            return;
        }

        if (
            forceUpdate ||
            propertyAgeField.value.trim() ===
            ""
        ) {
            const currentYear =
                new Date().getFullYear();

            const age =
                currentYear -
                yearBuilt;

            propertyAgeField.value =
                Math.max(
                    0,
                    age
                );
        }
    }

    yearBuiltField?.addEventListener(
        "change",
        function () {
            updatePropertyAge(
                true
            );
        }
    );

    updatePropertyAge(false);

    const amenityCards =
        document.querySelectorAll(
            ".amenity-card"
        );

    const amenityCategoryTabs =
        document.querySelectorAll(
            ".amenity-category-tab"
        );

    const amenitySections =
        document.querySelectorAll(
            "[data-category-section]"
        );

    amenityCards.forEach(
        function (card) {
            const checkbox =
                card.querySelector(
                    ".amenity-checkbox"
                );

            checkbox?.addEventListener(
                "change",
                function () {
                    card.classList.toggle(
                        "selected",
                        checkbox.checked
                    );
                }
            );
        }
    );

    amenityCategoryTabs.forEach(
        function (tab) {
            tab.addEventListener(
                "click",
                function () {
                    const category =
                        tab.dataset.category;

                    const target =
                        document.querySelector(
                            `[data-category-section="${category}"]`
                        );

                    if (!target) {
                        return;
                    }

                    target.scrollIntoView(
                        {
                            behavior: "smooth",
                            block: "start"
                        }
                    );
                }
            );
        }
    );

    function setActiveAmenityCategory(
        category
    ) {
        amenityCategoryTabs.forEach(
            function (tab) {
                tab.classList.toggle(
                    "active",
                    tab.dataset.category ===
                    category
                );
            }
        );
    }

    const amenityObserver =
        new IntersectionObserver(
            function (entries) {
                entries.forEach(
                    function (entry) {
                        if (
                            entry.isIntersecting
                        ) {
                            const category =
                                entry
                                    .target
                                    .dataset
                                    .categorySection;

                            setActiveAmenityCategory(
                                category
                            );
                        }
                    }
                );
            },
            {
                root: null,
                rootMargin:
                    "-15% 0px -65% 0px",
                threshold: 0
            }
        );

    amenitySections.forEach(
        function (section) {
            amenityObserver.observe(
                section
            );
        }
    );

    const MEDIA_LIMITS = {
        cover: {
            maxFiles: 1,
            maxSize:
                5 * 1024 * 1024,
            allowedExtensions: [
                ".png",
                ".jpg",
                ".jpeg"
            ]
        },

        gallery: {
            maxFiles: 7,
            maxSize:
                5 * 1024 * 1024,
            allowedExtensions: [
                ".png",
                ".jpg",
                ".jpeg"
            ]
        },

        video: {
            maxFiles: 2,
            maxSize:
                25 * 1024 * 1024,
            allowedExtensions: [
                ".mp4"
            ]
        },

        floorPlan: {
            maxFiles: 2,
            maxSize:
                5 * 1024 * 1024,
            allowedExtensions: [
                ".pdf"
            ]
        },

        documents: {
            maxFiles: 4,
            maxSize:
                5 * 1024 * 1024,
            allowedExtensions: [
                ".pdf",
                ".doc",
                ".docx"
            ]
        }
    };

    let coverFile = null;

    let coverPreviewDataUrl = null;

    let galleryFiles = [];

    let videoFiles = [];

    let floorPlanFiles = [];

    let documentFiles = [];

    function initializeMediaUpload() {
        const coverInput =
            document.getElementById(
                "coverPhotoInput"
            );

        const galleryInput =
            document.getElementById(
                "galleryImagesInput"
            );

        const videoInput =
            document.getElementById(
                "videoInput"
            );

        const floorPlanInput =
            document.getElementById(
                "floorPlanInput"
            );

        const documentsInput =
            document.getElementById(
                "documentsInput"
            );

        if (
            !coverInput ||
            !galleryInput ||
            !videoInput ||
            !floorPlanInput ||
            !documentsInput
        ) {
            console.error(
                "Media upload elements were not found."
            );

            return;
        }

        setupDropzone(
            "coverDropzone",
            coverInput,
            function (files) {
                handleCoverFiles(
                    files
                );
            }
        );

        setupDropzone(
            "galleryDropzone",
            galleryInput,
            function (files) {
                handleGalleryFiles(
                    files
                );
            }
        );

        setupDropzone(
            "videoDropzone",
            videoInput,
            function (files) {
                handleVideoFiles(
                    files
                );
            }
        );

        setupDropzone(
            "floorPlanDropzone",
            floorPlanInput,
            function (files) {
                handleFloorPlanFiles(
                    files
                );
            }
        );

        setupDropzone(
            "documentsDropzone",
            documentsInput,
            function (files) {
                handleDocumentFiles(
                    files
                );
            }
        );
    }

    function setupDropzone(
        dropzoneId,
        input,
        onFilesSelected
    ) {
        const dropzone =
            document.getElementById(
                dropzoneId
            );

        if (!dropzone) {
            return;
        }

        dropzone.addEventListener(
            "click",
            function () {
                input.click();
            }
        );

        dropzone.addEventListener(
            "keydown",
            function (event) {
                if (
                    event.key ===
                    "Enter" ||
                    event.key ===
                    " "
                ) {
                    event.preventDefault();

                    input.click();
                }
            }
        );

        input.addEventListener(
            "change",
            function () {
                const files =
                    Array.from(
                        input.files
                    );

                onFilesSelected(
                    files
                );
            }
        );

        dropzone.addEventListener(
            "dragenter",
            function (event) {
                event.preventDefault();

                dropzone.classList.add(
                    "drag-over"
                );
            }
        );

        dropzone.addEventListener(
            "dragover",
            function (event) {
                event.preventDefault();

                dropzone.classList.add(
                    "drag-over"
                );
            }
        );

        dropzone.addEventListener(
            "dragleave",
            function (event) {
                event.preventDefault();

                dropzone.classList.remove(
                    "drag-over"
                );
            }
        );

        dropzone.addEventListener(
            "drop",
            function (event) {
                event.preventDefault();

                dropzone.classList.remove(
                    "drag-over"
                );

                const files =
                    Array.from(
                        event.dataTransfer.files
                    );

                onFilesSelected(
                    files
                );
            }
        );
    }

    function handleCoverFiles(
        files
    ) {
        clearError(
            "coverError"
        );

        if (
            !files ||
            files.length === 0
        ) {
            return;
        }

        const file =
            files[0];

        const validation =
            validateFile(
                file,
                MEDIA_LIMITS.cover
            );

        if (
            !validation.valid
        ) {
            showError(
                "coverError",
                validation.message
            );

            return;
        }

        coverFile =
            file;

        existingCoverRemoved =
            false;

        updateCoverInput();

        const reader =
            new FileReader();

        reader.onload =
            function (event) {
                coverPreviewDataUrl =
                    event.target.result;

                renderCoverPreview();
            };

        reader.onerror =
            function () {
                coverPreviewDataUrl =
                    null;

                showError(
                    "coverError",
                    "Unable to read the cover photo."
                );
            };

        reader.readAsDataURL(
            file
        );
    }
    function getExistingMediaCount(mediaType) {
        return (
            serverReview?.media || []
        ).filter(function (item) {
            return (
                item.mediaType === mediaType &&
                !removedMediaIds.includes(
                    item.mediaID
                )
            );
        }).length;
    }

    function handleGalleryFiles(files) {
        clearError("galleryError");

        if (!files || files.length === 0) {
            pendingGalleryReplacementId = 0;
            return;
        }

        for (const file of files) {
            const existingCount =
                getExistingMediaCount(
                    "GalleryImage"
                );

            const replacementCount =
                pendingGalleryReplacementId > 0
                    ? 1
                    : 0;

            const totalCount =
                existingCount +
                galleryFiles.length -
                replacementCount;

            if (
                totalCount >=
                MEDIA_LIMITS.gallery.maxFiles
            ) {
                showError(
                    "galleryError",
                    "Maximum 7 gallery images are allowed in total."
                );

                break;
            }

            const validation =
                validateFile(
                    file,
                    MEDIA_LIMITS.gallery
                );

            if (!validation.valid) {
                showError(
                    "galleryError",
                    validation.message
                );

                pendingGalleryReplacementId = 0;

                continue;
            }

            if (
                isDuplicateFile(
                    file,
                    galleryFiles
                )
            ) {
                pendingGalleryReplacementId = 0;
                continue;
            }

            galleryFiles.push(file);

            galleryReplacementIds.push(
                pendingGalleryReplacementId
            );

            pendingGalleryReplacementId = 0;
        }

        updateGalleryInput();

        renderGalleryPreview();
    }

    function handleVideoFiles(files) {
        clearError("videoError");

        if (!files || files.length === 0) {
            pendingVideoReplacementId = 0;
            return;
        }

        for (const file of files) {
            const existingCount =
                getExistingMediaCount(
                    "Video"
                );

            const replacementCount =
                pendingVideoReplacementId > 0
                    ? 1
                    : 0;

            const totalCount =
                existingCount +
                videoFiles.length -
                replacementCount;

            if (
                totalCount >=
                MEDIA_LIMITS.video.maxFiles
            ) {
                showError(
                    "videoError",
                    "Maximum 2 videos are allowed in total."
                );

                break;
            }

            const validation =
                validateFile(
                    file,
                    MEDIA_LIMITS.video
                );

            if (!validation.valid) {
                showError(
                    "videoError",
                    validation.message
                );

                pendingVideoReplacementId = 0;

                continue;
            }

            if (
                isDuplicateFile(
                    file,
                    videoFiles
                )
            ) {
                pendingVideoReplacementId = 0;
                continue;
            }

            videoFiles.push(file);

            videoReplacementIds.push(
                pendingVideoReplacementId
            );

            pendingVideoReplacementId = 0;
        }

        updateVideoInput();

        renderVideoList();
    }

    function handleFloorPlanFiles(files) {
        clearError("floorPlanError");

        if (!files || files.length === 0) {
            pendingFloorPlanReplacementId = 0;
            return;
        }

        for (const file of files) {
            const existingCount =
                getExistingMediaCount(
                    "FloorPlan"
                );

            const replacementCount =
                pendingFloorPlanReplacementId > 0
                    ? 1
                    : 0;

            const totalCount =
                existingCount +
                floorPlanFiles.length -
                replacementCount;

            if (
                totalCount >=
                MEDIA_LIMITS.floorPlan.maxFiles
            ) {
                showError(
                    "floorPlanError",
                    "Maximum 2 floor plans are allowed in total."
                );

                break;
            }

            const validation =
                validateFile(
                    file,
                    MEDIA_LIMITS.floorPlan
                );

            if (!validation.valid) {
                showError(
                    "floorPlanError",
                    validation.message
                );

                pendingFloorPlanReplacementId = 0;

                continue;
            }

            if (
                isDuplicateFile(
                    file,
                    floorPlanFiles
                )
            ) {
                pendingFloorPlanReplacementId = 0;
                continue;
            }

            floorPlanFiles.push(file);

            floorPlanReplacementIds.push(
                pendingFloorPlanReplacementId
            );

            pendingFloorPlanReplacementId = 0;
        }

        updateFloorPlanInput();

        renderFloorPlanList();
    }

    function handleDocumentFiles(files) {
        clearError("documentsError");

        if (!files || files.length === 0) {
            pendingDocumentReplacementId = 0;
            return;
        }

        for (const file of files) {
            const existingCount =
                getExistingMediaCount(
                    "Document"
                );

            const replacementCount =
                pendingDocumentReplacementId > 0
                    ? 1
                    : 0;

            const totalCount =
                existingCount +
                documentFiles.length -
                replacementCount;

            if (
                totalCount >=
                MEDIA_LIMITS.documents.maxFiles
            ) {
                showError(
                    "documentsError",
                    "Maximum 4 documents are allowed in total."
                );

                break;
            }

            const validation =
                validateFile(
                    file,
                    MEDIA_LIMITS.documents
                );

            if (!validation.valid) {
                showError(
                    "documentsError",
                    validation.message
                );

                pendingDocumentReplacementId = 0;

                continue;
            }

            if (
                isDuplicateFile(
                    file,
                    documentFiles
                )
            ) {
                pendingDocumentReplacementId = 0;
                continue;
            }

            documentFiles.push(file);

            documentReplacementIds.push(
                pendingDocumentReplacementId
            );

            pendingDocumentReplacementId = 0;
        }

        updateDocumentsInput();

        renderDocumentList();
    }

    function validateFile(
        file,
        settings
    ) {
        const fileName =
            file.name.toLowerCase();

        const extension =
            getFileExtension(
                fileName
            );

        if (
            !settings.allowedExtensions.includes(
                extension
            )
        ) {
            return {
                valid: false,
                message:
                    `${file.name} is not a supported file type.`
            };
        }

        if (
            file.size >
            settings.maxSize
        ) {
            return {
                valid: false,
                message:
                    `${file.name} exceeds the maximum allowed size.`
            };
        }

        return {
            valid: true,
            message: ""
        };
    }

    function getFileExtension(
        fileName
    ) {
        const lastDotIndex =
            fileName.lastIndexOf(
                "."
            );

        if (
            lastDotIndex === -1
        ) {
            return "";
        }

        return fileName.substring(
            lastDotIndex
        );
    }

    function isDuplicateFile(
        file,
        existingFiles
    ) {
        return existingFiles.some(
            function (existingFile) {
                return (
                    existingFile.name ===
                    file.name &&
                    existingFile.size ===
                    file.size &&
                    existingFile.lastModified ===
                    file.lastModified
                );
            }
        );
    }

    function renderCoverPreview() {
        const emptyState =
            document.getElementById(
                "coverEmptyState"
            );

        const preview =
            document.getElementById(
                "coverPreview"
            );

        if (
            !emptyState ||
            !preview
        ) {
            return;
        }

        if (coverFile) {
            emptyState.hidden =
                true;

            preview.hidden =
                false;

            preview.innerHTML =
                "";

            const image =
                document.createElement(
                    "img"
                );

            const imageUrl =
                URL.createObjectURL(
                    coverFile
                );

            image.src =
                imageUrl;

            image.alt =
                "New cover photo";

            image.onload =
                function () {
                    URL.revokeObjectURL(
                        imageUrl
                    );
                };

            const fileName =
                document.createElement(
                    "div"
                );

            fileName.className =
                "cover-file-name";

            fileName.textContent =
                coverFile.name;

            const actionGroup =
                document.createElement(
                    "div"
                );

            actionGroup.className =
                "cover-action-group";

            const replaceButton =
                document.createElement(
                    "button"
                );

            replaceButton.type =
                "button";

            replaceButton.className =
                "cover-replace-button";

            replaceButton.textContent =
                "Replace";

            replaceButton.addEventListener(
                "click",
                function (event) {
                    event.preventDefault();
                    event.stopPropagation();

                    coverPhotoInput?.click();
                }
            );

            const removeButton =
                document.createElement(
                    "button"
                );

            removeButton.type =
                "button";

            removeButton.className =
                "cover-remove-button";

            removeButton.innerHTML =
                "×";

            removeButton.title =
                "Remove selected cover";

            removeButton.addEventListener(
                "click",
                function (event) {
                    event.preventDefault();
                    event.stopPropagation();

                    coverFile =
                        null;

                    coverPreviewDataUrl =
                        null;

                    updateCoverInput();

                    renderCoverPreview();
                }
            );

            actionGroup.appendChild(
                replaceButton
            );

            actionGroup.appendChild(
                removeButton
            );

            preview.appendChild(
                image
            );

            preview.appendChild(
                actionGroup
            );

            preview.appendChild(
                fileName
            );

            return;
        }

        const existingCover =
            serverReview?.media?.find(
                function (item) {
                    return (
                        item.mediaType ===
                        "CoverPhoto"
                    );
                }
            );

        if (
            existingCover &&
            !existingCoverRemoved
        ) {
            emptyState.hidden =
                true;

            preview.hidden =
                false;

            preview.innerHTML =
                "";

            const image =
                document.createElement(
                    "img"
                );

            image.src =
                buildMediaUrl(
                    existingCover.filePath
                );

            image.alt =
                "Existing cover photo";

            const fileName =
                document.createElement(
                    "div"
                );

            fileName.className =
                "cover-file-name";

            fileName.textContent =
                existingCover.fileName;

            const actionGroup =
                document.createElement(
                    "div"
                );

            actionGroup.className =
                "cover-action-group";

            const replaceButton =
                document.createElement(
                    "button"
                );

            replaceButton.type =
                "button";

            replaceButton.className =
                "cover-replace-button";

            replaceButton.textContent =
                "Replace";

            replaceButton.addEventListener(
                "click",
                function (event) {
                    event.preventDefault();
                    event.stopPropagation();

                    coverPhotoInput?.click();
                }
            );

            const removeButton =
                document.createElement(
                    "button"
                );

            removeButton.type =
                "button";

            removeButton.className =
                "cover-remove-button";

            removeButton.innerHTML =
                "×";

            removeButton.title =
                "Remove existing cover";

            removeButton.addEventListener(
                "click",
                function (event) {
                    event.preventDefault();
                    event.stopPropagation();

                    existingCoverRemoved =
                        true;

                    coverFile =
                        null;

                    coverPreviewDataUrl =
                        null;

                    updateCoverInput();

                    renderCoverPreview();
                }
            );

            actionGroup.appendChild(
                replaceButton
            );

            actionGroup.appendChild(
                removeButton
            );

            preview.appendChild(
                image
            );

            preview.appendChild(
                actionGroup
            );

            preview.appendChild(
                fileName
            );

            return;
        }

        emptyState.hidden =
            false;

        preview.hidden =
            true;

        preview.innerHTML =
            "";
    }

    function restoreMediaOnStepFour() {
        renderCoverPreview();

        renderGalleryPreview();

        renderExistingAndNewFileList(
            "videoFileList",
            "Video",
            videoFiles
        );

        renderExistingAndNewFileList(
            "floorPlanFileList",
            "FloorPlan",
            floorPlanFiles
        );

        renderExistingAndNewFileList(
            "documentsFileList",
            "Document",
            documentFiles
        );
    }

    function renderGalleryPreview() {
        const previewGrid =
            document.getElementById(
                "galleryPreviewGrid"
            );

        if (!previewGrid) {
            return;
        }

        previewGrid.innerHTML =
            "";

        const existingGalleryImages =
            (
                serverReview?.media ||
                []
            ).filter(
                function (item) {
                    return (
                        item.mediaType ===
                        "GalleryImage" &&
                        !removedMediaIds.includes(
                            item.mediaID
                        )
                    );
                }
            );

        existingGalleryImages.forEach(
            function (item, index) {

                const previewItem =
                    document.createElement(
                        "div"
                    );

                previewItem.className =
                    "gallery-preview-item";

                const image =
                    document.createElement(
                        "img"
                    );

                image.src =
                    buildMediaUrl(
                        item.filePath
                    );

                image.alt =
                    `Gallery image ${index + 1}`;

                const actionGroup =
                    document.createElement(
                        "div"
                    );

                actionGroup.className =
                    "media-action-group";

                const replaceButton =
                    document.createElement(
                        "button"
                    );

                replaceButton.type =
                    "button";

                replaceButton.className =
                    "media-replace-button";

                replaceButton.textContent =
                    "Replace";

                replaceButton.addEventListener(
                    "click",
                    function (event) {

                        event.preventDefault();
                        event.stopPropagation();

                        pendingGalleryReplacementId =
                            item.mediaID;

                        const input =
                            document.getElementById(
                                "galleryImagesInput"
                            );

                        if (input) {
                            input.value =
                                "";

                            input.click();
                        }
                    }
                );

                const removeButton =
                    document.createElement(
                        "button"
                    );

                removeButton.type =
                    "button";

                removeButton.className =
                    "gallery-remove-button";

                removeButton.innerHTML =
                    "×";

                removeButton.title =
                    "Remove image";

                removeButton.addEventListener(
                    "click",
                    function (event) {

                        event.preventDefault();
                        event.stopPropagation();

                        if (
                            !removedMediaIds.includes(
                                item.mediaID
                            )
                        ) {
                            removedMediaIds.push(
                                item.mediaID
                            );
                        }

                        renderGalleryPreview();
                    }
                );

                actionGroup.appendChild(
                    replaceButton
                );

                actionGroup.appendChild(
                    removeButton
                );

                previewItem.appendChild(
                    image
                );

                previewItem.appendChild(
                    actionGroup
                );

                previewGrid.appendChild(
                    previewItem
                );
            }
        );

        galleryFiles.forEach(
            function (file, index) {

                const previewItem =
                    document.createElement(
                        "div"
                    );

                previewItem.className =
                    "gallery-preview-item";

                const image =
                    document.createElement(
                        "img"
                    );

                const imageUrl =
                    URL.createObjectURL(
                        file
                    );

                image.src =
                    imageUrl;

                image.alt =
                    `New gallery image ${index + 1}`;

                image.onload =
                    function () {
                        URL.revokeObjectURL(
                            imageUrl
                        );
                    };

                const removeButton =
                    document.createElement(
                        "button"
                    );

                removeButton.type =
                    "button";

                removeButton.className =
                    "gallery-remove-button";

                removeButton.innerHTML =
                    "×";

                removeButton.title =
                    "Remove image";

                removeButton.addEventListener(
                    "click",
                    function (event) {

                        event.preventDefault();
                        event.stopPropagation();

                        galleryFiles.splice(
                            index,
                            1
                        );

                        galleryReplacementIds.splice(
                            index,
                            1
                        );

                        updateGalleryInput();

                        renderGalleryPreview();
                    }
                );

                previewItem.appendChild(
                    image
                );

                previewItem.appendChild(
                    removeButton
                );

                previewGrid.appendChild(
                    previewItem
                );
            }
        );
    }

    function renderVideoList() {
        renderExistingAndNewFileList(
            "videoFileList",
            "Video",
            videoFiles
        );
    }

    function renderFloorPlanList() {
        renderExistingAndNewFileList(
            "floorPlanFileList",
            "FloorPlan",
            floorPlanFiles
        );
    }

    function renderDocumentList() {
        renderExistingAndNewFileList(
            "documentsFileList",
            "Document",
            documentFiles
        );
    }

    function renderExistingAndNewFileList(
        containerId,
        mediaType,
        newFiles
    ) {
        const container =
            document.getElementById(
                containerId
            );

        if (!container) {
            return;
        }

        container.innerHTML =
            "";

        const existingFiles =
            (
                serverReview?.media ||
                []
            ).filter(
                function (item) {
                    return (
                        item.mediaType ===
                        mediaType &&
                        !removedMediaIds.includes(
                            item.mediaID
                        )
                    );
                }
            );

        existingFiles.forEach(
            function (mediaItem) {

                const item =
                    document.createElement(
                        "div"
                    );

                item.className =
                    "media-file-item";

                const fileName =
                    document.createElement(
                        "span"
                    );

                fileName.className =
                    "media-file-name";

                fileName.textContent =
                    mediaItem.fileName;

                fileName.title =
                    mediaItem.fileName;

                const actionGroup =
                    document.createElement(
                        "div"
                    );

                actionGroup.className =
                    "media-action-group";

                const replaceButton =
                    document.createElement(
                        "button"
                    );

                replaceButton.type =
                    "button";

                replaceButton.className =
                    "media-replace-button";

                replaceButton.textContent =
                    "Replace";

                replaceButton.addEventListener(
                    "click",
                    function (event) {

                        event.preventDefault();
                        event.stopPropagation();

                        if (
                            mediaType ===
                            "Video"
                        ) {
                            pendingVideoReplacementId =
                                mediaItem.mediaID;

                            const input =
                                document.getElementById(
                                    "videoInput"
                                );

                            if (input) {
                                input.value =
                                    "";

                                input.click();
                            }
                        }
                        else if (
                            mediaType ===
                            "FloorPlan"
                        ) {
                            pendingFloorPlanReplacementId =
                                mediaItem.mediaID;

                            const input =
                                document.getElementById(
                                    "floorPlanInput"
                                );

                            if (input) {
                                input.value =
                                    "";

                                input.click();
                            }
                        }
                        else if (
                            mediaType ===
                            "Document"
                        ) {
                            pendingDocumentReplacementId =
                                mediaItem.mediaID;

                            const input =
                                document.getElementById(
                                    "documentsInput"
                                );

                            if (input) {
                                input.value =
                                    "";

                                input.click();
                            }
                        }
                    }
                );

                const removeButton =
                    document.createElement(
                        "button"
                    );

                removeButton.type =
                    "button";

                removeButton.className =
                    "media-file-remove";

                removeButton.innerHTML =
                    "×";

                removeButton.title =
                    "Remove file";

                removeButton.addEventListener(
                    "click",
                    function (event) {

                        event.preventDefault();
                        event.stopPropagation();

                        if (
                            !removedMediaIds.includes(
                                mediaItem.mediaID
                            )
                        ) {
                            removedMediaIds.push(
                                mediaItem.mediaID
                            );
                        }

                        renderExistingAndNewFileList(
                            containerId,
                            mediaType,
                            newFiles
                        );
                    }
                );

                actionGroup.appendChild(
                    replaceButton
                );

                actionGroup.appendChild(
                    removeButton
                );

                item.appendChild(
                    fileName
                );

                item.appendChild(
                    actionGroup
                );

                container.appendChild(
                    item
                );
            }
        );

        newFiles.forEach(
            function (file, index) {

                const item =
                    document.createElement(
                        "div"
                    );

                item.className =
                    "media-file-item";

                const fileName =
                    document.createElement(
                        "span"
                    );

                fileName.className =
                    "media-file-name";

                fileName.textContent =
                    file.name;

                fileName.title =
                    file.name;

                const removeButton =
                    document.createElement(
                        "button"
                    );

                removeButton.type =
                    "button";

                removeButton.className =
                    "media-file-remove";

                removeButton.innerHTML =
                    "×";

                removeButton.title =
                    "Remove file";

                removeButton.addEventListener(
                    "click",
                    function (event) {

                        event.preventDefault();
                        event.stopPropagation();

                        newFiles.splice(
                            index,
                            1
                        );

                        if (
                            mediaType ===
                            "Video"
                        ) {
                            videoReplacementIds.splice(
                                index,
                                1
                            );

                            updateVideoInput();
                        }
                        else if (
                            mediaType ===
                            "FloorPlan"
                        ) {
                            floorPlanReplacementIds.splice(
                                index,
                                1
                            );

                            updateFloorPlanInput();
                        }
                        else if (
                            mediaType ===
                            "Document"
                        ) {
                            documentReplacementIds.splice(
                                index,
                                1
                            );

                            updateDocumentsInput();
                        }

                        renderExistingAndNewFileList(
                            containerId,
                            mediaType,
                            newFiles
                        );
                    }
                );

                item.appendChild(
                    fileName
                );

                item.appendChild(
                    removeButton
                );

                container.appendChild(
                    item
                );
            }
        );
    }
    function setInputFiles(input, files) {
        const dataTransfer = new DataTransfer();

        files.forEach(function (file) {
            dataTransfer.items.add(file);
        });

        input.files = dataTransfer.files;
    }

    function updateCoverInput() {
        const input = document.getElementById("coverPhotoInput");

        if (coverFile) {
            setInputFiles(input, [coverFile]);
        } else {
            setInputFiles(input, []);
        }
    }

    function updateGalleryInput() {
        const input = document.getElementById("galleryImagesInput");
        setInputFiles(input, galleryFiles);
    }

    function updateVideoInput() {
        const input = document.getElementById("videoInput");
        setInputFiles(input, videoFiles);
    }

    function updateFloorPlanInput() {
        const input = document.getElementById("floorPlanInput");
        setInputFiles(input, floorPlanFiles);
    }

    function updateDocumentsInput() {
        const input = document.getElementById("documentsInput");
        setInputFiles(input, documentFiles);
    }

    function showError(elementId, message) {
        const element = document.getElementById(elementId);

        if (element) {
            element.textContent = message;
        }
    }

    function clearError(elementId) {
        const element = document.getElementById(elementId);

        if (element) {
            element.textContent = "";
        }
    }
    function syncMediaChangeInputs() {
        if (!form) {
            return;
        }

        form.querySelectorAll(
            ".media-change-input"
        ).forEach(function (input) {
            input.remove();
        });

        removedMediaIds.forEach(function (id) {
            addHiddenInput(
                "RemovedMediaIds",
                id
            );
        });

        galleryReplacementIds.forEach(function (id) {
            addHiddenInput(
                "GalleryReplacementIds",
                id
            );
        });

        videoReplacementIds.forEach(function (id) {
            addHiddenInput(
                "VideoReplacementIds",
                id
            );
        });

        floorPlanReplacementIds.forEach(function (id) {
            addHiddenInput(
                "FloorPlanReplacementIds",
                id
            );
        });

        documentReplacementIds.forEach(function (id) {
            addHiddenInput(
                "DocumentReplacementIds",
                id
            );
        });
    }

    function addHiddenInput(
        name,
        value
    ) {
        if (!form) {
            return;
        }

        const input =
            document.createElement(
                "input"
            );

        input.type = "hidden";
        input.name = name;
        input.value = value;
        input.className =
            "media-change-input";

        form.appendChild(input);
    }

    form?.addEventListener(
        "submit",
        function () {
            syncMediaChangeInputs();
        }
    );

    function validateStepFour() {
        let isValid = true;

        const hasExistingCover =
            !existingCoverRemoved &&
            (
                serverReview?.media?.some(
                    function (item) {
                        return (
                            item.mediaType ===
                            "CoverPhoto"
                        );
                    }
                ) === true ||
                form?.dataset.hasExistingCover ===
                "true"
            );

        const existingGalleryCount =
            (
                serverReview?.media || []
            ).filter(
                function (item) {
                    return (
                        item.mediaType ===
                        "GalleryImage" &&
                        !removedMediaIds.includes(
                            item.mediaID
                        )
                    );
                }
            ).length;

        const hasNewCover =
            coverFile !== null &&
            coverFile.size > 0 &&
            !!coverFile.name;

        const hasCover =
            hasNewCover ||
            hasExistingCover;

        const newGalleryCount =
            galleryFiles.filter(
                function (file) {
                    return (
                        file &&
                        file.size > 0 &&
                        !!file.name
                    );
                }
            ).length;

        const totalGalleryCount =
            existingGalleryCount +
            newGalleryCount;

        const hasGallery =
            totalGalleryCount > 0;

        if (!hasCover) {
            showError(
                "coverError",
                "Please upload a cover photo."
            );

            isValid = false;
        }
        else {
            clearError(
                "coverError"
            );
        }

        if (!hasGallery) {
            showError(
                "galleryError",
                "Please upload at least one gallery image."
            );

            isValid = false;
        }
        else {
            clearError(
                "galleryError"
            );
        }

        return isValid;
    }

    let localStorageSaveTimer =
        null;

    const PROPERTY_DRAFT_STORAGE_KEY =
        "estateflow-property-draft";

    function scheduleLocalStorageSave() {
        clearTimeout(
            localStorageSaveTimer
        );

        localStorageSaveTimer =
            setTimeout(
                function () {
                    savePropertyDraftToLocalStorage();
                },
                150
            );
    }

    form?.addEventListener(
        "input",
        function () {
            scheduleLocalStorageSave();
        }
    );

    form?.addEventListener(
        "change",
        function () {
            scheduleLocalStorageSave();
        }
    );

    let reviewObjectUrls = [];

    function getFieldValue(id) {
        const element =
            document.getElementById(
                id
            );

        if (!element) {
            return "";
        }

        return element.value.trim();
    }

    function collectBasics() {
        return {
            propertyTitle:
                getFieldValue(
                    "Input_PropertyTitle"
                ),

            propertyType:
                getFieldValue(
                    "Input_PropertyType"
                ),

            listingType:
                getFieldValue(
                    "Input_ListingType"
                ),

            propertyStatus:
                getFieldValue(
                    "Input_PropertyStatus"
                ),

            locationAddress:
                getFieldValue(
                    "Input_LocationAddress"
                ),

            country:
                getFieldValue(
                    "selectedCountry"
                ),

            state:
                getFieldValue(
                    "selectedState"
                ),

            city:
                getFieldValue(
                    "selectedCity"
                ),

            zipCode:
                getFieldValue(
                    "Input_ZipCode"
                )
        };
    }

    function collectDetails() {
        return {
            price:
                getFieldValue(
                    "Details_Price"
                ),

            securityDeposit:
                getFieldValue(
                    "Details_SecurityDeposit"
                ),

            area:
                getFieldValue(
                    "Details_Area"
                ),

            areaUnit:
                getFieldValue(
                    "Details_AreaUnit"
                ),

            bedrooms:
                getFieldValue(
                    "Details_Bedrooms"
                ),

            bathrooms:
                getFieldValue(
                    "Details_Bathrooms"
                ),

            balconies:
                getFieldValue(
                    "Details_Balconies"
                ),

            floor:
                getFieldValue(
                    "Details_Floor"
                ),

            parkingSpaces:
                getFieldValue(
                    "Details_ParkingSpaces"
                ),

            yearBuilt:
                getFieldValue(
                    "Details_YearBuilt"
                ),

            propertyAgeYears:
                getFieldValue(
                    "Details_PropertyAgeYears"
                ),

            possessionDate:
                getFieldValue(
                    "Details_PossessionDate"
                ),

            furnishingType:
                getFieldValue(
                    "Details_FurnishingType"
                ),

            facingDirection:
                getFieldValue(
                    "Details_FacingDirection"
                ),

            preferredTenants:
                getFieldValue(
                    "Details_PreferredTenants"
                ),

            tenantFoodPreference:
                getFieldValue(
                    "Details_TenantFoodPreference"
                ),

            description:
                getFieldValue(
                    "Details_Description"
                )
        };
    }

    function collectAmenities() {
        const selectedCards =
            document.querySelectorAll(
                ".amenity-checkbox:checked"
            );

        const amenities = [];

        selectedCards.forEach(
            function (checkbox) {

                const card =
                    checkbox.closest(
                        ".amenity-card"
                    );

                const section =
                    checkbox.closest(
                        ".amenity-section"
                    );

                const icon =
                    card?.querySelector(
                        ".amenity-icon"
                    );

                const categoryTitle =
                    section?.querySelector(
                        ".amenity-section-title"
                    );

                amenities.push({
                    name: checkbox.value,

                    category:
                        categoryTitle
                            ?.textContent
                            ?.trim() ?? "",

                    iconPath:
                        icon?.getAttribute(
                            "src"
                        ) ?? null
                });
            }
        );

        return amenities;
    }

    function getFileMetadata(file) {
        return {
            name: file.name,
            type: file.type,
            size: file.size,
            lastModified:
                file.lastModified
        };
    }

    function collectMedia() {
        return {
            coverPhoto:
                coverFile
                    ? getFileMetadata(
                        coverFile
                    )
                    : null,

            galleryImages:
                galleryFiles.map(
                    getFileMetadata
                ),

            videos:
                videoFiles.map(
                    getFileMetadata
                ),

            floorPlans:
                floorPlanFiles.map(
                    getFileMetadata
                ),

            documents:
                documentFiles.map(
                    getFileMetadata
                )
        };
    }

    function collectPropertyDraft() {
        return {
            version: 1,

            updatedAt:
                new Date().toISOString(),

            basics:
                collectBasics(),

            details:
                collectDetails(),

            amenities:
                collectAmenities(),

            media:
                collectMedia()
        };
    }

    function savePropertyDraftToLocalStorage() {
        try {
            const draft =
                collectPropertyDraft();

            localStorage.setItem(
                PROPERTY_DRAFT_STORAGE_KEY,
                JSON.stringify(draft)
            );
        }
        catch (error) {
            console.error(
                "Unable to save property draft:",
                error
            );
        }
    }

    function formatReviewCurrency(
        value
    ) {
        if (!value) {
            return "—";
        }

        const number =
            Number(value);

        if (
            Number.isNaN(number)
        ) {
            return value;
        }

        return new Intl.NumberFormat(
            "en-IN",
            {
                style: "currency",
                currency: "INR",
                maximumFractionDigits: 0
            }
        ).format(number);
    }

    function formatReviewDate(
        value
    ) {
        if (!value) {
            return "—";
        }

        const dateOnly =
            String(value)
                .split("T")[0];

        const date =
            new Date(
                `${dateOnly}T00:00:00`
            );

        if (
            Number.isNaN(
                date.getTime()
            )
        ) {
            return value;
        }

        const day =
            String(
                date.getDate()
            ).padStart(2, "0");

        const month =
            String(
                date.getMonth() + 1
            ).padStart(2, "0");

        const year =
            date.getFullYear();

        return `${day}.${month}.${year}`;
    }

    function setReviewText(
        id,
        value
    ) {
        const element =
            document.getElementById(
                id
            );

        if (!element) {
            return;
        }

        element.textContent =
            value || "—";
    }

    function buildReviewLocation(
        basics
    ) {
        const parts = [];

        if (basics.locationAddress) {
            parts.push(
                basics.locationAddress
            );
        }

        if (basics.city) {
            parts.push(
                basics.city
            );
        }

        if (basics.state) {
            parts.push(
                basics.state
            );
        }

        if (basics.country) {
            parts.push(
                basics.country
            );
        }

        let result =
            parts.join(", ");

        if (basics.zipCode) {
            if (result) {
                result +=
                    ` - ${basics.zipCode}`;
            }
            else {
                result =
                    basics.zipCode;
            }
        }

        return result;
    }

    function clearReviewObjectUrls() {
        reviewObjectUrls.forEach(
            function (url) {
                URL.revokeObjectURL(
                    url
                );
            }
        );

        reviewObjectUrls = [];
    }

    function createReviewImageUrl(
        file
    ) {
        const url =
            URL.createObjectURL(
                file
            );

        reviewObjectUrls.push(
            url
        );

        return url;
    }

    function renderServerReview() {
        if (!serverReview) {
            console.warn(
                "Property review data was not found."
            );

            return;
        }

        const property =
            serverReview.property;

        const amenities =
            serverReview.amenities ||
            [];

        const media =
            serverReview.media ||
            [];

        if (!property) {
            console.warn(
                "Property data was not found."
            );

            return;
        }

        const basics = {
            propertyTitle:
                property.propertyTitle,

            propertyType:
                property.propertyType,

            listingType:
                property.listingType,

            propertyStatus:
                property.propertyStatus,

            locationAddress:
                property.locationAddress,

            country:
                property.country,

            state:
                property.state,

            city:
                property.city,

            zipCode:
                property.zipCode
        };

        const details = {
            price:
                property.price,

            securityDeposit:
                property.securityDeposit,

            area:
                property.area,

            areaUnit:
                property.areaUnit,

            bedrooms:
                property.bedrooms,

            bathrooms:
                property.bathrooms,

            balconies:
                property.balconies,

            floor:
                property.floor,

            parkingSpaces:
                property.parkingSpaces,

            yearBuilt:
                property.yearBuilt,

            propertyAgeYears:
                property.propertyAgeYears,

            possessionDate:
                property.possessionDate,

            furnishingType:
                property.furnishingType,

            facingDirection:
                property.facingDirection,

            preferredTenants:
                property.preferredTenants,

            tenantFoodPreference:
                property.tenantFoodPreference,

            description:
                property.description
        };

        renderReviewSummary({
            basics: basics,
            details: details
        });

        renderReviewDetails(
            details
        );

        const reviewAmenities =
            amenities.map(
                function (amenity) {
                    return {
                        name:
                            amenity.amenityName,

                        iconPath:
                            getAmenityIconPath(
                                amenity.amenityName
                            )
                    };
                }
            );

        renderReviewAmenities(
            reviewAmenities
        );

        const reviewMedia = {
            floorPlans:
                media
                    .filter(
                        function (item) {
                            return (
                                item.mediaType ===
                                "FloorPlan"
                            );
                        }
                    )
                    .map(
                        function (item) {
                            return {
                                name:
                                    item.fileName
                            };
                        }
                    ),

            documents:
                media
                    .filter(
                        function (item) {
                            return (
                                item.mediaType ===
                                "Document"
                            );
                        }
                    )
                    .map(
                        function (item) {
                            return {
                                name:
                                    item.fileName
                            };
                        }
                    ),

            videos:
                media
                    .filter(
                        function (item) {
                            return (
                                item.mediaType ===
                                "Video"
                            );
                        }
                    )
                    .map(
                        function (item) {
                            return {
                                name:
                                    item.fileName
                            };
                        }
                    )
        };

        renderReviewDocuments(
            reviewMedia
        );

        renderServerReviewGallery(
            media
        );
    }

    function renderReview() {
        const draft =
            collectPropertyDraft();

        renderReviewSummary(
            draft
        );

        renderReviewDetails(
            draft.details
        );

        renderReviewAmenities(
            draft.amenities
        );

        renderReviewDocuments(
            draft.media
        );

        renderReviewGallery();
    }

    function renderReviewSummary(
        draft
    ) {
        const basics =
            draft.basics;

        const details =
            draft.details;

        setReviewText(
            "reviewPropertyTitle",
            basics.propertyTitle
        );

        renderReviewPrice(
            details.price,
            basics.listingType
        );

        setReviewText(
            "reviewListingType",
            basics.listingType
        );

        setReviewText(
            "reviewSecurityDeposit",
            formatReviewCurrency(
                details.securityDeposit
            )
        );

        setReviewText(
            "reviewBedrooms",
            details.bedrooms
        );

        setReviewText(
            "reviewBathrooms",
            details.bathrooms
                ? `${details.bathrooms} Bath`
                : "—"
        );

        setReviewText(
            "reviewArea",
            details.area
                ? `${details.area} ${details.areaUnit}`
                : "—"
        );

        setReviewText(
            "reviewDescription",
            details.description ||
            "No description provided."
        );

        const status =
            document.getElementById(
                "reviewPropertyStatus"
            );

        if (status) {
            if (
                basics.propertyStatus
            ) {
                status.hidden =
                    false;

                status.textContent =
                    basics.propertyStatus;
            }
            else {
                status.hidden =
                    true;

                status.textContent =
                    "";
            }
        }

        const location =
            document.getElementById(
                "reviewLocation"
            );

        if (location) {
            location.textContent =
                buildReviewLocation(
                    basics
                );
        }
    }

    function renderReviewDetails(
        details
    ) {
        const container =
            document.getElementById(
                "reviewDetailsGrid"
            );

        if (!container) {
            return;
        }

        container.innerHTML =
            "";

        const detailItems = [
            {
                label: "Balconies",
                value:
                    details.balconies
            },

            {
                label: "Floor",
                value:
                    details.floor
            },

            {
                label: "Parking space",
                value:
                    details.parkingSpaces
            },

            {
                label: "Year built",
                value:
                    details.yearBuilt
            },

            {
                label: "Property age",
                value:
                    details.propertyAgeYears
            },

            {
                label:
                    "Possession date",

                value:
                    formatReviewDate(
                        details.possessionDate
                    )
            },

            {
                label:
                    "Type of furnishing",

                value:
                    details.furnishingType
            },

            {
                label:
                    "Facing directions",

                value:
                    details.facingDirection
            },

            {
                label:
                    "Preferred tenants",

                value:
                    details.preferredTenants
            },

            {
                label:
                    "Tenant food preference",

                value:
                    details.tenantFoodPreference
            }
        ];

        detailItems.forEach(
            function (item) {

                if (!item.value) {
                    return;
                }

                const itemElement =
                    document.createElement(
                        "div"
                    );

                itemElement.className =
                    "review-detail-item";

                const label =
                    document.createElement(
                        "span"
                    );

                label.className =
                    "review-detail-label";

                label.textContent =
                    item.label;

                const value =
                    document.createElement(
                        "span"
                    );

                value.className =
                    "review-detail-value";

                value.textContent =
                    item.value;

                itemElement.appendChild(
                    label
                );

                itemElement.appendChild(
                    value
                );

                container.appendChild(
                    itemElement
                );
            }
        );
    }

    function renderReviewPrice(
        price,
        listingType
    ) {
        const priceElement =
            document.getElementById(
                "reviewPrice"
            );

        const priceLabel =
            document.getElementById(
                "reviewPriceLabel"
            );

        if (!priceElement) {
            return;
        }

        priceElement.textContent =
            formatReviewCurrency(
                price
            );

        if (!priceLabel) {
            return;
        }

        if (
            listingType ===
            "For Sale"
        ) {
            priceLabel.textContent =
                "";

            priceLabel.hidden =
                true;
        }
        else {
            priceLabel.textContent =
                "/ month";

            priceLabel.hidden =
                false;
        }
    }

    function renderReviewAmenities(
        amenities
    ) {
        const container =
            document.getElementById(
                "reviewAmenitiesList"
            );

        if (!container) {
            return;
        }

        container.innerHTML =
            "";

        if (
            !amenities ||
            amenities.length === 0
        ) {
            const empty =
                document.createElement(
                    "span"
                );

            empty.className =
                "review-no-items";

            empty.textContent =
                "No amenities selected.";

            container.appendChild(
                empty
            );

            return;
        }

        amenities.forEach(
            function (amenity) {

                const item =
                    document.createElement(
                        "div"
                    );

                item.className =
                    "review-amenity-item";

                if (
                    amenity.iconPath
                ) {
                    const image =
                        document.createElement(
                            "img"
                        );

                    image.className =
                        "review-amenity-icon";

                    image.src =
                        amenity.iconPath;

                    image.alt =
                        "";

                    image.onerror =
                        function () {

                            image.remove();

                            addAmenityPlaceholder(
                                item
                            );
                        };

                    item.appendChild(
                        image
                    );
                }
                else {
                    addAmenityPlaceholder(
                        item
                    );
                }

                const name =
                    document.createElement(
                        "span"
                    );

                name.textContent =
                    amenity.name;

                item.appendChild(
                    name
                );

                container.appendChild(
                    item
                );
            }
        );
    }

    function addAmenityPlaceholder(
        container
    ) {
        const placeholder =
            document.createElement(
                "span"
            );

        placeholder.className =
            "review-amenity-placeholder";

        placeholder.textContent =
            "◈";

        container.insertBefore(
            placeholder,
            container.firstChild
        );
    }

    let serverGalleryImages = [];

    function buildMediaUrl(
        filePath
    ) {
        if (!filePath) {
            return "";
        }

        if (
            /^https?:\/\//i.test(
                filePath
            )
        ) {
            return filePath;
        }

        const apiBaseUrl =
            form?.dataset.apiBaseUrl ||
            "";

        return (
            apiBaseUrl.replace(
                /\/$/,
                ""
            ) +
            "/" +
            filePath.replace(
                /^\//,
                ""
            )
        );
    }

    function renderServerReviewGallery(
        media
    ) {
        const coverImage =
            document.getElementById(
                "reviewCoverImage"
            );

        const coverPlaceholder =
            document.getElementById(
                "reviewCoverPlaceholder"
            );

        const thumbnailContainer =
            document.getElementById(
                "reviewGalleryThumbnails"
            );

        if (!thumbnailContainer) {
            return;
        }

        const cover =
            media.find(
                function (item) {
                    return (
                        item.mediaType ===
                        "CoverPhoto"
                    );
                }
            );

        const gallery =
            media.filter(
                function (item) {
                    return (
                        item.mediaType ===
                        "GalleryImage"
                    );
                }
            );

        serverGalleryImages =
            gallery;

        if (
            cover &&
            coverImage
        ) {
            const coverUrl =
                buildMediaUrl(
                    cover.filePath
                );

            if (coverPlaceholder) {
                coverPlaceholder.hidden =
                    true;

                coverPlaceholder.style.display =
                    "none";
            }

            coverImage.hidden =
                false;

            coverImage.style.display =
                "block";

            coverImage.style.visibility =
                "visible";

            coverImage.style.opacity =
                "1";

            coverImage.onerror =
                function () {

                    coverImage.hidden =
                        true;

                    coverImage.style.display =
                        "none";

                    if (coverPlaceholder) {
                        coverPlaceholder.hidden =
                            false;

                        coverPlaceholder.style.display =
                            "flex";
                    }
                };

            coverImage.src =
                coverUrl;
        }
        else {
            if (coverImage) {
                coverImage.hidden =
                    true;

                coverImage.style.display =
                    "none";

                coverImage.removeAttribute(
                    "src"
                );
            }

            if (coverPlaceholder) {
                coverPlaceholder.hidden =
                    false;

                coverPlaceholder.style.display =
                    "flex";
            }
        }

        thumbnailContainer.innerHTML =
            "";

        const visibleImages =
            gallery.slice(0, 4);

        visibleImages.forEach(
            function (item) {

                const button =
                    document.createElement(
                        "button"
                    );

                button.type =
                    "button";

                button.className =
                    "review-gallery-thumbnail";

                const image =
                    document.createElement(
                        "img"
                    );

                image.src =
                    buildMediaUrl(
                        item.filePath
                    );

                image.alt =
                    "Property gallery image";

                button.appendChild(
                    image
                );

                thumbnailContainer.appendChild(
                    button
                );
            }
        );

        if (
            gallery.length > 4
        ) {
            const remaining =
                gallery.length - 4;

            const moreButton =
                document.createElement(
                    "button"
                );

            moreButton.type =
                "button";

            moreButton.className =
                "review-gallery-more";

            const image =
                document.createElement(
                    "img"
                );

            image.src =
                buildMediaUrl(
                    gallery[4].filePath
                );

            image.alt =
                "More gallery images";

            const overlay =
                document.createElement(
                    "span"
                );

            overlay.className =
                "review-gallery-more-overlay";

            overlay.textContent =
                `+${remaining} more`;

            moreButton.appendChild(
                image
            );

            moreButton.appendChild(
                overlay
            );

            moreButton.addEventListener(
                "click",
                function () {
                    openServerGallery();
                }
            );

            thumbnailContainer.appendChild(
                moreButton
            );
        }
    }

    function openServerGallery() {
        const modal =
            document.getElementById(
                "reviewGalleryModal"
            );

        const grid =
            document.getElementById(
                "reviewGalleryModalGrid"
            );

        if (!modal || !grid) {
            return;
        }

        grid.innerHTML =
            "";

        const remainingImages =
            serverGalleryImages.slice(
                4
            );

        remainingImages.forEach(
            function (item) {

                const image =
                    document.createElement(
                        "img"
                    );

                image.className =
                    "review-gallery-modal-image";

                image.src =
                    buildMediaUrl(
                        item.filePath
                    );

                image.alt =
                    "Property gallery image";

                grid.appendChild(
                    image
                );
            }
        );

        modal.hidden =
            false;
    }

    function renderReviewGallery() {
        clearReviewObjectUrls();

        const coverImage =
            document.getElementById(
                "reviewCoverImage"
            );

        const coverPlaceholder =
            document.getElementById(
                "reviewCoverPlaceholder"
            );

        const thumbnailContainer =
            document.getElementById(
                "reviewGalleryThumbnails"
            );

        const coverInput =
            document.getElementById(
                "coverPhotoInput"
            );

        const currentCoverFile =
            coverFile ||
            coverInput?.files?.[0] ||
            null;

        if (
            coverImage &&
            coverPreviewDataUrl
        ) {
            coverImage.src =
                coverPreviewDataUrl;

            coverImage.hidden =
                false;

            if (coverPlaceholder) {
                coverPlaceholder.hidden =
                    true;
            }
        }
        else if (
            coverImage &&
            currentCoverFile
        ) {
            const temporaryUrl =
                URL.createObjectURL(
                    currentCoverFile
                );

            coverImage.src =
                temporaryUrl;

            coverImage.hidden =
                false;

            if (coverPlaceholder) {
                coverPlaceholder.hidden =
                    true;
            }

            coverImage.onload =
                function () {
                    URL.revokeObjectURL(
                        temporaryUrl
                    );
                };
        }
        else {
            if (coverImage) {
                coverImage.hidden =
                    true;

                coverImage.removeAttribute(
                    "src"
                );
            }

            if (coverPlaceholder) {
                coverPlaceholder.hidden =
                    false;
            }
        }

        if (!thumbnailContainer) {
            return;
        }

        thumbnailContainer.innerHTML =
            "";

        const visibleImages =
            galleryFiles.slice(0, 4);

        visibleImages.forEach(
            function (file) {

                const button =
                    document.createElement(
                        "button"
                    );

                button.type =
                    "button";

                button.className =
                    "review-gallery-thumbnail";

                const image =
                    document.createElement(
                        "img"
                    );

                image.src =
                    createReviewImageUrl(
                        file
                    );

                image.alt =
                    "Property gallery image";

                button.appendChild(
                    image
                );

                thumbnailContainer.appendChild(
                    button
                );
            }
        );

        if (
            galleryFiles.length > 4
        ) {
            const remainingCount =
                galleryFiles.length - 4;

            const moreButton =
                document.createElement(
                    "button"
                );

            moreButton.type =
                "button";

            moreButton.className =
                "review-gallery-more";

            const previewImage =
                document.createElement(
                    "img"
                );

            previewImage.src =
                createReviewImageUrl(
                    galleryFiles[4]
                );

            previewImage.alt =
                "More gallery images";

            const overlay =
                document.createElement(
                    "span"
                );

            overlay.className =
                "review-gallery-more-overlay";

            overlay.textContent =
                `+${remainingCount} more`;

            moreButton.appendChild(
                previewImage
            );

            moreButton.appendChild(
                overlay
            );

            moreButton.addEventListener(
                "click",
                function () {
                    openRemainingGallery();
                }
            );

            thumbnailContainer.appendChild(
                moreButton
            );
        }
    }

    function openRemainingGallery() {
        const modal =
            document.getElementById(
                "reviewGalleryModal"
            );

        const grid =
            document.getElementById(
                "reviewGalleryModalGrid"
            );

        if (!modal || !grid) {
            return;
        }

        grid.innerHTML =
            "";

        const remainingImages =
            galleryFiles.slice(4);

        remainingImages.forEach(
            function (file) {

                const image =
                    document.createElement(
                        "img"
                    );

                image.className =
                    "review-gallery-modal-image";

                image.src =
                    createReviewImageUrl(
                        file
                    );

                image.alt =
                    "Property gallery image";

                grid.appendChild(
                    image
                );
            }
        );

        modal.hidden =
            false;
    }

    function closeRemainingGallery() {
        const modal =
            document.getElementById(
                "reviewGalleryModal"
            );

        if (modal) {
            modal.hidden =
                true;
        }
    }

    const reviewGalleryModalClose =
        document.getElementById(
            "reviewGalleryModalClose"
        );

    const reviewGalleryModalBackdrop =
        document.getElementById(
            "reviewGalleryModalBackdrop"
        );

    reviewGalleryModalClose?.addEventListener(
        "click",
        function () {
            closeRemainingGallery();
        }
    );

    reviewGalleryModalBackdrop?.addEventListener(
        "click",
        function () {
            closeRemainingGallery();
        }
    );

    function getDocumentIconMarkup(
        fileName,
        type
    ) {
        const lowerName =
            fileName.toLowerCase();

        let label = "FILE";

        if (
            type === "video"
        ) {
            label = "MP4";
        }
        else if (
            lowerName.endsWith(
                ".pdf"
            )
        ) {
            label = "PDF";
        }
        else if (
            lowerName.endsWith(
                ".docx"
            )
        ) {
            label = "DOCX";
        }
        else if (
            lowerName.endsWith(
                ".doc"
            )
        ) {
            label = "DOC";
        }

        return `
            <svg
                viewBox="0 0 24 24"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
                class="review-document-icon">

                <path
                    d="M6 3H14L19 8V21H6V3Z"
                    stroke="currentColor"
                    stroke-width="1.7"
                    stroke-linejoin="round" />

                <path
                    d="M14 3V8H19"
                    stroke="currentColor"
                    stroke-width="1.7"
                    stroke-linejoin="round" />

                <path
                    d="M9 13H16"
                    stroke="currentColor"
                    stroke-width="1.4"
                    stroke-linecap="round" />

                <path
                    d="M9 16H16"
                    stroke="currentColor"
                    stroke-width="1.4"
                    stroke-linecap="round" />

            </svg>
        `;
    }

    function createReviewDocumentGroup(
        title,
        files,
        type
    ) {
        if (
            !files ||
            files.length === 0
        ) {
            return null;
        }

        const group =
            document.createElement(
                "div"
            );

        group.className =
            "review-document-group";

        const groupTitle =
            document.createElement(
                "div"
            );

        groupTitle.className =
            "review-document-group-title";

        groupTitle.textContent =
            title;

        const itemContainer =
            document.createElement(
                "div"
            );

        itemContainer.className =
            "review-document-items";

        files.forEach(
            function (file) {

                const item =
                    document.createElement(
                        "div"
                    );

                item.className =
                    "review-document-item";

                const iconWrapper =
                    document.createElement(
                        "span"
                    );

                iconWrapper.innerHTML =
                    getDocumentIconMarkup(
                        file.name,
                        type
                    );

                const name =
                    document.createElement(
                        "span"
                    );

                name.className =
                    "review-document-name";

                name.textContent =
                    file.name;

                name.title =
                    file.name;

                item.appendChild(
                    iconWrapper
                );

                item.appendChild(
                    name
                );

                itemContainer.appendChild(
                    item
                );
            }
        );

        group.appendChild(
            groupTitle
        );

        group.appendChild(
            itemContainer
        );

        return group;
    }

    function renderReviewDocuments(media) {
        const container =
            document.getElementById(
                "reviewDocumentsList"
            );

        if (!container) {
            return;
        }

        container.innerHTML = "";

        const floorPlans =
            media?.floorPlans || [];

        const documents =
            media?.documents || [];

        const videos =
            media?.videos || [];

        let hasDocuments = false;

        const floorPlanGroup =
            createReviewDocumentGroup(
                "Floor plans",
                floorPlans,
                "floorPlan"
            );

        if (floorPlanGroup) {
            container.appendChild(
                floorPlanGroup
            );

            hasDocuments = true;
        }

        const documentGroup =
            createReviewDocumentGroup(
                "Other documents",
                documents,
                "document"
            );

        if (documentGroup) {
            container.appendChild(
                documentGroup
            );

            hasDocuments = true;
        }

        const videoGroup =
            createReviewDocumentGroup(
                "Videos",
                videos,
                "video"
            );

        if (videoGroup) {
            container.appendChild(
                videoGroup
            );

            hasDocuments = true;
        }

        if (!hasDocuments) {
            const empty =
                document.createElement(
                    "span"
                );

            empty.className =
                "review-no-items";

            empty.textContent =
                "No documents uploaded.";

            container.appendChild(
                empty
            );
        }
    }

    function getAmenityIconPath(
        amenityName
    ) {
        const checkboxes =
            document.querySelectorAll(
                ".amenity-checkbox"
            );

        for (
            const checkbox
            of checkboxes
        ) {
            if (
                checkbox.value
                    .trim()
                    .toLowerCase() ===
                amenityName
                    .trim()
                    .toLowerCase()
            ) {
                const card =
                    checkbox.closest(
                        ".amenity-card"
                    );

                const image =
                    card?.querySelector(
                        ".amenity-icon"
                    );

                return (
                    image?.getAttribute(
                        "src"
                    ) || null
                );
            }
        }

        return null;
    }

    initializeMediaUpload();

    showStep(
        currentStep
    );

    loadCountries();
});