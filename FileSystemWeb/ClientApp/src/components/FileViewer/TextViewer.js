import React, { Component } from 'react';
import { formatUrl } from '../../Helpers/Fetch';

export default class TextViewer extends Component {
    static displayName = TextViewer.name;

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
            <div>
                <p>Text</p>
            </div>
        );
    }
}