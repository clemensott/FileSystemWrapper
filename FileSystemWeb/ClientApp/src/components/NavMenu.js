import React, {Component} from 'react';
import {Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink} from 'reactstrap';
import {NavLink as RRNavLink} from 'react-router-dom';
import store from '../Helpers/store'
import './NavMenu.css';

export class NavMenu extends Component {
    static displayName = NavMenu.name;

    constructor(props) {
        super(props);

        this.toggleNavbar = this.toggleNavbar.bind(this);
        this.onUpdateIsLoggedIn = this.onUpdateIsLoggedIn.bind(this);

        this.state = {
            isLoggedIn: null,
            collapsed: true
        };
    }

    toggleNavbar() {
        this.setState({
            collapsed: !this.state.collapsed
        });
    }

    render() {
        const authUrl = this.state.isLoggedIn ? '/logout' : '/login';
        const authTitle = this.state.isLoggedIn ? 'Logout' : 'Login';

        return (
            <header>
                <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3" light>
                    <Container>
                        <NavbarBrand tag={RRNavLink} to="/">Root</NavbarBrand>
                        <NavbarToggler onClick={this.toggleNavbar} className="mr-2"/>
                        <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!this.state.collapsed}
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

    componentDidMount() {
        this.isLoggedInCallbackId = store.addCallback('isLoggedIn', this.onUpdateIsLoggedIn)
    }

    componentWillUnmount() {
        store.removeCallback('isLoggedIn', this.isLoggedInCallbackId);
    }

    onUpdateIsLoggedIn(value) {
        this.setState({
            isLoggedIn: value,
        });
    }
}
