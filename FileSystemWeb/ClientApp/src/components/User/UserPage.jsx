import React, {useEffect, useRef} from 'react';
import {Button, Label} from 'reactstrap';
import {setDocumentTitle} from '../../Helpers/setDocumentTitle';
import ChangePasswordModal from './ChangePasswordModal';
import {useAuth} from '../../contexts/AuthContext';

export default function () {
    const {user} = useAuth();
    const changePasswordModal = useRef();

    useEffect(() => {
        setDocumentTitle('User');
    }, []);

    return (
        <div>
            <ChangePasswordModal ref={changePasswordModal}/>

            <Label>Username:</Label>
            <div>{user ? user.name : ''}</div>

            <Button color="warning" className="float-end" onClick={() => {
                changePasswordModal.current.show();
            }}>
                <i className="fas fa-edit"/>{' '}Change Password
            </Button>
        </div>
    );
}