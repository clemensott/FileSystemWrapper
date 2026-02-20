import React, {forwardRef, useImperativeHandle, useState} from 'react'
import {Button, Modal, ModalHeader, ModalFooter, ModalBody} from 'reactstrap';

const modal = forwardRef((props, ref) => {
    const [promise, setPromise] = useState(null);
    const closeDeleteModal = () => {
        promise.resolve();
        setPromise(null);
    };

    useImperativeHandle(ref, () => ({
        show: (error) => {
            promise && promise.resolve(false);
            return new Promise(resolve => setPromise({
                resolve,
                error
            }));
        },
        close: closeDeleteModal,
    }));

    return (
        <Modal isOpen={!!promise} toggle={closeDeleteModal}>
            <ModalHeader toggle={closeDeleteModal}>An error occoured</ModalHeader>
            <ModalBody>
                {promise ? promise.error : null}
            </ModalBody>
            <ModalFooter>
                <Button color="primary" onClick={closeDeleteModal}>Cancel</Button>
            </ModalFooter>
        </Modal>
    );
});

export default modal; 