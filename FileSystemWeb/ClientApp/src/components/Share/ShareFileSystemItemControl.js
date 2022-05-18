import React, { useState, useEffect } from 'react';
import { useHistory } from 'react-router-dom';
import ShareFileSystemItemForm from './ShareFileSystemItemForm';
import Loading from '../Loading/Loading';
import { closeLoadingModal, showErrorModal, showLoadingModal } from '../../Helpers/storeExtensions';
import API from '../../Helpers/API';

async function updateItem(path, isFile, force = false) {
    let item = null;
    let infoError = null;
    if (path) {
        try {
            const response = await API.getFileSystemItemInfo(path, isFile);

            if (response.ok) {
                item = await response.json();
            } else if (response.status === 401) {
                infoError = 'Unauthorized. It seems like your are not logged in';
            } else if (response.status === 403) {
                infoError = `Forbidden to access ${isFile ? 'file' : 'folder'}`;
            } else if (response.status === 404) {
                infoError = `${isFile ? 'File' : 'Folder'} not found`;
            } else {
                infoError = 'An error occured';
            }
        } catch (e) {
            console.log(e);
            infoError = e.message;
        }
    } else {
        infoError = 'No path of given';
    }

    if (infoError) await showErrorModal(infoError);
    return item;
}

async function loadUsers() {
    let users = [];
    try {
        const response = await API.getAllUsers();
        if (response.ok) {
            users = await response.json();
        }
    } catch (e) {
        console.log(e);
    }

    return users;
}

export default function ({ path, isFile, defaultValues, onItemInfoLoaded }) {
    const [item, setItem] = useState(null);
    const [users, setUsers] = useState(null);
    const history = useHistory();

    useEffect(() => {
        updateItem(path, isFile).then(i => {
            setItem(i);
            onItemInfoLoaded && onItemInfoLoaded(i);
        });
    }, [path]);

    useEffect(() => {
        loadUsers().then(u => setUsers(u));
    }, []);

    return item ? (
        <ShareFileSystemItemForm item={item} isFile={isFile} users={users}
            defaultValues={defaultValues} onSubmit={async body => {
                let redirect = null;
                let submitError = null;
                try {
                    showLoadingModal();
                    const response = await API.createShareItem(body, isFile);
                    if (response.ok) {
                        const shareItem = await response.json();
                        redirect = isFile ?
                            `/file/view?path=${encodeURIComponent(shareItem.path)}` :
                            `/?folder=${encodeURIComponent(shareItem.path)}`;
                    } else if (response.status === 400) {
                        submitError = (await response.text()) || 'An error occured';
                    } else if (response.status === 404) {
                        submitError = `${isFile ? 'File' : 'Folder'} not found`;
                    } else if (response.status === 403) {
                        submitError = 'Forbidden. You may have not enough access rights';
                    } else if (response.status === 401) {
                        submitError = 'Unauthorized. It seems like your are not logged in';
                    } else {
                        submitError = 'An error occured';
                    }
                } catch (e) {
                    console.log(e);
                    submitError = e.message;
                } finally {
                    closeLoadingModal();
                }

                if (submitError) await showErrorModal(submitError);
                if (redirect) history.push(redirect);
            }} />
    ) : (
            <div className="center">
                <Loading />
            </div>
        )
}
