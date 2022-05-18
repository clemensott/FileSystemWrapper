import React, {useEffect, useState} from 'react';
import formatUrl from '../../Helpers/formatUrl';
import './MediaViewer.css'

export default function ({path, type, onError}) {
    const [internalType, setInternalType] = useState(null);
    const mediaUrl = formatUrl({ resource: '/api/files', path });

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