import React, {useEffect, useState} from 'react';
import {encodeBase64UnicodeCustom} from '../../Helpers/Path';
import './MediaViewer.css'

export default function ({path, type, onError}) {
    const [internalType, setInternalType] = useState(null);
    const mediaUrl = `/api/files/${encodeBase64UnicodeCustom(path)}`;

    const onLoaded = (target) => {
        setInternalType(!!target.videoWidth || !!target.videoHeight ? 'video' : 'audio');
    }

    useEffect(() => {
        setInternalType(null);
    }, [path, type])

    if ((internalType || type) === 'audio') {
        return (
            <audio
                className="media-viewer-content"
                src={mediaUrl}
                autoPlay={true}
                controls
                onCanPlay={e => onLoaded(e.target)}
                onError={e => e.target.error && onError && onError(e.target.error.message)}
            />
        );
    }

    return (
        <video
            className="media-viewer-content"
            src={mediaUrl}
            autoPlay={true}
            controls
            onCanPlay={e => onLoaded(e.target)}
            onError={e => e.target.error && onError && onError(e.target.error.message)}
        />
    );
}