document.addEventListener("DOMContentLoaded", () => {
    const editButton = document.getElementById("edit-profile-btn");
    const mobileEditButton = document.getElementById("mobile-edit-profile-btn");
    const cancelButton = document.getElementById("cancel-edit-btn");
    const editSection = document.getElementById("edit-profile-section");
    const bioDisplay = document.getElementById("bio-display");

    const openEditProfile = (e) => {
        e.preventDefault();
        editSection.style.display = "block";
        bioDisplay.style.display = "none";
    };

    if (editButton) {
        editButton.addEventListener("click", openEditProfile);
    }

    if (mobileEditButton) {
        mobileEditButton.addEventListener("click", openEditProfile);
    }

    cancelButton.addEventListener("click", () => {
        editSection.style.display = "none";
        bioDisplay.style.display = "block";
    });
});
