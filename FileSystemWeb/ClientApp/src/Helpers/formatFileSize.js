export default function formatFileSize(size) {
    const endings = ['B', 'kB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
    let ending = '';

    for (let i = 0; i < endings.length; i++) {
        ending = endings[i];
        if (size < 1024) break;

        size /= 1024;
    }

    return `${parseFloat(size.toPrecision(3))} ${ending}`;
}