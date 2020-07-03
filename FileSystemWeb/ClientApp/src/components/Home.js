import React from 'react';
import { useParams, Redirect } from 'react-router-dom';
import { normalizeFolder, normalizeFile, getName } from '../Helpers/Path';
import { FolderViewer } from './FolderViewer';
import FileViewer from './FileViewer/FileViewer';

export default function (props) {
    const password = localStorage.getItem('password');

    if (!password) {
        return <Redirect to="/login" />
    }

    const { folder, file } = useParams();
    const folderNorm = normalizeFolder(folder && decodeURIComponent(folder)) || '';
    const fileNorm = normalizeFile(file && decodeURIComponent(file));

    let name;
    if (fileNorm) {
        document.body.style.overflow = 'hidden';

        name = getName(fileNorm);
    } else {
        document.body.style.overflow = '';

        name = getName(folderNorm);
        if (!name) name = 'Root';
        else if (name === normalizeFile(folderNorm)) name = folderNorm;
    }
    document.title = `${name} - File System`;

    return (
        <div>
            <FolderViewer password={password} path={folderNorm} />
            <FileViewer password={password} path={fileNorm} />
        </div>
    );
}
