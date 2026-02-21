import React, {forwardRef, useImperativeHandle, useState} from 'react'
import {Button, Modal, ModalHeader, ModalFooter, ModalBody} from 'reactstrap';

const modal = forwardRef((props, ref) => {
    const [promise, setPromise] = useState(null);
    const closeDeleteModal = () => {
        promise.resolve(false);
        setPromise(null);
    };

    useImperativeHandle(ref, () => ({
        show: (params) => {
            promise && promise.resolve(false);
            return new Promise(resolve => setPromise({
                resolve,
                params
            }));
        },
        close: closeDeleteModal,
    }));

    return (
        <Modal isOpen={!!promise} toggle={closeDeleteModal}>
            <ModalHeader toggle={closeDeleteModal}>Delete user</ModalHeader>
            <ModalBody>
                Are you sure you want to delete the user?
                <br/>
                <b>{promise ? promise.params.name : null}</b>
            </ModalBody>
            <ModalFooter>
                <Button color="danger" onClick={() => {
                    promise.resolve(true);
                    setPromise(null);
                }}>Delete</Button>{' '}
                <Button color="secondary" onClick={closeDeleteModal}>Cancel</Button>
            </ModalFooter>
        </Modal>
    );
});

export default modal;