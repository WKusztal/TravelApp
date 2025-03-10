document.addEventListener("DOMContentLoaded", () => {
    const lightbox = document.getElementById("lightbox");
    const lightboxImg = document.getElementById("lightbox-img");
    const lightboxPrev = document.getElementById("lightbox-prev");
    const lightboxNext = document.getElementById("lightbox-next");
    const lightboxClose = document.getElementById("lightbox-close");

    const allPhotos = Array.from(document.querySelectorAll(".photo img, .hidden-photo"))
        .map((img, index) => ({
            src: img.src,
            index: index
        }));

    let currentIndex = 0;

    const openLightbox = (index) => {
        currentIndex = index;
        lightboxImg.src = allPhotos[currentIndex].src;
        lightbox.classList.add("visible");
    };

    const closeLightbox = () => {
        lightbox.classList.remove("visible");
    };

    const showPrev = () => {
        currentIndex = (currentIndex - 1 + allPhotos.length) % allPhotos.length;
        lightboxImg.src = allPhotos[currentIndex].src;
    };

    const showNext = () => {
        currentIndex = (currentIndex + 1) % allPhotos.length;
        lightboxImg.src = allPhotos[currentIndex].src;
    };

    document.querySelectorAll(".photo img, .more-photos img").forEach((photo, index) => {
        photo.addEventListener("click", () => {
            openLightbox(index);
        });
    });

    document.querySelector(".more-photos .overlay")?.addEventListener("click", () => {
        openLightbox(3);
    });

    lightboxClose.addEventListener("click", closeLightbox);
    lightboxPrev.addEventListener("click", showPrev);
    lightboxNext.addEventListener("click", showNext);

    lightbox.addEventListener("click", (event) => {
        if (event.target === lightbox) closeLightbox();
    });

    document.addEventListener("keydown", (event) => {
        if (lightbox.classList.contains("visible")) {
            if (event.key === "ArrowLeft") showPrev();
            if (event.key === "ArrowRight") showNext();
            if (event.key === "Escape") closeLightbox();
        }
    });
});
