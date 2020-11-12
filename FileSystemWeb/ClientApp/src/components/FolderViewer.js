import React, {Component} from 'react';
import FSItem from './FSItem'
import {getName, getParent, getFileType, encodeBase64Custom} from '../Helpers/Path'
import {Link} from 'react-router-dom';
import {fetchApi, formatUrl} from '../Helpers/Fetch';
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

        this.headOffsetTop = null;
        this.headContainerRef = React.createRef();
        this.onScroll = this.onScroll.bind(this);
    }

    async updateItems(path, force = false) {
        try {
            await Promise.all([
                this.updateFolders(path, force),
                this.updateFiles(path, force),
            ]);
        } catch (e) {
            console.log(e);
        }
    }

    async updateFolders(path, force) {
        if (!force && this.foldersFetchPath === path) return;
        this.foldersFetchPath = path;

        const response = await fetchApi({resource: '/api/folders/listfolders', path});

        let folders = [];
        if (response.ok) {
            folders = await response.json();
        }

        if (path === this.props.path) {
            this.setState({
                foldersPath: path,
                folders: folders.map(f => this.getItem(f, false)),
            });
        }
    }

    async updateFiles(path, force) {
        if (!force && this.filesFetchPath === path) return;
        this.filesFetchPath = path;

        const response = await fetchApi({resource: '/api/folders/listfiles', path});

        let files = [];
        if (response.ok) {
            files = await response.json();
        }

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
            const fileLink = `/${encodeBase64Custom(this.props.path)}/${encodeBase64Custom(item.name)}`;
            const fileOpenLink = formatUrl({
                resource: '/api/files',
                path: item.path,
            });
            const fileDownloadLink = formatUrl({
                resource: '/api/files/download',
                path: item.path,
            });
            const isSupportedFile = ['image', 'audio', 'video', 'text', 'pdf'].includes(getFileType(item.path));

            return (
                <div key={item.path} className="folder-viewer-file-item-container">
                    <div className="m-2 folder-viewer-file-item-content">
                        <Link to={fileLink}>
                            <FSItem isFile={true} path={item.path} name={item.name}/>
                        </Link>
                    </div>
                    <a href={fileOpenLink} target="_blank" className={isSupportedFile ? '' : 'd-none'}>
                        <i className="m-2 fas fa-external-link-square-alt fa-2x"/>
                    </a>
                    <a href={fileDownloadLink} download={item.name} className="text-dark">
                        <i className="m-2 fas fa-download fa-2x"/>
                    </a>
                </div>
            )
        }

        const folderLink = `/${encodeBase64Custom(item.path)}`;
        return (
            <div key={item.path} className="m-2">
                <Link to={folderLink}>
                    <FSItem isFile={false} path={item.path} name={item.name}/>
                </Link>
            </div>
        )
    }

    render() {
        const path = this.props.path;
        const parentPath = getParent(path);
        const parentUrl = parentPath ? '/' + encodeBase64Custom(parentPath) : '/';
        const folders = this.state.foldersPath === path ? this.state.folders.map(i => this.renderItem(i)) : [];
        const files = this.state.filesPath === path ? this.state.files.map(i => this.renderItem(i)) : [];

        const isLoading = this.state.foldersPath !== path || this.state.filesPath !== path;

        return (
            <div>
                <div ref={this.headContainerRef}
                     className={`folder-viewer-head-container ${this.state.isOnTop ? '' : 'folder-viewer-head-sticky'}`}>
                    <div onClick={() => this.updateItems(this.props.path, true)}>
                        <i className="folder-viewer-head-update-icon fa fa-retweet fa-2x"/>
                    </div>
                    <Link to={parentUrl}>
                        <i className={`pl-2 fa fa-arrow-up fa-2x ${parentPath === null ? 'd-none' : ''}`}/>
                    </Link>
                    <div className="path pl-2 folder-viewer-head-path">
                        {path}
                    </div>
                </div>
                <div className="folder-viewer-list" style={{
                    paddingTop: `${!this.state.isOnTop && this.headContainerRef.current && this.headContainerRef.current.offsetHeight || 0}px`
                }}>
                    {folders}
                    {files}
                </div>
                <div className={isLoading || folders.length || files.length ? 'd-none' : 'text-center'}>
                    <h3 className="font-italic">&lt;Empty&gt;</h3>
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
                    <Loading/>
                </div>
            </div>
        );
    }

    componentDidMount() {
        window.onscroll = this.onScroll;
        this.headOffsetTop = this.headContainerRef.current.offsetTop;
        return this.updateItems(this.props.path);
    }

    componentDidUpdate(prevProps, prevState, snapshot) {
        return this.updateItems(this.props.path);
    }

    componentWillUnmount() {
        window.onscroll = this.onScroll;
    }

    onScroll() {
        this.setState({
            isOnTop: document.body.scrollTop < this.headOffsetTop && document.documentElement.scrollTop < this.headOffsetTop,
        });
    }
}
