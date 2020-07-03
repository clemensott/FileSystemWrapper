import React from 'react';
import { getName, getParent, encodeToURI, getFileType } from '../../Helpers/Path';
import ImageViewer from './ImageViewer';
import TextViewer from './TextViewer';
import MediaViewer from './MediaViewer';
import './FileViewer.css';
import { Link, useHistory } from 'react-router-dom';
import PdfViewer from './PdfViewer';

function getViewer(path, password) {
    switch (getFileType(path)) {
        case 'image':
            return <ImageViewer path={path} password={password} />

        case 'text':
            return <TextViewer path={path} password={password} />;

        case 'audio':
            return <MediaViewer path={path} password={password} type="audio" />;

        case 'video':
            return <MediaViewer path={path} password={password} type="video" />;

        case 'pdf':
            return <PdfViewer path={path} password={password} />;
    }

    return '<No viewer>';
}

export default function (props) {
    const name = getName(props.path);
    const viewer = getViewer(props.path, props.password);

    const folderPath = getParent(props.path);
    const folderUrl = '/' + encodeToURI(folderPath);
    const history = useHistory();

    return (
        <div className={`file-overlay ${name ? '' : 'd-none'}`} onClick={e => {
            if (e.target.classList.contains('file-overlay') ||
                e.target.classList.contains('file-overlay-container') ||
                e.target.classList.contains('file-content')) {
                history.push(folderUrl);
            }
        }}>
            <div className="file-overlay-container">
                <div className="file-overlay-content">
                    <div>
                        <h4 className="file-title">{name}</h4>
                    </div>
                    <div className="file-overlay-content-remaining">
                        <div className="file-content-container">
                            <div className="file-content">
                                {viewer}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <Link to={folderUrl}>
                <i className="fas fa-times fa-3x file-close" />
            </Link>
        </div>
    );
}