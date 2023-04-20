import React, { forwardRef, useImperativeHandle, useRef, useState } from 'react'
import { Button, Modal, ModalHeader, ModalFooter, ModalBody, Form, FormGroup, Label, Input } from 'reactstrap';
import { closeLoadingModal, getAllRefs, showErrorModal, showLoadingModal } from '../../Helpers/storeExtensions';
import formatFileSize from '../../Helpers/formatFileSize';
import API from '../../Helpers/API';
import sleep from '../../Helpers/sleep'

function formatFile(file) {
    return `${file.name} (${formatFileSize(file.size)})`
}

const modal = forwardRef((props, ref) => {
    const allRefs = getAllRefs();
    const [promise, setPromise] = useState(null);
    const [suggestedName, setSuggestedName] = useState('');
    const [formattedFileName, setFormattedFileName] = useState(null);
    const [isFileInvalid, setIsFileInvalid] = useState(false);
    const [isNameInvalid, setIsNameInvalid] = useState(false);

    const fileInputRef = useRef();
    const nameInputRef = useRef();

    const closeUploadModal = (uploaded = false) => {
        promise.resolve(uploaded);
        setPromise(null);
    };

    const cancelUploadModal = () => closeUploadModal(false);

    const submit = async () => {
        const file = fileInputRef.current.files.length && fileInputRef.current.files[0];
        const name = nameInputRef.current.value;

        if (!file) setIsFileInvalid(true);
        if (!name) setIsNameInvalid(true);

        if (!file || !name) return;

        try {
            const filePath = promise.folderPath + name;
            const fileExistsResponse = await API.getFileExists(filePath);
            const fileExists = fileExistsResponse.ok && await fileExistsResponse.json();
            if (fileExists && !await allRefs.overrideFileModal.current.show({ name })) {
                return;
            }

            showLoadingModal();
            const response = await API.createFile(promise.folderPath + name, file);
            closeLoadingModal();
            await sleep(300);

            if (response.ok) {
                closeUploadModal(true);
            } else {
                const text = await response.text();
                await showErrorModal(
                    <div>
                        Status: {response.status}
                        <br />
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
        close: cancelUploadModal,
    }));

    const onFileChange = e => {
        if (e.target.files.length) {
            setSuggestedName(e.target.files[0].name);
            setFormattedFileName(formatFile(e.target.files[0]));
            setIsFileInvalid(false);
        } else {
            setIsFileInvalid(true);
            setFormattedFileName(null);
        }
    };

    return (
        <Modal isOpen={!!promise} toggle={cancelUploadModal}>
            <ModalHeader toggle={cancelUploadModal}>Upload file</ModalHeader>
            <ModalBody>
                <Form onSubmit={e => {
                    e.preventDefault();
                    return submit();
                }}>
                    <FormGroup>
                        <Label>File</Label>
                        <Input innerRef={fileInputRef} type="file"
                            className={isFileInvalid ? 'is-invalid' : ''} onChange={onFileChange} />
                    </FormGroup>
                    <FormGroup>
                        <Label>Name</Label>
                        <Input innerRef={nameInputRef} type="text" defaultValue={suggestedName}
                            className={`form-control ${isNameInvalid ? 'is-invalid' : ''}`}
                            onChange={e => setIsNameInvalid(!e.target.value)} />
                    </FormGroup>
                </Form>
            </ModalBody>
            <ModalFooter>
                <Button color="primary" onClick={submit}>Upload</Button>{' '}
                <Button color="secondary" onClick={cancelUploadModal}>Cancel</Button>
            </ModalFooter>
        </Modal>
    );
});

export default modal;