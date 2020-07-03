import React, { Component } from 'react';
import FSItem from './FSItem'
import { getName, getParent, encodeToURI } from '../Helpers/Path'
import { Link } from 'react-router-dom';
import { fetchApi } from '../Helpers/Fetch';
import Loading from './Loading/Loading';
import './FolderViewer.css';

export class FolderViewer extends Component {
    static displayName = FolderViewer.name;

    constructor(props) {
        super(props);

        this.foldersFetchPath = null;
        this.filesFetchPath = null;

        this.state = {
            foldersPath: null,
            filesPath: null,
            folders: [],
            files: [],
            isOnTop: true,
        }

        this.onScroll = this.onScroll.bind(this);
    }

    async updateItems(path) {
        try {
            await Promise.all([
                this.updateFolders(path),
                this.updateFiles(path),
            ]);
        }
        catch (e) {
            console.log(e);
        }
    }

    async updateFolders(path) {
        if (this.foldersFetchPath === path) return;
        this.foldersFetchPath = path;

        const response = await fetchApi({ resource: '/api/folders/listfolders', path, password: this.props.password });
        const folders = await response.json();

        if (path === this.props.path) {
            this.setState({
                foldersPath: path,
                folders: folders.map(f => this.getItem(f, false)),
            });
        }
    }

    async updateFiles(path) {
        if (this.filesFetchPath === path) return;
        this.filesFetchPath = path;

        const response = await fetchApi({ resource: '/api/folders/listfiles', path, password: this.props.password });
        const files = await response.json();

        if (path === this.props.path) {
            this.setState({
                filesPath: path,
                files: files.map(f => this.getItem(f, true)),
            });
        }
    }

    getItem(path, isFile) {
        return {
            isFile,
            path: path,
            name: getName(path),
        }
    }

    renderItem(item) {

        if (item.isFile) {
            const fileLink = `/${encodeToURI(this.props.path)}/${encodeToURI(item.path)}`;
            return (
                <div key={item.path} className="m-2">
                    <Link to={fileLink}>
                        <FSItem isFile={true} path={item.path} name={item.name} />
                    </Link>
                </div>
            )
        }

        const folderLink = `/${encodeToURI(item.path)}`;
        return (
            <div key={item.path} className="m-2">
                <Link to={folderLink}>
                    <FSItem isFile={false} path={item.path} name={item.name} />
                </Link>
            </div>
        )
    }

    render() {
        this.updateItems(this.props.path);

        const path = this.props.path;
        const parentPath = getParent(path);
        const parentUrl = parentPath ? '/' + encodeToURI(parentPath) : '/';
        const folders = this.state.foldersPath === path ? this.state.folders.map(i => this.renderItem(i)) : [];
        const files = this.state.filesPath === path ? this.state.files.map(i => this.renderItem(i)) : [];

        const isLoading = this.state.foldersPath !== path || this.state.filesPath !== path;

        return (
            <div>
                <div className="folder-viewer-head-container">
                    <Link to={parentUrl} >
                        <i className={`fa fa-arrow-up fa-2x ${parentPath === null ? 'd-none' : ''}`} />
                    </Link>
                    <div className="path pl-2 folder-viewer-head-path">
                        {path}
                    </div>
                </div>
                <div className="folder-viewer-list">
                    {folders}
                    {files}
                </div>
                <div className={`folder-viewer-to-top-container ${this.state.isOnTop ? 'd-none' : ''}`}>
                    <button className="btn btn-info" onClick={() => {
                        document.body.scrollTop = 0; // For Safari
                        document.documentElement.scrollTop = 0; // For Chrome, Firefox, IE and Opera
                    }}>
                        BACK TO TOP
                    </button>
                </div>
                <div className={isLoading ? 'center' : 'center d-none'}>
                    <Loading />
                </div>
            </div>
        );
    }

    componentDidMount() {
        window.onscroll = this.onScroll;
    }

    componentWillUnmount() {
        window.onscroll = this.onScroll;
    }

    onScroll() {
        this.setState({
            isOnTop: document.body.scrollTop < 20 && document.documentElement.scrollTop < 20,
        });
    }
}
