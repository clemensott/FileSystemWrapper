﻿import API from './API';

//const bigFileSize = 50 * 1024 * 1024; // 10 MB
//const bigFileChunckSize = 5 * 1024 * 1024; // 5 MB
const bigFileSize = 1 * 1024 * 1024;
const bigFileChunckSize = 0.5 * 1024 * 1024;

export default async function uploadFile(path, file) {
    console.log('uploadfile2:', file.size, bigFileSize)
    if (file.size <= bigFileSize) {
        return API.createFile(path, file);
    }

    let uploadUuid = null;
    try {
        const startResponse = await API.startBigFileUpload(path);
        if (!startResponse.ok) {
            return startResponse;
        }
        uploadUuid = await startResponse.text();

        for (let i = 0; i < file.size; i += bigFileChunckSize) {
            const chuck = file.slice(i, bigFileChunckSize);
            const chuckResponse = await API.appendBigFileUpload(uploadUuid, chuck);

            if (!chuckResponse.ok) {
                return chuckResponse;
            }
        }
        const finishResponse = await API.finshBigFileUpload(uploadUuid);
        if (finishResponse.ok) {
            uploadUuid = null;
        }
        return finishResponse;
    } finally {
        if (uploadUuid) {
            API.cancelBigFileUpload(uploadUuid);
        }
    }
}
