import React, {useEffect, useRef, useState} from 'react';
import {Button, Label} from 'reactstrap';
import {setDocumentTitle} from '../../Helpers/setDocumentTitle';
import store from '../../Helpers/store';
import ChangePasswordModal from './ChangePasswordModal';

export default function () {
    const [me, setMe] = useState(null);
    const [meCallbackId, setMeCallbackId] = useState(null);
    const changePasswordModal = useRef();

    useEffect(() => {
        setDocumentTitle('User');

        setMeCallbackId(store.addCallback('me', value => setMe(value)));
        setMe(store.get('me'));
    }, []);

    useEffect(() => {
        return () => meCallbackId && store.removeCallback('me', meCallbackId)
    }, [meCallbackId]);

    return (
        <div>
            <ChangePasswordModal ref={changePasswordModal} />

            <Label>Username:</Label>
            <div>{me ? me.name : ''}</div>

            <Button color="warning" className="float-end" onClick={() => {
                changePasswordModal.current.show();
            }}>
                <i className="fas fa-edit" />{' '}Change Password
            </Button>
        </div>
    );
}