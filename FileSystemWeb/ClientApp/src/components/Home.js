import React from 'react';
import {useParams, Redirect} from 'react-router-dom';
import {normalizeFolder, normalizeFile, getName, decodeBase64Custom} from '../Helpers/Path';
import {FolderViewer} from './FolderViewer';
import FileViewer from './FileViewer/FileViewer';
import {getCookieValue} from "../Helpers/cookies";

export default function () {
    const isLoggedIn = !!getCookieValue('fs_login');
    if (!isLoggedIn) {
        return <Redirect to="/login"/>
    }

    const {folder, file} = useParams();
    const folderNorm = normalizeFolder(folder && decodeBase64Custom(folder)) || '';
    const fileNameDecoded = normalizeFile(file && decodeBase64Custom(file));
    const fileNorm = folderNorm && fileNameDecoded && (folderNorm + fileNameDecoded);

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
            <FolderViewer path={folderNorm}/>
            <FileViewer path={fileNorm}/>
        </div>
    );
}
