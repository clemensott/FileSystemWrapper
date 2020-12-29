import React from 'react';
import {encodeBase64UnicodeCustom} from './Path';
import {closeLoadingModal, getAllRefs, showErrorModal, showLoadingModal} from './storeExtensions';

export default async function (item, callback = null) {
    const allRefs = getAllRefs();
    const deleteItem = await allRefs.deleteFSItemModal.current.show(item);
    if (!deleteItem) return;

    try {
        showLoadingModal();
        const url = item.isFile ?
            `/api/files/${encodeBase64UnicodeCustom(item.path)}` :
            `/api/folders/${encodeBase64UnicodeCustom(item.path)}`;
        const response = await fetch(url, {
            method: 'DELETE'
        });

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
        await showErrorModal(e.message);
    } finally {
        closeLoadingModal();
    }
}