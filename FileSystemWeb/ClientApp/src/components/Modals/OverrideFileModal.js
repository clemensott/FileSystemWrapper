import React, { forwardRef, useImperativeHandle, useState } from 'react'
import { Button, Modal, ModalHeader, ModalFooter, ModalBody } from 'reactstrap';

const modal = forwardRef((props, ref) => {
    const [promise, setPromise] = useState(null);
    const closeModal = () => {
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
        close: closeModal,
    }));

    return (
        <Modal isOpen={!!promise} toggle={closeModal}>
            <ModalHeader toggle={closeModal}>Override file?</ModalHeader>
            <ModalBody>
                A file with this name does already exist in this folder. Do you want to override this file?
                <br />
                <b>{promise ? promise.params.name : null}</b>
            </ModalBody>
            <ModalFooter>
                <Button color="danger" onClick={() => {
                    promise.resolve(true);
                    setPromise(null);
                }}>Override</Button>{' '}
                <Button color="secondary" onClick={closeModal}>Cancel</Button>
            </ModalFooter>
        </Modal>
    );
});

export default modal; 