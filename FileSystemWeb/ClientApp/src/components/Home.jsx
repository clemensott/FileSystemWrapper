import React, { useEffect, useState } from 'react';
import { normalizeFolder, normalizeFile } from '../Helpers/Path';
import FolderViewer from './FolderViewer/FolderViewer';
import FileViewerOverlay from './FileViewerOverlay';
import { generateQueryUrl } from '../Helpers/generateNavigationUrl';
import { useLocation } from 'react-router-dom';
import {setDocumentTitle} from "../Helpers/setDocumentTitle";

function setHomeDocumentTitle(folderContent, fileInfo) {
    let name = null;
    if (fileInfo && fileInfo.name) {
        name = fileInfo.name;
    } else if (folderContent && folderContent.path) {
        name = folderContent.path.length ? folderContent.path[folderContent.path.length - 1].name : 'Root';
    }

    setDocumentTitle(name);
}

export default function () {
    const location = useLocation();
    const query = new URLSearchParams(location.search);
    const folder = query.get('folder');
    const file = query.get('file');
    const folderNorm = folder && normalizeFolder(folder) || '';
    const fileNameDecoded = file && normalizeFile(file);
    const fileNorm = fileNameDecoded && (folderNorm + fileNameDecoded);
    const closeFileOverlayUrl = generateQueryUrl({ file: null });

    const [folderContent, setFolderContent] = useState(null);
    const [fileInfo, setFileInfo] = useState(null);

    let previousFile = null;
    let nextFile = null;
    if (fileNameDecoded && folderContent && folderContent.files && folderContent.files.length) {
        const currentFileIndex = folderContent.files.findIndex(file => file.name === fileNameDecoded);
        if (currentFileIndex !== -1) {
            const size = folderContent.files.length;
            previousFile = folderContent.files[(currentFileIndex - 1 + size) % size];
            nextFile = folderContent.files[(currentFileIndex + 1) % size];
        }
    }

    useEffect(() => {
        const expectedFolderPath = folderNorm || null;
        const actualFolderPath = folderContent && folderContent.path && folderContent.path.length &&
            normalizeFolder(folderContent.path[folderContent.path.length - 1].path) || null;
        const expectedFilePath = fileNorm || null;
        const actualFilePath = fileInfo && fileInfo.path || null;
        setHomeDocumentTitle(
            expectedFolderPath === actualFolderPath ? folderContent : null,
            expectedFilePath === actualFilePath ? fileInfo : null);
    }, [folderNorm, fileNorm, folderContent, fileInfo])

    return (
        <div>
            <FolderViewer path={folderNorm} onFolderLoaded={setFolderContent} />
            {fileNorm && (
                <FileViewerOverlay path={fileNorm} closeUrl={closeFileOverlayUrl}
                    previousItem={previousFile && ({
                        url: generateQueryUrl({ file: previousFile.name }),
                        title: previousFile.name,
                    })}
                    nextItem={nextFile && ({
                        url: generateQueryUrl({ file: nextFile.name }),
                        title: nextFile.name,
                    })}
                    onFileInfoLoaded={setFileInfo} />
            )}
        </div>
    );
}
