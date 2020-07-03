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

        document.title = 'Login - File System';

        return (
            <form onSubmit={e => {
                e.preventDefault();
                this.submit();
            }}>
                <div className="form-group">
                    <label >Password</label>
                    <input ref={this.passwordInput} type="password" className="form-control" defaultValue={this.state.password} />
                </div>
                <div>
                    <button className="btn btn-primary" onClick={() => this.submit()}>Login</button>
                </div>
            </form>
        );
    }

    componentDidMount() {
        this.passwordInput.current.focus();
    }
}