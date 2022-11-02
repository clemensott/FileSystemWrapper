import React, { memo, useEffect, useState, useRef, useCallback } from 'react';
import { getParent } from '../../Helpers/Path';
import { Link } from 'react-router-dom';
import Loading from '../Loading/Loading';
import FolderSortButton from './FolderSortButton';
import UploadFileButton from './UploadFileButton';
import FolderViewerContent from './FolderViewerContent';
import deleteFileSystemItem from '../../Helpers/deleteFileSystemItem';
import API from '../../Helpers/API';
import useSortByState from '../../Helpers/useSortByState';
import sleep from '../../Helpers/sleep';
import './FolderViewer.css';

const startMaxItemCount = 100;
const maxItemStepSize = 150;
const bottomAppendDistance = 1000;

const FolderViewer = ({ path, onFolderLoaded }) => {
    const [state] = useState({ isUnmounted: false, loadIndex: 0 });
    const [sortBy, setSortBy] = useSortByState('home-page');
    const [content, setContent] = useState(null);
    const [update, setUpdate] = useState(false);
    const [isLoading, setIsLoading] = useState(true);
    const [isOnTop, setIsOnTop] = useState(true);
    const [lastItemsCount, setLastItemsCount] = useState(0);
    const [maxItemsCount, setMaxItemsCount] = useState(startMaxItemCount);

    const forceUpdate = () => setUpdate(!update);

    const headContainerRef = useRef();

    const renderPathParts = (parts) => {
        const renderParts = [];
        parts.forEach((part, i) => {
            if (i + 1 === parts.length) {
                renderParts.push(part.name);
            } else {
                const link = `/?folder=${encodeURIComponent(part.path)}`
                renderParts.push(<Link key={part.path} to={link}>{part.name}</Link>);
                renderParts.push('\\');
            }
        });
        return renderParts;
    }

    const onScroll = () => {
        const scrollOffset = document.body.scrollTop || document.documentElement.scrollTop;
        const scrollHeight = document.body.scrollHeight || document.documentElement.scrollHeight;
        const height = document.body.offsetHeight || document.documentElement.offsetHeight;

        if (state.lastItemsCount === state.maxItemsCount &&
            (scrollOffset + height + bottomAppendDistance > scrollHeight)) {
            setMaxItemsCount(state.maxItemsCount + maxItemStepSize);
        }

        const newIsOnTop = scrollOffset < state.headOffsetTop;
        if (state.isOnTop !== newIsOnTop) {
            setIsOnTop(newIsOnTop);
        }
    }

    useEffect(() => {
        window.addEventListener('scroll', onScroll);
        state.headOffsetTop = headContainerRef.current.offsetTop;

        return () => {
            window.removeEventListener('scroll', onScroll);
            state.isUnmounted = true;
        };
    }, []);

    const updateContent = async () => {
        const currentIndex = ++state.loadIndex;
        let content = null;
        try {
            setIsLoading(true);
            const response = await API.getFolderContent(path, sortBy.type, sortBy.direction);
            if (response.ok) {
                content = await response.json();
                content.folders.forEach(f => f.isFile = false);
                content.files.forEach(f => f.isFile = true);
            }
        } catch (e) {
            console.error(e);
        }

        if (currentIndex === state.loadIndex && !state.isUnmounted) {
            setContent(content);
            onFolderLoaded && onFolderLoaded(content);
            setIsLoading(false);
        }
    }

    useEffect(() => {
        updateContent();
    }, [path, sortBy, update]);

    const onDeleteItem = useCallback(item => deleteFileSystemItem(item, () => updateContent(true)), []);

    const onBackToTop = async () => {
        document.body.scrollTop = 0; // For Safari
        document.documentElement.scrollTop = 0; // For Chrome, Firefox, IE and Opera

        while (document.body.scrollTop || document.documentElement.scrollTop) {
            await sleep(100);
        }
        setMaxItemsCount(startMaxItemCount);
    };

    const parentPath = getParent(path);
    const parentUrl = `/?folder=${encodeURIComponent(parentPath || '')}`;
    const pathParts = !isLoading && content && content.path ? renderPathParts(content.path) : [];

    state.isOnTop = isOnTop;
    state.lastItemsCount = lastItemsCount;
    state.maxItemsCount = maxItemsCount;

    return (
        <div>
            <div ref={headContainerRef}
                className={`folder-viewer-head-container ${isOnTop ? '' : 'folder-viewer-head-sticky container'}`}>
                <div onClick={forceUpdate}>
                    <i className="folder-viewer-head-update-icon fa fa-retweet fa-2x" />
                </div>
                <Link to={parentUrl}>
                    <i className={`ps-2 fa fa-arrow-up fa-2x ${parentPath === null ? 'd-none' : ''}`} />
                </Link>
                <div className="path ps-2 folder-viewer-head-path">
                    {pathParts}
                </div>
                <FolderSortButton sortBy={sortBy} onSortByChange={setSortBy} />
                <UploadFileButton
                    path={path}
                    disabled={isLoading || !(content && content.permission.write)}
                    onUploaded={forceUpdate}
                />
            </div>
            <FolderViewerContent
                content={isLoading ? null : content}
                maxItemsCount={maxItemsCount}
                paddingTop={`${!isOnTop && headContainerRef.current && headContainerRef.current.offsetHeight || 0}px`}
                onItemCountChanged={setLastItemsCount}
                onDeleteItem={onDeleteItem}
            />
            <div className={`folder-viewer-to-top-container ${isOnTop ? 'd-none' : ''}`}>
                <button className="btn btn-info" onClick={onBackToTop}>
                    BACK TO TOP
                </button>
            </div>
            <div className={isLoading ? 'center' : 'center d-none'}>
                <Loading />
            </div>
        </div>
    );
}

export default memo(FolderViewer);
