function submitDeleteForm() {
    if (confirm('Czy na pewno chcesz usunąć tę relację?')) {
        document.getElementById('deleteForm').submit();
    }
}