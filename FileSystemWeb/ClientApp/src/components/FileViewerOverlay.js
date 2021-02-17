import React from 'react';
import {useHistory} from 'react-router-dom';
import {Link} from 'react-router-dom';
import FileViewer from './FileViewer/FileViewer';
import './FileViewerOverlay.css';

export default function ({closeUrl, previousItem, nextItem, ...rest}) {
    const history = useHistory();

    return (
        <div className="file-viewer-overlay" onClick={e => {
            if (e.target.classList.contains('file-viewer-overlay') ||
                e.target.classList.contains('file-viewer-overlay-container') ||
                e.target.classList.contains('file-viewer-content') ||
                e.target.classList.contains('image-container')) {
                history.push(closeUrl);
            }
        }}>
            <div className="file-viewer-overlay-container">
                <FileViewer theme="dark" onClose={() => history.push(nextItem ? nextItem.url : closeUrl)} {...rest} />
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