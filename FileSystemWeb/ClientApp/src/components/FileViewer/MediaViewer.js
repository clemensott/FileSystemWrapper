import React, { Component } from 'react';
import { formatUrl } from '../../Helpers/Fetch';
import './MediaViewer.css'

export default class MediaViewer extends Component {
    static displayName = MediaViewer.name;

    constructor(props) {
        super(props);
    }

    render() {
        const imageUrl = formatUrl({
            resource: '/api/files',
            path: this.props.path,
            password: this.props.password,
        });

        return (
            <video
                className="media-content"
                src={imageUrl}
                autoPlay="true"
                controls
            />
        );
    }
}