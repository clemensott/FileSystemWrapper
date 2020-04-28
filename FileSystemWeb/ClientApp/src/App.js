import React from 'react';
import { Route, Switch } from 'react-router-dom';
import { Login } from './components/Login';
import { NavMenu } from './components/NavMenu';
import { Container } from 'reactstrap';
import Logout from './components/Logout';
import TestCom from './components/TestComponent';
import Home from './components/Home';
import './App.css';

export default function () {
    console.log('render app', window.location);
    return (
        <div>
            <NavMenu />
            <Container>
                <Switch>
                    <Route path='/login' component={Login} />
                    <Route path='/logout' component={Logout} />
                    <Route path='/test/:path'>
                        <TestCom />
                    </Route>
                    <Route exact path='/' component={Home} />
                    <Route exact path='/:folder' component={Home} />
                    <Route exact path='/:folder/:file' component={Home} />
                </Switch>
            </Container>
        </div>
    );
}
