document.addEventListener("DOMContentLoaded", function () {

    const emailInput = document.getElementById("Register_Email");
    const warningIcon = document.querySelector(".input-warning-icon");

    if (!emailInput || !warningIcon) {
        return;
    }

    emailInput.addEventListener("input", function () {

        const email = emailInput.value.trim();

        if (email === "") {
            warningIcon.classList.remove("show");
            return;
        }

        if (emailInput.validity.valid) {
            warningIcon.classList.remove("show");
        }
        else {
            warningIcon.classList.add("show");
        }
    });

});