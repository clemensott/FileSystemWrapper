import React, { memo, useEffect, useState } from 'react';
import FolderViewerItems from './FolderViewerItems';
import Loading from '../Loading/Loading';


const FolderViewerContent = ({ content, maxItemsCount, paddingTop, onItemCountChanged, onDeleteItem }) => {
    const [itemGroups, setItemGroups] = useState([]);

    const createItemGroup = (startIndex, maxLength) => {
        const items = [];
        for (let i = startIndex; i < content.folders.length; i++) {
            if (items.length >= maxLength) break;
            items.push(content.folders[i]);
        }
        for (let i = startIndex - content.folders.length + items.length; i < content.files.length; i++) {
            if (items.length >= maxLength) break;
            items.push(content.files[i]);
        }

        return {
            startIndex,
            items,
        };
    };

    const itemsCount = itemGroups.map(group => group.items.length).reduce((sum, length) => sum + length, 0);

    useEffect(() => {
        if (!content) {
            return;
        }
        let currentCount = itemsCount;
        if (currentCount < maxItemsCount) {
            setItemGroups([
                ...itemGroups,
                createItemGroup(currentCount, maxItemsCount - currentCount),
            ]);
        } else if (currentCount > maxItemsCount) {
            setItemGroups(itemGroups.map(group => {
                const { startIndex, items } = group;
                if (startIndex + items.length <= maxItemsCount) {
                    return group;
                }
                if (startIndex >= maxItemsCount) {
                    return null;
                }
                return {
                    startIndex,
                    items: items.filter((_, index) => index + startIndex <= maxItemsCount),
                };
            }).filter(Boolean));
        }
    }, [maxItemsCount]);

    useEffect(() => {
        const groups = [];
        if (content) {
            groups.push(createItemGroup(0, maxItemsCount));
        }
        setItemGroups(groups);
    }, [content]);

    useEffect(() => {
        if (typeof onItemCountChanged === 'function') {
            onItemCountChanged(itemsCount);
        }
    }, [itemsCount]);

    const path = content && content.path.length ? content.path[content.path.length - 1].path : '';
    const totalItemsCount = content ? content.folders.length + content.files.length : 0;

    return (
        <>
            <div className="folder-viewer-list" style={{
                paddingTop,
            }}>
                {
                    itemGroups.map(group => (
                        <FolderViewerItems
                            key={group.startIndex}
                            path={path}
                            items={group.items}
                            onDeleteItem={onDeleteItem}
                        />
                    ))
                }
                {content && totalItemsCount > itemsCount ? (
                    <div className="text-center m-4">
                        <Loading />
                    </div>
                ) : null}
            </div>
            <div className={!content || totalItemsCount ? 'd-none' : 'text-center'}>
                <h3 className="font-italic">&lt;Empty&gt;</h3>
            </div>
        </>
    );
};

export default memo(FolderViewerContent);
