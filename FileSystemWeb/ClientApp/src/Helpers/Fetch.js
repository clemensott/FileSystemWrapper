import {encodeBase64UnicodeCustom} from "./Path";

export function formatUrl({resource, path, query = []}) {
    if (typeof (path) === 'string') query.push({key: 'path', value: encodeBase64UnicodeCustom(path)});

    const queryString = query.map(q => q.key + '=' + encodeURIComponent(q.value)).join('&');
    return resource + '?' + queryString;
}

export function fetchApi({resource, path, query = []}) {
    const url = formatUrl({resource, path, query});

    return fetch(url, {
        credentials: 'include'
    });
}

export default {
    formatUrl,
    fetchApi,
}