import React, {Component} from 'react';
import {getFileType, encodeBase64UnicodeCustom} from '../../Helpers/Path';
import ImageViewer from './ImageViewer';
import TextViewer from './TextViewer';
import MediaViewer from './MediaViewer';
import PdfViewer from './PdfViewer';
import FileActionsDropdown from "../FileActionsDropdown";
import './FileViewer.css';

export class FileViewer extends Component {
    static displayName = FileViewer.name;

    constructor(props) {
        super(props);

        this.fileFetchPath = null;

        this.state = {
            filePath: null,
            file: null,
            error: null,
        }
    }

    async updateFile(path, force = false) {
        if (!force && this.fileFetchPath === path) return;
        this.fileFetchPath = path;

        let file = null;
        let error = false;
        if (path) {
            try {
                const response = await fetch(`/api/files/${encodeBase64UnicodeCustom(path)}/info`, {
                    credentials: 'include',
                });

                if (response.ok) {
                    file = await response.json();
                } else if (response.status === 403) {
                    error = 'Forbidden to access file';
                } else if (response.status === 404) {
                    error = 'File not found';
                } else if (response.status === 401) {
                    error = 'Unauthorized. It seems like your are not logged in';
                } else {
                    error = 'An error occured';
                }
            } catch (e) {
                console.log(e);
                error = e.message;
            }
        }

        if (path === this.props.path) {
            this.props.onFileInfoLoaded && this.props.onFileInfoLoaded(file);
            this.setState({
                filePath: path,
                file,
                error,
            });
        }
    }

    getViewer(file) {
        if (!file.permission.read) {
            return (
                <div className="text-center'">
                    <h3 className={`font-italic file-viewer-info-text ${this.props.theme}`}>
                        Not authorized for reading
                    </h3>
                </div>
            );
        }

        switch (getFileType(file.extension)) {
            case 'image':
                return <ImageViewer path={file.path}/>

            case 'text':
                return <TextViewer path={file.path}/>;

            case 'audio':
                return <MediaViewer path={file.path} type="audio"/>;

            case 'video':
                return <MediaViewer path={file.path} type="video"/>;

            case 'pdf':
                return <PdfViewer path={file.path}/>;
        }

        return (
            <div className="text-center'">
                <h3 className={`font-italic file-viewer-info-text ${this.props.theme}`}>
                    No preview for this file available
                </h3>
            </div>
        );
    }

    render() {
        if (this.state.error) {
            return (
                <div className="text-center'">
                    <h3 className={`font-italic file-viewer-info-text ${this.props.theme}`}>
                        {this.state.error}
                    </h3>
                </div>
            );
        }
        if (!this.state.file) {
            return null;
        }

        const file = this.state.file;
        const viewer = this.getViewer(file);

        return (
            <div className="file-viewer-container">
                <div className="file-viewer-header-container">
                    <div style={{flexGrow: 1}}/>
                    <h4 className={`file-viewer-title ${this.props.theme}`}>{file.name}</h4>
                    <div style={{flexGrow: 1}}/>
                    <div className="m-1">
                        <FileActionsDropdown file={file}
                                             title="Options"
                                             hideOpenFileLink={this.props.hideOpenFileLinkAction}/>
                    </div>
                </div>
                <div className="file-viewer-content-remaining">
                    <div className="file-viewer-content-container">
                        <div className="file-viewer-content">
                            {viewer}
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    componentDidMount() {
        return this.updateFile(this.props.path);
    }

    componentDidUpdate(prevProps, prevState, snapshot) {
        return this.updateFile(this.props.path);
    }
}
