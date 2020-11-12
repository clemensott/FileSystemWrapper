import React, { Component } from 'react';
import { formatUrl } from '../../Helpers/Fetch';
import Loading from '../Loading/Loading';
import './PdfViewer.css'

export default class PdfViewer extends Component {
    static displayName = PdfViewer.name;

    constructor(props) {
        super(props);

        this.isUnmounted = true;
        this.fetchPath = null;
        this.state = {
            loading: true,
            localUrl: null,
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
        if (this.state.localUrl) {
            return (
                <embed src={this.state.localUrl} type="application/pdf" className="pdf-viewer-pdf" />
            );
        }
        if (this.state.error) {
            return (
                <div className="pdf-viewer-error">
                    {this.state.error}
                </div>
            );
        }
        return (
            <label className="pdf-viewer-no-pdf">
                &lt;No Text&gt;
            </label>
        );
    }

    async componentDidMount() {
        this.isUnmounted = false;
        await this.checkUpdatePdf();
    }

    async componentDidUpdate() {
        await this.checkUpdatePdf();
    }

    async checkUpdatePdf() {
        const path = this.props.path;
        if (path !== this.fetchPath && path !== this.state.filePath) await this.updatePdf(path);
    }

    async updatePdf(path) {
        const pdfUrl = formatUrl({
            resource: '/api/files',
            path: path,
        });

        try {
            this.fetchPath = path;
            const response = await fetch(pdfUrl);

            if (this.isUnmounted || this.fetchPath !== path) return;

            if (response.ok) {
                const blob = await response.blob();
                this.setState({
                    loading: false,
                    localUrl: URL.createObjectURL(blob),
                });
            } else {
                const error = await response.text();
                this.setState({
                    loading: false,
                    error: error || response.statusText || response.status,
                });
            }
        } catch (err) {
            console.error(err);
            if (this.fetchPath === path) {
                this.setState({
                    loading: false,
                    error: err.message,
                });
            }
        }
    }

    componentWillUnmount() {
        this.isUnmounted = true;
        if (this.state.localUrl) URL.revokeObjectURL(this.state.localUrl);
    }
}