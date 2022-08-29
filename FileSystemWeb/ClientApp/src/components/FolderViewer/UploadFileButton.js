import React, { useRef } from 'react';
import { Button } from 'reactstrap';
import UploadFileModal from '../Modals/UploadFileModal';
import './UploadFileButton.css';


export default function ({ path, disabled, onUploaded }) {
    const uploadFileModalRef = useRef();

    const upload = async () => {
        await uploadFileModalRef.current.show(path)
        if (typeof onUploaded === 'function') {
            onUploaded();
        }
    };

    return (
        <React.Fragment>
            <div className="folder-viewer-upload-container">
                <Button color="dark" outline={true} disabled={disabled} onClick={upload}>
                    <i className="fa fa-upload" />
                </Button>
            </div>
            <UploadFileModal ref={uploadFileModalRef} />
        </React.Fragment>
    );
}