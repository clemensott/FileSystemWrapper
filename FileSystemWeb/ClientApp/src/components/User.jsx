import React, {useEffect, useState} from "react";
import {Button, Label} from 'reactstrap';
import {setDocumentTitle} from "../Helpers/setDocumentTitle";
import store from "../Helpers/store";
import API from "../Helpers/API";
import {getAllRefs} from "../Helpers/storeExtensions";

export default function () {
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [isLoggedInCallbackId, setIsLoggedInCallbackId] = useState(null);
    const [user, setUser] = React.useState(null);

    useEffect(() => {
        setDocumentTitle('User');

        setIsLoggedInCallbackId(store.addCallback('isLoggedIn', value => setIsLoggedIn(value)));
        setIsLoggedIn(store.get('isLoggedIn'));
    }, []);

    useEffect(() => {
        return () => isLoggedInCallbackId && store.removeCallback('isLoggedIn', isLoggedInCallbackId)
    }, [isLoggedInCallbackId]);

    useEffect(() => {
        if (isLoggedIn) {
            (async () => {
                const res = await API.getMe();
                if (res.ok) {
                    setUser(await res.json());
                }
            })();
        }
    }, [isLoggedIn]);

    return (
        <div>
            <Label>Username:</Label>
            <div>{user ? user.name : ''}</div>

            <Button color="warning" className="float-end" onClick={() => {
                const {changePasswordModal} = getAllRefs();
                changePasswordModal.current.show();
            }}>Change Password</Button>
        </div>
    );
}