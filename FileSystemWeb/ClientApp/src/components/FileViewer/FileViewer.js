import React from 'react';
import {getName, getParent, getFileType, encodeBase64Custom} from '../../Helpers/Path';
import ImageViewer from './ImageViewer';
import TextViewer from './TextViewer';
import MediaViewer from './MediaViewer';
import { Link, useHistory } from 'react-router-dom';
import PdfViewer from './PdfViewer';
import {formatUrl} from "../../Helpers/Fetch";
import './FileViewer.css';

function getViewer(path) {
    switch (getFileType(path)) {
        case 'image':
            return <ImageViewer path={path} />

        case 'text':
            return <TextViewer path={path} />;

        case 'audio':
            return <MediaViewer path={path} type="audio" />;

        case 'video':
            return <MediaViewer path={path} type="video" />;

        case 'pdf':
            return <PdfViewer path={path} />;
    }

    return (
        <div className="text-center'">
            <h3 className="font-italic"
                style={{color: 'lightgray'}}>
                &lt;MINE type is not supported&gt;
            </h3>
        </div>
    );
}

export default function (props) {
    const name = getName(props.path);
    const viewer = getViewer(props.path);

    const folderPath = getParent(props.path);
    const folderUrl = '/' + encodeBase64Custom(folderPath);
    const history = useHistory();

    const isSupportedFile = ['image', 'audio', 'video', 'text', 'pdf'].includes(getFileType(props.path));
    const fileOpenLink = formatUrl({
        resource: '/api/files',
        path: props.path,
    });
    const fileDownloadLink = formatUrl({
        resource: '/api/files/download',
        path: props.path,
    });

    return (
        <div className={`file-overlay ${name ? '' : 'd-none'}`} onClick={e => {
            if (e.target.classList.contains('file-overlay') ||
                e.target.classList.contains('file-overlay-container') ||
                e.target.classList.contains('file-content') ||
                e.target.classList.contains('image-container')) {
                history.push(folderUrl);
            }
        }}>
            <div className="file-overlay-container">
                <div className="file-overlay-content">
                    <div className="file-overlay-header-container">
                        <h4 className="file-title">{name}</h4>
                        <a href={fileOpenLink} target="_blank"
                           className={`text-info ${isSupportedFile ? '' : 'd-none'}`}>
                            <i className="m-2 fas fa-external-link-square-alt fa-2x"/>
                        </a>
                        <a href={fileDownloadLink} download={props.name} className="text-light">
                            <i className="m-2 fas fa-download fa-2x"/>
                        </a>
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