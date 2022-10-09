import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { normalizeFile } from '../Helpers/Path';
import FileViewer from './FileViewer/FileViewer';
import './FilePage.css'

function setDocumentTitle(file) {
    let name = null;
    if (file && file.name) {
        name = file.name;
    }

    document.title = name ? `${name} - File System` : 'File System';
}

export default function () {
    const location = useLocation();
    const query = new URLSearchParams(location.search);
    const path = normalizeFile(query.get('path'));
    const navigate = useNavigate();

    return (
        <div className="file-page-container">
            <FileViewer theme="light" path={path} hideOpenFileLinkAction={true}
                onFileInfoLoaded={setDocumentTitle}
                onClose={() => navigate('/')} />
        </div>
    );
}
