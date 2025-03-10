const selectedFiles = [];

document.getElementById('images').addEventListener('change', function (event) {
    const files = Array.from(event.target.files);

    files.forEach(file => {
        if (!selectedFiles.find(f => f.name === file.name && f.lastModified === file.lastModified && f.size === file.size)) {
            selectedFiles.push(file);
        }
    });

    event.target.value = '';

    updateSelectedFilesView();
});

function updateSelectedFilesView() {
    const fileList = document.getElementById('selected-files-list');
    fileList.innerHTML = '';

    selectedFiles.forEach((file, index) => {
        const listItem = document.createElement('li');
        listItem.textContent = file.name;

        const removeButton = document.createElement('button');
        removeButton.textContent = 'Usuń';
        removeButton.classList.add('btn', 'btn-danger', 'btn-sm', 'ml-2');
        removeButton.onclick = () => {
            selectedFiles.splice(index, 1);
            updateSelectedFilesView();
        };

        listItem.appendChild(removeButton);
        fileList.appendChild(listItem);
    });
}

document.querySelector('form').addEventListener('submit', function (event) {
    const formData = new FormData();

    selectedFiles.forEach(file => {
        formData.append('images', file);
    });

    new FormData(this).forEach((value, key) => {
        formData.append(key, value);
    });

    fetch(this.action, {
        method: this.method,
        body: formData,
    })
        .then(response => {
            if (response.ok) {
                window.location.href = response.url;
            } else {
                alert('Wystąpił błąd podczas zapisywania zmian.');
            }
        })
        .catch(error => console.error('Błąd:', error));

    event.preventDefault();
});
