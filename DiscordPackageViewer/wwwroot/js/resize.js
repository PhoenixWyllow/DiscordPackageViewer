// Sidebar resize drag helper.
// Captures pointermove/pointerup on the document so the drag works even when
// the cursor leaves the narrow resize handle.
window.sidebarResize = {
    start(sidebarEl, dotnetRef, startX, startWidth, minWidth, maxWidth) {
        const onMove = (e) => {
            const delta = e.clientX - startX;
            const newWidth = Math.min(maxWidth, Math.max(minWidth, startWidth + delta));
            sidebarEl.style.width = newWidth + "px";
        };

        const onUp = (e) => {
            document.removeEventListener("pointermove", onMove);
            document.removeEventListener("pointerup", onUp);
            const finalWidth = parseInt(sidebarEl.style.width, 10) || startWidth;
            dotnetRef.invokeMethodAsync("OnResizeEnd", finalWidth);
        };

        document.addEventListener("pointermove", onMove);
        document.addEventListener("pointerup", onUp);
    }
};
