import React, { useEffect, useState, useRef } from 'react';
import FSItem from '../FSItem/FSItem'
import { getParent, getName } from '../../Helpers/Path'
import { Link } from 'react-router-dom';
import Loading from '../Loading/Loading';
import FileActionsDropdown from '../FSItem/FileActionsDropdown';
import FolderActionsDropdown from '../FSItem/FolderActionsDropdown';
import FolderSortButton from './FolderSortButton';
import UploadFileButton from './UploadFileButton';
import deleteFileSystemItem from '../../Helpers/deleteFileSystemItem';
import API from '../../Helpers/API';
import useSortByState from '../../Helpers/useSortByState';
import './FolderViewer.css';

const startMaxItemCount = 100;
const maxItemStepSize = 150;
const bottomAppendDistance = 1000;

export default function ({ path, onFolderLoaded }) {
    const [state] = useState({ isUnmounted: false, loadIndex: 0 });
    const [sortBy, setSortBy] = useSortByState('home-page');
    const [content, setContent] = useState(null);
    const [update, setUpdate] = useState(false);
    const [isLoading, setIsLoading] = useState(true);
    const [isOnTop, setIsOnTop] = useState(true);
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

    const renderItem = (item) => {
        let link = null;
        if (item.isFile && item.permission.info) {
            link = `/?folder=${encodeURIComponent(path)}&file=${encodeURIComponent(getName(item.path))}`;
        } else if (!item.isFile && item.permission.list) {
            link = `/?folder=${encodeURIComponent(item.path)}`;
        }

        return (
            <div key={item.path} className="p-2 folder-viewer-item-container">
                <div className={`folder-viewer-file-item-content ${link ? 'folder-viewer-item-container-link' : ''}`}>
                    {link ? (
                        <Link to={link}>
                            <FSItem item={item} />
                        </Link>
                    ) : (
                        <div>
                            <FSItem item={item} />
                        </div>
                    )}
                </div>
                {item.isFile ? (
                    <FileActionsDropdown file={item}
                        onDelete={() => deleteFileSystemItem(item, () => updateContent(true))} />
                ) : (
                    <FolderActionsDropdown folder={item}
                        onDelete={() => deleteFileSystemItem(item, () => updateContent(true))} />
                )}
            </div>
        );
    }

    const renderItems = (content, maxItemsCount) => {
        const items = [];
        for (let i = 0; i < content.folders.length; i++) {
            if (items.length >= maxItemsCount) return { items, all: false };
            items.push(renderItem(content.folders[i]));
        }
        for (let i = 0; i < content.files.length; i++) {
            if (items.length >= maxItemsCount) return { items, all: false };
            items.push(renderItem(content.files[i]));
        }
        return { items, all: true };
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

    const parentPath = getParent(path);
    const parentUrl = `/?folder=${encodeURIComponent(parentPath)}`;
    const pathParts = !isLoading && content && content.path ? renderPathParts(content.path) : [];
    const { items, all } = !isLoading && content ? renderItems(content, maxItemsCount) : {
        items: [],
        all: true
    };

    state.isOnTop = isOnTop;
    state.lastItemsCount = items.length;
    state.maxItemsCount = maxItemsCount;

    return (
        <div>
            <div ref={headContainerRef}
                className={`folder-viewer-head-container ${isOnTop ? '' : 'folder-viewer-head-sticky container'}`}>
                <div onClick={forceUpdate}>
                    <i className="folder-viewer-head-update-icon fa fa-retweet fa-2x" />
                </div>
                <Link to={parentUrl}>
                    <i className={`pl-2 fa fa-arrow-up fa-2x ${parentPath === null ? 'd-none' : ''}`} />
                </Link>
                <div className="path pl-2 folder-viewer-head-path">
                    {pathParts}
                </div>
                <FolderSortButton sortBy={sortBy} onSortByChange={setSortBy} />
                <UploadFileButton path={path} disabled={isLoading || !(content && content.permission.write)} />
            </div>
            <div className="folder-viewer-list" style={{
                paddingTop: `${!isOnTop && headContainerRef.current && headContainerRef.current.offsetHeight || 0}px`
            }}>
                {items}
                {all ? null : (
                    <div className="text-center m-4">
                        <Loading />
                    </div>
                )}
            </div>
            <div className={isLoading || items.length ? 'd-none' : 'text-center'}>
                <h3 className="font-italic">&lt;Empty&gt;</h3>
            </div>
            <div className={`folder-viewer-to-top-container ${isOnTop ? 'd-none' : ''}`}>
                <button className="btn btn-info" onClick={() => {
                    document.body.scrollTop = 0; // For Safari
                    document.documentElement.scrollTop = 0; // For Chrome, Firefox, IE and Opera

                    setMaxItemsCount(startMaxItemCount);
                }}>
                    BACK TO TOP
                </button>
            </div>
            <div className={isLoading ? 'center' : 'center d-none'}>
                <Loading />
            </div>
        </div>
    );
}
