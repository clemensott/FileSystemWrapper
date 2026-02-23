export function setDocumentTitle(prefix = null) {
    document.title = prefix ? `${prefix} - File System` : 'File System';
}