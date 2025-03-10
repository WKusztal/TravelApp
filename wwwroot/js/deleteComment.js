document.addEventListener("DOMContentLoaded", function () {
    document.querySelector("#commentList").addEventListener("click", function (event) {
        if (event.target.classList.contains("options-btn")) {
            const commentItem = event.target.closest(".comment-item");
            const deleteOption = commentItem.querySelector(".delete-option");

            deleteOption.classList.toggle("visible");

            document.querySelectorAll(".delete-option").forEach(option => {
                if (option !== deleteOption) {
                    option.classList.remove("visible");
                }
            });

            console.log("Przełączono opcje dla komentarza ID:", commentItem.dataset.commentId);
        }

        if (event.target.classList.contains("delete-btn")) {
            console.log("Kliknięto usuń");

            event.preventDefault();

            const commentItem = event.target.closest(".comment-item");
            const commentId = commentItem.dataset.commentId;

            if (!commentId) {
                console.error("Nie znaleziono ID komentarza.");
                return;
            }

            fetch(`/Comment/DeleteComment`, {
                method: "DELETE",
                headers: {
                    "Content-Type": "application/json",
                    "X-CSRF-TOKEN": document.querySelector("input[name='__RequestVerificationToken']").value
                },
                body: JSON.stringify({ CommentId: commentId })
            })
            .then(response => response.json().then(data => {
                if (!response.ok) {
                    console.error("Błąd serwera:", data);
                    alert("Błąd: " + data.message);
                } else {
                    commentItem.remove();
                    console.log("Komentarz usunięty!");
                }
            }))
            .catch(error => {
                console.error("Błąd:", error);
                alert("Wystąpił błąd podczas usuwania komentarza.");
            });
        }
    });
});