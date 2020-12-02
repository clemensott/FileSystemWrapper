import React from 'react';
import {normalizeFolder, normalizeFile} from '../Helpers/Path';
import {FolderViewer} from './FolderViewer';
import {FileViewerOverlay} from './FileViewerOverlay';
import {generateQueryUrl} from '../Helpers/generateNavigationUrl';

function setDocumentTitle(folderContent, file) {
    let name = null;
    if (file && file.name) {
        name = file.name;
    } else if (folderContent && folderContent.path) {
        name = folderContent.path.length ? folderContent.path[folderContent.path.length - 1].name : 'Root';
    }

    document.title = name ? `${name} - File System` : 'File System';
}

export default function () {
    const query = new URLSearchParams(window.location.search);
    const folder = query.get('folder');
    const file = query.get('file');
    const folderNorm = folder && normalizeFolder(folder) || '';
    const fileNameDecoded = file && normalizeFile(file);
    const fileNorm = fileNameDecoded && (folderNorm + fileNameDecoded);
    const closeFileOverlayUrl = generateQueryUrl({file: null});

    let folderContent;
    let fileInfo;

    return (
        <div>
            <FolderViewer path={folderNorm} onFolderLoaded={content => {
                folderContent = content;
                setDocumentTitle(folderContent, file);
            }}/>
            {fileNorm && (
                <FileViewerOverlay path={fileNorm} closeUrl={closeFileOverlayUrl}
                                   onFileInfoLoaded={file => {
                                       fileInfo = file;
                                       setDocumentTitle(folderContent, file);
                                   }}/>
            )}
        </div>
    );
}
