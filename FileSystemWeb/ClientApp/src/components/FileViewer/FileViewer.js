import React from 'react';
import { getName, getExtension, getParent, encodeToURI } from '../../Helpers/Path';
import ImageViewer from './ImageViewer';
import TextViewer from './TextViewer';
import MediaViewer from './MediaViewer';
import './FileViewer.css';
import { Link } from 'react-router-dom';

function getViewer(path, password) {
    const extension = getExtension(path) || '';

    switch (extension.toLowerCase()) {
        case ".apng":
        case ".bmp":
        case ".gif":
        case ".ico":
        case ".cur":
        case ".jpg":
        case ".jpeg":
        case ".jfif":
        case ".pjpeg":
        case ".pjp":
        case ".png":
        case ".svg":
        case ".tif":
        case ".tiff":
        case ".webp":
            return <ImageViewer path={path} password={password} />

        case ".js":
        case ".json":
        case ".xml":
        case ".css":
        case ".htm":
        case ".html":
        case ".txt":
        case ".log":
        case ".ini":
        case ".rtx":
        case ".rtf":
        case ".tsv":
        case ".csv":
            return <TextViewer path={path} password={password} />;

        case ".aac":
        case ".mp3":
        case ".mp2":
        case ".wav":
        case ".oga":
        case ".mpeg":
        case ".mpg":
        case ".mpe":
        case ".mp4":
        case ".ogg":
        case ".ogv":
        case ".qt":
        case ".mov":
        case ".viv":
        case ".vivo":
        case ".webm":
        case ".mkv":
        case ".avi":
        case ".movie":
        case ".3gp":
        case ".3g2":
            return <MediaViewer path={path} password={password} />;
    }

    return <div />
}

export default function (props) {
    const normal = getName(props.path);
    const viewer = getViewer(props.path, props.password);

    const folderPath = getParent(props.path);
    const folderUrl = '/' + encodeToURI(folderPath);

    const className = normal ? 'file-overlay' : 'file-overlay hidden';

    return (
        <div className={className}>
            <div className="file-container">
                <div className="file-content">
                    <div>
                        <h4 className="file-title">{normal}</h4>
                    </div>
                    <div className="file-remaining">
                        {viewer}
                    </div>
                </div>
            </div>
            <Link to={folderUrl} className="file-close">
                <button>Close</button>
            </Link>
        </div>
    );
}