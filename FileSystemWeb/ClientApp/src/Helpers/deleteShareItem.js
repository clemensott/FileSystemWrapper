import React from 'react';
import {closeLoadingModal, getAllRefs, showErrorModal, showLoadingModal} from './storeExtensions';

export default async function (item, callback = null) {
    const allRefs = getAllRefs();
    const deleteItem = await allRefs.deleteShareItem.current.show(item);
    if (!deleteItem) return;

    try {
        showLoadingModal();

        const url = item.isFile ?
            `/api/share/file/${encodeURIComponent(item.id)}` :
            `/api/share/folder/${encodeURIComponent(item.id)}`;
        const response = await fetch(url, {
            method: 'DELETE'
        });

        closeLoadingModal();
        if (response.ok) await callback && callback();
        else {
            const text = await response.text();
            await showErrorModal(
                <div>
                    Status: {response.status}
                    <br/>
                    {text}
                </div>
            );
        }
    } catch (e) {
        closeLoadingModal();
        await showErrorModal(e.message);
    }
}