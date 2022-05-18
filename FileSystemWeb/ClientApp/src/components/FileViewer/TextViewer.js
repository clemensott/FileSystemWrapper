import React, {useEffect, useState} from 'react';
import Loading from '../Loading/Loading';
import API from '../../Helpers/API';
import './TextViewer.css'

export default function ({path, onError}) {
    const [state] = useState({isUnmounted: false, loadIndex: 0});
    const [isLoading, setIsLoading] = useState(false);
    const [text, setText] = useState(null);

    const updateText = async () => {
        const currentIndex = ++state.loadIndex;
        let newText = null;
        let error = null;
        try {
            setIsLoading(true);
            setText(null);

            const response = await API.getFile(path);

            if (response.ok) {
                newText = await response.text();
                if (newText === '') error = 'File is empty';
            } else {
                error = (await response.text()) || response.statusText || `HTTP: ${response.status}`;
            }
        } catch (err) {
            console.error(err);
            error = err.message;
        }

        if (currentIndex === state.loadIndex && !state.isUnmounted) {
            error && onError && onError(error);
            setText(newText);
            setIsLoading(false);
        }
    }

    useEffect(() => {
        return () => {
            state.isUnmounted = true;
        };
    }, []);

    useEffect(() => {
        updateText();
    }, [path]);

    if (isLoading) {
        return (
            <div className="center">
                <Loading/>
            </div>
        );
    }
    if (text) {
        return (
            <textarea className="text-viewer-text" readOnly defaultValue={text}/>
        );
    }
    return (
        <label className="text-viewer-no-text">
            &lt;No Text&gt;
        </label>
    );
}