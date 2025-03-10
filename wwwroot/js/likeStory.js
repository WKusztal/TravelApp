document.addEventListener("DOMContentLoaded", () => {
    const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    const viewContainer = document.querySelector('[data-view]');
    if (!viewContainer) {
        console.error("Nie znaleziono kontenera z atrybutem `data-view`.");
        return;
    }

    const viewType = viewContainer.getAttribute('data-view');
    console.log("Rozpoznany widok:", viewType);

    const likeClass = viewType === "story-details" ? "btn-details-like" : "btn-story-like";
    const dislikeClass = viewType === "story-details" ? "btn-details-dislike" : "btn-story-dislike";

    const likeButtons = document.querySelectorAll(`.${likeClass}`);
    const dislikeButtons = document.querySelectorAll(`.${dislikeClass}`);

    console.log("Przyciski like:", likeButtons);
    console.log("Przyciski dislike:", dislikeButtons);

    if (likeButtons.length === 0 || dislikeButtons.length === 0) {
        console.warn("Nie znaleziono przycisków dla widoku:", viewType);
    }

    likeButtons.forEach(button => {
        button.addEventListener("click", () => toggleReaction(button, "LikeStory"));
    });

    dislikeButtons.forEach(button => {
        button.addEventListener("click", () => toggleReaction(button, "DislikeStory"));
    });

    async function toggleReaction(button, action) {
        const storyId = button.getAttribute("data-story-id");

        if (!storyId || isNaN(parseInt(storyId, 10))) {
            console.error("Nieprawidłowe ID relacji:", storyId);
            return;
        }

        try {
            const response = await fetch(`/Home/${action}`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": csrfToken,
                },
                body: JSON.stringify({ storyId: parseInt(storyId, 10) }),
            });

            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }

            const data = await response.json();

            if (data.success) {
                const likesCountElement = document.querySelector(`#likes-count-${storyId}`);
                const dislikesCountElement = document.querySelector(`#dislikes-count-${storyId}`);

                if (likesCountElement) {
                    likesCountElement.textContent = data.likes;
                }

                if (dislikesCountElement) {
                    dislikesCountElement.textContent = data.dislikes;
                }

                updateButtonState(storyId, action === "LikeStory");
            } else {
                alert(`Błąd: ${data.message}`);
            }
        } catch (error) {
            console.error("Błąd podczas wysyłania żądania:", error);
        }
    }

    function updateButtonState(storyId, isLike) {
        const likeButton = document.querySelector(`.${likeClass}[data-story-id="${storyId}"]`);
        const dislikeButton = document.querySelector(`.${dislikeClass}[data-story-id="${storyId}"]`);

        if (isLike) {
            likeButton?.classList.toggle("active");
            dislikeButton?.classList.remove("active");
        } else {
            dislikeButton?.classList.toggle("active");
            likeButton?.classList.remove("active");
        }
    }
});
