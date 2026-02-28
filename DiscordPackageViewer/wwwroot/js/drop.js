// Minimal JS interop for file drag-and-drop.
// Blazor's DragEventArgs cannot access DataTransfer.files,
// so we bridge dropped files to a hidden <InputFile> component.
window.initDropZone = (dropZoneId, hiddenInputId) => {
    const zone = document.getElementById(dropZoneId);
    if (!zone) return;

    // Prevent defaults on dragover to allow drop
    zone.addEventListener('dragover', (e) => {
        e.preventDefault();
        e.stopPropagation();
    });

    zone.addEventListener('drop', (e) => {
        e.preventDefault();
        e.stopPropagation();

        const files = e.dataTransfer?.files;
        if (!files || files.length === 0) return;

        const input = document.getElementById(hiddenInputId);
        if (!input) return;

        // Forward the dropped file to the hidden InputFile element
        const dt = new DataTransfer();
        dt.items.add(files[0]); // only take the first file
        input.files = dt.files;
        input.dispatchEvent(new Event('change', { bubbles: true }));
    });
};
