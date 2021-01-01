import React, {useState, useEffect} from 'react';
import {useParams, useHistory} from 'react-router-dom';
import {Button} from 'reactstrap';
import Loading from '../Loading/Loading';
import deleteShareItem from '../../Helpers/deleteShareItem'
import {showErrorModal} from '../../Helpers/storeExtensions';
import ShareFileSystemItemControl from "./ShareFileSystemItemControl";

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

export default function () {
    const {id} = useParams();
    const decodedId = decodeURIComponent(id);
    const isFile = window.location.pathname.startsWith('/share/file/edit/');

    const history = useHistory();
    const [shareItem, setShareItem] = useState(null);

    useEffect(() => {
        loadShareItem(decodedId, isFile).then(item => {
            if (item) {
                setShareItem(item);
                setDocumentTitle(item)
            } else history.push('/');
        });
    }, [decodedId]);

    return shareItem ? (
        <div>
            <ShareFileSystemItemControl path={shareItem.id} isFile={shareItem.isFile} defaultValues={shareItem}/>

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