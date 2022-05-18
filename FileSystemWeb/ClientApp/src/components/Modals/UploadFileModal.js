import React, {forwardRef, useImperativeHandle, useRef, useState} from 'react'
import {Button, Modal, ModalHeader, ModalFooter, ModalBody, Form} from 'reactstrap';
import {closeLoadingModal, showErrorModal, showLoadingModal} from '../../Helpers/storeExtensions';
import formatFileSize from '../../Helpers/formatFileSize';
import API from '../../Helpers/API';

function formatFile(file) {
    return `${file.name} (${formatFileSize(file.size)})`
}

const modal = forwardRef((props, ref) => {
    const [promise, setPromise] = useState(null);
    const [suggestedName, setSuggestedName] = useState('');
    const [formattedFileName, setFormattedFileName] = useState(null);
    const [isFileInvalid, setIsFileInvalid] = useState(false);
    const [isNameInvalid, setIsNameInvalid] = useState(false);

    const fileInputRef = useRef();
    const nameInputRef = useRef();

    const closeUploadModal = () => {
        promise.resolve();
        setPromise(null);
    };

    const submit = async () => {
        const file = fileInputRef.current.files.length && fileInputRef.current.files[0];
        const name = nameInputRef.current.value;

        if (!file) setIsFileInvalid(true);
        if (!name) setIsNameInvalid(true);

        if (!file || !name) return;

        try {
            showLoadingModal();
            const response = await API.createFile(promise.folderPath + name, file);
            closeLoadingModal();

            if (response.ok) {
                closeUploadModal();
            } else {
                const text = await response.text();
                await showErrorModal(
                    <div>
                        Status: {response.status}
                        <br/>
                        {text}
                    </div>
                );
            }
        } catch (e) {
            closeLoadingModal();
            await showErrorModal(e.message);
        }
    };

    useImperativeHandle(ref, () => ({
        show: (folderPath) => {
            promise && promise.resolve(false);
            return new Promise(resolve => setPromise({
                resolve,
                folderPath
            }));
        },
        close: closeUploadModal,
    }));

    return (
        <Modal isOpen={!!promise} toggle={closeUploadModal}>
            <ModalHeader toggle={closeUploadModal}>Upload file</ModalHeader>
            <ModalBody>
                <Form onSubmit={e => {
                    e.preventDefault();
                    return submit();
                }}>
                    <div className="input-group">
                        <div className="custom-file">
                            <label>File</label>
                            <input ref={fileInputRef} type="file"
                                   className={`custom-file-input ${isFileInvalid ? 'is-invalid' : ''}`}
                                   onChange={e => {
                                       if (e.target.files.length) {
                                           setSuggestedName(e.target.files[0].name);
                                           setFormattedFileName(formatFile(e.target.files[0]));
                                           setIsFileInvalid(false);
                                       } else {
                                           setIsFileInvalid(true);
                                           setFormattedFileName(null);
                                       }
                                   }}/>
                            <label className="custom-file-label">{formattedFileName || 'Choose file'}</label>
                        </div>
                    </div>
                    <div className="form-group">
                        <label>Name</label>
                        <input ref={nameInputRef} type="text" defaultValue={suggestedName}
                               className={`form-control ${isNameInvalid ? 'is-invalid' : ''}`}
                               onChange={e => setIsNameInvalid(!e.target.value)}/>
                    </div>
                </Form>
            </ModalBody>
            <ModalFooter>
                <Button color="primary" onClick={submit}>Upload</Button>{' '}
                <Button color="secondary" onClick={closeUploadModal}>Cancel</Button>
            </ModalFooter>
        </Modal>
    );
});

export default modal; 