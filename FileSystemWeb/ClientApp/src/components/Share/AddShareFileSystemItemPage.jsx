import React, {useEffect} from 'react';
import { useLocation } from 'react-router-dom';
import {normalizeFile, normalizeFolder} from '../../Helpers/Path';
import ShareFileSystemItemControl from './ShareFileSystemItemControl';
import {setDocumentTitle} from '../../Helpers/setDocumentTitle';

export default function () {
    const location = useLocation();
    const isFile = location.pathname.startsWith('/share/file/add');
    const query = new URLSearchParams(location.search);
    const path = isFile ? normalizeFile(query.get('path')) : normalizeFolder(query.get('path'));

    useEffect(() => {
        setDocumentTitle(null);
    }, [path, isFile]);

    return (
        <ShareFileSystemItemControl path={path} isFile={isFile}
                                    onItemInfoLoaded={item => setDocumentTitle(item && item.name)}/>
    );
}