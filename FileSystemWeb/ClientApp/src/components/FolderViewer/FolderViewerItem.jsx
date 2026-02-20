import React, { memo } from 'react';
import { Link } from 'react-router-dom';
import FSItem from '../FSItem/FSItem';
import FileActionsDropdown from '../FSItem/FileActionsDropdown';
import FolderActionsDropdown from '../FSItem/FolderActionsDropdown';
import { getName } from '../../Helpers/Path';

const FolderViewerItem = ({ path, item, onDeleteItem }) => {
    let link = null;
    if (item.isFile && item.permission.info) {
        link = `/?folder=${encodeURIComponent(path)}&file=${encodeURIComponent(getName(item.path))}`;
    } else if (!item.isFile && item.permission.list) {
        link = `/?folder=${encodeURIComponent(item.path)}`;
    }

    return (
        <div className="p-2 folder-viewer-item-container">
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
                <FileActionsDropdown file={item} onDelete={onDeleteItem} />
            ) : (
                <FolderActionsDropdown folder={item} onDelete={onDeleteItem} />
            )}
        </div>
    );
};

export default memo(FolderViewerItem);
