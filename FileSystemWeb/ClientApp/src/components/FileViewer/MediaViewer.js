import React, { Component } from 'react';
import { formatUrl } from '../../Helpers/Fetch';
import './MediaViewer.css'

export default class MediaViewer extends Component {
    static displayName = MediaViewer.name;

    constructor(props) {
        super(props);

        this.state = {
            type: this.props.type || null,
            error: null,
        };
    }

    onLoaded(target) {
        this.setState({
            type: !!target.videoWidth || !!target.videoHeight ? 'video' : 'audio'
        });
    }

    render() {
        if (this.state.error) {
            return (
                <div className="media-viewer-error">
                    {this.state.error}
                </div>
            );
        }

        const mediaUrl = formatUrl({
            resource: '/api/files',
            path: this.props.path,
        });

        if (this.state.type === 'audio') {
            return (
                <audio
                    className="media-viewer-content"
                    src={mediaUrl}
                    autoPlay={true}
                    controls
                    onCanPlay={e => this.onLoaded(e.target)}
                    onError={e => this.setState({ error: e.target.error })}
                />
            );
        }

        return (
            <video
                className="media-viewer-content"
                src={mediaUrl}
                autoPlay={true}
                controls
                onCanPlay={e => this.onLoaded(e.target)}
                onError={e => this.setState({ error: e.target.error })}
            />
        );
    }
}