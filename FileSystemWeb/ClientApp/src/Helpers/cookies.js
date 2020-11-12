function getCookieValue(name) {
    const cookies = document.cookie.split(";");
    for (let i = 0; i < cookies.length; i++) {
        const pair = cookies[i].split('=');
        if (pair[0].trim() === name) return decodeURIComponent(pair[1]);
    }
    return null;
}

function setCookie({name, value, expires, path = '/'}) {
    document.cookie = `${name}=${value}; expires=${expires.toUTCString()}; path=${path}`
}

function deleteCookie(name) {
    document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;`
}

export {
    getCookieValue,
    setCookie,
    deleteCookie,
}