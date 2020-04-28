import React from 'react';
import { useParams, Redirect } from 'react-router-dom';
import { normalizeFolder, normalizeFile } from '../Helpers/Path';
import { FolderViewer } from './FolderViewer';
import FileViewer from './FileViewer/FileViewer';

export default function (props) {
    const password = localStorage.getItem('password');

    if (!password) {
        return <Redirect to="/login" />
    }

    const { folder, file } = useParams();
    const folderNorm = normalizeFolder(folder) || '';
    const fileNorm = normalizeFile(file);

    return (
        <div>
            <FolderViewer password={password} path={folderNorm} />
            <FileViewer password={password} path={fileNorm} />
        </div>
    );
}
