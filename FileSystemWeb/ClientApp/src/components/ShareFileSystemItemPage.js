import React from 'react';
import {useParams} from "react-router-dom";
import {normalizeFile} from "../Helpers/Path";
import {ShareFileSystemItemControl} from "./ShareFileSystemItemControl";

function setDocumentTitle(file) {
    let name = null;
    if (file && file.name) {
        name = file.name;
    }

    document.title = name ? `${name} - File System` : 'File System';
}

export default function () {
    const {path} = useParams();
    const pathDecoded = path && normalizeFile(decodeURIComponent( path));
    const isFile = window.location.pathname.startsWith('/share/file/');

    return (
        <ShareFileSystemItemControl path={pathDecoded} isFile={isFile}
                                    onItemInfoLoaded={setDocumentTitle}/>
    );
}