import React, {useEffect, useState} from 'react';
import {getFileType, encodeBase64UnicodeCustom} from '../../Helpers/Path';
import ImageViewer from './ImageViewer';
import TextViewer from './TextViewer';
import MediaViewer from './MediaViewer';
import PdfViewer from './PdfViewer';
import Loading from '../Loading/Loading';
import FileActionsDropdown from '../FSItem/FileActionsDropdown';
import deleteFileSystemItem from '../../Helpers/deleteFileSystemItem';
import API from '../../Helpers/API';
import './FileViewer.css';

export default function ({path, theme, onFileInfoLoaded, hideOpenFileLinkAction, onClose}) {
    const [state] = useState({isUnmounted: false, loadIndex: 0});
    const [error, setError] = useState(null);
    const [contentError, setContentError] = useState(null);
    const [isLoading, setIsLoading] = useState(false);
    const [file, setFile] = useState(null);

    const setViewerError = (viewerError) => {
        if (!state.isUnmounted) setContentError(viewerError);
    }

    const updateFile = async () => {
        const currentIndex = ++state.loadIndex;
        let newFile = null;
        let newError = false;
        if (path) {
            try {
                setIsLoading(true);
                const response = await API.getFileInfo(path);

                if (response.ok) {
                    newFile = await response.json();
                    newFile.isFile = true;
                } else if (response.status === 403) {
                    newError = 'Forbidden to access file';
                } else if (response.status === 404) {
                    newError = 'File not found';
                } else if (response.status === 401) {
                    newError = 'Unauthorized. It seems like your are not logged in';
                } else {
                    newError = 'An error occured';
                }
            } catch (e) {
                console.log(e);
                newError = e.message;
            }
        }

        if (currentIndex === state.loadIndex && !state.isUnmounted) {
            onFileInfoLoaded && onFileInfoLoaded(newFile);
            setError(newError);
            setFile(newFile);
            setIsLoading(false);
        }
    }

    const renderContentError = (text) => {
        return (
            <label className={`file-viewer-content-error ${theme}`}>
                {text}
            </label>
        );
    }

    const renderViewer = () => {
        if (!file.permission.read) {
            return (
                <div className="text-center'">
                    <h3 className={`font-italic file-viewer-info-text ${theme}`}>
                        Not authorized for reading
                    </h3>
                </div>
            );
        }

        switch (getFileType(file.extension)) {
            case 'image':
                return <ImageViewer path={file.path} onError={setViewerError}/>;

            case 'text':
                return file.size <= 500 * 1024 ? (
                    <TextViewer path={file.path} onError={setViewerError}/>
                ) : renderContentError('File to big (> 500kB)');

            case 'audio':
                return <MediaViewer path={file.path} type="audio" onError={setViewerError}/>;

            case 'video':
                return <MediaViewer path={file.path} type="video" onError={setViewerError}/>;

            case 'pdf':
                return file.size <= 100 * 1024 * 1024 ? (
                    <PdfViewer path={file.path} onError={setViewerError}/>
                ) : renderContentError('File to big (> 100MB)');
        }

        return (
            <div className="text-center">
                <h3 className={`font-italic file-viewer-info-text ${theme}`}>
                    No preview for this file available
                </h3>
            </div>
        );
    };

    useEffect(() => {
        return () => {
            state.isUnmounted = true;
        };
    }, []);

    useEffect(() => {
        updateFile();
    }, [path]);

    if (error) {
        return (
            <div className="text-center'">
                <h3 className={`font-italic file-viewer-info-text ${theme}`}>
                    {error}
                </h3>
            </div>
        );
    }

    if (isLoading) {
        return (
            <div className="center">
                <Loading/>
            </div>
        );
    }

    if (!file) {
        return null;
    }

    return (
        <div className="file-viewer-container">
            <div className="file-viewer-header-container">
                <div style={{flexGrow: 1}}/>
                <h4 className={`file-viewer-title ${theme}`}>{file.name}</h4>
                <div style={{flexGrow: 1}}/>
                <div className="m-1">
                    <FileActionsDropdown file={file}
                                         title="Options"
                                         hideOpenFileLink={hideOpenFileLinkAction}
                                         onDelete={() => deleteFileSystemItem(
                                             file,
                                             onClose,
                                         )}/>
                </div>
            </div>
            <div className="file-viewer-content-remaining">
                <div className="file-viewer-content-container">
                    <div className="file-viewer-content">
                        {contentError ? renderContentError(contentError) : renderViewer()}
                    </div>
                </div>
            </div>
        </div>
    );
}