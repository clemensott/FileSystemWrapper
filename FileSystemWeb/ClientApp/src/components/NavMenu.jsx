import React, {useState} from 'react';
import {Collapse, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink} from 'reactstrap';
import {NavLink as RRNavLink} from 'react-router-dom';
import {roles} from '../constants';
import {useAuth} from '../contexts/AuthContext';
import './NavMenu.css';

export default function () {
    const [collapsed, setCollapsed] = useState(true);
    const {user} = useAuth();

    const toggleCollapsed = () => setCollapsed(!collapsed);

    const authUrl = user ? '/logout' : '/login';
    const authTitle = user ? 'Logout' : 'Login';

    return (
        <header>
            <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3"
                    container="sm" light>
                <NavbarBrand tag={RRNavLink} to="/">Home</NavbarBrand>
                <NavbarToggler onClick={toggleCollapsed} className="me-2"/>
                <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!collapsed}
                          navbar>
                    <ul className="navbar-nav flex-grow">
                        {user ? (
                            <NavItem>
                                <NavLink tag={RRNavLink} className="text-dark" to="/user">User</NavLink>
                            </NavItem>
                        ) : null}
                        {user && user.roles.includes(roles.USER_MANAGER) ? (
                            <NavItem>
                                <NavLink tag={RRNavLink} className="text-dark" to="/users">Users</NavLink>
                            </NavItem>
                        ) : null}
                        {user && user.roles.includes(roles.SHARE_MANAGER) ? (
                            <NavItem>
                                <NavLink tag={RRNavLink} className="text-dark" to="/share">Shares</NavLink>
                            </NavItem>
                        ) : null}
                        <NavItem>
                            <NavLink tag={RRNavLink} className="text-dark" to={authUrl}>{authTitle}</NavLink>
                        </NavItem>
                    </ul>
                </Collapse>
            </Navbar>
        </header>
    );
}
