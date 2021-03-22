import React, {useState, useEffect} from 'react';
import {useParams, useHistory} from 'react-router-dom';
import {Button} from 'reactstrap';
import ShareFileSystemItemForm from './ShareFileSystemItemForm';
import Loading from '../Loading/Loading';
import deleteShareItem from '../../Helpers/deleteShareItem'
import {closeLoadingModal, showErrorModal, showLoadingModal} from '../../Helpers/storeExtensions';

function setDocumentTitle(shareItem) {
    let name = null;
    if (shareItem && shareItem.name) {
        name = shareItem.name;
    }

    document.title = name ? `${name} - Edit Share - File System` : 'Edit Share - File System';
}

async function loadShareItem(id, isFile) {
    let item = null;
    let infoError = null;
    if (id) {
        try {
            const url = isFile ?
                `/api/share/file/${encodeURIComponent(id)}` :
                `/api/share/folder/${encodeURIComponent(id)}`;
            const response = await fetch(url, {
                credentials: 'include',
            });

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
        infoError = 'No id of share item given';
    }

    if (infoError) await showErrorModal(infoError);
    return item;
}

async function loadUsers() {
    let users = [];
    try {
        const response = await fetch('/api/users/all', {
            credentials: 'include',
        });

        if (response.ok) {
            users = await response.json();
        }
    } catch (e) {
        console.log(e);
    }

    return users;
}

export default function () {
    const {id} = useParams();
    const decodedId = decodeURIComponent(id);
    const isFile = window.location.pathname.startsWith('/share/file/edit/');

    const history = useHistory();
    const [shareItem, setShareItem] = useState(null);
    const [users, setUsers] = useState(null);

    useEffect(() => {
        setDocumentTitle(null);
        loadShareItem(decodedId, isFile).then(item => {
            if (item) {
                setShareItem(item);
                setDocumentTitle(item)
            } else history.push('/');
        });
    }, [decodedId]);

    useEffect(() => {
        loadUsers().then(u => setUsers(u));
    }, []);

    return shareItem ? (
        <div>
            <ShareFileSystemItemForm item={shareItem} isFile={shareItem.isFile} users={users}
                                     isEdit={true} defaultValues={shareItem} onSubmit={async body => {
                let redirect = null;
                let submitError = null;
                try {
                    showLoadingModal();

                    const url = `${isFile ? '/api/share/file/' : '/api/share/folder'}${shareItem.id}`;
                    const response = await fetch(url, {
                        method: 'PUT',
                        headers: {
                            'Content-Type': 'application/json;charset=utf-8'
                        },
                        body: JSON.stringify(body),
                        credentials: 'include',
                    });

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
            }}/>

            <Button color="danger" className="float-right"
                    onClick={() => deleteShareItem(shareItem, () => history.push('/'))}>
                Delete
            </Button>
        </div>
    ) : (
        <div className="center">
            <Loading/>
        </div>
    );
}