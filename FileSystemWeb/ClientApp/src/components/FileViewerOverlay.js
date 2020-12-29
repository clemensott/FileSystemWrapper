import React, {Component} from 'react';
import {Link} from 'react-router-dom';
import {FileViewer} from './FileViewer/FileViewer';
import './FileViewerOverlay.css';

export class FileViewerOverlay extends Component {
    static displayName = FileViewerOverlay.name;

    constructor(props) {
        super(props);

        this.closeRef = React.createRef();
    }

    render() {
        return (
            <div className="file-viewer-overlay" onClick={e => {
                if (e.target.classList.contains('file-viewer-overlay') ||
                    e.target.classList.contains('file-viewer-overlay-container') ||
                    e.target.classList.contains('file-viewer-content') ||
                    e.target.classList.contains('image-container')) {
                    this.closeRef.current.click();
                }
            }}>
                <div className="file-viewer-overlay-container">
                    <FileViewer theme="dark" onClose={() => this.closeRef.current.click()} {...this.props} />
                </div>
                <Link to={this.props.closeUrl} innerRef={this.closeRef}>
                    <i className="fas fa-times fa-3x file-viewer-overlay-close"/>
                </Link>
            </div>
        );
    }
}