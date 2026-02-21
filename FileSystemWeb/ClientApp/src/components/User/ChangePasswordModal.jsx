import React, {forwardRef, useImperativeHandle, useState} from 'react'
import {Button, Modal, ModalHeader, ModalFooter, ModalBody, Form, Label, Input, FormGroup, Alert} from 'reactstrap';
import API from '../../Helpers/API';

const modal = forwardRef((props, ref) => {
    const [promise, setPromise] = useState(null);
    const [successAlertVisible, setSuccessAlertVisible] = useState(false);
    const [errorAlertVisible, setErrorAlertVisible] = useState(false);
    const [oldPassword, setOldPassword] = useState('');
    const [isOldPasswordInvalid, setIsOldPasswordInvalid] = useState(false);
    const [newPassword, setNewPassword] = useState('');
    const [isNewPasswordInvalid, setIsNewPasswordInvalid] = useState(false);
    const [repeatPassword, setRepeatPassword] = useState('');
    const [isRepeatPasswordInvalid, setIsRepeatPasswordInvalid] = useState(false);

    const closeChangePasswordModal = () => {
        promise.resolve();
        setPromise(null);
    };
    const changePassword = async () => {
        setIsOldPasswordInvalid(!oldPassword);
        setIsNewPasswordInvalid(!newPassword);
        setIsRepeatPasswordInvalid(!repeatPassword || newPassword !== repeatPassword);

        if (oldPassword && newPassword && newPassword === repeatPassword) {
            setErrorAlertVisible(false);
            setSuccessAlertVisible(false);
            const res = await API.changePassword({oldPassword, newPassword});
            if (res.ok) setSuccessAlertVisible(true);
            else setErrorAlertVisible(true);
        }
    };

    useImperativeHandle(ref, () => ({
        show: () => {
            promise && promise.resolve(false);
            return new Promise(resolve => setPromise({
                resolve
            }));
        },
        close: closeChangePasswordModal,
    }));

    return (
        <Modal isOpen={!!promise} toggle={closeChangePasswordModal}>
            <ModalHeader toggle={closeChangePasswordModal}>Change Password</ModalHeader>
            <ModalBody>
                <Alert color="success" isOpen={successAlertVisible} toggle={() => setSuccessAlertVisible(false)}>
                    Changed password successfully
                </Alert>
                <Alert color="danger" isOpen={errorAlertVisible} toggle={() => setErrorAlertVisible(false)}>
                    An error occurred
                </Alert>
                <Form onSubmit={changePassword}>
                    <FormGroup>
                        <Label>Old password</Label>
                        <Input
                            id="oldPassword"
                            name="oldPassword"
                            type="password"
                            invalid={isOldPasswordInvalid}
                            value={oldPassword}
                            onChange={e => setOldPassword(e.target.value)}
                        />
                    </FormGroup>
                    <FormGroup>
                        <Label>New password</Label>
                        <Input
                            id="newPassword"
                            name="newPassword"
                            type="password"
                            invalid={isNewPasswordInvalid}
                            value={newPassword}
                            onChange={e => setNewPassword(e.target.value)}
                        />
                    </FormGroup>
                    <FormGroup>
                        <Label>Repeat new password</Label>
                        <Input
                            id="repeatPassword"
                            name="repeatPassword"
                            type="password"
                            invalid={isRepeatPasswordInvalid}
                            value={repeatPassword}
                            onChange={e => setRepeatPassword(e.target.value)}
                        />
                    </FormGroup>
                </Form>
            </ModalBody>
            <ModalFooter>
                <Button color="secondary" onClick={closeChangePasswordModal}>Cancel</Button>
                <Button color="danger" onClick={changePassword}>Change Password</Button>
            </ModalFooter>
        </Modal>
    );
});

export default modal; 