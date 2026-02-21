import React, { useState, useEffect } from 'react';
import { Button, FormGroup, Input, Label, Table } from 'reactstrap';
import deleteShareItem from '../../Helpers/deleteShareItem';
import { Link } from 'react-router-dom';
import API from '../../Helpers/API';
import {setDocumentTitle} from '../../Helpers/setDocumentTitle';
import './SharesPage.css'

async function load(promise) {
    try {
        const response = await promise;

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
        const response = await API.getShareItems(isFile);
        if (response.ok) {
            return await response.json();
        }
    } catch (e) {
        console.log(e);
    }

    return null;
}

export default function () {
    const [state] = useState([{ isUnmounted: false }]);
    const [updateItems, setUpdateItems] = useState(0);
    const [shareFiles, setShareFiles] = useState(null);
    const [shareFolders, setShareFolders] = useState(null);
    const [users, setUsers] = useState(null);

    state.updateItems = updateItems;

    useEffect(() => {
        setDocumentTitle('Shares');
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
        load(API.getAllUsers()).then(users => {
            if (!state.isUnmounted && state.updateItems === updateItems && users) setUsers(users);
        });
    }, [updateItems]);

    function renderShareItem(item) {
        const user = item.userId && users && users.find(u => u.id === item.userId);
        const itemLink = item.isFile ?
            `/file/view?path=${encodeURIComponent(item.id)}` :
            `/?folder=${encodeURIComponent(item.id)}`;
        const editLink = item.isFile ? `/share/file/edit/${item.id}` : `/share/folder/edit/${item.id}`

        return (
            <tr key={item.id}>
                <td>
                    <Link to={itemLink}>{item.name}</Link>
                </td>
                <td className="text-center">
                    <Input type="checkbox" className="share-page-exists-item" checked={item.exists} readOnly />
                </td>
                <td className="text-center">
                    <Input type="checkbox" checked={item.isListed} readOnly />
                </td>
                <td>{item.userId ? user && user.name : (
                    <i>PUBLIC</i>
                )}</td>
                <td className="shares-page-removable-column">
                    <FormGroup check inline className="share-page-permission-item">
                        <Input type="checkbox" checked={item.permission.info} readOnly />
                        <Label check>info</Label>
                    </FormGroup>
                    <FormGroup check inline className={item.isFile ? 'd-none' : 'share-page-permission-item'}>
                        <Input type="checkbox" checked={item.permission.list} readOnly />
                        <Label check>list</Label>
                    </FormGroup>
                    <FormGroup check inline className="share-page-permission-item">
                        <Input type="checkbox" checked={item.permission.hash} readOnly />
                        <Label check>hash</Label>
                    </FormGroup>
                    <FormGroup check inline className="share-page-permission-item">
                        <Input type="checkbox" checked={item.permission.read} readOnly />
                        <Label check>read</Label>
                    </FormGroup>
                    <FormGroup check inline className="share-page-permission-item">
                        <Input type="checkbox" checked={item.permission.write} readOnly />
                        <Label check>write</Label>
                    </FormGroup>
                </td>
                <td className="text-center">
                    <Button color="info" className="mb-2 me-2" tag={Link} to={editLink}>
                        <i className="fas fa-edit" />
                    </Button>
                    <Button color="danger" className="mb-2 me-2"
                        onClick={() => deleteShareItem(item, () => setUpdateItems(updateItems + 1))}>
                        <i className="fas fa-trash" />
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
            <div className="float-end m-4" onClick={() => setUpdateItems(updateItems + 1)}>
                <i className="folder-viewer-head-update-icon fa fa-retweet fa-2x" />
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