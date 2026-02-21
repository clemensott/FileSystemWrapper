import React, {forwardRef, useImperativeHandle, useState} from 'react'
import {Button, Modal, ModalHeader, ModalFooter, ModalBody, Form, FormGroup, Label, Input} from 'reactstrap';
import {showErrorModal} from "../../Helpers/storeExtensions";
import API from "../../Helpers/API";

const modal = forwardRef((props, ref) => {
    const [promise, setPromise] = useState(null);
    const [roles, setRoles] = useState([]);
    const [username, setUsername] = useState('');
    const [isUsernameInvalid, setIsUsernameInvalid] = useState(false);
    const [password, setPassword] = useState('');
    const [isPasswordInvalid, setIsPasswordInvalid] = useState(false);
    const [roleIds, setRoleIds] = useState([]);

    const closeCreateModal = () => {
        promise.resolve(false);
        setPromise(null);
    };

    async function loadRoles() {
        try {
            const res = await API.getAllRoles();
            if (res.ok) {
                setRoles(await res.json());
            } else {
                await showErrorModal('Could not find any role');
            }
        } catch (error) {
            await showErrorModal(error.message);
        }
    }

    useImperativeHandle(ref, () => ({
        show: (params) => {
            setUsername('');
            setPassword('');
            setRoleIds([]);
            loadRoles();
            promise && promise.resolve(false);
            return new Promise(resolve => setPromise({
                resolve,
                params
            }));
        },
        close: closeCreateModal,
    }));

    function handleSubmit() {
        setIsUsernameInvalid(!username);
        setIsPasswordInvalid(!password);

        if (username && password) {
            promise.resolve({
                userName: username,
                password,
                roleIds,
            });
            setPromise(null);
        }
    }

    return (
        <Modal isOpen={!!promise} toggle={closeCreateModal}>
            <ModalHeader toggle={closeCreateModal}>Create user</ModalHeader>
            <ModalBody>
                <Form>
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
                        <Label>Password</Label>
                        <Input
                            type="password"
                            invalid={isPasswordInvalid}
                            value={password}
                            onChange={e => setPassword(e.target.value)}
                        />
                    </FormGroup>
                    <FormGroup>
                        <Label>Roles</Label>
                        <Input
                            type="select"
                            multiple
                            value={password}
                            onChange={e => console.log(e.target)}
                        >
                            {roles.map((role) => (<option key={role.id} value={role.id}>{role.name}</option>))}
                        </Input>
                    </FormGroup>
                </Form>
            </ModalBody>
            <ModalFooter>
                <Button color="primary" onClick={handleSubmit}>Create</Button>{' '}
                <Button color="secondary" onClick={closeCreateModal}>Cancel</Button>
            </ModalFooter>
        </Modal>
    );
});

export default modal;