import React, { useEffect, useState } from 'react';
import { Collapse, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { NavLink as RRNavLink } from 'react-router-dom';
import store from '../Helpers/store'
import './NavMenu.css';

export default function () {
    const [collapsed, setCollapsed] = useState(true);
    const toggleCollapsed = () => setCollapsed(!collapsed);

    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [isLoggedInCallbackId, setIsLoggedInCallbackId] = useState(null);

    useEffect(() => {
        setIsLoggedInCallbackId(store.addCallback('isLoggedIn', value => setIsLoggedIn(value)));
        setIsLoggedIn(store.get('isLoggedIn'));
    }, []);
    useEffect(() => {
        return () => isLoggedInCallbackId && store.removeCallback('isLoggedIn', isLoggedInCallbackId)
    }, [isLoggedInCallbackId]);

    const authUrl = isLoggedIn ? '/logout' : '/login';
    const authTitle = isLoggedIn ? 'Logout' : 'Login';

    return (
        <header>
            <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3" container="sm" light>
                <NavbarBrand tag={RRNavLink} to="/">Home</NavbarBrand>
                <NavbarToggler onClick={toggleCollapsed} className="me-2" />
                <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!collapsed}
                    navbar>
                    <ul className="navbar-nav flex-grow">
                        {isLoggedIn ? (
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
