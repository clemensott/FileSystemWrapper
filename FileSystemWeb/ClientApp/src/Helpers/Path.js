
// replaces / and | with \ and removes last \
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

export function encodeToURI(path) {
    return path && normalizeFile(path).split('\\').join('|');
}

export function getExtension(path) {
    if (!path) return null;

    const normal = normalizeFile(path);
    const index = normal.lastIndexOf('.');
    return index >= 0 ? normal.substr(index) : '';
}


export default {
    normalizeFile,
    normalizeFolder,
    getName,
    getParent,
    encodeToURI,
}