import React, {useEffect, useState} from 'react';
import Loading from '../Loading/Loading';
import {encodeBase64UnicodeCustom} from '../../Helpers/Path';
import './ImageViewer.css'

export default function ({path, onError}) {
    const [isLoading, setIsLoading] = useState(false);

    const imageUrl = `/api/files/${encodeBase64UnicodeCustom(path)}`;

    useEffect(() => {
        setIsLoading(true);
    }, [imageUrl]);

    return (
        <div className="image-container">
            <img src={imageUrl} className="image-content"
                 onLoad={e => setIsLoading(false)}
                 onError={() => {
                     setIsLoading(false);
                     onError && onError('An error occurred during loading of image');
                 }} alt="preview"/>

            <div className={isLoading ? 'center' : 'd-none'}>
                <Loading/>
            </div>
        </div>
    );
}