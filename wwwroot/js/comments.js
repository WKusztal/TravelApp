document.addEventListener("DOMContentLoaded", function () {
    const commentForm = document.getElementById("commentForm");
    const commentList = document.getElementById("commentList");
    const addCommentBtn = document.querySelector("#addCommentBtn");

    if (addCommentBtn) {
        addCommentBtn.addEventListener("click", function () {
            const commentText = document.querySelector("#commentText").value.trim();
            const storyId = this.getAttribute("data-story-id");

            if (!commentText) {
                alert("Komentarz nie może być pusty.");
                return;
            }

            console.log("Dodawanie komentarza...");
            console.log("Story ID: ", storyId);
            console.log("Komentarz: ", commentText);

            fetch("/Comment/AddComment", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "X-CSRF-TOKEN": document.querySelector("input[name='__RequestVerificationToken']").value
                },
                body: JSON.stringify({
                    StoryId: storyId,
                    CommentText: commentText
                })
            })
            .then(response => response.json())
            .then(data => {
                console.log("Odpowiedź serwera: ", data);

                if (data.success) {
                    const newComment = document.createElement("div");
                    newComment.classList.add("comment-item");
                    newComment.dataset.commentId = data.commentId;

                    let avatarPath = data.userAvatar ? `/uploads/${data.userAvatar}` : "/images/default-user-image.jpg";

                    console.log("Dane użytkownika: ");
                    console.log("User ID: ", data.userId);
                    console.log("Username: ", data.username);
                    console.log("Avatar: ", data.userAvatar);
                    console.log("Is current user: ", data.isCurrentUser);

                    const createdAtDate = new Date(data.createdAt);
                    const formattedDate = createdAtDate.toLocaleString("pl-PL", {
                        day: "2-digit",
                        month: "2-digit",
                        year: "numeric",
                        hour: "2-digit",
                        minute: "2-digit"
                    }).replace(",", "");

                    newComment.innerHTML = `
                        <div class="comment-header">
                            <div class="comment-user">
                                ${
                                    data.userId ? 
                                    (data.isCurrentUser ? `
                                        <a href="/Account/Profile" class="comment-user-link d-flex align-items-center">
                                            <img src="${avatarPath}" class="comment-avatar" alt="Avatar">
                                            <strong class="comment-username">${data.username}</strong>
                                        </a>
                                    ` : `
                                        <a href="/Account/UserProfile?userId=${data.userId}" class="comment-user-link d-flex align-items-center">
                                            <img src="${avatarPath}" class="comment-avatar" alt="Avatar">
                                            <strong class="comment-username">${data.username}</strong>
                                        </a>
                                    `) 
                                    : `
                                        <span class="deleted-user">[Usunięty użytkownik]</span>
                                    `
                                }
                            </div>
                            <span class="comment-date">${formattedDate}</span>
                            <div class="comment-options">
                                <button class="options-btn">⋮</button> <!-- Widoczny przycisk opcji -->
                                <div class="delete-option"> <!-- Opcja usuwania niewidoczna -->
                                    <button class="delete-btn">Usuń</button>
                                </div>
                            </div>
                        </div>
                        <p>${data.content}</p>
                    `;

                    commentList.prepend(newComment);
                    document.querySelector("#commentText").value = "";

                    console.log("Dodano komentarz ID:", data.commentId);
                } else {
                    alert("Wystąpił błąd: " + (data.message || "Nie udało się dodać komentarza."));
                }
            })
            .catch(error => {
                console.error("Błąd:", error);
                alert("Wystąpił błąd podczas dodawania komentarza.");
            });
        });
    }
});
