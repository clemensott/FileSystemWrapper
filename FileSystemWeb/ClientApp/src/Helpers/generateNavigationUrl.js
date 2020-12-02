export function generateQueryUrl(replace) {
    const query = new URLSearchParams(window.location.search)

    let newSearch = '';
    query.forEach((oldValue, key) => {
        let value;
        if (typeof replace[key] === 'string' || typeof replace[key] === 'number') {
            value = replace[key];
        } else if (replace[key] === null) {
            return;
        } else value = oldValue;

        const pair = `${encodeURIComponent(key)}=${encodeURIComponent(value)}`
        newSearch = newSearch ? `${newSearch}&${pair}` : pair;
    });

    if (newSearch) {
        newSearch = `?${newSearch}`;
    }
    return window.location.pathname + newSearch;
}