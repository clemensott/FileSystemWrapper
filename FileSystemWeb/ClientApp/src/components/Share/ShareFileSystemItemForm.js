import React, {useRef, useState, useEffect} from 'react';
import {Button, CustomInput, Form, FormGroup, Input, Label} from 'reactstrap';
import './ShareFileSystemItemForm.css'

const invalidUserId = 'invalid';

export default function ({
                             users,
                             item,
                             isFile,
                             defaultValues: {
                                 name: defaultName,
                                 userId: defaultUserId,
                                 isListed: defaultIsListed,
                             } = {},
                             onSubmit
                         }) {
    const [isFileNameInvalid, setIsFileNameInvalid] = useState(false);
    const [isUserSelectInvalid, setIsUserSelectInvalid] = useState(false);
    const [userId, setUserId] = useState(undefined);

    useEffect(() => {
        if (defaultUserId !== undefined && users && users.some(u => u.id === defaultUserId)) {
            setUserId(defaultUserId);
        }
    }, [defaultUserId, users]);

    let userOptions = [];
    const publicUserOption = <option key="public" value="">Public</option>;
    if (!users) {
        userOptions.push(<option key="loading" value={invalidUserId}>Loading...</option>, publicUserOption);
    } else if (users.length) {
        userOptions.push(
            publicUserOption,
            ...users.map(user => <option key={user.id} value={user.id}>{user.name}</option>)
        );
    } else {
        userOptions.push(<option key="empty" value={invalidUserId}>Empty</option>, publicUserOption);
    }

    const nameRef = useRef();
    const userIdRef = useRef();
    const permissionInfoRef = useRef();
    const permissionListRef = useRef();
    const permissionHashRef = useRef();
    const permissionReadRef = useRef();
    const permissionWriteRef = useRef();
    const isListedRef = useRef();

    function submit() {
        if (!onSubmit) return;

        const body = {
            name: nameRef.current.value,
            path: item.path,
            userId: userIdRef.current.value || null,
            permission: {
                info: !!permissionInfoRef.current.checked,
                list: !!permissionListRef.current.checked,
                hash: !!permissionHashRef.current.checked,
                read: !!permissionReadRef.current.checked,
                write: !!permissionWriteRef.current.checked,
            },
            isListed: !!isListedRef.current.checked,
        };

        let invalid = false;
        if (!body.name) {
            invalid = true;
        }
        if (body.userId === invalidUserId) {
            invalid = true;
            setIsUserSelectInvalid(true);
        }

        if (!invalid) onSubmit(body);
    }

    return (
        <div>
            <h2>{item.sharedId ? 'Edit' : ''} Share {isFile ? 'File' : 'Folder'}:</h2>
            <h4>{item.name}</h4>
            <Form>
                <FormGroup>
                    <Label for="name">File name</Label>
                    <Input innerRef={nameRef} type="text" name="name" placeholder="File name"
                           defaultValue={defaultName || item.name}
                           invalid={isFileNameInvalid} onChange={e => setIsFileNameInvalid(!e.target.value)}/>
                </FormGroup>

                <FormGroup>
                    <Label for="userId">User</Label>
                    <Input innerRef={userIdRef} type="select" name="select" invalid={isUserSelectInvalid}
                           value={userId}
                           onChange={e => {
                               setUserId(e.target.value);
                               setIsUserSelectInvalid(e.target.value === invalidUserId);
                           }}>
                        {userOptions}
                    </Input>
                </FormGroup>

                <FormGroup className={item.sharedId ? 'd-none' : ''}>
                    <Label for="permission">Permission</Label>
                    <div>
                        <CustomInput innerRef={permissionInfoRef} type="checkbox" id="permission-info"
                                     label="info" defaultChecked={true} disabled inline/>
                        <CustomInput innerRef={permissionListRef} type="checkbox" id="permission-list"
                                     label="list" className={isFile ? 'd-none' : ''} inline/>
                        <CustomInput innerRef={permissionHashRef} type="checkbox"
                                     id="permission-hash" label="hash" inline/>
                        <CustomInput innerRef={permissionReadRef} type="checkbox"
                                     id="permission-read" label="read" inline/>
                        <CustomInput innerRef={permissionWriteRef} type="checkbox"
                                     id="permission-write" label="write" inline/>
                    </div>
                </FormGroup>

                <FormGroup>
                    <Label for="permission">Visibility</Label>
                    <div>
                        <CustomInput type="radio" id="is-link" name="is-listed" label="Link only"
                                     defaultChecked={defaultIsListed !== undefined ? !defaultIsListed : true}
                                     inline/>
                        <CustomInput innerRef={isListedRef} type="radio" id="is-listed" name="is-listed" label="Listed"
                                     defaultChecked={defaultIsListed !== undefined ? defaultIsListed : false}
                                     inline/>
                    </div>
                </FormGroup>

                <Button className="mt-2" type="button" onClick={() => submit()}>
                    {item.sharedId ? 'Save' : 'Share'}
                </Button>
            </Form>
        </div>
    );
}