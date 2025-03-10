document.addEventListener('DOMContentLoaded', () => {
    const followButton = document.getElementById('follow-button');

    if (!followButton) {
        console.error("❌ Nie znaleziono przycisku obserwowania.");
        return;
    }

    const userId = followButton.getAttribute('data-user-id');

    fetch(`/Account/GetFollowStatus?userId=${userId}`)
        .then(response => response.json())
        .then(data => {
            if (data.isFollowing) {
                followButton.textContent = 'Obserwujesz';
                followButton.classList.add('following');
            } else {
                followButton.textContent = 'Obserwuj';
                followButton.classList.remove('following');
            }
        })
        .catch(error => console.error("❌ Błąd pobierania statusu obserwacji:", error));

    followButton.addEventListener('click', () => {
        const csrfTokenElement = document.querySelector('input[name="__RequestVerificationToken"]');

        if (!csrfTokenElement) {
            console.error("❌ Brak tokena CSRF w formularzu.");
            alert("Błąd uwierzytelnienia. Odśwież stronę i spróbuj ponownie.");
            return;
        }

        const csrfToken = csrfTokenElement.value;

        fetch('/Account/ToggleFollow', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': csrfToken
            },
            body: JSON.stringify({ userId: parseInt(userId, 10) })
        })
        .then(response => response.json())
        .then(data => {
            if (!data || typeof data.followersCount === 'undefined') {
                throw new Error("Niepoprawna odpowiedź serwera.");
            }

            const followersLink = document.querySelector('.stat a[href*="Followers"]');
            if (followersLink) {
                followersLink.textContent = `Obserwujący: ${data.followersCount}`;
            }
            
            if (followButton.textContent.trim() === 'Obserwuj') {
                followButton.textContent = 'Obserwujesz';
                followButton.classList.add('following');
            } else {
                followButton.textContent = 'Obserwuj';
                followButton.classList.remove('following');
            }

            console.log(`ℹ️ Zmiana przycisku: ${followButton.textContent}`);
        })
        .catch(error => {
            console.error('❌ Błąd:', error);
            alert('Wystąpił błąd. Spróbuj ponownie później.');
        });
    });
});
