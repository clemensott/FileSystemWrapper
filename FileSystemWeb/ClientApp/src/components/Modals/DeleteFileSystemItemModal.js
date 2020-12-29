import React, {forwardRef, useImperativeHandle, useState} from 'react'
import {Button, Modal, ModalHeader, ModalFooter, ModalBody} from 'reactstrap';

const modal = forwardRef((props, ref) => {
    const [obj, setObj] = useState(null);
    const closeDeleteModal = () => setObj(null);

    useImperativeHandle(ref, () => ({
        show: (obj) => setObj(obj),
        close: closeDeleteModal,
    }));

    return (
        <Modal isOpen={!!obj} toggle={closeDeleteModal}>
            <ModalHeader toggle={closeDeleteModal}>Delete file</ModalHeader>
            <ModalBody>
                Are you sure you want to delete the file?
                <br/>
                <b>{obj ? obj.item.name : null}</b>
            </ModalBody>
            <ModalFooter>
                <Button color="danger" onClick={() => {
                    closeDeleteModal();
                    props.onDelete && props.onDelete(obj);
                }}>Delete</Button>{' '}
                <Button color="secondary" onClick={closeDeleteModal}>Cancel</Button>
            </ModalFooter>
        </Modal>
    );
});

export default modal; 