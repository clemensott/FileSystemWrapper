import React, {Component} from 'react';
import {Redirect} from 'react-router-dom';
import Loading from "./Loading/Loading";

export class Login extends Component {
    static displayName = Login.name;

    constructor(props) {
        super(props);

        this.usernameInput = React.createRef();
        this.passwordInput = React.createRef();
        this.state = {
            isUsernameInvalid: false,
            isPasswordInvalid: false,
            successful: false,
            loading: false,
            error: null,
        }
    }

    async submit() {
        const username = this.usernameInput.current.value;
        const password = this.passwordInput.current.value;

        if (!username) {
            this.setState({
                isUsernameInvalid: true,
            });
        }
        if (!password) {
            this.setState({
                isPasswordInvalid: true,
            });
        }
        if (!username || !password) {
            return;
        }

        try {
            this.setState({
                loading: true,
            });

            const response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json;charset=utf-8'
                },
                body: JSON.stringify({
                    Username: username,
                    Password: password,
                    keepLoggedIn: true,
                }),
                credentials: 'include'
            });

            if (response.ok) {
                this.setState({
                    loading: false,
                    successful: true,
                });
            } else if (response.status === 400) {
                this.setState({
                    loading: false,
                    error: 'Please enter a correct Username and password',
                });
            }
        } catch (e) {
            this.setState({
                loading: false,
                error: e.message,
            });
        }
    }

    render() {
        if (this.state.successful) {
            return (
                <Redirect to="/"/>
            );
        }

        document.title = 'Login - File System';

        return (
            <div>
                <form className={this.state.loading ? 'd-none' : ''} onSubmit={e => {
                    e.preventDefault();
                    return this.submit();
                }}>
                    <div className="form-group">
                        <label>Username</label>
                        <input ref={this.usernameInput} type="text"
                               className={`form-control ${this.state.isUsernameInvalid ? 'is-invalid' : ''}`}
                               onChange={e => {
                                   this.setState({
                                       isUsernameInvalid: !e.target.value,
                                   });
                               }}/>
                    </div>
                    <div className="form-group">
                        <label>Password</label>
                        <input ref={this.passwordInput} type="password"
                               className={`form-control ${this.state.isPasswordInvalid ? 'is-invalid' : ''}`}
                               onChange={e => {
                                   this.setState({
                                       isPasswordInvalid: !e.target.value,
                                   });
                               }}/>
                    </div>

                    <div className={`form-group form-check  ${this.state.error ? '' : 'd-none'}`}>
                        <label className="form-check-label">
                            <input className="is-invalid d-none"/>
                            <div className="invalid-feedback">{this.state.error}</div>
                        </label>
                    </div>

                    <div>
                        <button className="btn btn-primary" type="submit">Login</button>
                    </div>
                </form>
                <div className={this.state.loading ? 'center' : 'd-none'}>
                    <Loading/>
                </div>
            </div>
        );
    }

    componentDidMount() {
        this.usernameInput.current.focus();
    }
}