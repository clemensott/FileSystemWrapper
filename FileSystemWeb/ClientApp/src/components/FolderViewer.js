import React, {Component} from 'react';
import FSItem from './FSItem'
import {getParent, encodeBase64UnicodeCustom, getName} from '../Helpers/Path'
import {Link} from 'react-router-dom';
import Loading from './Loading/Loading';
import './FolderViewer.css';
import FileActionsDropdown from "./FileActionsDropdown";

export class FolderViewer extends Component {
    static displayName = FolderViewer.name;

    constructor(props) {
        super(props);

        this.contentFetchPath = null;

        this.state = {
            contentPath: null,
            content: null,
            isOnTop: true,
        }

        this.headOffsetTop = null;
        this.headContainerRef = React.createRef();
        this.onScroll = this.onScroll.bind(this);
    }

    async updateContent(path, force = false) {
        if (!force && this.contentFetchPath === path) return;
        this.contentFetchPath = path;

        let content = null;
        try {
            const response = await fetch(`/api/folders/content/${encodeBase64UnicodeCustom(path)}`, {
                credentials: 'include',
            });

            if (response.ok) {
                content = await response.json();
                content.folders.forEach(f => f.isFile = false);
                content.files.forEach(f => f.isFile = true);
            }
        } catch (e) {
            console.log(e);
        }

        if (path === this.props.path) {
            this.props.onFolderLoaded && this.props.onFolderLoaded(content);
            this.setState({
                contentPath: path,
                content,
            });
        }
    }

    renderPathParts(parts) {
        const renderParts = [];
        parts.forEach((part, i) => {
            if (i + 1 === parts.length) {
                renderParts.push(part.name);
            } else {
                const link = `/?folder=${encodeURIComponent(part.path)}`
                renderParts.push(<Link key={part.path} to={link}>{part.name}</Link>);
                renderParts.push('\\');
            }
        });
        return renderParts;
    }

    renderItem(item) {
        if (item.isFile) {
            try{
                encodeURIComponent(this.props.path)
            }catch (e) {
                console.log('error encode:',this.props.path);
            }
            try{
                encodeURIComponent(getName(item.path))
            }catch (e) {
                console.log('error encode:',getName(item.path));
            }
            const fileLink = `/?folder=${encodeURIComponent(this.props.path)}&file=${encodeURIComponent(getName(item.path))}`;

            return (
                <div key={item.path} className="p-2 folder-viewer-item-container folder-viewer-file-item-container">
                    <div
                        className={`folder-viewer-file-item-content ${item.permission.info ? 'folder-viewer-item-container-link' : ''}`}>
                        {item.permission.info ? (
                            <Link to={fileLink}>
                                <FSItem item={item}/>
                            </Link>
                        ) : (
                            <div>
                                <FSItem item={item}/>
                            </div>
                        )}
                    </div>
                    <FileActionsDropdown file={item}/>
                </div>
            )
        }

        const folderLink = `/?folder=${encodeURIComponent(item.path)}`;
        return (
            <div key={item.path}
                 className={`p-2 folder-viewer-item-container ${item.permission.list ? 'folder-viewer-item-container-link' : ''}`}>
                {item.permission.list ? (
                    <Link to={folderLink}>
                        <FSItem item={item}/>
                    </Link>
                ) : (
                    <div>
                        <FSItem item={item}/>
                    </div>
                )}
            </div>
        )
    }

    render() {
        const path = this.props.path;
        const parentPath = getParent(path);
        const parentUrl = `/?folder=${encodeURIComponent(parentPath)}`;
        const {contentPath, content} = this.state;
        const pathParts = contentPath === path && content && content.path ? this.renderPathParts(content.path) : [];
        const folders = contentPath === path && content && content.folders ? content.folders.map(i => this.renderItem(i)) : [];
        const files = contentPath === path && content && content.files ? content.files.map(i => this.renderItem(i)) : [];

        const isLoading = contentPath !== path;

        return (
            <div>
                <div ref={this.headContainerRef}
                     className={`folder-viewer-head-container ${this.state.isOnTop ? '' : 'folder-viewer-head-sticky'}`}>
                    <div onClick={() => this.updateContent(this.props.path, true)}>
                        <i className="folder-viewer-head-update-icon fa fa-retweet fa-2x"/>
                    </div>
                    <Link to={parentUrl}>
                        <i className={`pl-2 fa fa-arrow-up fa-2x ${parentPath === null ? 'd-none' : ''}`}/>
                    </Link>
                    <div className="path pl-2 folder-viewer-head-path">
                        {pathParts}
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
        return this.updateContent(this.props.path);
    }

    componentDidUpdate(prevProps, prevState, snapshot) {
        return this.updateContent(this.props.path);
    }

    componentWillUnmount() {
        window.onscroll = this.onScroll;
    }

    onScroll() {
        const newIsOnTop = document.body.scrollTop < this.headOffsetTop && document.documentElement.scrollTop < this.headOffsetTop;
        if (this.state.isOnTop !== newIsOnTop) {
            this.setState({
                isOnTop: document.body.scrollTop < this.headOffsetTop && document.documentElement.scrollTop < this.headOffsetTop,
            });
        }
    }
}
