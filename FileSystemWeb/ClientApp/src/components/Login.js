import React, { Component } from 'react';
import { Redirect } from 'react-router-dom';

export class Login extends Component {
    static displayName = Login.name;

    constructor(props) {
        super(props);
        
        this.passwordInput = React.createRef();
        this.state = {
            password: localStorage.getItem('password') || '',
            submited: false,
        }
    }

    submit() {
        const password = this.passwordInput.current.value;
        if (password) {
            localStorage.setItem('password', password);
            this.setState({ password, submited: true });
        }
    }

    render() {
        if (this.state.submited) {
            return (
                <Redirect to="/" />
            );
        }

        return (
            <div>
                <div>
                    <label>Password</label>
                </div>
                <div>
                    <input ref={this.passwordInput} type="password" defaultValue={this.state.password} />
                </div>
                <div>
                    <button onClick={() => this.submit()}>Login</button>
                </div>
            </div>
        );
    }

    componentDidMount() {
        this.passwordInput.current.focus();
    }
}