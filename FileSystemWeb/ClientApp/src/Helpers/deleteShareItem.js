import React from "react";
import store from './store'

export default async function (item, callback = null) {
    const allRefs = store.get('refs');
    const deleteItem = await allRefs.deleteShareItem.current.show(item);
    if (!deleteItem) return;

    try {
        allRefs.loadingModal.current.show();

        const url = item.isFile ? `/api/share/file/${encodeURIComponent(item.id)}` :
            `/api/share/folder/${encodeURIComponent(item.id)}`;
        const response = await fetch(url, {
            method: 'DELETE'
        });

        if (response.ok) await callback && callback();
        else {
            const text = await response.text();
            allRefs.errorModal.current.show(
                <div>
                    Status: {response.status}
                    <br/>
                    {text}
                </div>
            );
        }
    } catch (e) {
        allRefs.errorModal.current.show(e.message);
    } finally {
        allRefs.loadingModal.current && allRefs.loadingModal.current.close();
    }
}