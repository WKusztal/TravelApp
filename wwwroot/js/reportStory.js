document.addEventListener("DOMContentLoaded", function () {
    const reportBtns = document.querySelectorAll(".report-btn");
    const modal = document.getElementById("reportModal");
    const closeBtn = document.querySelector(".close");
    const cancelBtn = document.getElementById("cancelReport");
    const reportForm = document.getElementById("reportForm");
    const submitBtn = reportForm.querySelector("button[type='submit']");

    if (reportBtns.length === 0) {
        console.error("❌ Nie znaleziono żadnego przycisku zgłaszania!");
        return;
    }

    console.log("✅ Przycisk(i) zgłaszania znalezione:", reportBtns);

    reportBtns.forEach(btn => {
        btn.addEventListener("click", () => {
            console.log("🛑 Kliknięto przycisk zgłaszania!");
            modal.style.display = "block";
        });
    });

    function closeModal() {
        modal.style.display = "none";
        reportForm.reset();
    }

    closeBtn.addEventListener("click", closeModal);
    cancelBtn.addEventListener("click", closeModal);

    reportForm.addEventListener("submit", function (e) {
        e.preventDefault();
        submitBtn.disabled = true;

        const formData = new FormData(reportForm);

        fetch("/Home/ReportStory", {
            method: "POST",
            body: new URLSearchParams(formData),
            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            }
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert("✅ Zgłoszenie zostało pomyślnie wysłane!");
                closeModal();
            } else {
                alert("❌ Wystąpił błąd: " + (data.message || "Spróbuj ponownie później."));
            }
        })
        .catch(error => {
            console.error("Błąd:", error);
            alert("⚠️ Wystąpił problem z połączeniem. Spróbuj ponownie.");
        })
        .finally(() => {
            submitBtn.disabled = false;
        });
    });

    modal.style.display = "none";
});
