import React, {useCallback, useEffect} from 'react';
import {useNavigate} from 'react-router-dom';
import {Link} from 'react-router-dom';
import FileViewer from './FileViewer/FileViewer';
import './FileViewerOverlay.css';

export default function ({closeUrl, previousItem, nextItem, ...rest}) {
    const navigate = useNavigate();

    const onKeypressCallback = useCallback((e) => {
        if (['video', 'audio'].includes(e.target.tagName.toLowerCase())) {
            return;
        }

        switch (e.keyCode) {
            case 37: // left arrow
                if (previousItem) {
                    navigate(previousItem.url);
                }
                break;

            case 39: // right arrow
                if (nextItem) {
                    navigate(nextItem.url);
                }
                break;

            case 27: // esc arrow
                navigate(closeUrl)
                break;
        }
    }, [nextItem, previousItem, closeUrl]);

    useEffect(() => {
        document.addEventListener('keydown', onKeypressCallback);
        return () => {
            document.removeEventListener('keydown', onKeypressCallback);
        };
    }, [onKeypressCallback]);

    return (
        <div className="file-viewer-overlay" onClick={e => {
            if (e.target.classList.contains('file-viewer-overlay') ||
                e.target.classList.contains('file-viewer-overlay-container') ||
                e.target.classList.contains('file-viewer-content') ||
                e.target.classList.contains('image-container')) {
                navigate(closeUrl);
            }
        }}>
            <div className="file-viewer-overlay-container">
                <FileViewer theme="dark" onClose={() => navigate(nextItem ? nextItem.url : closeUrl)} {...rest} />
            </div>
            <Link to={closeUrl}>
                <i className="fas fa-times-circle fa-3x file-viewer-overlay-close"/>
            </Link>
            {previousItem && (
                <Link to={previousItem.url}>
                    <i className="fas fa-chevron-circle-left fa-3x file-viewer-overlay-previous"
                       title={previousItem.title}/>
                </Link>
            )}
            {nextItem && (
                <Link to={nextItem.url}>
                    <i className="fas fa-chevron-circle-right fa-3x file-viewer-overlay-next"
                       title={nextItem.title}/>
                </Link>
            )}
        </div>
    );
}