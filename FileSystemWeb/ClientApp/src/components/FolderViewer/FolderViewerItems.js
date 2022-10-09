import React, { memo } from 'react';
import FolderViewerItem from './FolderViewerItem';

const FolderViewerItems = ({ path, items, onDeleteItem }) => {
    return Array.isArray(items) ? items.map(item => (
        <FolderViewerItem key={item.path} path={path} item={item} onDeleteItem={onDeleteItem} />
    )) : null;
};

export default memo(FolderViewerItems);
