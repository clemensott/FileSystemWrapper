import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { normalizeFile } from '../Helpers/Path';
import FileViewer from './FileViewer/FileViewer';
import { setDocumentTitle } from '../Helpers/setDocumentTitle';
import './FilePage.css'

export default function () {
    const location = useLocation();
    const query = new URLSearchParams(location.search);
    const path = normalizeFile(query.get('path'));
    const navigate = useNavigate();

    return (
        <div className="file-page-container">
            <FileViewer theme="light" path={path} hideOpenFileLinkAction={true}
                onFileInfoLoaded={file => setDocumentTitle(file && file.name)}
                onClose={() => navigate('/')} />
        </div>
    );
}
