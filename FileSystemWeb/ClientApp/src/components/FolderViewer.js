import React, { Component } from 'react';
import FSItem from './FSItem'
import { getName, getParent, encodeToURI } from '../Helpers/Path'
import { Link } from 'react-router-dom';
import { fetchApi } from '../Helpers/Fetch';
import Loading from './Loading/Loading';

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
        }
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
                <li key={item.path}>
                    <Link to={fileLink}>
                        <FSItem isFile={true} path={item.path} name={item.name} />
                    </Link>
                </li>
            )
        }

        const folderLink = `/${encodeToURI(item.path)}`;
        return (
            <li key={item.path}>
                <Link to={folderLink}>
                    <FSItem isFile={false} path={item.path} name={item.name} />
                </Link>
            </li>
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
                <span>
                    <Link to={parentUrl} >
                        <button disabled={parentPath === null}>
                            Parent
                        </button>
                    </Link>
                    <label className="path">
                        {path}
                    </label>
                </span>
                <ul>
                    {folders}
                    {files}
                </ul>
                <div className={isLoading ? 'center' : 'center hidden'}>
                    <Loading />
                </div>
            </div>
        );
    }
}
