import { encodeBase64UnicodeCustom } from './Path';

export default function formatUrl({ resource, path, query = [] }) {
    if (typeof (path) === 'string') query.push({ key: 'path', value: encodeBase64UnicodeCustom(path) });

    let queryString = query.map(q => q.key + '=' + encodeURIComponent(q.value)).join('&');
    if (queryString) {
        queryString = `?${queryString}`;
    }
    return `${resource}${queryString}`;
}
