import React from 'react';
import store from './store'
import {encodeBase64UnicodeCustom} from './Path';

export default async function (item, callback = null) {
    const allRefs = store.get('refs');
    const deleteItem = await allRefs.deleteFSItemModal.current.show(item);
    if (!deleteItem) return;

    try {
        allRefs.loadingModal.current.show();
        const url = item.isFile ?
            `/api/files/${encodeBase64UnicodeCustom(item.path)}` :
            `/api/folders/${encodeBase64UnicodeCustom(item.path)}`;
        const response = await fetch(url, {
            method: 'DELETE'
        });

        if (response.ok) await callback && callback();
        else {
            const text = await response.text();
            await allRefs.errorModal.current.show(
                <div>
                    Status: {response.status}
                    <br/>
                    {text}
                </div>
            );
        }
    } catch (e) {
        await allRefs.errorModal.current.show(e.message);
    } finally {
        allRefs.loadingModal.current && allRefs.loadingModal.current.close();
    }
}