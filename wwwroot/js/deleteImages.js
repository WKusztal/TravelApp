document.addEventListener("DOMContentLoaded", () => {
    const removeButtons = document.querySelectorAll(".remove-image-btn");

    removeButtons.forEach(button => {
        button.addEventListener("click", async () => {
            const imageId = button.getAttribute("data-image-id");

            if (!imageId || isNaN(parseInt(imageId, 10))) {
                return;
            }

            if (confirm("Czy na pewno chcesz usunąć to zdjęcie?")) {
                try {
                    const response = await fetch(`/Home/DeleteImage`, {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/json",
                            "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]').value,
                        },
                        body: JSON.stringify({ imageId: parseInt(imageId, 10) }),
                    });

                    if (!response.ok) {
                        throw new Error(`HTTP error! Status: ${response.status}`);
                    }

                    const data = await response.json();

                    if (data.success) {
                        const imageContainer = button.closest(".image-container");
                        if (imageContainer) {
                            imageContainer.remove();
                        }
                    } else {
                        alert(`Błąd: ${data.message}`);
                    }
                } catch (error) {
                    alert(`Wystąpił błąd: ${error.message}`);
                }
            }
        });
    });
});
