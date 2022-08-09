import formatUrl from './formatUrl';

export default class API {
    static fetch(resource, { path, query, method, body, headers, ...options } = {}) {
        const contentType = (method && method !== 'GET' && body) ? 'application/json;charset=utf-8' : undefined;
        return window.fetch(`/api${formatUrl({ resource, path, query })}`, {
            credentials: 'include',
            method,
            headers: {
                ...(contentType ? { 'Content-Type': contentType } : null),
                ...headers,
            },
            body: body ? JSON.stringify(body) : undefined,
            ...options,
        });
    }

    static login(username, password, keepLoggedIn) {
        return this.fetch('/auth/login', {
            method: 'POST',
            body: {
                Username: username,
                Password: password,
                KeepLoggedIn: keepLoggedIn,
            },
        });
    }

    static logout() {
        return this.fetch('/auth/logout', {
            method: 'POST',
        });
    }

    static isAuthorized() {
        return this.fetch('/ping/auth');
    }

    static getAllUsers() {
        return this.fetch('/users/all');
    }

    static getFile(path) {
        return this.fetch('/files', {
            path,
        });
    }

    static getFileExists(path) {
        return this.fetch('/files/exists', {
            path,
        });
    }

    static getFileInfo(path) {
        return this.fetch('/files/info', {
            path,
        });
    }

    static createFile(path, file) {
        return this.fetch('/files', {
            path,
            method: 'POST',
            body: file,
            headers: {
                'Content-Type': undefined,
            },
        });
    }

    static deleteFile(path) {
        return this.fetch('/files', {
            path,
            method: 'DELETE',
        });
    }

    static getFolderContent(path) {
        return this.fetch('/folders/content', {
            path,
        });
    }

    static getFolderExists(path) {
        return this.fetch('/folders/exists', {
            path,
        });
    }

    static getFolderInfo(path) {
        return this.fetch('/folders/info', {
            path,
        });
    }

    static deleteFolder(path) {
        return this.fetch('/folders', {
            path,
            method: 'DELETE',
        });
    }

    static getShareFiles() {
        return this.fetch('/share/files');
    }

    static getShareFile(id) {
        return this.fetch(`/share/file/${encodeURIComponent(id)}`);
    }

    static createShareFile(data) {
        return this.fetch('/share/file', {
            method: 'POST',
            body: data,
        });
    }

    static putShareFile(id, data) {
        return this.fetch(`/share/file/${encodeURIComponent(id)}`, {
            method: 'PUT',
            body: data,
        });
    }

    static deleteShareFile(id) {
        return this.fetch(`/share/file/${id}`, {
            method: 'DELETE',
        });
    }

    static getShareFolders() {
        return this.fetch('/share/folders');
    }

    static getShareFolder(id) {
        return this.fetch(`/share/folder/${encodeURIComponent(id)}`);
    }

    static createShareFolder(data) {
        return this.fetch('/share/folder', {
            method: 'POST',
            body: data,
        });
    }

    static putShareFolder(id, data) {
        return this.fetch(`/share/folder/${encodeURIComponent(id)}`, {
            method: 'PUT',
            body: data,
        });
    }

    static deleteShareFolder(id) {
        return this.fetch(`/share/folder/${id}`, {
            method: 'DELETE',
        });
    }


    static getFileSystemItemInfo(path, isFile) {
        return isFile ? this.getFileInfo(path) : this.getFolderInfo(path);
    }

    static deleteFileSystemItem(path, isFile) {
        return isFile ? this.deleteFile(path) : this.deleteFolder(path);
    }

    static getShareItems(isFile) {
        return isFile ? this.getShareFiles() : this.getShareFolders();
    }

    static getShareItem(id, isFile) {
        return isFile ? this.getShareFile(id) : this.getShareFolder(id);
    }

    static createShareItem(data, isFile) {
        return isFile ? this.createShareFile(data) : this.createShareFolder(data);
    }

    static putShareItem(id, data, isFile) {
        return isFile ? this.putShareFile(id, data) : this.putShareFolder(id, data);
    }

    static deleteShareItem(id, isFile) {
        return isFile ? this.deleteShareFile(id) : this.deleteShareFolder(id);
    }
}
