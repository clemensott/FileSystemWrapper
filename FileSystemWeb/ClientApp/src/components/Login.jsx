import React, {useEffect, useState} from 'react';
import {useNavigate} from 'react-router-dom';
import API from '../Helpers/API';
import store from '../Helpers/store';
import {closeLoadingModal, showErrorModal, showLoadingModal} from '../Helpers/storeExtensions';
import {Alert, Button, Form, FormGroup, Input, Label} from 'reactstrap';
import {setDocumentTitle} from '../Helpers/setDocumentTitle';

export default function () {
    const navigate = useNavigate();

    const [username, setUsername] = useState('');
    const [isUsernameInvalid, setIsUsernameInvalid] = useState(false);
    const [password, setPassword] = useState('');
    const [isPasswordInvalid, setIsPasswordInvalid] = useState(false);
    const [error, setError] = useState(null);

    useEffect(() => {
        setDocumentTitle('Login');
    }, []);

    async function submit() {
        setError(null);
        if (!username) setIsUsernameInvalid(true);
        if (!password) setIsPasswordInvalid(true);
        if (!username || !password) {
            return;
        }

        try {
            showLoadingModal();

            const response = await API.login(username, password, true);

            closeLoadingModal();
            if (response.ok) {
                navigate('/');
                store.set('isLoggedIn', true);
            } else if (response.status === 400) {
                setError('Please enter a correct Username and password');
            } else {
                const text = await response.text();
                await showErrorModal(
                    <div>
                        Status: {response.status}
                        <br/>
                        {text}
                    </div>
                );
            }
        } catch (e) {
            closeLoadingModal();
            await showErrorModal(e.message);
        }
    }

    return (
        <div>
            <Form onSubmit={e => {
                e.preventDefault();
                return submit();
            }}>
                <FormGroup>
                    <Label>Username</Label>
                    <Input
                        type="text"
                        invalid={isUsernameInvalid}
                        value={username}
                        onChange={e => setUsername(e.target.value)}
                    />
                </FormGroup>
                <FormGroup>
                    <Label>Username</Label>
                    <Input
                        type="password"
                        invalid={isPasswordInvalid}
                        value={password}
                        onChange={e => setPassword(e.target.value)}
                    />
                </FormGroup>

                <Alert color="danger" isOpen={!!error} toggle={() => setError(null)}>{error}</Alert>

                <div>
                    <Button color="primary" type="submit">Login</Button>
                </div>
            </Form>
        </div>
    );
}