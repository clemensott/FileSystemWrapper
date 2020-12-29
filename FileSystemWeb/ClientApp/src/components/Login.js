import React, {useEffect, useRef, useState} from 'react';
import {useHistory} from 'react-router-dom';
import store from '../Helpers/store';

export default function () {
    const history = useHistory();

    const [isUsernameInvalid, setIsUsernameInvalid] = useState(false);
    const [isPasswordInvalid, setIsPasswordInvalid] = useState(false);
    const [error, setError] = useState(null);

    const usernameInputRef = useRef();
    const passwordInputRef = useRef();

    useEffect(() => {
        document.title = 'Login - File System';
        usernameInputRef.current.focus();
    }, []);

    async function submit() {
        const allRefs = store.get('refs');
        const username = usernameInputRef.current.value;
        const password = passwordInputRef.current.value;

        if (!username) setIsUsernameInvalid(true);
        if (!password) setIsPasswordInvalid(true);
        if (!username || !password) {
            return;
        }

        try {
            allRefs.loadingModal.current.show();

            const response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json;charset=utf-8'
                },
                body: JSON.stringify({
                    Username: username,
                    Password: password,
                    keepLoggedIn: true,
                }),
                credentials: 'include'
            });

            if (response.ok) {
                history.push('/');
                store.set('isLoggedIn', true);
            } else if (response.status === 400) {
                setError('Please enter a correct Username and password');
            } else {
                const text = await response.text();
                await allRefs.errorModal.current.show(
                    <div>
                        Status: {response.status}
                        <br/>
                        {text}
                    </div>
                );
            }
        } catch (e) {
            await allRefs.errorModal.current.show(e.message);
        } finally {
            allRefs.loadingModal.current && allRefs.loadingModal.current.close();
        }
    }

    return (
        <div>
            <form onSubmit={e => {
                e.preventDefault();
                return submit();
            }}>
                <div className="form-group">
                    <label>Username</label>
                    <input ref={usernameInputRef} type="text"
                           className={`form-control ${isUsernameInvalid ? 'is-invalid' : ''}`}
                           onChange={e => setIsUsernameInvalid(!e.target.value)}/>
                </div>
                <div className="form-group">
                    <label>Password</label>
                    <input ref={passwordInputRef} type="password"
                           className={`form-control ${isPasswordInvalid ? 'is-invalid' : ''}`}
                           onChange={e => setIsPasswordInvalid(!e.target.value)}/>
                </div>

                <div className={`form-group form-check  ${error ? '' : 'd-none'}`}>
                    <label className="form-check-label">
                        <input className="is-invalid d-none"/>
                        <div className="invalid-feedback">{error}</div>
                    </label>
                </div>

                <div>
                    <button className="btn btn-primary" type="submit">Login</button>
                </div>
            </form>
        </div>
    );
} 