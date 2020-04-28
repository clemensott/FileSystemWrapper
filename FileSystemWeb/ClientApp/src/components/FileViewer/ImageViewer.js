import React, { Component } from 'react';
import { formatUrl } from '../../Helpers/Fetch';
import './ImageViewer.css'

export default class ImageViewer extends Component {
    static displayName = ImageViewer.name;

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
            <img src={imageUrl} className="image-content" />
        );
    }
}