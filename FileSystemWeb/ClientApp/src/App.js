import React from 'react';
import { Route, Switch } from 'react-router-dom';
import { Login } from './components/Login';
import { NavMenu } from './components/NavMenu';
import { Container } from 'reactstrap';
import { Logout } from './components/Logout';
import Home from './components/Home';
import {getCookieValue} from "./Helpers/cookies";
import './App.css';

export default function () {
    const isLoggedIn = !!getCookieValue('fs_login');
    
    return (
        <div>
            <NavMenu isLoggedIn={isLoggedIn}/>
            <Container>
                <Switch>
                    <Route path='/login' component={Login} />
                    <Route path='/logout' component={Logout} />
                    <Route exact path='/' component={Home} />
                    <Route exact path='/:folder' component={Home} />
                    <Route exact path='/:folder/:file' component={Home} />
                </Switch>
            </Container>
        </div>
    );
}
