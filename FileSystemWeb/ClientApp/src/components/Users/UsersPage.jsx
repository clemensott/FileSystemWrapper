import React, {useEffect, useRef} from 'react';
import {Button, Table} from 'reactstrap';
import API from '../../Helpers/API';
import DeleteUserModal from './DeleteUserModal';
import CreateUserModal from './CreateUserModal';
import {useGlobalRefs} from '../../contexts/GlobalRefsContext';
import './UsersPage.css'

export default function () {
    const {showErrorModal} = useGlobalRefs();
    const [users, setUsers] = React.useState(null);
    const createUserModal = useRef();
    const deleteUserModal = useRef();

    async function loadUsers() {
        try {
            const res = await API.getAllOverviewUsers();
            if (res.ok) {
                setUsers(await res.json());
            } else {
                await showErrorModal('Could not load users');
            }
        } catch (error) {
            await showErrorModal(error.message);
        }
    }

    async function handleCreateUser() {
        const user = await createUserModal.current.show();
        if (user) {
            try {
                const res = await API.createUser(user);
                if (res.ok) {
                    await loadUsers();
                } else {
                    await showErrorModal('Could not create user');
                }
            } catch (error) {
                await showErrorModal(error.message);
            }
        }
    }

    async function handleDeleteUser(user) {
        const deleteUser = await deleteUserModal.current.show({name: user.name});
        if (deleteUser) {
            try {
                const res = await API.deleteUser(user.id);
                if (res.ok) {
                    await loadUsers();
                } else {
                    await showErrorModal('Could not delete user');
                }
            } catch (error) {
                await showErrorModal(error.message);
            }
        }
    }

    useEffect(() => {
        loadUsers();
    }, []);

    function renderUser(user) {
        return (
            <tr key={user.id}>
                <td>{user.name}</td>
                <td>{user.roles.map((role, index) => (
                    <React.Fragment key={role}>
                        {index > 0 ? <br/> : null}
                        {role}
                    </React.Fragment>
                ))}</td>
                <td>
                    <Button color="warning" className="mb-2 me-2">
                        <i className="fas fa-edit"/>
                    </Button>
                    <Button color="danger" className="mb-2 me-2" onClick={() => handleDeleteUser(user)}>
                        <i className="fas fa-trash"/>
                    </Button>
                </td>
            </tr>
        )
    }

    return (
        <div id="users-page-container">
            <CreateUserModal ref={createUserModal}/>
            <DeleteUserModal ref={deleteUserModal}/>

            <div id="users-page-actions" className="float-end m-2">
                <Button color="primary" className="me-2" onClick={handleCreateUser}>
                    <i className="fas fa-add"/>
                </Button>
                <div className="m-2" onClick={() => loadUsers()}>
                    <i className="folder-viewer-head-update-icon fa fa-retweet fa-2x"/>
                </div>
            </div>

            <h2>Users</h2>

            <div>
                {users ? (
                    <Table striped>
                        <thead>
                        <tr>
                            <th>Name</th>
                            <th>Roles</th>
                            <th className="users-page-header-actions">Action</th>
                        </tr>
                        </thead>
                        <tbody>
                        {users.map(renderUser)}
                        </tbody>
                    </Table>
                ) : (
                    <div>Loading users...</div>
                )}
            </div>
        </div>
    );
}