import React, {useEffect, useState} from 'react';
import Loading from '../Loading/Loading';
import API from '../../Helpers/API';
import './PdfViewer.css'

export default function ({path, onError}) {
    const [state] = useState({isUnmounted: false, loadIndex: 0});
    const [isLoading, setIsLoading] = useState(false);
    const [localUrl, setLocalUrl] = useState(null);

    const clearLocalUrl = () => {
        if (localUrl) URL.revokeObjectURL(localUrl);
        if (!state.isUnmounted) setLocalUrl(localUrl);
    }

    const updatePdf = async () => {
        const currentIndex = ++state.loadIndex;
        let blob = null;
        let error = null;
        try {
            setIsLoading(true);
            clearLocalUrl();

            const response = await API.getFile(path);
            if (response.ok) {
                blob = await response.blob();
            } else {
                error = (await response.text()) || response.statusText || `HTTP: ${response.status}`;
            }
        } catch (err) {
            console.error(err);
            error = err.message;
        }

        if (currentIndex === state.loadIndex && !state.isUnmounted) {
            error && onError && onError(error);
            setLocalUrl(blob ? URL.createObjectURL(blob) : null);
            setIsLoading(false);
        }
    }

    useEffect(() => {
        return () => {
            state.isUnmounted = true;
            clearLocalUrl();
        };
    }, []);

    useEffect(() => {
        updatePdf();
    }, [path]);

    if (isLoading) {
        return (
            <div className="center">
                <Loading/>
            </div>
        );
    }
    if (localUrl) {
        return (
            <embed src={localUrl} type="application/pdf" className="pdf-viewer-pdf"/>
        );
    }
    return (
        <label className="pdf-viewer-no-pdf">
            &lt;No PDF&gt;
        </label>
    );
}