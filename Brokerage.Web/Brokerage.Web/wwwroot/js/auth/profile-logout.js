document.addEventListener('DOMContentLoaded', () => {

    const profileToggle =
        document.getElementById('userProfileToggle');

    const profileDropdown =
        document.getElementById('profileDropdown');

    const profileWrapper =
        document.querySelector('.user-profile-wrapper');

    const logoutBtn =
        document.getElementById('logoutBtn');

    if (!profileToggle || !profileDropdown) {
        return;
    }

    // TOGGLE DROPDOWN


    profileToggle.addEventListener('click', (e) => {

        e.stopPropagation();

        const isHidden =
            profileDropdown.hasAttribute('hidden');

        if (isHidden) {

            profileDropdown.removeAttribute('hidden');

            profileWrapper?.classList.add('active');

            profileToggle.setAttribute(
                'aria-expanded',
                'true'
            );

        } else {

            closeDropdown();
        }
    });

    // CLOSE WHEN CLICKING OUTSIDE


    document.addEventListener('click', (e) => {

        if (
            profileWrapper &&
            !profileWrapper.contains(e.target)
        ) {
            closeDropdown();
        }

    });


    // CLOSE WITH ESCAPE


    document.addEventListener('keydown', (e) => {

        if (e.key === 'Escape') {

            closeDropdown();

        }

    });


    function closeDropdown() {

        profileDropdown.setAttribute(
            'hidden',
            ''
        );

        profileWrapper?.classList.remove(
            'active'
        );

        profileToggle?.setAttribute(
            'aria-expanded',
            'false'
        );
    }

    // LOGOUT


    if (logoutBtn) {

        logoutBtn.addEventListener(
            'click',
            async () => {

                logoutBtn.disabled = true;

                try {

                    const token =
                        document.querySelector(
                            'input[name="__RequestVerificationToken"]'
                        )?.value;

                    const response =
                        await fetch(
                            '/Account/Logout',
                            {
                                method: 'POST',
                                headers: {
                                    'RequestVerificationToken':
                                        token
                                }
                            }
                        );

                    if (response.redirected) {

                        window.location.href =
                            response.url;

                        return;
                    }

                    if (!response.ok) {

                        throw new Error(
                            'Logout failed.'
                        );

                    }

                    window.location.href =
                        '/Account/Login';

                }
                catch (error) {

                    console.error(
                        'Logout error:',
                        error
                    );

                    logoutBtn.disabled = false;

                    alert(
                        'Unable to logout. Please try again.'
                    );
                }

            }
        );

    }

});