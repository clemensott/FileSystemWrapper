import React from 'react';
import {useParams, Redirect} from 'react-router-dom';
import {normalizeFolder, normalizeFile, getName, DecodePath} from '../Helpers/Path';
import {FolderViewer} from './FolderViewer';
import FileViewer from './FileViewer/FileViewer';
import {getCookieValue} from "../Helpers/cookies";

export default function () {
    const isLoggedIn = !!getCookieValue('fs_login');
    if (!isLoggedIn) {
        return <Redirect to="/login"/>
    }

    const {folder, file} = useParams();
    const folderNorm = normalizeFolder(folder && DecodePath(folder)) || '';
    const fileNameDecoded = normalizeFile(file && DecodePath(file));
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
