import React, {useRef} from 'react';
import {Link} from 'react-router-dom';
import {FileViewer} from './FileViewer/FileViewer';
import './FileViewerOverlay.css';

export default function ({closeUrl, previousItem, nextItem, ...rest}) {
    const closeRef = useRef();

    console.log('overlay:', previousItem, nextItem)
    return (
        <div className="file-viewer-overlay" onClick={e => {
            if (e.target.classList.contains('file-viewer-overlay') ||
                e.target.classList.contains('file-viewer-overlay-container') ||
                e.target.classList.contains('file-viewer-content') ||
                e.target.classList.contains('image-container')) {
                closeRef.current.click();
            }
        }}>
            <div className="file-viewer-overlay-container">
                <FileViewer theme="dark" onClose={() => closeRef.current.click()} {...rest} />
            </div>
            <Link to={closeUrl} innerRef={closeRef}>
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