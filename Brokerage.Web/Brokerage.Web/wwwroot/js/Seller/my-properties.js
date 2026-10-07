document.addEventListener(
    "DOMContentLoaded",
    function () {

        initializePropertySorting();

        initializePropertyImages();

    }
);


/* 
   PROPERTY SORTING
 */

function initializePropertySorting() {

    const sortSelect =
        document.getElementById(
            "propertySort"
        );

    const propertiesGrid =
        document.getElementById(
            "propertiesGrid"
        );


    if (
        !sortSelect ||
        !propertiesGrid
    ) {
        return;
    }


    sortSelect.addEventListener(
        "change",
        function () {

            sortPropertyCards(
                propertiesGrid,
                sortSelect.value
            );

        }
    );

}


/* SORT PROPERTY CARDS */

function sortPropertyCards(
    grid,
    sortType) {

    const cards =
        Array.from(
            grid.querySelectorAll(
                ".property-card"
            )
        );


    cards.sort(
        function (firstCard, secondCard) {

            const firstCreated =
                Number(
                    firstCard.dataset.createdAt
                );


            const secondCreated =
                Number(
                    secondCard.dataset.createdAt
                );


            const firstPrice =
                Number(
                    firstCard.dataset.price
                );


            const secondPrice =
                Number(
                    secondCard.dataset.price
                );


            switch (sortType) {

                case "oldest":

                    return (
                        firstCreated -
                        secondCreated
                    );


                case "price-high":

                    return (
                        secondPrice -
                        firstPrice
                    );


                case "price-low":

                    return (
                        firstPrice -
                        secondPrice
                    );


                case "latest":

                default:

                    return (
                        secondCreated -
                        firstCreated
                    );

            }

        }
    );


    cards.forEach(
        function (card) {

            grid.appendChild(card);

        }
    );

}


/* =========================================================
   PROPERTY IMAGE FALLBACK
   ========================================================= */

function initializePropertyImages() {

    const images =
        document.querySelectorAll(
            "[data-property-image]"
        );


    images.forEach(
        function (image) {

            image.addEventListener(
                "error",
                function () {

                    const placeholder =
                        image.dataset.placeholder;


                    if (
                        placeholder &&
                        image.src !==
                        new URL(
                            placeholder,
                            window.location.origin
                        ).href
                    ) {

                        image.src =
                            placeholder;

                    }

                }
            );

        }
    );

}