import React from 'react';
import {normalizeFile, normalizeFolder} from '../../Helpers/Path';
import ShareFileSystemItemControl from "./ShareFileSystemItemControl";

function setDocumentTitle(file) {
    let name = null;
    if (file && file.name) {
        name = file.name;
    }

    document.title = name ? `${name} - File System` : 'File System';
}

export default function () {
    const isFile = window.location.pathname.startsWith('/share/file/add');
    const query = new URLSearchParams(window.location.search);
    const path = isFile ? normalizeFile(query.get('path')) : normalizeFolder(query.get('path'));

    return (
        <ShareFileSystemItemControl path={path} isFile={isFile}
                                    onItemInfoLoaded={setDocumentTitle}/>
    );
}