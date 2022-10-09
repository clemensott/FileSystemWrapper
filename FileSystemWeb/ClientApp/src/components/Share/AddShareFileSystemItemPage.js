import React, {useEffect} from 'react';
import { useLocation } from 'react-router-dom';
import {normalizeFile, normalizeFolder} from '../../Helpers/Path';
import ShareFileSystemItemControl from "./ShareFileSystemItemControl";

function setDocumentTitle(item) {
    let name = null;
    if (item && item.name) {
        name = item.name;
    }

    document.title = name ? `${name} - Add Share - File System` : 'Add Share - File System';
}

export default function () {
    const location = useLocation();
    const isFile = location.pathname.startsWith('/share/file/add');
    const query = new URLSearchParams(location.search);
    const path = isFile ? normalizeFile(query.get('path')) : normalizeFolder(query.get('path'));

    useEffect(() => {
        setDocumentTitle(null);
    }, [path, isFile]);

    return (
        <ShareFileSystemItemControl path={path} isFile={isFile}
                                    onItemInfoLoaded={setDocumentTitle}/>
    );
}