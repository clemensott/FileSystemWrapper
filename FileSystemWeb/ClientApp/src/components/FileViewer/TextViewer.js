import React, { Component } from 'react';
import Loading from '../Loading/Loading';
import {encodeBase64UnicodeCustom} from "../../Helpers/Path";
import './TextViewer.css'

export default class TextViewer extends Component {
    static displayName = TextViewer.name;

    constructor(props) {
        super(props);

        this.isUnmounted = true;
        this.fetchPath = null;
        this.state = {
            loading: true,
            filePath: null,
            text: null,
            error: null,
        };
    }

    render() {
        if (this.state.loading) {
            return (
                <div className="center">
                    <Loading />
                </div>
            );
        }
        if (this.state.text) {
            return (
                <textarea className="text-viewer-text" readOnly="true" defaultValue={this.state.text}/>
            );
        }
        if (this.state.error) {
            return (
                <label className="text-viewer-error">
                    {this.state.error}
                </label>
            );
        }
        return (
            <label className="text-viewer-no-text">
                &lt;No Text&gt;
            </label>
        );
    }

    async componentDidMount() {
        this.isUnmounted = false;
        await this.checkUpdateText();
    }

    async componentDidUpdate() {
        await this.checkUpdateText();
    }

    async checkUpdateText() {
        const path = this.props.path;
        if (path !== this.fetchPath && path !== this.state.filePath) await this.updateText(path);
    }

    async updateText(path) {
        const textUrl = `/api/files/${encodeBase64UnicodeCustom(path)}`;

        try {
            this.fetchPath = path;
            this.setState({
                loading: true,
                error: null,
            });
            const response = await fetch(textUrl);

            if (this.isUnmounted || this.fetchPath !== path) return;
            const text = await response.text();

            if (response.ok) {
                this.setState({
                    loading: false,
                    filePath: path,
                    text,
                });
            } else {
                this.setState({
                    loading: false,
                    filePath: path,
                    error: text || response.statusText || response.status,
                });
            }
        } catch (err) {
            console.error(err);
            if (this.fetchPath === path) {
                this.setState({
                    loading: false,
                    filePath: path,
                    error: err.message,
                });
            }
        }
    }

    componentWillUnmount() {
        this.isUnmounted = true;
    }
}