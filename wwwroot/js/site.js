// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", () => {
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    [...tooltipTriggerList].forEach((tooltipTriggerEl) => {
        new bootstrap.Tooltip(tooltipTriggerEl);
    });

    document.querySelectorAll("[data-confirm]").forEach((element) => {
        element.addEventListener("click", (event) => {
            const message = element.getAttribute("data-confirm");
            if (message && !window.confirm(message)) {
                event.preventDefault();
                event.stopPropagation();
            }
        });
    });

    const loginTypeInput = document.querySelector("#LoginType");
    const loginModeButtons = document.querySelectorAll("[data-login-mode]");
    if (loginTypeInput && loginModeButtons.length) {
        loginModeButtons.forEach((button) => {
            button.addEventListener("click", () => {
                loginTypeInput.value = button.getAttribute("data-login-mode") || "Student";
                loginModeButtons.forEach((item) => item.classList.remove("active"));
                button.classList.add("active");
            });
        });
    }

    const notificationButton = document.querySelector(".notification-button");
    if (notificationButton) {
        const notificationKey = notificationButton.getAttribute("data-notification-key") || "empty";
        const storageKey = "topluluk-last-read-notification-key";
        notificationButton.addEventListener("shown.bs.dropdown", () => {
            window.localStorage.setItem(storageKey, notificationKey);
            const badge = notificationButton.querySelector(".notification-badge");
            if (badge) {
                badge.remove();
            }
        });

        if (window.localStorage.getItem(storageKey) === notificationKey) {
            const badge = notificationButton.querySelector(".notification-badge");
            if (badge) {
                badge.remove();
            }
        }
    }
});
