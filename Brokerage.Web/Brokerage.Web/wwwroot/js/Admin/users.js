        const searchInput =
            document.getElementById("userSearch");

        const filterForm =
            document.getElementById("userFilterForm");

        let searchTimer;

        searchInput?.addEventListener(
            "input",
            function () {

                clearTimeout(searchTimer);

                searchTimer =
                    setTimeout(
                        function () {
                            filterForm.submit();
                        },
                        350);
            });