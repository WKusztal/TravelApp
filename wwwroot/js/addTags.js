document.addEventListener('DOMContentLoaded', () => {
    const tagsInput = document.getElementById('tags-input');
    const hiddenTags = document.getElementById('tags');

    tagsInput.addEventListener('blur', () => {
        const tags = tagsInput.value.split(',').map(tag => tag.trim()).filter(tag => tag !== '');
        if (tags.length < 3 || tags.length > 10) {
            alert('Dodaj od 3 do 10 tagów.');
            return;
        }
        hiddenTags.value = JSON.stringify(tags);
    });
});