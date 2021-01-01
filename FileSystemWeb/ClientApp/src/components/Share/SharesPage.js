import React, {useState, useEffect} from 'react';
import {Button, CustomInput, Input, Table} from 'reactstrap';
import './SharesPage.css'
import deleteShareItem from "../../Helpers/deleteShareItem";
import {Link} from "react-router-dom";
import {encodeBase64UnicodeCustom} from "../../Helpers/Path";

async function load(url) {
    try {
        const response = await fetch(url, {
            credentials: 'include',
        });

        if (response.ok) {
            return await response.json();
        }
    } catch (e) {
        console.log(e);
    }

    return null;
}

async function loadShareItems(isFile) {
    try {
        const url = isFile ? '/api/share/files' : '/api/share/folders';
        const response = await fetch(url, {
            credentials: 'include',
        });

        if (response.ok) {
            return await response.json();
        }
    } catch (e) {
        console.log(e);
    }

    return null;
}

export default function () {
    const [state] = useState([{isUnmounted: false}]);
    const [updateItems, setUpdateItems] = useState(0);
    const [shareFiles, setShareFiles] = useState(null);
    const [shareFolders, setShareFolders] = useState(null);
    const [shareItemsExists, setShareItemsExists] = useState({
        files: {},
        folders: {},
    });
    const [users, setUsers] = useState(null);

    state.updateItems = updateItems;

    useEffect(() => {
        return () => {
            state.isUnmounted = true;
        };
    }, []);

    useEffect(() => {
        loadShareItems(true).then(files => {
            if (!state.isUnmounted && state.updateItems === updateItems && files) setShareFiles(files);
        });
    }, [updateItems]);

    useEffect(() => {
        loadShareItems(false).then(folders => {
            if (!state.isUnmounted && state.updateItems === updateItems && folders) setShareFolders(folders);
        });
    }, [updateItems]);

    useEffect(() => {
        load('/api/users/all').then(users => {
            if (!state.isUnmounted && state.updateItems === updateItems && users) setUsers(users);
        });
    }, [updateItems]);

    useEffect(() => {
        if (!shareFolders) return;
        shareFolders.forEach(folder => load(`/api/folders/${encodeBase64UnicodeCustom(folder.id)}/exists`).then(e => {
            if (!state.isUnmounted && state.updateItems === updateItems) {
                shareItemsExists.folders[folder.id] = e;
                setShareItemsExists({...shareItemsExists});
            }
        }));
    }, [shareFolders]);

    useEffect(() => {
        if (!shareFiles) return;
        shareFiles.forEach(file => load(`/api/files/${encodeBase64UnicodeCustom(file.id)}/exists`).then(e => {
            if (!state.isUnmounted && state.updateItems === updateItems) {
                shareItemsExists.files[file.id] = e;
                setShareItemsExists({...shareItemsExists});
            }
        }));
    }, [shareFiles]);

    function renderShareItem(item) {
        const user = item.userId && users && users.find(u => u.id === item.userId);
        const itemLink = item.isFile ?
            `/file/view?path=${encodeURIComponent(item.id)}` :
            `/?folder=${encodeURIComponent(item.id)}`;
        const exists = (item.isFile ? shareItemsExists.files : shareItemsExists.folders)[item.id];
        const editLink = item.isFile ? `/share/file/edit/${item.id}` : `/share/folder/edit/${item.id}`

        return (
            <tr key={item.id}>
                <td>
                    <Link to={itemLink}>{item.name}</Link>
                </td>
                <td className="text-center">
                    {exists !== undefined ? (
                        <Input type="checkbox" className="share-page-exists-item"
                               checked={exists} readOnly/>
                    ) : null}
                </td>
                <td className="text-center">
                    <Input type="checkbox" checked={item.isListed} readOnly/>
                </td>
                <td>{item.userId ? user && user.name : (
                    <i>PUBLIC</i>
                )}</td>
                <td className="shares-page-removable-column">
                    <CustomInput type="checkbox" id="permission-info" className="share-page-permission-item"
                                 label="info" checked={item.permission.info} inline readOnly/>
                    <CustomInput type="checkbox" id="permission-info"
                                 className={item.isFile ? 'd-none' : '"share-page-permission-item"'}
                                 label="list" checked={item.permission.list} inline readOnly/>
                    <CustomInput type="checkbox" id="permission-hash" className="share-page-permission-item"
                                 label="hash" checked={item.permission.hash} inline readOnly/>
                    <CustomInput type="checkbox" id="permission-read" className="share-page-permission-item"
                                 label="read" checked={item.permission.read} inline readOnly/>
                    <CustomInput type="checkbox" id="permission-write" className="share-page-permission-item"
                                 label="write" checked={item.permission.write} inline readOnly/>
                </td>
                <td className="text-center">
                    <Button color="info" className="mb-2 mr-2" tag={Link} to={editLink}>
                        <i className="fas fa-edit"/>
                    </Button>
                    <Button color="danger" className="mb-2 mr-2"
                            onClick={() => deleteShareItem(item, () => setUpdateItems(updateItems + 1))}>
                        <i className="fas fa-trash"/>
                    </Button>
                </td>
            </tr>
        );
    }

    function renderShareItemsTable(items) {
        return (
            <Table striped>
                <thead>
                <tr>
                    <th>Name</th>
                    <th className="shares-page-header-exists">Exists</th>
                    <th className="shares-page-header-is-listed">Is listed</th>
                    <th className="shares-page-header-user">User</th>
                    <th className="shares-page-header-permissions shares-page-removable-column">
                        Permissions
                    </th>
                    <th className="shares-page-header-actions">Actions</th>
                </tr>
                </thead>
                <tbody>
                {items && items.map(renderShareItem)}
                </tbody>
            </Table>
        );
    }

    return (
        <div id="shares-page-container">
            <div className="float-right m-4" onClick={() => setUpdateItems(updateItems + 1)}>
                <i className="folder-viewer-head-update-icon fa fa-retweet fa-2x"/>
            </div>
            <h2>Share Items</h2>

            <div>
                <h4>Folders</h4>
                {renderShareItemsTable(shareFolders)}
            </div>

            <div>
                <h4>Files</h4>
                {renderShareItemsTable(shareFiles)}
            </div>
        </div>
    );
}