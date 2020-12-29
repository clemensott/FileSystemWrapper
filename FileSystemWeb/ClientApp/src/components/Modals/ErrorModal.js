import React, {forwardRef, useImperativeHandle, useState} from 'react'
import {Button, Modal, ModalHeader, ModalFooter, ModalBody} from 'reactstrap';

const modal = forwardRef((props, ref) => {
    const [modalOpen, setModalOpen] = useState(false);
    const closeDeleteModal = () => setModalOpen(false);

    useImperativeHandle(ref, () => ({
        show: (error) => setModalOpen(error),
        close: closeDeleteModal,
    }));

    return (
        <Modal isOpen={modalOpen} toggle={closeDeleteModal}>
            <ModalHeader toggle={closeDeleteModal}>An error occoured</ModalHeader>
            <ModalBody>
                {modalOpen}
            </ModalBody>
            <ModalFooter>
                <Button color="primary" onClick={closeDeleteModal}>Cancel</Button>
            </ModalFooter>
        </Modal>
    );
});

export default modal; 