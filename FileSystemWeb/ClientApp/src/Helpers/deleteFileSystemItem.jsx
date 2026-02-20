import React from 'react';
import API from './API';
import sleep from './sleep';
import { closeLoadingModal, getAllRefs, showErrorModal, showLoadingModal } from './storeExtensions';

export default async function (item, callback = null) {
    const allRefs = getAllRefs();
    const deleteItem = await allRefs.deleteFSItemModal.current.show(item);
    if (!deleteItem) return;

    try {
        const promise = API.deleteFileSystemItem(item.path, item.isFile);
        await sleep(200);
        showLoadingModal();
        const response = await promise;
        closeLoadingModal();

        if (response.ok) callback && await callback();
        else {
            const text = await response.text();
            await showErrorModal(
                <div>
                    Status: {response.status}
                    <br />
                    {text}
                </div>
            );
        }
    } catch (e) {
        closeLoadingModal();
        await showErrorModal(e.message);
    }
}