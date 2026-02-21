import React, {useEffect, useState} from 'react';
import {Collapse, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink} from 'reactstrap';
import {NavLink as RRNavLink} from 'react-router-dom';
import store from '../Helpers/store'
import './NavMenu.css';
import {roles} from "../constants";

export default function () {
    const [collapsed, setCollapsed] = useState(true);
    const toggleCollapsed = () => setCollapsed(!collapsed);

    const [me, setMe] = useState(null);
    const [meCallbackId, setMeCallbackId] = useState(null);

    useEffect(() => {
        setMeCallbackId(store.addCallback('me', value => setMe(value)));
        setMe(store.get('me'));
    }, []);
    useEffect(() => {
        return () => meCallbackId && store.removeCallback('me', meCallbackId)
    }, [meCallbackId]);

    const authUrl = me ? '/logout' : '/login';
    const authTitle = me ? 'Logout' : 'Login';

    return (
        <header>
            <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3"
                    container="sm" light>
                <NavbarBrand tag={RRNavLink} to="/">Home</NavbarBrand>
                <NavbarToggler onClick={toggleCollapsed} className="me-2"/>
                <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!collapsed}
                          navbar>
                    <ul className="navbar-nav flex-grow">
                        {me && me.roles.includes(roles.USER_MANAGER) ? (
                            <NavItem>
                                <NavLink tag={RRNavLink} className="text-dark" to="/user">User</NavLink>
                            </NavItem>
                        ) : null}
                        {me ? (
                            <NavItem>
                                <NavLink tag={RRNavLink} className="text-dark" to="/users">Users</NavLink>
                            </NavItem>
                        ) : null}
                        {me && me.roles.includes(roles.SHARE_MANAGER) ? (
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
