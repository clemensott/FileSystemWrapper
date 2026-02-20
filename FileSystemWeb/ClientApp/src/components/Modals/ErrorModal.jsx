import React, {forwardRef, useImperativeHandle, useState} from 'react'
import {Button, Modal, ModalHeader, ModalFooter, ModalBody} from 'reactstrap';

const modal = forwardRef((props, ref) => {
    const [promise, setPromise] = useState(null);
    const closeErrorModal = () => {
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
        close: closeErrorModal,
    }));

    return (
        <Modal isOpen={!!promise} toggle={closeErrorModal}>
            <ModalHeader toggle={closeErrorModal}>An error occurred</ModalHeader>
            <ModalBody>
                {promise ? promise.error : null}
            </ModalBody>
            <ModalFooter>
                <Button color="primary" onClick={closeErrorModal}>Cancel</Button>
            </ModalFooter>
        </Modal>
    );
});

export default modal; 