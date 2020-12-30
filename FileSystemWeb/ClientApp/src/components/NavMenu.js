import React, {useEffect, useState} from 'react';
import {Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink} from 'reactstrap';
import {NavLink as RRNavLink} from 'react-router-dom';
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
            <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3" light>
                <Container>
                    <NavbarBrand tag={RRNavLink} to="/">Root</NavbarBrand>
                    <NavbarToggler onClick={toggleCollapsed} className="mr-2"/>
                    <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!collapsed}
                              navbar>
                        <ul className="navbar-nav flex-grow">
                            <NavItem>
                                <NavLink tag={RRNavLink} className="text-dark" to={authUrl}>{authTitle}</NavLink>
                            </NavItem>
                        </ul>
                    </Collapse>
                </Container>
            </Navbar>
        </header>
    );
}
