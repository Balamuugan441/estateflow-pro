document.addEventListener("DOMContentLoaded", function () {

    const form = document.querySelector("form");

    // Definition of fields, input elements, error containers, and validation rules
    const fields = {
        fullName: {
            input: document.getElementById("Register_FullName"),
            errorSpan: document.querySelector('[data-valmsg-for="Register.FullName"]'),
            validate: function () {
                const value = this.input ? this.input.value.trim() : "";
                if (!value) {
                    return "Full Name is required.";
                }
                if (value.length > 50) {
                    return "Full Name cannot exceed 50 characters.";
                }
                return "";
            }
        },
        email: {
            input: document.getElementById("Register_Email"),
            errorSpan: document.querySelector('[data-valmsg-for="Register.Email"]'),
            warningIcon: document.querySelector(".input-warning-icon"),
            regex: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/,
            validate: function () {
                const value = this.input ? this.input.value.trim() : "";
                if (!value) {
                    if (this.warningIcon) this.warningIcon.classList.remove("show");
                    return "Email Address is required.";
                }
                if (!this.regex.test(value)) {
                    if (this.warningIcon) this.warningIcon.classList.add("show");
                    return "Please enter a valid email address.";
                }
                if (this.warningIcon) this.warningIcon.classList.remove("show");
                return "";
            }
        },
        mobileNumber: {
            input: document.getElementById("Register_MobileNumber"),
            errorSpan: document.querySelector('[data-valmsg-for="Register.MobileNumber"]'),
            regex: /^(?:\+91[\s-]?)?[6-9]\d{9}$/,
            validate: function () {
                const value = this.input ? this.input.value.trim() : "";
                if (!value) {
                    return "Phone Number is required.";
                }
                if (!this.regex.test(value)) {
                    return "Please enter a valid international mobile number.";
                }
                return "";
            }
        },
        role: {
            input: document.getElementById("Register_Role"),
            errorSpan: document.querySelector('[data-valmsg-for="Register.Role"]'),
            validate: function () {
                const value = this.input ? this.input.value.trim() : "";
                if (!value) {
                    return "Please select a role.";
                }
                return "";
            }
        },
        password: {
            input: document.getElementById("Register_Password"),
            errorSpan: document.querySelector('[data-valmsg-for="Register.Password"]'),
            regex: /^(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{8,}$/,
            validate: function () {
                const value = this.input ? this.input.value : "";
                if (!value) {
                    return "Password is required.";
                }
                if (value.length < 8 || value.length > 100) {
                    return "Password must be between 8 and 100 characters.";
                }
                if (!this.regex.test(value)) {
                    return "Password must contain at least one uppercase letter, one lowercase letter, and one special character.";
                }
                return "";
            }
        },
        confirmPassword: {
            input: document.getElementById("Register_ConfirmPassword"),
            errorSpan: document.querySelector('[data-valmsg-for="Register.ConfirmPassword"]'),
            validate: function () {
                const value = this.input ? this.input.value : "";
                const passwordVal = fields.password.input ? fields.password.input.value : "";
                if (!value) {
                    return "Please confirm your password.";
                }
                if (value !== passwordVal) {
                    return "Passwords do not match.";
                }
                return "";
            }
        }
    };

    // Displays or clears field error messages dynamically
    function setFieldError(fieldKey, errorMessage) {
        const field = fields[fieldKey];
        if (!field) return;

        if (field.errorSpan) {
            field.errorSpan.textContent = errorMessage;
            field.errorSpan.classList.toggle("field-validation-error", !!errorMessage);
            field.errorSpan.classList.toggle("field-validation-valid", !errorMessage);
        }

        if (field.input) {
            field.input.classList.toggle("input-validation-error", !!errorMessage);
        }
    }

    // Attach real-time validation on typing and blur
    Object.keys(fields).forEach(key => {
        const field = fields[key];
        if (field.input) {
            field.input.addEventListener("input", function () {
                const error = field.validate();
                setFieldError(key, error);

                // Re-validate Confirm Password when Password changes
                if (key === "password" && fields.confirmPassword.input && fields.confirmPassword.input.value) {
                    setFieldError("confirmPassword", fields.confirmPassword.validate());
                }
            });

            field.input.addEventListener("blur", function () {
                const error = field.validate();
                setFieldError(key, error);
            });
        }
    });

    // Form submission interceptor
    if (form) {
        form.addEventListener("submit", function (event) {
            let isFormValid = true;

            // Run validation across all fields on submit
            Object.keys(fields).forEach(key => {
                const error = fields[key].validate();
                setFieldError(key, error);
                if (error) {
                    isFormValid = false;
                }
            });

            // Prevent page reload if any validation error exists
            if (!isFormValid) {
                event.preventDefault();
            }
        });
    }
});