﻿// replaces / and | with \ and removes last \
export function normalizeFile(path) {
    return path && path.split(/[\/\\|]/).filter(Boolean).join('\\');
}

// replaces / and | with \ and adds last \
export function normalizeFolder(path) {
    return path && (normalizeFile(path) + '\\');
}

export function getName(path) {
    const normal = normalizeFile(path);
    return normal && normal.substr(normal.lastIndexOf('\\') + 1);
}

export function getParent(path) {
    if (!path) return null;

    const normal = normalizeFile(path);
    const index = normal.lastIndexOf('\\');
    return index >= 0 ? normal.substr(0, index) : '';
}

export function encodeBase64Custom(path) {
    return path && window.btoa(normalizeFile(path))
        .replace(/\//g, '_')   // replaces all '/' with '_'
        .replace(/=+$/g, '');  // trims all '=' at the end
}

export function decodeBase64Custom(raw) {
    return window.atob(raw.replace(/_/g, '/'));
}

function toUnicode(char) {
    const code = char.charCodeAt(0);
    return String.fromCharCode(code % 256) + String.fromCharCode(code / 256);
}

export function encodeBase64UnicodeCustom(path) {
    const unicodeString = path && normalizeFile(path).split('')
        .reduce((sum, cur) => sum + toUnicode(cur), '');
    console.log('unicode:', unicodeString, window.btoa(unicodeString));
    return unicodeString &&
        window.btoa(unicodeString)
            .replace(/\//g, '_')   // replaces all '/' with '_'
            .replace(/=/g, '!');   // replaces all '=' with '!'
}

export function getExtension(path) {
    if (!path) return null;

    const normal = normalizeFile(path);
    const index = normal.lastIndexOf('.');
    return index >= 0 ? normal.substr(index) : '';
}

export function getFileType(path) {
    const extension = getExtension(path) || '';
    switch (extension.toLowerCase()) {
        case '.apng':
        case '.bmp':
        case '.gif':
        case '.ico':
        case '.cur':
        case '.jpg':
        case '.jpeg':
        case '.jfif':
        case '.pjpeg':
        case '.pjp':
        case '.png':
        case '.svg':
        case '.tif':
        case '.tiff':
        case '.webp':
            return 'image';

        case '.js':
        case '.json':
        case '.xml':
        case '.css':
        case '.htm':
        case '.html':
        case '.txt':
        case '.log':
        case '.ini':
        case '.rtx':
        case '.rtf':
        case '.tsv':
        case '.csv':
            return 'text';

        case '.aac':
        case '.mp3':
        case '.mp2':
            return 'audio';

        case '.wav':
        case '.oga':
        case '.mpeg':
        case '.mpg':
        case '.mpe':
        case '.mp4':
        case '.ogg':
        case '.ogv':
        case '.qt':
        case '.mov':
        case '.viv':
        case '.vivo':
        case '.webm':
        case '.mkv':
        case '.avi':
        case '.movie':
        case '.3gp':
        case '.3g2':
            return 'video';

        case '.pdf':
            return 'pdf';
    }

    return null;
}
