import React, {Component} from 'react';
import {Redirect} from 'react-router-dom';
import Loading from './Loading/Loading';
import store from '../Helpers/store';

export class Logout extends Component {
    static displayName = Logout.name;

    constructor(props) {
        super(props);

        this.state = {
            finished: false,
        }
    }

    render() {
        if (this.state.finished) {
            return (
                <Redirect to="/login"/>
            );
        }

        return (
            <div className="center">
                <Loading/>
            </div>
        );
    }

    async componentDidMount() {
        try {
            this.setState({
                loading: true,
            });

            const response = await fetch('/api/auth/logout', {
                method: 'POST',
                credentials: 'include'
            });

            if (response.ok) store.set('isLoggedIn', false);
            else {
                store.get('refs').errorModal.current.show(await response.text());
            }
        } catch (e) {
            alert(e.message);
        } finally {
            this.setState({
                finished: true,
            });
        }
    }
}