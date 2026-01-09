import formatUrl from './formatUrl';

export default class API {
    static antiforgaryToken = null;
    static config = null;

    static async fetch(resource, { path, query, method = 'GET', body, headers, ...options } = {}) {
        let antiforgaryHeaders = null;
        if (method !== 'GET') {
            if (!API.antiforgaryToken || true) {
                const antiforgary = await API.getAntiforgary();
                if (!antiforgary.ok) {
                    return antiforgary;
                }
                API.antiforgaryToken = await antiforgary.text();
            }
            antiforgaryHeaders = {
                'X-CSRF-TOKEN-HEADERNAME': API.antiforgaryToken,
            };
        }

        const isFormData = body instanceof FormData;
        const contentType = (!isFormData && method !== 'GET' && body) ? 'application/json;charset=utf-8' : undefined;
        return window.fetch(`/api${formatUrl({ resource, path, query })}`, {
            credentials: 'include',
            method,
            headers: {
                ...(contentType ? { 'Content-Type': contentType } : null),
                ...antiforgaryHeaders,
                ...headers,
            },
            body: body && !isFormData ? JSON.stringify(body) : body,
            ...options,
        });
    }

    static async login(username, password, keepLoggedIn) {
        try {
            API.antiforgaryToken = null;
            return await this.fetch('/auth/login', {
                method: 'POST',
                body: {
                    Username: username,
                    Password: password,
                    KeepLoggedIn: keepLoggedIn,
                },
            });
        } finally {
            API.antiforgaryToken = null;
        }
    }

    static async logout() {
        try {
            API.antiforgaryToken = null;
            return await this.fetch('/auth/logout', {
                method: 'POST',
            });
        } finally {
            API.antiforgaryToken = null;
        }
    }

    static getAntiforgary() {
        return this.fetch('/auth/antiforgary', {
            method: 'GET',
        });
    }

    static isAuthorized() {
        return this.fetch('/ping/auth');
    }

    static async loadConfig() {
        const response = await this.fetch('/config');
        if (response.ok) {
            API.config = await response.json();
        }
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
        const formData = new FormData();
        formData.append('FileContent', file);
        return this.fetch('/files', {
            path,
            method: 'POST',
            body: formData,
        });
    }

    static deleteFile(path) {
        return this.fetch('/files', {
            path,
            method: 'DELETE',
        });
    }

    static getFolderContent(path, sortType = undefined, sortDirection = undefined) {
        return this.fetch('/folders/content', {
            path,
            query: [
                {
                    key: 'sortType',
                    value: sortType,
                },
                {
                    key: 'sortDirection',
                    value: sortDirection,
                }
            ].filter(q => q.value),
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

    static deleteFolder(path, recursive) {
        return this.fetch('/folders', {
            path,
            query: [
                {
                    key: 'recursive',
                    value: recursive,
                }
            ],
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

    static startBigFileUpload(path) {
        return this.fetch('/bigFile/start', {
            method: 'POST',
            path,
        });
    }

    static appendBigFileUpload(uuid, blob) {
        const formData = new FormData();
        formData.append('partialFile', blob);
        return this.fetch(`/bigFile/${uuid}/append`, {
            method: 'POST',
            body: formData,
        });
    }

    static finshBigFileUpload(uuid) {
        return this.fetch(`/bigFile/${uuid}/finish`, {
            method: 'PUT',
        });
    }

    static cancelBigFileUpload(uuid) {
        return this.fetch(`/bigFile/${uuid}`, {
            method: 'DELETE',
        });
    }


    static getFileSystemItemInfo(path, isFile) {
        return isFile ? this.getFileInfo(path) : this.getFolderInfo(path);
    }

    static deleteFileSystemItem(path, isFile) {
        return isFile ? this.deleteFile(path) : this.deleteFolder(path, true);
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
