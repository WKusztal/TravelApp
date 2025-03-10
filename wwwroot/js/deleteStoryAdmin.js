document.addEventListener("DOMContentLoaded", function () {
    const deleteButton = document.getElementById("deleteStoryAdminBtn");
    if (deleteButton) {
        deleteButton.addEventListener("click", function () {
            if (confirm("Czy na pewno chcesz usunąć tę relację?")) {
                document.getElementById("deleteStoryAdminForm").submit();
            }
        });
    }
});
