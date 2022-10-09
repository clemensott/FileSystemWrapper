import React from 'react';
import { getFileType } from '../../Helpers/Path';
import './FSItem.css';

function getImagePath(item) {
    if (!item.isFile) return 'fas fa-folder fs-item-folder';

    switch (getFileType(item.extension)) {
        case 'image':
            return 'fas fa-file-image fs-item-image';
        case 'audio':
            return 'fas fa-file-audio fs-item-audio';
        case 'video':
            return 'fas fa-file-video fs-item-video';
        case 'text':
            return 'fas fa-file-alt fs-item-text';
        case 'pdf':
            return 'fas fa-file-pdf fs-item-pdf';
        default:
            return 'fas fa-file fs-item-file';
    }
}

export default function (props) {
    const icon = getImagePath(props.item);

    return (
        <div className="fs-item-container">
            <i className={`${icon} fa-2x`} />
            <div className="ps-2">
                {props.item.name}
            </div>
        </div>
    );
}