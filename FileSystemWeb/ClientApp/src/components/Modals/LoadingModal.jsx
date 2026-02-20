import React, {forwardRef, useImperativeHandle, useState} from 'react'
import {Modal, ModalBody} from 'reactstrap';
import Loading from '../Loading/Loading';

const modal = forwardRef((props, ref) => {
    const [modalOpen, setModalOpen] = useState(false);

    useImperativeHandle(ref, () => ({
        show: () => setModalOpen(true),
        close: () => setModalOpen(false),
    }));

    return (
        <Modal isOpen={modalOpen}>
            <ModalBody>
                <div className="d-flex justify-content-center align-items-center" 
                     style={{height: '300px'}}>
                    <Loading/>
                </div>
            </ModalBody>
        </Modal>
    );
});

export default modal; 