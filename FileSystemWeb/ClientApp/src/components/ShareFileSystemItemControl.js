import React, {Component} from 'react';
import {Redirect} from "react-router-dom";
import {Button, CustomInput, Form, FormGroup, Input, Label} from "reactstrap";
import Loading from "./Loading/Loading";
import {encodeBase64UnicodeCustom} from "../Helpers/Path";
import './ShareFileSystemItemControl.css'

const invalidUserId = 'invalid';

export class ShareFileSystemItemControl extends Component {
    static displayName = ShareFileSystemItemControl.name;

    constructor(props) {
        super(props);

        this.itemFetchPath = null;

        this.state = {
            itemPath: null,
            item: null,
            infoError: null,
            users: null,
            isFileNameInvalid: false,
            isUserSelectInvalid: false,
            isSubmitting: false,
            redirect: null,
            submitError: null,
        }
    }

    async updateItem(path, force = false) {
        if (!force && this.itemFetchPath === path) return;
        this.itemFetchPath = path;

        let item = null;
        let infoError = false;
        if (path) {
            try {
                const url = this.props.isFile ?
                    `/api/files/${encodeBase64UnicodeCustom(path)}/info` :
                    `/api/folders/${encodeBase64UnicodeCustom(path)}/info`;
                const response = await fetch(url, {
                    credentials: 'include',
                });

                if (response.ok) {
                    item = await response.json();
                } else if (response.status === 401) {
                    infoError = 'Unauthorized. It seems like your are not logged in';
                } else if (response.status === 403) {
                    infoError = `Forbidden to access ${this.props.isFile ? 'file' : 'folder'}`;
                } else if (response.status === 404) {
                    infoError = `${this.props.isFile ? 'File' : 'Folder'} not found`;
                } else {
                    infoError = 'An error occured';
                }
            } catch (e) {
                console.log(e);
                infoError = e.message;
            }
        }

        if (path === this.props.path) {
            this.props.onItemInfoLoaded && this.props.onItemInfoLoaded(item);
            this.setState({
                itemPath: path,
                item,
                infoError,
            });
        }
    }

    async loadUsers() {
        let users = [];
        try {
            const response = await fetch('/api/users/all', {
                credentials: 'include',
            });

            if (response.ok) {
                users = await response.json();
            }
        } catch (e) {
            console.log(e);
        }

        this.setState({
            users,
        });
    }

    async submit() {
        const body = {
            name: document.getElementById('name').value,
            path: this.props.path,
            userId: document.getElementById('userId').value || null,
            permission: {
                info: !!document.getElementById('permission-info').checked,
                list: !!document.getElementById('permission-list').checked,
                hash: !!document.getElementById('permission-hash').checked,
                read: !!document.getElementById('permission-read').checked,
                write: !!document.getElementById('permission-write').checked,
            },
            isListed: !!document.getElementById('listed').checked,
        };

        let invalid = false;
        if (!body.name) {
            invalid = true;
        }
        if (body.userId === invalidUserId) {
            invalid = true;
            this.setState({isUserSelectInvalid: true});
        }

        if (invalid) return;

        let redirect = null;
        let submitError = null;
        try {
            body.userId=null;
            this.setState({
                isSubmitting: true,
            });

            const url = this.props.isFile ? '/api/share/file' : '/api/share/folders';
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json;charset=utf-8'
                },
                body: JSON.stringify(body),
                credentials: 'include',
            });

            if (response.ok) {
                const shareFile = await response.json();
                redirect = `/file/view/${encodeURIComponent(shareFile.path)}`;
            } else if (response.status === 400) {
                submitError = (await response.text()) || 'An error occured';
            } else if (response.status === 404) {
                submitError = `${this.props.isFile ? 'File' : 'Folder'} not found`;
            } else if (response.status === 403) {
                submitError = 'Forbidden. You may have not enough access rights';
            } else if (response.status === 401) {
                submitError = 'Unauthorized. It seems like your are not logged in';
            } else {
                submitError = 'An error occured';
            }
        } catch (e) {
            console.log(e);
            submitError = e.message;
        }

        this.setState({
            isSubmitting: false,
            redirect,
            submitError,
        });
    }

    render() {
        if (this.state.redirect) {
            return (
                <Redirect to={this.state.redirect}/>
            )
        }
        if (this.state.infoError) {
            return (
                <div className="text-center">
                    <h3 className="font-italic share-item-info-text">
                        {this.state.infoError}
                    </h3>
                </div>
            );
        }
        if (!this.state.item) {
            return (
                <div className="center">
                    <Loading/>
                </div>
            )
        }

        const item = this.state.item;

        let userOptions = [];
        const publicUserOption = <option key="public" value="">Public</option>;
        if (!this.state.users) {
            userOptions.push(<option key="loading" value={invalidUserId}>Loading...</option>, publicUserOption);
        } else if (this.state.users.length) {
            userOptions.push(
                publicUserOption,
                ...this.state.users.map(user => <option key={user.id} value={user.id}>{user.name}</option>)
            );
        } else {
            userOptions.push(<option key="empty" value={invalidUserId}>Empty</option>, publicUserOption);
        }

        return (
            <div>
                <h2>Share {this.props.isFile ? 'File' : 'Folder'}:</h2>
                <h4>{item.name}</h4>
                <Form className={this.state.isSubmitting ? 'd-none' : ''}>
                    <FormGroup>
                        <Label for="name">File name</Label>
                        <Input type="text" name="name" id="name" placeholder="File name" defaultValue={item.name}
                               invalid={this.state.isFileNameInvalid} onChange={e => this.setState({
                            isFileNameInvalid: !e.target.value,
                        })}/>
                    </FormGroup>

                    <FormGroup>
                        <Label for="userId">User</Label>
                        <Input type="select" name="select" id="userId"
                               invalid={this.state.isUserSelectInvalid} onChange={e => this.setState({
                            isUserSelectInvalid: e.target.value === invalidUserId,
                        })}>
                            {userOptions}
                        </Input>
                    </FormGroup>

                    <FormGroup>
                        <Label for="permission">Permission</Label>
                        <div>
                            <CustomInput type="checkbox" id="permission-info" label="info"
                                         defaultChecked={true} disabled inline/>
                            <CustomInput type="checkbox" id="permission-list" label="list"
                                         className={this.props.isFile ? 'd-none' : ''} inline/>
                            <CustomInput type="checkbox" id="permission-hash" label="hash" inline/>
                            <CustomInput type="checkbox" id="permission-read" label="read" inline/>
                            <CustomInput type="checkbox" id="permission-write" label="write" inline/>
                        </div>
                    </FormGroup>

                    <FormGroup>
                        <Label for="permission">Visibility</Label>
                        <div>
                            <CustomInput type="radio" name="is-listed" id="unlisted"
                                         label="Link only" defaultChecked inline/>
                            <CustomInput type="radio" name="is-listed" id="listed" label="Listed" inline/>
                        </div>
                    </FormGroup>


                    {this.state.submitError ? (
                        <FormGroup>
                            <Label className="text-danger">{this.state.submitError}</Label>
                        </FormGroup>
                    ) : null}

                    <Button className="mt-2" type="button" onClick={() => this.submit()}>Share</Button>
                </Form>
                <div className={this.state.isSubmitting ? 'center' : 'd-none'}>
                    <Loading/>
                </div>
            </div>
        );
    }

    componentDidMount() {
        return Promise.all([this.loadUsers(), this.updateItem(this.props.path)]);
    }

    componentDidUpdate(prevProps, prevState, snapshot) {
        return this.updateItem(this.props.path);
    }
}
