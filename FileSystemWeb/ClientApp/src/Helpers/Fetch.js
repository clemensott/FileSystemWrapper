export function formatUrl({ resource, path, password, query = [] }) {
    if (typeof (path) === 'string') query.push({ key: 'path', value: path });
    if (typeof (password) === 'string') query.push({ key: 'password', value: password });

    const queryString = query.map(q => q.key + '=' + encodeURIComponent(q.value)).join('&');
    return resource + '?' + queryString;
}

export function fetchApi({ resource, path, password, query = [] }) {
    const url = formatUrl({ resource, path, query });

    return fetch(url, {
        headers: {
            password,
        },
        credentials: 'include'
    });
}

export default {
    formatUrl,
    fetchApi,
}