        const approvalsGrid =
            document.getElementById("approvalsGrid");

        const loadMoreButton =
            document.getElementById("loadMoreButton");

        const countIndicator =
            document.getElementById("countIndicator");

        if (approvalsGrid && loadMoreButton)
        {
            let currentPage =
                Number(
                    approvalsGrid.dataset.currentPage
                );

            const totalPages =
                Number(
                    approvalsGrid.dataset.totalPages
                );

            let loadedCount =
                approvalsGrid.children.length;

            const totalRecords =
                Number(
                    approvalsGrid.dataset.totalRecords
                );

            loadMoreButton.addEventListener(
                "click",
                async function ()
                {
                    const nextPage =
                        currentPage + 1;

                    if (nextPage > totalPages)
                    {
                        return;
                    }

                    loadMoreButton.disabled = true;

                    try
                    {
                        const response =
                            await fetch(
                                `?handler=LoadMore&pageNumber=${nextPage}`,
                                {
                                    method: "GET",
                                    headers:
                                    {
                                        "X-Requested-With":
                                            "XMLHttpRequest"
                                    }
                                });

                        if (!response.ok)
                        {
                            throw new Error(
                                "Unable to load more listings."
                            );
                        }

                        const html =
                            await response.text();

                        const temporaryContainer =
                            document.createElement("div");

                        temporaryContainer.innerHTML =
                            html;

                        const cards =
                            temporaryContainer.querySelectorAll(
                                ".approval-card"
                            );

                        cards.forEach(
                            function (card)
                            {
                                approvalsGrid.appendChild(
                                    card
                                );
                            });

                        loadedCount +=
                            cards.length;

                        currentPage =
                            nextPage;

                        approvalsGrid.dataset.currentPage =
                            currentPage;

                        countIndicator.textContent =
                            `SHOWING ${loadedCount} OF ${totalRecords} PENDING LISTINGS`;

                        if (currentPage >= totalPages)
                        {
                            loadMoreButton.style.display =
                                "none";
                        }
                    }
                    catch (error)
                    {
                        alert(
                            error.message
                        );
                    }
                    finally
                    {
                        if (currentPage < totalPages)
                        {
                            loadMoreButton.disabled =
                                false;
                        }
                    }
                });
        }