import React from 'react';
import {useParams} from 'react-router-dom';
import {normalizeFile, decodeBase64Custom} from '../Helpers/Path';
import {FileViewer} from './FileViewer/FileViewer';
import './FilePage.css'

function setDocumentTitle(file) {
    let name = null;
    if (file && file.name) {
        name = file.name;
    }

    document.title = name ? `${name} - File System` : 'File System';
}

export default function () {
    const {path} = useParams();
    const pathDecoded = path && normalizeFile(decodeURIComponent(path));

    return (
        <div className="file-page-container">
            <FileViewer theme="light" path={pathDecoded} hideOpenFileLinkAction={true}
                        onFileInfoLoaded={setDocumentTitle}/>
        </div>
    );
}
